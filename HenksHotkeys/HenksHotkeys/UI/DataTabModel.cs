using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// A <see cref="TabModel"/> built from data (<see cref="TabEntry"/> / tabs.json)
/// rather than hand-written C#. Lays buttons out row-primary, computing each
/// button's pixel position directly so the result is identical to the equivalent
/// code-built tab.
/// </summary>
internal sealed class DataTabModel : TabModel
{
  private readonly TabEntry m_entry;
  private int m_cols;

  /// <summary>The underlying tab model (same instance held by <see cref="TabStore"/>),
  /// so the editor can append a button to an otherwise-empty tab.</summary>
  public TabEntry Entry => m_entry;

  public DataTabModel( TabEntry e ) : base( e.Name ?? "Tab" )
  {
    m_entry           = e;
    FontSize          = (float)e.FontSize;
    FontName          = e.FontName;
    SymBtnSizeX       = e.ButtonWidth;
    SymBtnSizeY       = e.Square ? e.ButtonWidth : e.ButtonHeight; // square: height = width
    UseEmojiImages    = e.EmojiImages;
    EnableStripEmojis = e.StripEmojis;
    m_cols            = e.Columns;

    if( e.Proportional )
    {
      // Defer the layout until the locked window width is known: the cells expand
      // to fill it (see ApplyProportionalLayout). Until then expose the natural
      // minimum width so the window can still be sized from the widest tab.
      MinContentWidth = 2 * Layout.TabEdgeGap + m_cols * e.ButtonWidth
                      + Math.Max( 0, m_cols - 1 ) * Layout.ButtonGap;
    }
    else
    {
      SetRowsOf( m_cols );
      BuildGrid();
      RecalcSizes();
    }
  }

  /// <summary>A proportional tab's natural minimum width (its width at the natural
  /// <c>buttonWidth</c>); 0 for a fixed tab, which sizes from its laid-out content.</summary>
  public int MinContentWidth { get; }

  public override int SizingWidth => Math.Max( ContentWidth, MinContentWidth );

  /// <summary>For a proportional tab, expand the cells to fill <paramref name="contentWidth"/>
  /// and lay the buttons out (done once, after the window width is known). The cell
  /// never shrinks below the natural <c>buttonWidth</c>. No-op for a fixed tab.</summary>
  public void ApplyProportionalLayout( int contentWidth )
  {
    if( !m_entry.Proportional )
    {
      return;
    }
    int gaps = 2 * Layout.TabEdgeGap + Math.Max( 0, m_cols - 1 ) * Layout.ButtonGap;
    int cell = m_cols > 0 ? Math.Max( m_entry.ButtonWidth, ( contentWidth - gaps ) / m_cols )
                          : m_entry.ButtonWidth;

    SymBtnSizeX = cell;
    if( m_entry.Square )
    {
      SymBtnSizeY = cell; // square: the height grows with the (now wider) cell
    }

    SetRowsOf( m_cols );
    BuildGrid();
    RecalcSizes();
  }

  /// <summary>Lay the buttons and section headers out from their explicit grid
  /// coordinates. Empty cells/rows aren't materialised — they're simply the gaps in the
  /// coordinates (so a skipped row index is a blank row, a skipped column a blank cell).
  /// Section rows are variable-height, so each row's Y is the running sum of the heights
  /// of the rows above it.</summary>
  private void BuildGrid()
  {
    List<ButtonDef>  buttons  = m_entry.Buttons  ?? new();
    List<SectionDef> sections = m_entry.Sections ?? new();
    if( buttons.Count == 0 && sections.Count == 0 )
    {
      return;
    }

    // How tall the grid is, and which rows are section headers.
    int maxRow = 0;
    foreach( ButtonDef b in buttons )  maxRow = Math.Max( maxRow, b.Row );
    foreach( SectionDef s in sections ) maxRow = Math.Max( maxRow, s.Row );

    var sectionAt = new Dictionary<int, SectionDef>();
    foreach( SectionDef s in sections ) sectionAt[s.Row] = s; // later wins on a clash

    int HeaderPx( SectionDef s ) => s.Height > 0 ? (int)Math.Round( s.Height ) : Layout.SectionHeaderHeight;

    // Top Y of each row = sum of the heights of the rows above it (section rows take
    // their own height; every other row — content or empty — takes one RowHeight).
    var rowY = new int[maxRow + 1];
    int y = SymOrgY;
    for( int r = 0; r <= maxRow; r++ )
    {
      rowY[r] = y;
      y += sectionAt.TryGetValue( r, out SectionDef? sec ) ? HeaderPx( sec ) : RowHeight;
    }

    // Width a section header spans: the full grid of columns.
    int headerWidth = m_cols > 0 ? SymBtnSizeX * m_cols + Layout.ButtonGap * ( m_cols - 1 )
                                 : SymBtnSizeX;

    foreach( SectionDef s in sections )
    {
      Headers.Add( new SectionHeader
      {
        Name   = s.Name,
        X      = SymOrgX,
        Y      = rowY[s.Row],
        Width  = headerWidth,
        Height = HeaderPx( s ),
      } );
    }

    foreach( ButtonDef b in buttons )
    {
      int x = SymOrgX + b.Col * ColWidth;

      // Secret buttons decrypt their value on demand at send time (it is never kept in
      // memory) and never show it (face = desc, no value in the tooltip).
      Action? action   = b.IsSecret ? () => SecretCommands.Send( b ) : null;
      string  ch       = b.IsSecret ? "" : b.Text;
      int     showText = b.IsSecret ? 0 : ( b.ShowText ? 1 : 0 );
      int     tipText  = b.IsSecret ? 0 : ( b.TipText  ? 1 : 0 );

      SymbolElement sym = PlaceSymbol( b.Row + 1, b.Col + 1, b.Width, x, rowY[b.Row],
                                       ch, b.Desc, b.Hotkey, action,
                                       b.Align, showText, tipText );
      sym.Source = b; // back-link to the model so the right-click menu can edit it
    }
  }
}
