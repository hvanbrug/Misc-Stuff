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
    super.RegisterSymbolX( 1, "", "Set favourite spot", unset,
                           (*) => SetFavouriteSpot(), "left", 0, 1 )
    super.RegisterSymbolX( 1, "", "Toggle clipboard send mode", "^!v",
                           (*) => ToggleClipboardSendMode(), "left", 0, 1 )
    super.NextLine()

    super.RegisterSymbolX( 1, "", "Move to favourite spot", unset,
                           (*) => MoveToFavouriteSpot(), "left", 0, 1 )
    super.RegisterSymbolX( 1, "", "Toggle strip emojis send mode", "^!e",
                           (*) => ToggleStripSendEmojis(), "left", 0, 1 )
    super.NextLine()
    super.ShiftLineByThird()

    super.RegisterSpace()
    super.RegisterSymbolX( 1, "", "Move window to work area", "^!w",
                           (*) => MoveWindowToWorkArea(), "left", 0, 1 )
  }

}