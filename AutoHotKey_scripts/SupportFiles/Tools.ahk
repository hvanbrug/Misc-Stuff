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
    super.RegisterSymbolX( 1, "", "Move window to work area", unset, (*) => this.MoveWindowToWorkArea(), "left", 0, 1 )
  }

  MoveWindowToWorkArea()
  {
    global g_activeWindow
    if( !IsSet( g_activeWindow ) )
    {
      return
    }

    WinRestore( g_activeWindow )

    ; Get the invisible DWM frame borders by comparing GetWindowRect
    ; (includes invisible borders) with DwmGetWindowAttribute's
    ; DWMWA_EXTENDED_FRAME_BOUNDS (visible frame only).
    winRect   := Buffer( 16, 0 )
    frameRect := Buffer( 16, 0 )

    DllCall( "GetWindowRect", "Ptr", g_activeWindow, "Ptr", winRect )
    DllCall( "dwmapi\DwmGetWindowAttribute",
             "Ptr",  g_activeWindow,
             "UInt", 9,
             "Ptr",  frameRect,
             "UInt", 16 )

    borderL := NumGet( frameRect,  0, "Int" ) - NumGet( winRect,    0, "Int" )
    borderT := NumGet( frameRect,  4, "Int" ) - NumGet( winRect,    4, "Int" ) + 1
    borderR := NumGet( winRect,    8, "Int" ) - NumGet( frameRect,  8, "Int" )
    borderB := NumGet( winRect,   12, "Int" ) - NumGet( frameRect, 12, "Int" )

    MonitorGetWorkArea( , &areaL, &areaT, &areaR, &areaB )
    x := areaL - borderL
    y := areaT - borderT
    w := (areaR - areaL) + borderL + borderR
    h := (areaB - areaT) + borderT + borderB

    ; Try non-elevated first via SetWindowPos.
    static SWP_NOZORDER   := 0x0004
    static SWP_NOACTIVATE := 0x0010
    DllCall( "SetWindowPos",
             "Ptr",  g_activeWindow,
             "Ptr",  0,
             "Int",  x,
             "Int",  y,
             "Int",  w,
             "Int",  h,
             "UInt", SWP_NOZORDER | SWP_NOACTIVATE )

    ; Check if it actually moved by re-reading the window position.
    DllCall( "GetWindowRect", "Ptr", g_activeWindow, "Ptr", winRect )
    actualX := NumGet( winRect, 0, "Int" )
    actualY := NumGet( winRect, 4, "Int" )
    if( actualX = x && actualY = y )
    {
      return
    }

    ; Non-elevated move failed (UIPI blocked it). Spawn an elevated helper.
    this.MoveWindowElevated( g_activeWindow, x, y, w, h )
  }

  MoveWindowElevated( hwnd, x, y, w, h )
  {
    script := Format(
      '#Requires AutoHotkey v2.0`n'
      'DllCall("SetWindowPos", "Ptr", {1}, "Ptr", 0, "Int", {2}, "Int", {3}, "Int", {4}, "Int", {5}, "UInt", 0x0014)`n'
      'ExitApp()',
      hwnd, x, y, w, h )

    tmpFile := A_Temp "\MoveWindowHelper.ahk"
    try FileDelete( tmpFile )
    FileAppend( script, tmpFile )
    Run( '*RunAs "' A_AhkPath '" "' tmpFile '"' )
  }
}