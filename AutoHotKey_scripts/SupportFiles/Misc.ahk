; Misc.ahk
; A collection of miscellaneous hotkeys that don't fit into other categories.

; Tools.ahk
; A collection of utility tool buttons.




class MiscTabPage extends TabPage
{
  __New()
  {
    super.__New( "Misc" )

    super.m_fontSize    := "s10"
    super.m_symBtnSizeX := 214
    super.m_symBtnSizeY := 24

    super.SetRowsOf( 3 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbolX( 1, "%appdata%",      unset, "!+1", unset, "left", 0, 1 )
    super.RegisterSymbolX( 1, "%localappdata%", unset, "!+2", unset, "left", 0, 1 )
  }

}
