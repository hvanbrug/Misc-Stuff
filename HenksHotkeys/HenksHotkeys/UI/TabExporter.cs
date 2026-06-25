using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Reconstructs the declarative <see cref="TabEntry"/> (rows of buttons) from a
/// built <see cref="TabModel"/>. Used once to generate tabs.json from the original
/// code tabs; the round-trip (export → <see cref="DataTabModel"/>) is verified to
/// reproduce the exact same layout. Assumes row-primary tabs (all the text tabs).
/// </summary>
internal static class TabExporter
{
  public static TabEntry Export( TabModel m )
  {
    var e = new TabEntry
    {
      Name         = m.Name,
      Columns      = m.MaxSlots,
      FontSize     = m.FontSize,
      FontName     = m.FontName,
      ButtonWidth  = m.SymBtnSizeX,
      ButtonHeight = m.SymBtnSizeY,
      Gap          = m.SymBtnGap,
      OriginX      = m.SymOrgX,
      OriginY      = m.SymOrgY,
      EmojiImages  = m.UseEmojiImages,
      StripEmojis  = m.EnableStripEmojis,
      Rows         = new List<RowDef>(),
    };

    var lines = m.Symbols.GroupBy( s => s.Line ).OrderBy( g => g.Key ).ToList();
    int prevY = m.SymOrgY;

    for( int idx = 0; idx < lines.Count; idx++ )
    {
      List<SymbolElement> btns = lines[idx].OrderBy( s => s.Slot ).ToList();
      int rowY   = btns[0].Y;
      int indent = btns[0].Slot - 1;

      double gapBefore = idx == 0
                           ? (double)( rowY - m.SymOrgY ) / m.RowHeight
                           : (double)( rowY - prevY ) / m.RowHeight - 1.0;
      prevY = rowY;

      // Snap to twelfths so the half/third shifts (and whole blank rows) used by
      // the original tabs come back as clean numbers (0.5, 0.3333, 1, …) instead
      // of pixel-rounding noise. DataTabModel re-accumulates these exactly.
      gapBefore = Math.Round( Math.Round( gapBefore * 12.0 ) / 12.0, 4 );
      if( Math.Abs( gapBefore ) < 0.0005 )
      {
        gapBefore = 0;
      }

      var row = new RowDef { GapBefore = gapBefore, Indent = indent };
      foreach( SymbolElement s in btns )
      {
        row.Buttons.Add( new ButtonDef
        {
          Text     = s.Char,
          Desc     = s.Desc == s.Char ? null : s.Desc,
          Hotkey   = s.Hotkey.Length > 0 ? s.Hotkey : null,
          Width    = s.Width,
          Align    = s.Align,
          ShowText = s.ShowChar,
          TipText  = s.TipChar,
        } );
      }
      e.Rows.Add( row );
    }

    return e;
  }
}
