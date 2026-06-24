using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

/// <summary>Sensitive info tab — emails / passwords (Sensitive.ahk). Char hidden from tooltip.</summary>
internal sealed class SensitiveTab : TabModel
{
  public SensitiveTab() : base( "Sensitive" )
  {
    FontSize    = 10f;
    SymBtnSizeX = 190;
    SymBtnSizeY = 24;

    SetRowsOf( 3 );
    RegisterButtons();
    RecalcSizes();
  }

  private void RegisterButtons()
  {
    RegisterSymbolX( 1, "henk.vanbruggen@microsurvey.com",             "email - MicroSurvey VHE",  "^+#F9",  null, "left", 0, 0 );
    RegisterSymbolX( 1, "H{!}Pircotha29",                              "pswd - MicroSurvey VHE",   "^+#F10", null, "left", 0, 0 );
    NextLine();

    RegisterSymbolX( 1, "henk.vanbruggen@microsurvey.onmicrosoft.com", "email - OnMicrosoft",      "^+#F11", null, "left", 0, 0 );
    RegisterSymbolX( 1, "HVB13{#}dvlp{@}",                             "pswd - OnMicrosoft",       "^+#F12", null, "left", 0, 0 );
    NextLine();

    RegisterSymbolX( 1, "buildprogrammer.geo@microsurvey.com",         "email - Build Programmer", "^+#F7",  null, "left", 0, 0 );
    RegisterSymbolX( 1, "MS2023bp{!}run",                              "pswd - Build Programmer",  "^+#F8",  null, "left", 0, 0 );
    NextLine();

    RegisterSpace();
    RegisterSymbolX( 1, "MyDogIs1Cut3Puppy",                           "pswd - BitWarden",         "^+#F6",  null, "left", 0, 0 );
  }
}
