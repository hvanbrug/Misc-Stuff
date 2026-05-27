; Tools.ahk
; A collection of utility tool buttons.




class ToolsTabPage extends TabPage
{
  __New()
  {
    super.__New( "Tools" )

    super.m_fontSize    := "s10"
    super.m_symBtnSizeX := 214
    super.m_symBtnSizeY := 24

    super.SetRowsOf( 3 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbolX( 1, "", "Move window to work area", "^!m", (*) => MoveWindowToWorkArea(), "left", 0, 1 )
  }

}