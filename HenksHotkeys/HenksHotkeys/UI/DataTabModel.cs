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
      BuildRows( e.Rows );
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
    BuildRows( m_entry.Rows );
    RecalcSizes();
  }

  private void BuildRows( List<RowDef>? rows )
  {
    if( rows is null )
    {
      return;
    }

    // Width a section header spans: the full grid of columns.
    int headerWidth = m_cols > 0 ? SymBtnSizeX * m_cols + Layout.ButtonGap * ( m_cols - 1 )
                                 : SymBtnSizeX;

    // The offset is accumulated in row-height units; a row consumes one unit,
    // except a section header, which consumes its (pixel) height. prevAdvance
    // carries the previous row's consumed height so the next row lands below it.
    double rowOffset   = 0;
    double prevAdvance = 0;
    for( int r = 0; r < rows.Count; r++ )
    {
      RowDef row = rows[r];

      // Row 0 sits at the origin plus any leading gap; each later row adds the
      // previous row's height plus this row's gap. Round once per row, mirroring
      // the cursor engine's CalcSymbolY.
      rowOffset = r == 0 ? row.GapBefore : rowOffset + prevAdvance + row.GapBefore;
      int y = SymOrgY + (int)Math.Round( rowOffset * RowHeight );

      if( row.IsSection )
      {
        int hPx = row.HeaderHeight > 0 ? (int)Math.Round( row.HeaderHeight ) : Layout.SectionHeaderHeight;
        Headers.Add( new SectionHeader
        {
          Name   = row.Section ?? "",
          X      = SymOrgX,
          Y      = y,
          Width  = headerWidth,
          Height = hPx,
        } );
        prevAdvance = (double)hPx / RowHeight; // header height back to row-units
        continue;
      }

      if( row.Blank )
      {
        prevAdvance = 1.0;
        continue; // blank row: just occupies the vertical space
      }
      prevAdvance = 1.0;

      // Walk the row horizontally; every entry (button or blank) takes one cell,
      // and a button's own gapBefore inserts space ahead of it.
      double col = row.Indent;
      for( int i = 0; i < row.Buttons.Count; i++ )
      {
        ButtonDef b = row.Buttons[i];
        col += b.GapBefore;
        int x = SymOrgX + (int)Math.Round( col * ColWidth );

        if( b.Blank )
        {
          // A blank occupies one cell, draws nothing, and sends nothing — but it is
          // placed so it can be hovered / right-clicked / turned into a real button.
          SymbolElement spacer = PlaceSymbol( r + 1, (int)Math.Round( col ) + 1, 1, x, y,
                                              "", null, null, static () => { },
                                              "center", 0, 0 );
          spacer.Source  = b;
          spacer.IsBlank = true;
        }
        else
        {
          // Secret buttons decrypt their value on demand at send time (it is never kept
          // in memory) and never show it (face = desc, no value in the tooltip).
          Action? action   = b.IsSecret ? () => SecretCommands.Send( b ) : null;
          string  ch       = b.IsSecret ? "" : b.Text;
          int     showText = b.IsSecret ? 0 : ( b.ShowText ? 1 : 0 );
          int     tipText  = b.IsSecret ? 0 : ( b.TipText  ? 1 : 0 );

          SymbolElement sym = PlaceSymbol( r + 1, (int)Math.Round( col ) + 1, b.Width, x, y,
                                           ch, b.Desc, b.Hotkey, action,
                                           b.Align, showText, tipText );
          sym.Source = b; // back-link to the model so the right-click menu can edit it
        }

        col += 1.0; // the cell this entry occupies
      }
    }
  }
}
