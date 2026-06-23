using HenksHotkeys.Core;
using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

/// <summary>Utility tool buttons (Tools.ahk).</summary>
internal sealed class ToolsTab : TabModel
{
  public ToolsTab() : base( "Tools" )
  {
    FontSize    = 10f;
    SymBtnSizeX = 214;
    SymBtnSizeY = 24;

    SetRowsOf( 3 );
    RegisterButtons();
    RecalcSizes();
  }

  private void RegisterButtons()
  {
    RegisterSymbolX( 1, "", "Set favourite spot",              null,  () => AppState.Window?.SetFavouriteSpot(), "left", 0, 1 );
    RegisterSymbolX( 1, "", "Toggle clipboard send mode",      "^!v", AppActions.ToggleClipboardSendMode,        "left", 0, 1 );
    NextLine();

    RegisterSymbolX( 1, "", "Move to favourite spot",          null,  () => AppState.Window?.MoveToFavouriteSpot(), "left", 0, 1 );
    RegisterSymbolX( 1, "", "Toggle strip emojis send mode",   "^!e", AppActions.ToggleStripSendEmojis,          "left", 0, 1 );
    NextLine();
    ShiftLineByThird();

    RegisterSpace();
    RegisterSymbolX( 1, "", "Move window to work area",        "^!w", AppActions.MoveWindowToWorkArea,           "left", 0, 1 );

    NextLine();
    NextLine();
    NextLine();
    RegisterSymbolX( 1, "", "Test Function",                   null,  AppActions.TestFunction,                   "left", 0, 1 );
  }
}
