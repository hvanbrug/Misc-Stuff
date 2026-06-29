namespace HenksHotkeys.Core;

/// <summary>
/// Change-tracking for sharing tabs.json across machines. Every tab and button
/// carries a stable <c>id</c> and a <c>mod</c> clock; deletions leave tombstones.
/// <see cref="VersionStamp"/> assigns ids and bumps clocks for local edits;
/// <see cref="VersionMerge"/> combines two files with last-writer-wins per element
/// (commutative — importing in any order/direction converges, and no button is
/// ever dropped: any not placed by the winning layout is appended).
/// </summary>
internal static class VersionStamp
{
  private const char Sep = (char)1;

  public static long Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

  public static string NewId() => Guid.NewGuid().ToString( "N" )[..12];

  public static IEnumerable<ButtonDef> Buttons( TabEntry t )
    => t.Buttons ?? Enumerable.Empty<ButtonDef>();

  /// <summary>A button's content only (no grid position) — used to match the "same"
  /// button across machines, which may have moved it to different coordinates.</summary>
  public static string ContentSig( ButtonDef b )
    => string.Join( Sep, b.Text, b.Secret, b.Desc, b.Hotkey, b.Width, b.Align,
                    b.ShowText, b.TipText );

  /// <summary>Full signature: content plus grid position, so a move (row/col change)
  /// bumps the button's clock and wins on merge.</summary>
  public static string ButtonSig( ButtonDef b )
    => ContentSig( b ) + Sep + b.Row + "/" + b.Col;

  /// <summary>Split any row wider than the tab's column count into multiple rows,
  /// so a merge/repair that piled buttons into one row can't make the tab (and
  /// window) absurdly wide. Returns true if it changed anything.</summary>
  public static bool NormalizeRows( TabEntry t )
  {
    if( t.Rows is null )
    {
      return false;
    }

    bool changed = false;

    // Blank rows and section headers draw no buttons — drop any they carry.
    foreach( RowDef r in t.Rows )
    {
      if( ( r.Blank || r.IsSection ) && r.Buttons.Count > 0 ) { r.Buttons.Clear(); changed = true; }
    }

    // Split any (non-blank) row that has more cells than the tab is wide.
    if( t.Columns > 0 && t.Rows.Any( r => !r.Blank && r.Indent + r.Buttons.Count > t.Columns ) )
    {
      var newRows = new List<RowDef>();
      foreach( RowDef r in t.Rows )
      {
        if( r.Blank || r.Indent + r.Buttons.Count <= t.Columns )
        {
          newRows.Add( r );
          continue;
        }
        bool first = true;
        for( int i = 0; i < r.Buttons.Count; )
        {
          int indent = first ? r.Indent : 0;
          int cap    = Math.Max( 1, t.Columns - indent );
          newRows.Add( new RowDef
          {
            GapBefore = first ? r.GapBefore : 0,
            Indent    = indent,
            Buttons   = r.Buttons.Skip( i ).Take( cap ).ToList(),
          } );
          i += cap;
          first = false;
        }
      }
      t.Rows = newRows;
      changed = true;
    }

    return changed;
  }

  // Tab attributes + section dividers (the tab-level layout), but NOT the buttons —
  // each button's content and position are tracked per button.
  public static string TabSig( TabEntry t )
  {
    string attrs = string.Join( Sep, t.Builtin, t.Name, t.Columns, t.FontSize, t.FontName,
                                t.ButtonWidth, t.ButtonHeight, t.EmojiImages, t.StripEmojis,
                                t.Proportional, t.Square );
    string sections = t.Sections is null
      ? ""
      : string.Join( ",", t.Sections.Select( s => s.Row + "/" + s.Name + "/" + s.Height ) );
    return attrs + Sep + sections;
  }

  /// <summary>Assign ids, bump clocks for changes vs the shadow, and add tombstones
  /// for elements that disappeared. Returns true if anything changed.</summary>
  public static bool Stamp( TabFile file, TabFile? shadow )
  {
    long now = Now();
    bool changed = false;

    var shTabs    = new Dictionary<string, TabEntry>();
    var shButtons = new Dictionary<string, ButtonDef>();
    if( shadow is not null )
    {
      foreach( TabEntry t in shadow.Tabs )
      {
        if( t.Id is not null ) shTabs[t.Id] = t;
        foreach( ButtonDef b in Buttons( t ) ) if( b.Id is not null ) shButtons[b.Id] = b;
      }
    }

    var live = new HashSet<string>();

    foreach( TabEntry t in file.Tabs )
    {
      foreach( ButtonDef b in Buttons( t ) )
      {
        if( string.IsNullOrEmpty( b.Id ) )                       { b.Id = NewId(); b.Mod = now; changed = true; }
        else if( shButtons.TryGetValue( b.Id, out ButtonDef? sb ) ) { if( ButtonSig( b ) != ButtonSig( sb ) ) { b.Mod = now; changed = true; } }
        else if( b.Mod == 0 )                                    { b.Mod = now; changed = true; }
        live.Add( b.Id! );
      }

      if( string.IsNullOrEmpty( t.Id ) )                         { t.Id = NewId(); t.Mod = now; changed = true; }
      else if( shTabs.TryGetValue( t.Id, out TabEntry? st ) )    { if( TabSig( t ) != TabSig( st ) ) { t.Mod = now; changed = true; } }
      else if( t.Mod == 0 )                                      { t.Mod = now; changed = true; }
      live.Add( t.Id! );
    }

    // Tombstone anything the shadow had that is gone now.
    List<Tombstone> tombs = file.Deleted ??= new();
    var tombIds = new HashSet<string>( tombs.Select( x => x.Id ) );
    foreach( string id in shTabs.Keys.Concat( shButtons.Keys ) )
    {
      if( !live.Contains( id ) && tombIds.Add( id ) )
      {
        tombs.Add( new Tombstone { Id = id, Mod = now } );
        changed = true;
      }
    }

    // An id that is live again can't be tombstoned locally.
    if( tombs.RemoveAll( x => live.Contains( x.Id ) ) > 0 ) changed = true;
    if( tombs.Count == 0 ) file.Deleted = null;

    return changed;
  }
}

internal static class VersionMerge
{
  private readonly record struct Owned( ButtonDef Btn, string TabId );

  /// <summary>Merge <paramref name="incoming"/> into <paramref name="local"/>
  /// (last-writer-wins per element). Both must already be stamped.</summary>
  public static TabFile Merge( TabFile local, TabFile incoming )
  {
    Dictionary<string, Owned> lb = IndexButtons( local );
    Dictionary<string, Owned> ib = IndexButtons( incoming );
    Dictionary<string, TabEntry> lt = IndexTabs( local );
    Dictionary<string, TabEntry> it = IndexTabs( incoming );

    // Merged tombstones: max mod per id.
    var tomb = new Dictionary<string, long>();
    foreach( Tombstone d in ( local.Deleted ?? new() ).Concat( incoming.Deleted ?? new() ) )
    {
      tomb[d.Id] = Math.Max( tomb.GetValueOrDefault( d.Id ), d.Mod );
    }

    long ActiveMod( Dictionary<string, Owned> a, Dictionary<string, Owned> b, string id )
    {
      long m = 0;
      if( a.TryGetValue( id, out Owned x ) ) m = Math.Max( m, x.Btn.Mod );
      if( b.TryGetValue( id, out Owned y ) ) m = Math.Max( m, y.Btn.Mod );
      return m;
    }

    bool ButtonLive( string id ) => ActiveMod( lb, ib, id ) > tomb.GetValueOrDefault( id );
    bool TabLive( string id, long mod ) => mod > tomb.GetValueOrDefault( id );

    ButtonDef WinButton( string id )
    {
      ButtonDef? a = lb.TryGetValue( id, out Owned x ) ? x.Btn : null;
      ButtonDef? b = ib.TryGetValue( id, out Owned y ) ? y.Btn : null;
      if( a is null ) return b!;
      if( b is null ) return a;
      return b.Mod > a.Mod ? b : a;
    }

    // Which buttons belong to which tab (union of both files).
    var tabButtons = new Dictionary<string, HashSet<string>>();
    foreach( (string id, Owned o) in lb.Concat( ib ) )
    {
      ( tabButtons.TryGetValue( o.TabId, out HashSet<string>? set ) ? set : tabButtons[o.TabId] = new() ).Add( id );
    }

    // Merged tab order: local order first, then incoming-only tabs.
    var order = new List<string>();
    var seen  = new HashSet<string>();
    foreach( TabEntry t in local.Tabs )    if( t.Id is not null && seen.Add( t.Id ) ) order.Add( t.Id );
    foreach( TabEntry t in incoming.Tabs ) if( t.Id is not null && seen.Add( t.Id ) ) order.Add( t.Id );

    var mergedTabs = new List<TabEntry>();
    foreach( string tid in order )
    {
      TabEntry? a = lt.GetValueOrDefault( tid );
      TabEntry? b = it.GetValueOrDefault( tid );
      TabEntry win = a is null ? b! : b is null ? a : ( b.Mod > a.Mod ? b : a );

      if( !TabLive( tid, win.Mod ) )
      {
        continue; // deleted wins
      }

      mergedTabs.Add( BuildTab( win, tid, tabButtons.GetValueOrDefault( tid ) ?? new(), ButtonLive, WinButton ) );
    }

    return new TabFile
    {
      Readme = local.Readme ?? incoming.Readme,
      Crypto = local.Crypto ?? incoming.Crypto,
      Tabs   = mergedTabs,
      Deleted = tomb.Count == 0 ? null : tomb.Select( kv => new Tombstone { Id = kv.Key, Mod = kv.Value } ).ToList(),
    };
  }

  private static TabEntry BuildTab( TabEntry win, string tid, HashSet<string> belongs,
                                    Func<string, bool> live, Func<string, ButtonDef> winButton )
  {
    var t = CloneAttrs( win ); // carries the winning tab's attrs + section dividers
    if( !string.IsNullOrEmpty( win.Builtin ) )
    {
      return t; // builtin tab — nothing to merge
    }

    // Each button carries its own coordinate, so order doesn't matter: collect every
    // live button that belongs to this tab, taking the last-writer-wins copy.
    var placed  = new HashSet<string>();
    var buttons = new List<ButtonDef>();
    foreach( string id in belongs )
    {
      if( live( id ) && placed.Add( id ) )
      {
        buttons.Add( winButton( id ) );
      }
    }
    t.Buttons = ResolveCollisions( buttons, t.Columns );
    return t;
  }

  /// <summary>Two machines may independently drop different buttons on the same
  /// (row, col). Keep them all: order deterministically and push each colliding button
  /// to the next free cell (row-major), so a merge never silently overwrites one.</summary>
  private static List<ButtonDef> ResolveCollisions( List<ButtonDef> buttons, int columns )
  {
    int cols = columns > 0 ? columns : int.MaxValue;
    var taken = new HashSet<long>();
    long Key( int r, int c ) => (long)r * ( cols == int.MaxValue ? 100000 : cols ) + c;

    // Deterministic order: newest edit first, then by current position / id, so both
    // machines converge on the same placement.
    foreach( ButtonDef b in buttons.OrderByDescending( b => b.Mod )
                                   .ThenBy( b => b.Row ).ThenBy( b => b.Col )
                                   .ThenBy( b => b.Id ) )
    {
      while( !taken.Add( Key( b.Row, b.Col ) ) )
      {
        b.Col++;
        if( b.Col >= cols ) { b.Col = 0; b.Row++; }
      }
    }
    return buttons;
  }

  // ── Identity reconciliation (handles files that were stamped with separate
  //    id baselines on each machine, which would otherwise duplicate on merge) ──

  private static string Identity( TabEntry t )
    => !string.IsNullOrEmpty( t.Builtin ) ? "b:" + t.Builtin : "n:" + ( t.Name ?? t.Id ?? "" );

  /// <summary>Rewrite <paramref name="incoming"/> ids to match <paramref name="local"/>
  /// for tabs/buttons that are clearly the same (tab by name/builtin, button by
  /// content), so the id-based merge dedups them instead of keeping both.</summary>
  public static void ReconcileIds( TabFile local, TabFile incoming )
  {
    var localById = new HashSet<string>( local.Tabs.Where( t => t.Id is not null ).Select( t => t.Id! ) );
    var localByKey = new Dictionary<string, List<TabEntry>>();
    foreach( TabEntry t in local.Tabs )
    {
      string k = Identity( t );
      ( localByKey.TryGetValue( k, out List<TabEntry>? l ) ? l : localByKey[k] = new() ).Add( t );
    }

    var usedTab = new HashSet<string>();
    foreach( TabEntry inc in incoming.Tabs )
    {
      TabEntry? match = inc.Id is not null && localById.Contains( inc.Id )
        ? local.Tabs.First( x => x.Id == inc.Id )
        : localByKey.GetValueOrDefault( Identity( inc ) )?.FirstOrDefault( l => l.Id is not null && !usedTab.Contains( l.Id! ) );

      if( match?.Id is null )
      {
        continue;
      }
      inc.Id = match.Id;              // adopt the local id
      usedTab.Add( match.Id );
      ReconcileButtons( match, inc );
    }
  }

  private static void ReconcileButtons( TabEntry local, TabEntry incoming )
  {
    var localIds = new HashSet<string>( VersionStamp.Buttons( local ).Where( b => b.Id is not null ).Select( b => b.Id! ) );
    var bySig = new Dictionary<string, List<ButtonDef>>();
    foreach( ButtonDef b in VersionStamp.Buttons( local ) )
    {
      // Match by content, not position — the same button may sit at a different cell
      // on each machine.
      string s = VersionStamp.ContentSig( b );
      ( bySig.TryGetValue( s, out List<ButtonDef>? l ) ? l : bySig[s] = new() ).Add( b );
    }

    var used = new HashSet<string>();
    foreach( ButtonDef b in VersionStamp.Buttons( incoming ) )
    {
      if( b.Id is not null && localIds.Contains( b.Id ) ) { used.Add( b.Id ); continue; }
      ButtonDef? cand = bySig.GetValueOrDefault( VersionStamp.ContentSig( b ) )
                             ?.FirstOrDefault( x => x.Id is not null && !used.Contains( x.Id! ) );
      if( cand is not null ) { b.Id = cand.Id; used.Add( cand.Id! ); }
    }
  }

  /// <summary>Merge tabs that share an identity (same name/builtin) into one,
  /// keeping every distinct button. Returns the number of duplicate tabs removed.</summary>
  public static int CollapseDuplicateTabs( TabFile f )
  {
    var order  = new List<string>();
    var groups = new Dictionary<string, List<TabEntry>>();
    foreach( TabEntry t in f.Tabs )
    {
      string k = Identity( t );
      if( !groups.TryGetValue( k, out List<TabEntry>? list ) ) { groups[k] = list = new(); order.Add( k ); }
      list.Add( t );
    }

    int removed = 0;
    var result  = new List<TabEntry>();
    foreach( string k in order )
    {
      List<TabEntry> list = groups[k];
      if( list.Count == 1 ) { result.Add( list[0] ); continue; }
      removed += list.Count - 1;
      result.Add( CollapseTabs( list ) );
    }
    f.Tabs = result;
    return removed;
  }

  private static TabEntry CollapseTabs( List<TabEntry> list )
  {
    TabEntry win = list.Aggregate( ( a, b ) => b.Mod > a.Mod ? b : a );
    TabEntry t   = CloneAttrs( win );
    if( !string.IsNullOrEmpty( win.Builtin ) )
    {
      return t;
    }

    // Last-writer-wins per id across all the duplicate copies.
    var byId = new Dictionary<string, ButtonDef>();
    foreach( TabEntry tab in list )
    {
      foreach( ButtonDef b in VersionStamp.Buttons( tab ) )
      {
        if( b.Id is not null && ( !byId.TryGetValue( b.Id, out ButtonDef? ex ) || b.Mod > ex.Mod ) )
        {
          byId[b.Id] = b;
        }
      }
    }

    // Keep one button per distinct content (a duplicate tab often holds identical
    // copies); coordinates then settle any cell clashes.
    var seenSig = new HashSet<string>();
    var buttons = new List<ButtonDef>();
    foreach( ButtonDef b in byId.Values.OrderBy( b => b.Row ).ThenBy( b => b.Col ).ThenBy( b => b.Id ) )
    {
      if( seenSig.Add( VersionStamp.ContentSig( b ) ) ) buttons.Add( b );
    }

    t.Buttons = ResolveCollisions( buttons, t.Columns );
    return t;
  }

  private static TabEntry CloneAttrs( TabEntry s ) => new()
  {
    Builtin = s.Builtin, Name = s.Name, Columns = s.Columns,
    FontSize = s.FontSize, FontName = s.FontName,
    ButtonWidth = s.ButtonWidth, ButtonHeight = s.ButtonHeight,
    EmojiImages = s.EmojiImages, StripEmojis = s.StripEmojis,
    Proportional = s.Proportional, Square = s.Square,
    Sections = s.Sections?.Select( x => new SectionDef { Row = x.Row, Name = x.Name, Height = x.Height } ).ToList(),
    Id = s.Id, Mod = s.Mod,
  };

  private static Dictionary<string, Owned> IndexButtons( TabFile f )
  {
    var map = new Dictionary<string, Owned>();
    foreach( TabEntry t in f.Tabs )
    {
      if( t.Id is null ) continue;
      foreach( ButtonDef b in VersionStamp.Buttons( t ) )
      {
        if( b.Id is not null ) map[b.Id] = new Owned( b, t.Id );
      }
    }
    return map;
  }

  private static Dictionary<string, TabEntry> IndexTabs( TabFile f )
  {
    var map = new Dictionary<string, TabEntry>();
    foreach( TabEntry t in f.Tabs ) if( t.Id is not null ) map[t.Id] = t;
    return map;
  }
}
