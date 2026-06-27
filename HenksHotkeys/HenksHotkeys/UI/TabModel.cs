using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Base class for a tab's button layout. Ports the builder + geometry portion of
/// TabPage in UITabPage.ahk (everything except the Win32 control creation, which
/// the WinForms <see cref="TabPanelControl"/> handles). Concrete tab classes call
/// the Register*/Shift*/NextLine helpers from their constructor.
/// </summary>
internal abstract class TabModel
{
  public string Name        { get; }
  public float  FontSize    { get; protected set; } = 14f;
  public string FontName    { get; protected set; } = "Segoe UI";
  public int    SymOrgX     => Layout.EdgeGap;  // buttons start one edge-gap in
  public int    SymOrgY     => Layout.EdgeGap;
  public int    SymBtnSizeX { get; protected set; } = 35;
  public int    SymBtnSizeY { get; protected set; } = 35;
  public bool   UseEmojiImages   { get; protected set; }
  public bool   EnableStripEmojis{ get; protected set; }

  public int ContentWidth  { get; private set; }
  public int ContentHeight { get; private set; }

  public List<SymbolElement> Symbols { get; } = new();

  /// <summary>Section-header labels laid out among the rows (data tabs only).</summary>
  public List<SectionHeader> Headers { get; } = new();

  private int    m_maxSlots;
  private bool   m_lineIsRow = true;
  private double m_lineShift;
  private int    m_nextLine = 1;
  private int    m_nextSlot = 1;

  protected TabModel( string name )
  {
    Name = name;
  }

  // ── Layout configuration ─────────────────────────────────────────
  protected void SetColsOf( int maxRows )
  {
    m_maxSlots  = maxRows;
    m_lineIsRow = false;
  }

  protected void SetRowsOf( int maxCols )
  {
    m_maxSlots  = maxCols;
    m_lineIsRow = true;
  }

  public int RowHeight => SymBtnSizeY + Layout.ButtonGap;
  public int ColWidth  => SymBtnSizeX + Layout.ButtonGap;

  // Exposed so the data exporter can reconstruct the grid from a built tab.
  public int  MaxSlots  => m_maxSlots;
  public bool LineIsRow => m_lineIsRow;

  private int CalcSymbolX( int line, int slot )
  {
    return m_lineIsRow
             ? SymOrgX + (slot - 1) * ColWidth
             : SymOrgX + (int)Math.Round( ((line - 1) + m_lineShift) * ColWidth );
  }

  private int CalcSymbolY( int line, int slot )
  {
    return m_lineIsRow
             ? SymOrgY + (int)Math.Round( ((line - 1) + m_lineShift) * RowHeight )
             : SymOrgY + (slot - 1) * RowHeight;
  }

  protected void RecalcSizes()
  {
    int maxRight  = 0;
    int maxBottom = 0;
    foreach( SymbolElement s in Symbols )
    {
      maxRight  = Math.Max( maxRight,  s.X + s.W );
      maxBottom = Math.Max( maxBottom, s.Y + s.H );
    }
    foreach( SectionHeader h in Headers )
    {
      maxRight  = Math.Max( maxRight,  h.X + h.Width );
      maxBottom = Math.Max( maxBottom, h.Y + h.Height );
    }

    // Buttons start at (EdgeGap, EdgeGap); add the matching trailing edge-gap.
    ContentWidth  = maxRight  == 0 ? SymBtnSizeX + 2 * Layout.EdgeGap : maxRight  + Layout.EdgeGap;
    ContentHeight = maxBottom == 0 ? SymBtnSizeY + 2 * Layout.EdgeGap : maxBottom + Layout.EdgeGap;
  }

  // ── Line / slot cursor ───────────────────────────────────────────
  protected void NextLine( bool testForEOL = false )
  {
    if( !testForEOL || (m_nextSlot > 1) )
    {
      m_nextLine++;
    }
    m_nextSlot = 1;
  }

  protected void ForceNextSlot( int line, int slot )
  {
    m_nextLine = line;
    m_nextSlot = slot;
  }

  protected void ShiftLineByHalf(  double num = 1 ) => ShiftLineByFraction( num, 2 );
  protected void ShiftLineByThird( double num = 1 ) => ShiftLineByFraction( num, 3 );

  protected void ShiftLineByFraction( double numerator = 1, double denominator = 2 )
  {
    if( denominator != 0 )
    {
      m_lineShift += numerator / denominator;
    }
  }

  protected void RegisterSpace( int slots = 1 ) => AdvanceSlot( slots );

  private void AdvanceSlot( int slots = 1 )
  {
    if( (m_maxSlots <= 0) || (slots <= 0) )
    {
      return;
    }
    m_nextSlot += slots;
    while( m_nextSlot > m_maxSlots )
    {
      m_nextSlot -= m_maxSlots;
      m_nextLine++;
    }
  }

  // ── Registration ─────────────────────────────────────────────────
  protected void RegisterSymbolX( int     width,
                                  string  ch,
                                  string? desc     = null,
                                  string? hotkey   = null,
                                  Action? action   = null,
                                  string  align    = "center",
                                  int     showChar = 1,
                                  int     tipChar  = 0 )
  {
    RegisterSymbol( m_nextLine, m_nextSlot, 1, width, ch, desc, hotkey, action, align, showChar, tipChar );
  }

  protected void RegisterSymbol( int     line,
                                 int     slot,
                                 int     advanceBy,
                                 int     width,
                                 string  ch,
                                 string? desc     = null,
                                 string? hotkey   = null,
                                 Action? action   = null,
                                 string  align    = "center",
                                 int     showChar = 1,
                                 int     tipChar  = 0 )
  {
    int x = CalcSymbolX( line, slot );
    int y = CalcSymbolY( line, slot );

    AdvanceSlot( advanceBy );

    PlaceSymbol( line, slot, width, x, y, ch, desc, hotkey, action, align, showChar, tipChar );
  }

  /// <summary>
  /// Create and register a button at an explicit pixel position. This is the
  /// shared core of <see cref="RegisterSymbol"/> (cursor-based) and the
  /// data-driven <see cref="DataTabModel"/>, so both produce identical elements.
  /// </summary>
  protected void PlaceSymbol( int     line,
                              int     slot,
                              int     width,
                              int     x,
                              int     y,
                              string  ch,
                              string? desc,
                              string? hotkey,
                              Action? action,
                              string  align,
                              int     showChar,
                              int     tipChar )
  {
    int w = SymBtnSizeX * width + Layout.ButtonGap * ( width - 1 );
    int h = SymBtnSizeY;

    string hk = hotkey ?? "";
    string d  = desc   ?? ch;

    // Sends are async now; buttons/hotkeys fire-and-forget (the send serialises
    // itself and swallows its own errors), so the click returns immediately.
    Action clickAction = action ?? (() => { _ = TextSender.SendText( TransformSendText( ch ) ); });

    Symbols.Add( new SymbolElement
    {
      Line     = line,
      Slot     = slot,
      Width    = width,
      X        = x,
      Y        = y,
      W        = w,
      H        = h,
      Char     = ch,
      Desc     = d,
      ShowChar = showChar != 0,
      TipChar  = tipChar  != 0,
      Hotkey   = hk,
      Align    = align,
      ClickAction = clickAction
    } );

    HotkeyRegistry.Add( hk, clickAction );

    if( hk.Length > 0 )
    {
      AppState.HotkeyHelp.Add( (HotkeyParser.Label( hk ), d) );
    }
  }

  /// <summary>A laid-out section header: a label (blank = a plain separator line)
  /// spanning the columns at a given Y, taking <see cref="Height"/> vertical space.</summary>
  public sealed class SectionHeader
  {
    public string Name   { get; init; } = "";
    public int    X      { get; init; }
    public int    Y      { get; init; }
    public int    Width  { get; init; }
    public int    Height { get; init; }
  }

  // ── Send-time transform (Comments tab strips emojis when enabled) ─
  public string TransformSendText( string text )
  {
    if( AppState.StripSendEmojis && EnableStripEmojis )
    {
      return AppState.StripEmojis( text );
    }
    return text;
  }
}
