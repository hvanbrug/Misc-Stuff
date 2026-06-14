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
                           (*) => g_hotkeyWnd.SetFavouriteSpot(), "left", 0, 1 )
    super.RegisterSymbolX( 1, "", "Toggle clipboard send mode", "^!v",
                           (*) => ToggleClipboardSendMode(), "left", 0, 1 )
    super.NextLine()

    super.RegisterSymbolX( 1, "", "Move to favourite spot", unset,
                           (*) => g_hotkeyWnd.MoveToFavouriteSpot(), "left", 0, 1 )
    super.RegisterSymbolX( 1, "", "Toggle strip emojis send mode", "^!e",
                           (*) => ToggleStripSendEmojis(), "left", 0, 1 )
    super.NextLine()
    super.ShiftLineByThird()

    super.RegisterSpace()
    super.RegisterSymbolX( 1, "", "Move window to work area", "^!w",
                           (*) => MoveWindowToWorkArea(), "left", 0, 1 )

    super.NextLine()
    super.NextLine()
    super.NextLine()
    super.RegisterSymbolX( 1, "", "Test Function", unset,
                           (*) => TestFunction(), "left", 0, 1 )
  }

}

TestFunction()
{
  global g_hotkeyWnd
  ;WinGetPos( &x1, &y1, &w1, &h1, g_hotkeyWnd.Hwnd )

  rc := Buffer( 16, 0 )

  DllCall( "GetWindowRect", "Ptr", g_hotkeyWnd.Hwnd, "Ptr", rc )

  leftWindow   := NumGet( rc,  0, "Int" )
  topWindow    := NumGet( rc,  4, "Int" )
  rightWindow  := NumGet( rc,  8, "Int" )
  bottomWindow := NumGet( rc, 12, "Int" )

  windowW := rightWindow - leftWindow
  windowH := bottomWindow - topWindow


  DllCall( "GetClientRect", "Ptr", g_hotkeyWnd.Hwnd, "Ptr", rc )

  leftClient   := NumGet( rc,  0, "Int" )
  topClient    := NumGet( rc,  4, "Int" )
  rightClient  := NumGet( rc,  8, "Int" )
  bottomClient := NumGet( rc, 12, "Int" )

  clientW := rightClient - leftClient
  clientH := bottomClient - topClient

  MsgBox( "Left: "   SysGet(32) "`n"
          "Top: "    SysGet(33) "`n"
          "Right: "  SysGet(34) "`n"
          "Bottom: " SysGet(35) "`n"
          "92: "     SysGet(92) "`n"
    "Window W: " windowW " - H: " windowH "`n"
    "Client W: " clientW " - H: " clientH "`n"
    "Border each side: " (windowW - clientW) / 2 " - " (windowH - clientH) / 2 "`n"
  )
}
