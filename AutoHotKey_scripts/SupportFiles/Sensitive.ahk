; Sensitive.ahk
; A collection of hotkeys for sensitive information like passwords and email addresses.

; Ctrl + Shift + Wnd + F?? => secret passwords and email addresses

class SensitiveTabPage extends TabPage
{
  __New()
  {
    super.__New( "Sensitive" )

    super.m_fontSize    := "s10"
    super.m_symBtnSizeX := 190
    super.m_symBtnSizeY := 24

    super.SetRowsOf( 3 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSpace()
    super.RegisterSymbolX( 1, "MyDogIs1Cut3Puppy",                           "BitWarden - pswd",         "^+#F6",  unset, "left", 0 )
    super.NextLine()

    super.RegisterSymbolX( 1, "buildprogrammer.geo@microsurvey.com",         "Build Programmer - email", "^+#F7",  unset, "left", 0 )
    super.RegisterSymbolX( 1, "MS2023bp{!}run",                              "Build Programmer - pswd",  "^+#F8",  unset, "left", 0 )
    super.NextLine()

    super.RegisterSymbolX( 1, "henk.vanbruggen@microsurvey.com",             "MicroSurvey VHE - email",  "^+#F9",  unset, "left", 0 )
    super.RegisterSymbolX( 1, "H{!}Pircotha29",                              "MicroSurvey VHE - pswd",   "^+#F10", unset, "left", 0 )
    super.NextLine()

    super.RegisterSymbolX( 1, "henk.vanbruggen@microsurvey.onmicrosoft.com", "OnMicrosoft - email",      "^+#F11", unset, "left", 0 )
    super.RegisterSymbolX( 1, "HVB13{#}dvlp{@}",                             "OnMicrosoft - pswd",       "^+#F12", unset, "left", 0 )
  }
}
