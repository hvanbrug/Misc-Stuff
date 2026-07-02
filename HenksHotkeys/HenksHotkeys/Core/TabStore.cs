using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using HenksHotkeys.Tabs;
using HenksHotkeys.UI;
using Newtonsoft.Json;

namespace HenksHotkeys.Core;

/// <summary>
/// Loads the tab/button content from %LocalAppData%\HenksHotkeys\tabs.json,
/// writing the embedded default there on first run. Data entries become
/// <see cref="DataTabModel"/>s; entries that name a built-in (Emojis / Tools)
/// become the corresponding code tab. Falls back to the embedded default if the
/// user's file is missing or unreadable.
/// </summary>
internal static class TabStore
{
  private const string ResourceName = "tabs.default.json";

  public static string Path
  {
    get => System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                                   "HenksHotkeys", "tabs.json" );
  }

  // Last fully-known state, used to detect local edits for version stamping.
  private static string ShadowPath
  {
    get => System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                                   "HenksHotkeys", "tabs.shadow.json" );
  }

  private static readonly JsonSerializerSettings JsonSettings = new()
  {
    Formatting           = Formatting.Indented,
    NullValueHandling    = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore,
  };

  /// <summary>False when the user's tabs.json failed to parse and the embedded
  /// default was used instead (so a reload can warn about a bad edit).</summary>
  public static bool LastParseOk { get; private set; } = true;

  /// <summary>The live config from the last <see cref="Load"/> — kept so in-app edits
  /// can be persisted with the file-level crypto header and tombstones intact (a tab's
  /// <c>m_entry</c> is the same instance held here, so editing a button mutates this).</summary>
  private static TabFile? s_file;

  public static List<TabModel> Load()
  {
    TabFile? file = ReadAndSync();

    var tabs = new List<TabModel>();
    if( file is null )
    {
      return tabs;
    }

    foreach( TabEntry entry in file.Tabs )
    {
      TabModel? tab = Build( entry );
      if( tab is not null )
      {
        tabs.Add( tab );
      }
    }
    return tabs;
  }

  // Parse tabs.json, version-stamp local edits, process secrets, persist, and
  // refresh the shadow. The single funnel for reading the current config.
  private static TabFile? ReadAndSync()
  {
    string json = ReadOrSeed();
    TabFile? file = TryParse( json );
    LastParseOk = file is not null;
    file ??= TryParse( ReadDefault() );
    if( file is null )
    {
      return null;
    }

    // Upgrade a legacy (rows-based) file to the coordinate format before stamping.
    bool dirty = TabMigrate.Migrate( file );

    dirty |= VersionStamp.Stamp( file, LoadShadow() );
    dirty |= ProcessSecrets( file );
    if( dirty )
    {
      TrySave( file );
    }
    SaveShadow( file );
    s_file = file;
    return file;
  }

  /// <summary>Persist an in-app edit to the live config: re-stamp against the shadow
  /// (which bumps the changed button's clock, or tombstones a removed one), reseal any
  /// secrets, write the file, and refresh the shadow. The model objects were mutated in
  /// place by the caller, so there is nothing to pass in.</summary>
  public static void SaveCurrent()
  {
    if( s_file is null )
    {
      return;
    }
    VersionStamp.Stamp( s_file, LoadShadow() );
    ProcessSecrets( s_file );
    TrySave( s_file );
    SaveShadow( s_file );
  }

  /// <summary>Place a new button at an explicit grid cell of a tab, then persist. If the
  /// cell is already occupied, the occupant and the contiguous run after it shift one
  /// column to the right (wrapping to the next row at the column edge).</summary>
  public static void AddButtonAt( TabEntry tab, int row, int col, ButtonDef newButton )
  {
    tab.Buttons ??= new List<ButtonDef>();
    newButton.Row = row;
    newButton.Col = Math.Max( 0, col );
    if( Occupied( tab, row, newButton.Col, newButton ) )
    {
      ShiftRight( tab, row, newButton.Col );
    }
    tab.Buttons.Add( newButton );
    SaveCurrent();
  }

  /// <summary>Move a button to a grid cell (drag-to-reposition), then persist. Dropping
  /// on an empty cell places it there; dropping on a button inserts before/after it,
  /// shifting that row. The cell it came from is simply left empty. Returns false if the
  /// button isn't found.</summary>
  public static bool MoveButtonToCell( ButtonDef moving, int row, int col )
  {
    if( !Locate( moving, out TabEntry tab ) )
    {
      return false;
    }
    col = Math.Max( 0, col );
    moving.Row = -1;                       // lift it out so it doesn't block itself
    if( Occupied( tab, row, col, moving ) )
    {
      ShiftRight( tab, row, col );
    }
    moving.Row = row;
    moving.Col = col;
    SaveCurrent();
    return true;
  }

  // True when placing <paramref name="mover"/> at cell (row, col) would overlap an existing
  // button there. Sub-cell siblings coexist: a whole-cell button (either side) fills the cell,
  // mismatched sub-divisions overlap, and matching divisions clash only in the same slot.
  private static bool Occupied( TabEntry tab, int row, int col, ButtonDef mover )
  {
    if( tab.Buttons is null )
    {
      return false;
    }
    foreach( ButtonDef b in tab.Buttons )
    {
      if( b.Row != row || b.Col != col || ReferenceEquals( b, mover ) )
      {
        continue;
      }
      if( b.SubCells <= 1 || mover.SubCells <= 1 ) return true; // a whole-cell button fills it
      if( b.SubCells != mover.SubCells )           return true; // mismatched divisions overlap
      if( b.SubCell  == mover.SubCell )            return true; // same division → same slot only
    }
    return false;
  }

  // Open a cell at (row, col) by pushing the contiguous run of cells to its right one column
  // along — staying on this row (only the clicked row is affected). If the run reaches the
  // last column the row simply extends past the tab's column count rather than wrapping the
  // overflow onto the next row. Every button in a cell (incl. a sub-cell group) moves together.
  private static void ShiftRight( TabEntry tab, int row, int col )
  {
    if( tab.Buttons is null )
    {
      return;
    }

    // First free column at or after `col` (a column is occupied if any button sits in it).
    int end = col;
    while( tab.Buttons.Any( b => b.Row == row && b.Col == end ) ) end++;
    if( end == col )
    {
      return; // the target cell is already free
    }

    // Shift the run [col, end) one column right, all on this row — they were contiguous, so
    // each lands on the next column (the last one now sits at `end`, which was free).
    foreach( ButtonDef b in tab.Buttons.Where( b => b.Row == row && b.Col >= col && b.Col < end ) )
    {
      b.Col++;
    }
  }

  private static bool Locate( ButtonDef button, out TabEntry tab )
  {
    if( s_file is not null )
    {
      foreach( TabEntry t in s_file.Tabs )
      {
        if( t.Buttons is not null && t.Buttons.Contains( button ) )
        {
          tab = t;
          return true;
        }
      }
    }
    tab = null!;
    return false;
  }

  /// <summary>Remove a button from the live config, then persist — the re-stamp leaves a
  /// tombstone so the deletion propagates on merge. The cell simply becomes empty.
  /// Returns false if the button wasn't found.</summary>
  public static bool DeleteButton( ButtonDef button )
  {
    if( s_file is null )
    {
      return false;
    }
    foreach( TabEntry t in s_file.Tabs )
    {
      if( t.Buttons is not null && t.Buttons.Remove( button ) )
      {
        SaveCurrent();
        return true;
      }
    }
    return false;
  }

  /// <summary>Add a heading (section divider) to a tab, then persist.</summary>
  public static void AddHeading( TabEntry tab, SectionDef heading )
  {
    tab.Sections ??= new List<SectionDef>();
    tab.Sections.Add( heading );
    SaveCurrent();
  }

  /// <summary>Insert an empty row at <paramref name="row"/> (everything at or below shifts
  /// down one row), then persist — a blank spacer row between two rows.</summary>
  public static void InsertBlankRow( TabEntry tab, int row )
  {
    ShiftRowsDown( tab, row );
    SaveCurrent();
  }

  /// <summary>Insert <paramref name="heading"/> at <paramref name="row"/>, opening space for it
  /// by pushing down *only the cells under it* — the columns the heading spans, at or below its
  /// row (content in other columns stays put) — then persist.</summary>
  public static void InsertHeadingRow( TabEntry tab, int row, SectionDef heading )
  {
    ShiftCellsDown( tab, row, heading.Col, heading.Span );
    heading.Row  = row;
    tab.Sections ??= new List<SectionDef>();
    tab.Sections.Add( heading );
    SaveCurrent();
  }

  // ── Multi-selection operations (buttons + headings together) ─────
  /// <summary>Move a whole selection (buttons + headings) by (<paramref name="dRow"/>,
  /// <paramref name="dCol"/>) within a tab — but only if every moved item lands in-bounds and on
  /// cells not occupied by any *unselected* item. Collisions are prevented and nothing is
  /// inserted/shifted; on any conflict the whole move is refused and nothing changes. Returns
  /// true when the move was applied. (Designed to extend to cross-tab moves later.)</summary>
  public static bool MoveSelection( TabEntry tab, IReadOnlyCollection<ButtonDef> buttons,
                                    IReadOnlyCollection<SectionDef> sections, int dRow, int dCol )
  {
    if( ( dRow == 0 && dCol == 0 ) || ( buttons.Count == 0 && sections.Count == 0 ) )
    {
      return false;
    }
    int cols = tab.Columns > 0 ? tab.Columns : int.MaxValue;
    int SpanOf( SectionDef s ) => s.Span > 0 ? s.Span : ( cols == int.MaxValue ? 1 : cols );

    var moving = new HashSet<object>();
    foreach( ButtonDef b in buttons )   moving.Add( b );
    foreach( SectionDef s in sections ) moving.Add( s );

    // Every cell occupied by an item that is NOT moving.
    var taken = new HashSet<(int Row, int Col)>();
    if( tab.Buttons is not null )
      foreach( ButtonDef b in tab.Buttons )
        if( !moving.Contains( b ) )
          for( int c = 0; c < Math.Max( 1, b.Width ); c++ ) taken.Add( ( b.Row, b.Col + c ) );
    if( tab.Sections is not null )
      foreach( SectionDef s in tab.Sections )
        if( !moving.Contains( s ) )
          for( int c = 0; c < SpanOf( s ); c++ ) taken.Add( ( s.Row, s.Col + c ) );

    bool Fits( int row, int col, int extent )
    {
      if( row < 0 || col < 0 ) return false;
      if( cols != int.MaxValue && col + extent > cols ) return false;
      for( int c = col; c < col + extent; c++ ) if( taken.Contains( ( row, c ) ) ) return false;
      return true;
    }

    foreach( ButtonDef b in buttons )
      if( !Fits( b.Row + dRow, b.Col + dCol, Math.Max( 1, b.Width ) ) ) return false;
    foreach( SectionDef s in sections )
      if( !Fits( s.Row + dRow, s.Col + dCol, SpanOf( s ) ) ) return false;

    foreach( ButtonDef b in buttons )   { b.Row += dRow; b.Col += dCol; }
    foreach( SectionDef s in sections ) { s.Row += dRow; s.Col += dCol; }
    SaveCurrent();
    return true;
  }

  /// <summary>Delete a whole selection (buttons + headings) from a tab, then persist. Returns
  /// true if anything was removed (the re-stamp tombstones the deleted buttons for merge).</summary>
  public static bool DeleteSelection( TabEntry tab, IReadOnlyCollection<ButtonDef> buttons,
                                      IReadOnlyCollection<SectionDef> sections )
  {
    bool any = false;
    if( tab.Buttons is not null )  foreach( ButtonDef b in buttons )   any |= tab.Buttons.Remove( b );
    if( tab.Sections is not null ) foreach( SectionDef s in sections ) any |= tab.Sections.Remove( s );
    if( any ) SaveCurrent();
    return any;
  }

  /// <summary>Move a selection from <paramref name="src"/> to <paramref name="dst"/>, offset by
  /// (<paramref name="dRow"/>, <paramref name="dCol"/>), keeping the items' relative layout. Only
  /// applied if every item lands in-bounds of <paramref name="dst"/> and on a cell not already
  /// occupied there (collisions prevented, nothing inserted); otherwise nothing changes. Returns
  /// true when moved. The items keep their ids, so merge sees a re-homed element, not a delete+add.</summary>
  public static bool MoveSelectionToTab( TabEntry src, TabEntry dst,
                                         IReadOnlyCollection<ButtonDef> buttons,
                                         IReadOnlyCollection<SectionDef> sections, int dRow, int dCol )
  {
    if( ReferenceEquals( src, dst ) || ( buttons.Count == 0 && sections.Count == 0 ) )
    {
      return false;
    }
    int cols = dst.Columns > 0 ? dst.Columns : int.MaxValue;
    int SpanOf( SectionDef s ) => s.Span > 0 ? s.Span : ( cols == int.MaxValue ? 1 : cols );

    // Everything already in the destination occupies cells the move must avoid.
    var taken = new HashSet<(int Row, int Col)>();
    if( dst.Buttons is not null )
      foreach( ButtonDef b in dst.Buttons )
        for( int c = 0; c < Math.Max( 1, b.Width ); c++ ) taken.Add( ( b.Row, b.Col + c ) );
    if( dst.Sections is not null )
      foreach( SectionDef s in dst.Sections )
        for( int c = 0; c < SpanOf( s ); c++ ) taken.Add( ( s.Row, s.Col + c ) );

    bool Fits( int row, int col, int extent )
    {
      if( row < 0 || col < 0 ) return false;
      if( cols != int.MaxValue && col + extent > cols ) return false;
      for( int c = col; c < col + extent; c++ ) if( taken.Contains( ( row, c ) ) ) return false;
      return true;
    }

    foreach( ButtonDef b in buttons )
      if( !Fits( b.Row + dRow, b.Col + dCol, Math.Max( 1, b.Width ) ) ) return false;
    foreach( SectionDef s in sections )
      if( !Fits( s.Row + dRow, s.Col + dCol, SpanOf( s ) ) ) return false;

    dst.Buttons  ??= new List<ButtonDef>();
    dst.Sections ??= new List<SectionDef>();
    foreach( ButtonDef b in buttons )
    {
      src.Buttons?.Remove( b );
      b.Row += dRow; b.Col += dCol;
      dst.Buttons.Add( b );
    }
    foreach( SectionDef s in sections )
    {
      src.Sections?.Remove( s );
      s.Row += dRow; s.Col += dCol;
      dst.Sections.Add( s );
    }
    SaveCurrent();
    return true;
  }

  // Push every button and section at or below <paramref name="row"/> down one row, opening a
  // fresh row there.
  private static void ShiftRowsDown( TabEntry tab, int row )
  {
    if( tab.Buttons is not null )  foreach( ButtonDef b in tab.Buttons )  if( b.Row >= row ) b.Row++;
    if( tab.Sections is not null ) foreach( SectionDef s in tab.Sections ) if( s.Row >= row ) s.Row++;
  }

  // Open space for an inserted heading by pushing down only the cells under its columns —
  // but a multi-column entity (a wider heading or a wide button) can't be split, so from the
  // row where such an entity overlaps the current span, the affected column range widens to
  // include it (and stays widened below, chaining). So the shift is a staircase: narrow above
  // a wider heading, widening at that heading's row and downward.
  private static void ShiftCellsDown( TabEntry tab, int row, int col, int span )
  {
    int cols = tab.Columns;
    int lo   = col;
    int hi   = col + Math.Max( 1, span ) - 1;

    int BtnHi( ButtonDef b )  => b.Col + Math.Max( 1, b.Width ) - 1;
    int SecHi( SectionDef s ) => s.Span > 0 ? s.Col + s.Span - 1 : Math.Max( 0, cols - 1 );

    int maxRow = row;
    if( tab.Buttons is not null )  foreach( ButtonDef b in tab.Buttons )  maxRow = Math.Max( maxRow, b.Row );
    if( tab.Sections is not null ) foreach( SectionDef s in tab.Sections ) maxRow = Math.Max( maxRow, s.Row );

    // Pass 1: scan downward, widening [lo, hi] whenever an entity on the row overlaps it, and
    // record the range in force at each row. The range only ever grows, so it carries down.
    var rangeAt = new (int Lo, int Hi)[maxRow - row + 1];
    for( int r = row; r <= maxRow; r++ )
    {
      for( bool changed = true; changed; )
      {
        changed = false;
        if( tab.Buttons is not null )
          foreach( ButtonDef b in tab.Buttons )
            if( b.Row == r && b.Col <= hi && BtnHi( b ) >= lo )
            {
              if( b.Col   < lo ) { lo = b.Col;   changed = true; }
              if( BtnHi(b) > hi ) { hi = BtnHi(b); changed = true; }
            }
        if( tab.Sections is not null )
          foreach( SectionDef s in tab.Sections )
            if( s.Row == r && s.Col <= hi && SecHi( s ) >= lo )
            {
              if( s.Col   < lo ) { lo = s.Col;   changed = true; }
              if( SecHi(s) > hi ) { hi = SecHi(s); changed = true; }
            }
      }
      rangeAt[r - row] = (lo, hi);
    }

    // Pass 2: shift each entity at/below `row` whose columns fall in the range in force at its
    // (original) row. Using the recorded ranges means the moves don't perturb the computation.
    if( tab.Buttons is not null )
      foreach( ButtonDef b in tab.Buttons )
        if( b.Row >= row )
        {
          (int rlo, int rhi) = rangeAt[b.Row - row];
          if( b.Col <= rhi && BtnHi( b ) >= rlo ) b.Row++;
        }
    if( tab.Sections is not null )
      foreach( SectionDef s in tab.Sections )
        if( s.Row >= row )
        {
          (int rlo, int rhi) = rangeAt[s.Row - row];
          if( s.Col <= rhi && SecHi( s ) >= rlo ) s.Row++;
        }
  }

  /// <summary>Remove a heading (section divider) from the live config, then persist. Returns
  /// false if it wasn't found.</summary>
  public static bool DeleteHeading( SectionDef heading )
  {
    if( s_file is null )
    {
      return false;
    }
    foreach( TabEntry t in s_file.Tabs )
    {
      if( t.Sections is not null && t.Sections.Remove( heading ) )
      {
        SaveCurrent();
        return true;
      }
    }
    return false;
  }

  /// <summary>Write the current (stamped) config to <paramref name="targetPath"/> for sharing.</summary>
  public static bool Export( string targetPath )
  {
    if( ReadAndSync() is null )
    {
      return false;
    }
    try
    {
      File.Copy( Path, targetPath, overwrite: true );
      return true;
    }
    catch
    {
      return false;
    }
  }

  /// <summary>Merge a shared file into the local config (last-writer-wins per tab/
  /// button). Returns false if the file couldn't be read.</summary>
  public static bool Import( string sourcePath )
  {
    TabFile? incoming;
    try   { incoming = TryParse( File.ReadAllText( sourcePath ) ); }
    catch { return false; }
    if( incoming is null )
    {
      return false;
    }
    TabMigrate.Migrate( incoming );           // a shared file may still be the old format
    VersionStamp.Stamp( incoming, null );     // ensure ids/clocks if it was hand-made

    TabFile local = TryParse( ReadOrSeed() ) ?? new TabFile();
    TabMigrate.Migrate( local );
    VersionStamp.Stamp( local, LoadShadow() ); // capture local edits first

    // The two machines seal secrets under their own per-file salts; re-seal the incoming
    // secrets to this machine's salt (same passphrase) so they aren't orphaned on merge.
    ReSealIncomingSecrets( incoming, local );

    // Heal any pre-existing duplicate tabs, then match the two files by identity
    // (name/content) so independently-stamped ids don't duplicate on merge.
    VersionMerge.CollapseDuplicateTabs( local );
    VersionMerge.ReconcileIds( local, incoming );

    TabFile merged = VersionMerge.Merge( local, incoming );
    VersionMerge.CollapseDuplicateTabs( merged );
    ProcessSecrets( merged );
    TrySave( merged );
    SaveShadow( merged );
    return true;
  }

  /// <summary>Merge duplicate (same-name) tabs in the current config into one,
  /// keeping every distinct button. Returns the number of duplicates removed.</summary>
  public static int RepairDuplicates()
  {
    TabFile? file = TryParse( ReadOrSeed() );
    if( file is null )
    {
      return 0;
    }
    TabMigrate.Migrate( file );
    VersionStamp.Stamp( file, LoadShadow() );
    int removed = VersionMerge.CollapseDuplicateTabs( file );
    ProcessSecrets( file );
    TrySave( file );
    SaveShadow( file );
    return removed;
  }

  private static TabFile? LoadShadow()
  {
    try   { return File.Exists( ShadowPath ) ? TryParse( File.ReadAllText( ShadowPath ) ) : null; }
    catch { return null; }
  }

  private static void SaveShadow( TabFile file )
  {
    try
    {
      Directory.CreateDirectory( System.IO.Path.GetDirectoryName( ShadowPath )! );
      File.WriteAllText( ShadowPath, JsonConvert.SerializeObject( file, JsonSettings ) );
    }
    catch
    {
      // shadow is an optimisation; failing to write it is non-fatal
    }
  }

  /// <summary>
  /// Decrypt the secret buttons for use and (re)seal any that need it: plaintext the
  /// user typed, or legacy DPAPI values being migrated to the portable format.
  /// Obtains the passphrase from the local cache or by prompting. Returns true when
  /// the file changed and should be rewritten.
  /// </summary>
  internal static bool ProcessSecrets( TabFile file )
  {
    List<ButtonDef> secrets = file.Tabs
      .Where( t => t.Buttons is not null )
      .SelectMany( t => t.Buttons! )
      .Where( b => !string.IsNullOrEmpty( b.Secret ) )
      .ToList();

    if( secrets.Count == 0 )
    {
      return false;
    }

    // Salt lives in the file (so it travels with it). Created in memory now; only
    // persisted once we actually seal something, so cancelling leaves the file as-is.
    file.Crypto ??= new CryptoHeader();
    if( string.IsNullOrEmpty( file.Crypto.Salt ) )
    {
      file.Crypto.Salt = Convert.ToBase64String( RandomNumberGenerator.GetBytes( 16 ) );
    }
    byte[] salt       = Convert.FromBase64String( file.Crypto.Salt );
    int    iterations = file.Crypto.Iterations > 0 ? file.Crypto.Iterations : Secrets.DefaultIterations;

    bool dirty = false;
    byte[]? key = ObtainKey( file.Crypto, salt, iterations, ref dirty );

    // Record the params so sends can re-derive the key; never keep the key itself.
    SecretSession.Configure( salt, iterations, key is not null );
    if( key is null )
    {
      foreach( ButtonDef b in secrets ) b.Locked = true;
      LockedSecretCount = secrets.Count;
      return dirty; // no passphrase available / cancelled → leave secrets locked
    }

    dirty |= ApplySecrets( secrets, key );
    LockedSecretCount = secrets.Count( b => b.Locked );
    Array.Clear( key ); // the load-time key is done; send time re-derives its own
    return dirty;
  }

  /// <summary>Number of secret buttons that couldn't be decrypted on the last load
  /// (orphaned by a passphrase/salt mismatch). Drives the one-time warning.</summary>
  public static int LockedSecretCount { get; private set; }

  /// <summary>(Re)seal the plaintext / legacy-DPAPI secrets so they're portable, and flag
  /// any portable secret that can't be decrypted with the current key as locked. The
  /// decrypted value is never retained. Returns true when the file changed. Testable.</summary>
  internal static bool ApplySecrets( List<ButtonDef> secrets, byte[] key )
  {
    bool dirty = false;
    foreach( ButtonDef b in secrets )
    {
      string s = b.Secret!;
      if( Secrets.IsPassSealed( s ) )
      {
        b.Locked = !Secrets.CanDecrypt( key, s ); // validate without keeping the plaintext
      }
      else if( Secrets.IsDpapiSealed( s ) )
      {
        // Legacy per-machine secret → migrate to the portable format (only works on
        // the machine that originally sealed it; elsewhere it just stays locked).
        try
        {
          b.Secret = Secrets.Encrypt( key, Secrets.DpapiUnseal( s ) );
          b.Locked = false;
          dirty    = true;
        }
        catch { b.Locked = true; }
      }
      else // plaintext the user just typed in (edit/add) → seal it
      {
        b.Secret = Secrets.Encrypt( key, s );
        b.Locked = false;
        dirty    = true;
      }
    }
    return dirty;
  }

  // Re-encrypt the incoming file's portable secrets from its salt to the local salt so a
  // merge doesn't orphan them. Requires the same passphrase on both machines (the usual
  // case). No-op when the salts already match or either side has no crypto header.
  private static void ReSealIncomingSecrets( TabFile incoming, TabFile local )
  {
    string? inSalt  = incoming.Crypto?.Salt;
    string? locSalt = local.Crypto?.Salt;
    if( string.IsNullOrEmpty( inSalt ) || string.IsNullOrEmpty( locSalt ) || inSalt == locSalt )
    {
      return;
    }

    string? pass = PassphraseStore.Load() ?? PassphrasePrompt.Ask( false, false );
    if( string.IsNullOrEmpty( pass ) )
    {
      return; // no passphrase → can't re-seal (secrets may stay locked, as before)
    }

    byte[] keyIn  = Secrets.DeriveKey( pass, Convert.FromBase64String( inSalt ),
                      incoming.Crypto!.Iterations > 0 ? incoming.Crypto.Iterations : Secrets.DefaultIterations );
    byte[] keyLoc = Secrets.DeriveKey( pass, Convert.FromBase64String( locSalt ),
                      local.Crypto!.Iterations > 0 ? local.Crypto.Iterations : Secrets.DefaultIterations );

    foreach( ButtonDef b in incoming.Tabs.Where( t => t.Buttons is not null )
                                          .SelectMany( t => t.Buttons! ) )
    {
      if( b.Secret is { } s && Secrets.IsPassSealed( s ) )
      {
        try   { b.Secret = Secrets.Encrypt( keyLoc, Secrets.Decrypt( keyIn, s ) ); }
        catch { /* different passphrase / already this salt → leave it */ }
      }
    }
  }

  // Resolve the AES key from the cached passphrase or by prompting (up to a few
  // attempts). Returns null if there is no passphrase available (cancelled / no UI).
  private static byte[]? ObtainKey( CryptoHeader crypto, byte[] salt, int iterations, ref bool dirty )
  {
    bool creating = string.IsNullOrEmpty( crypto.Verifier );

    // Try the locally-cached passphrase first.
    string? cached = PassphraseStore.Load();
    if( cached is not null )
    {
      byte[] k = Secrets.DeriveKey( cached, salt, iterations );
      if( creating || Secrets.VerifyKey( k, crypto.Verifier! ) )
      {
        if( creating ) { crypto.Verifier = Secrets.Encrypt( k, Secrets.Verifier ); dirty = true; }
        return k;
      }
      // cached passphrase no longer matches this file → fall through to prompting
    }

    for( int attempt = 0; attempt < 3; attempt++ )
    {
      string? pass = PassphrasePrompt.Ask( creating, attempt > 0 );
      if( string.IsNullOrEmpty( pass ) )
      {
        return null; // cancelled or no UI hooked up
      }
      byte[] k = Secrets.DeriveKey( pass, salt, iterations );
      if( creating || Secrets.VerifyKey( k, crypto.Verifier! ) )
      {
        PassphraseStore.Save( pass );
        if( creating ) { crypto.Verifier = Secrets.Encrypt( k, Secrets.Verifier ); dirty = true; }
        return k;
      }
      // wrong passphrase → loop (retry flag set)
    }
    return null;
  }

  private static void TrySave( TabFile file )
  {
    try
    {
      Directory.CreateDirectory( System.IO.Path.GetDirectoryName( Path )! );
      File.WriteAllText( Path, JsonConvert.SerializeObject( file, JsonSettings ) );
    }
    catch
    {
      // a failed rewrite must never crash startup
    }
  }

  private static TabModel? Build( TabEntry entry )
  {
    if( !string.IsNullOrEmpty( entry.Builtin ) )
    {
      return entry.Builtin switch
             {
               "Emojis" => new EmojisTab(),
               "Tools"  => new ToolsTab(),
               _        => null, // unknown built-in name → skip
             };
    }
    return new DataTabModel( entry );
  }

  // Read the user's file, creating it from the embedded default the first time.
  private static string ReadOrSeed()
  {
    try
    {
      if( File.Exists( Path ) )
      {
        return File.ReadAllText( Path );
      }

      string seed = ReadDefault();
      Directory.CreateDirectory( System.IO.Path.GetDirectoryName( Path )! );
      File.WriteAllText( Path, seed );
      return seed;
    }
    catch
    {
      return ReadDefault();
    }
  }

  private static string ReadDefault()
  {
    using Stream? s = Assembly.GetExecutingAssembly().GetManifestResourceStream( ResourceName );
    if( s is null )
    {
      return "{\"tabs\":[]}";
    }
    using var reader = new StreamReader( s );
    return reader.ReadToEnd();
  }

  private static TabFile? TryParse( string json )
  {
    try   { return JsonConvert.DeserializeObject<TabFile>( json ); }
    catch { return null; }
  }
}
