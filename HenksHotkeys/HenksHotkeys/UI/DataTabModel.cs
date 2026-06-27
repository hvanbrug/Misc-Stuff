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
  public DataTabModel( TabEntry e ) : base( e.Name ?? "Tab" )
  {
    FontSize          = (float)e.FontSize;
    FontName          = e.FontName;
    SymBtnSizeX       = e.ButtonWidth;
    SymBtnSizeY       = e.ButtonHeight;
    UseEmojiImages    = e.EmojiImages;
    EnableStripEmojis = e.StripEmojis;

    m_cols = e.Columns;
    SetRowsOf( e.Columns );
    BuildRows( e.Rows );
    RecalcSizes();
  }

  private int m_cols;

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

        if( !b.Blank )
        {
          int x = SymOrgX + (int)Math.Round( col * ColWidth );

          // Secret buttons send the decrypted value but never show it (face = desc,
          // no value in the tooltip), regardless of the show/tip flags.
          string sendText = b.IsSecret ? ( b.Plain ?? "" ) : b.Text;
          int    showText = b.IsSecret ? 0 : ( b.ShowText ? 1 : 0 );
          int    tipText  = b.IsSecret ? 0 : ( b.TipText  ? 1 : 0 );

          PlaceSymbol( r + 1, (int)Math.Round( col ) + 1, b.Width, x, y,
                       sendText, b.Desc, b.Hotkey, null,
                       b.Align, showText, tipText );
        }

        col += 1.0; // the cell this entry occupies
      }
    }
  }
}
