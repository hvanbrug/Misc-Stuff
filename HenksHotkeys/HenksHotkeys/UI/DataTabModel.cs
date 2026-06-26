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
    SymBtnGap         = e.Gap;
    SymOrgX           = e.OriginX;
    SymOrgY           = e.OriginY;
    UseEmojiImages    = e.EmojiImages;
    EnableStripEmojis = e.StripEmojis;

    SetRowsOf( e.Columns );
    BuildRows( e.Rows );
    RecalcSizes();
  }

  private void BuildRows( List<RowDef>? rows )
  {
    if( rows is null )
    {
      return;
    }

    double rowOffset = 0;
    for( int r = 0; r < rows.Count; r++ )
    {
      RowDef row = rows[r];

      // Accumulate the row offset (in row-height units) and round once per row,
      // exactly mirroring the cursor engine's CalcSymbolY. Row 0 sits at the
      // origin plus any leading gap; each later row adds one row plus its gap.
      rowOffset = r == 0 ? row.GapBefore : rowOffset + 1.0 + row.GapBefore;
      int y = SymOrgY + (int)Math.Round( rowOffset * RowHeight );

      for( int i = 0; i < row.Buttons.Count; i++ )
      {
        ButtonDef b    = row.Buttons[i];
        int       slot = row.Indent + i + 1;
        int       x    = SymOrgX + ( row.Indent + i ) * ColWidth;

        // Secret buttons send the decrypted value but never show it (face = desc,
        // no value in the tooltip), regardless of the show/tip flags.
        string sendText = b.IsSecret ? ( b.Plain ?? "" ) : b.Text;
        int    showText = b.IsSecret ? 0 : ( b.ShowText ? 1 : 0 );
        int    tipText  = b.IsSecret ? 0 : ( b.TipText  ? 1 : 0 );

        PlaceSymbol( r + 1, slot, b.Width, x, y,
                     sendText, b.Desc, b.Hotkey, null,
                     b.Align, showText, tipText );
      }
    }
  }
}
