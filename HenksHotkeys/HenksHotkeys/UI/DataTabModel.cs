using System.Windows;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>How a drag/add resolves against the grid, for both the drop action and the
/// on-screen indicator.</summary>
internal enum DropKind { PlaceEmpty, InsertBefore, InsertAfter }

/// <summary>The resolved grid target for a pointer position: the cell to act on
/// (<see cref="Row"/>/<see cref="Col"/>), what it means, and the pixel geometry the
/// caret / cell highlight is drawn from.</summary>
internal readonly record struct DropSpot(
  int Row, int Col, DropKind Kind, Core.ButtonDef? Target,
  int CellX, int CellY, int CellW, int CellH, int CaretX );

/// <summary>
/// A <see cref="TabModel"/> built from data (<see cref="TabEntry"/> / tabs.json)
/// rather than hand-written C#. Lays buttons out from their explicit grid coordinates,
/// computing each button's pixel position directly so the result is identical to the
/// equivalent code-built tab.
/// </summary>
internal sealed class DataTabModel : TabModel
{
  private readonly TabEntry m_entry;
  private int m_cols;

  // Row-top Y for rows 0..m_maxRow, plus the bottom at [m_maxRow+1]. Set by BuildGrid;
  // drives hit-testing a pointer back to a grid cell.
  private int[] m_rowTops = Array.Empty<int>();
  private int   m_maxRow  = -1;

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

    // Explicit height wins; otherwise a partial-width heading (span > 0) is a button-row
    // tall, while a classic full-width divider (span 0) uses the section-header height.
    int HeaderPx( SectionDef s ) => s.Height > 0 ? (int)Math.Round( s.Height )
                                  : s.Span   > 0 ? RowHeight
                                  :                Layout.SectionHeaderHeight;

    // Top Y of each row = sum of the heights of the rows above it (section rows take
    // their own height; every other row — content or empty — takes one RowHeight). The
    // extra slot [maxRow+1] holds the bottom, so a drop below everything finds a new row.
    m_maxRow  = maxRow;
    m_rowTops = new int[maxRow + 2];
    int[] rowY = m_rowTops;
    int y = SymOrgY;
    for( int r = 0; r <= maxRow; r++ )
    {
      rowY[r] = y;
      y += sectionAt.TryGetValue( r, out SectionDef? sec ) ? HeaderPx( sec ) : RowHeight;
    }
    rowY[maxRow + 1] = y;

    // Width of a full-width (span 0) header: the whole grid of columns.
    int fullWidth = m_cols > 0 ? SymBtnSizeX * m_cols + Layout.ButtonGap * ( m_cols - 1 )
                               : SymBtnSizeX;

    foreach( SectionDef s in sections )
    {
      // A spanning heading starts at its column and covers `span` cells; span 0 = full width.
      int span  = s.Span > 0 ? s.Span : 0;
      int width = span > 0 ? SymBtnSizeX * span + Layout.ButtonGap * ( span - 1 ) : fullWidth;

      Headers.Add( new SectionHeader
      {
        Name   = s.Name,
        X      = SymOrgX + Math.Max( 0, s.Col ) * ColWidth,
        Y      = rowY[s.Row],
        Width  = width,
        Height = HeaderPx( s ),
        Align  = s.Align,
        Source = s,
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

      // Sub-cell buttons split their cell horizontally: the button becomes 1/n of the cell
      // width and sits at its slot index (full cell height is kept).
      if( b.SubCells > 1 )
      {
        int n     = b.SubCells;
        int i     = Math.Clamp( b.SubCell, 0, n - 1 );
        int span  = Math.Max( 1, b.Width );
        int cellW = SymBtnSizeX * span + Layout.ButtonGap * ( span - 1 );
        int subW  = Math.Max( 1, ( cellW - Layout.ButtonGap * ( n - 1 ) ) / n );
        sym.X = x + i * ( subW + Layout.ButtonGap );
        sym.W = subW;
      }
    }
  }

  // ── Hit-testing (pointer → grid cell) ────────────────────────────
  /// <summary>The highest occupied/sectioned row index (−1 when the tab is empty).</summary>
  public int MaxRow => m_maxRow;

  /// <summary>The grid row whose vertical band contains <paramref name="y"/>. Below the
  /// last laid-out row it keeps counting in RowHeight steps, so dropping two (or more) rows
  /// down lands two (or more) rows past the content — leaving the rows between empty.</summary>
  public int RowAt( double y )
  {
    if( m_maxRow < 0 )
    {
      return Math.Max( 0, (int)( ( y - SymOrgY ) / RowHeight ) ); // empty tab: pure grid
    }
    for( int r = 0; r <= m_maxRow; r++ )
    {
      if( y < m_rowTops[r + 1] ) return r;
    }
    int bottom = m_rowTops[m_maxRow + 1];
    return m_maxRow + 1 + Math.Max( 0, (int)( ( y - bottom ) / RowHeight ) );
  }

  /// <summary>The grid column the pointer is physically over (the cell <paramref name="x"/>
  /// falls in — floored, not rounded — so the whole width of an empty cell reads as that
  /// cell rather than snapping onto a neighbour).</summary>
  public int ColAt( double x ) => Math.Max( 0, (int)Math.Floor( ( x - SymOrgX ) / (double)ColWidth ) );

  /// <summary>Top Y of a row, extrapolated past the last laid-out row with RowHeight.</summary>
  public int RowTop( int row )
  {
    if( m_maxRow < 0 )    return SymOrgY + Math.Max( 0, row ) * RowHeight;
    if( row <= m_maxRow ) return m_rowTops[row];
    return m_rowTops[m_maxRow + 1] + ( row - ( m_maxRow + 1 ) ) * RowHeight;
  }

  /// <summary>The placed button covering cell (<paramref name="row"/>, <paramref name="col"/>)
  /// — accounting for multi-column width — excluding <paramref name="exclude"/>. Null if the
  /// cell is empty.</summary>
  public SymbolElement? SymbolAt( int row, int col, Core.ButtonDef? exclude )
  {
    foreach( SymbolElement s in Symbols )
    {
      if( s.Source is null || ReferenceEquals( s.Source, exclude ) || s.Line - 1 != row )
      {
        continue;
      }
      int c0 = s.Slot - 1;
      if( (col >= c0) &&
          (col <= (c0 + Math.Max( 1, s.Width )) - 1) )
      {
        return s;
      }
    }
    return null;
  }

  /// <summary>Resolve a pointer position to a grid action + the geometry to indicate it.
  /// Dropping on an empty cell (including one on a fresh row below the content) places there;
  /// dropping on a button inserts before/after it (by which half). A sub-cell button always
  /// targets the whole cell it's over, so it can join another cell's free sub-slot rather than
  /// insert-shifting the row. <paramref name="dragging"/> (if any) is ignored when probing, so
  /// a button never collides with itself.</summary>
  public DropSpot ResolveDrop( Point p, Core.ButtonDef? dragging )
  {
    int row = RowAt( p.Y );
    int col = ColAt( p.X );
    if( m_cols > 0 ) col = Math.Min( col, m_cols - 1 ); // never past the last column
    int h   = SymBtnSizeY;
    int w   = SymBtnSizeX;

    int top = RowTop( row );
    SymbolElement? hit = dragging is { SubCells: > 1 } ? null : SymbolAt( row, col, dragging );
    if( hit?.Source is not Core.ButtonDef tb )
    {
      return new DropSpot( row, col, DropKind.PlaceEmpty, null,
                           SymOrgX + col * ColWidth, top, w, h, 0 );
    }

    bool after  = p.X >= hit.X + hit.W * 0.5;
    int  insCol = after ? tb.Col + Math.Max( 1, tb.Width ) : tb.Col;
    int  caretX = SymOrgX + insCol * ColWidth - ( Layout.ButtonGap + 2 ) / 2;
    return new DropSpot( row, insCol, after ? DropKind.InsertAfter : DropKind.InsertBefore, tb,
                         SymOrgX + insCol * ColWidth, top, w, h, caretX );
  }
}
