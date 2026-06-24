using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

/// <summary>Miscellaneous hotkeys (Misc.ahk).</summary>
internal sealed class MiscTab : TabModel
{
  public MiscTab() : base( "Misc" )
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
    RegisterSymbolX( 1, "%appdata%",      null, "!+1", null, "left", 0, 1 );
    RegisterSymbolX( 1, "%localappdata%", null, "!+2", null, "left", 0, 1 );
  }
}
