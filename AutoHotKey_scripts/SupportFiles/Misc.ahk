; Misc.ahk
; A collection of miscellaneous hotkeys that don't fit into other categories.

; Alt + Shift + 1 => AppData folder path
RegisterAction( "Alt+Shift+1", "%appdata%", AppData )
!+1::AppData()
AppData()
{
  SendInput( "%appdata%" )
}

; Alt + Shift + 2 => LocalAppData folder path
RegisterAction( "Alt+Shift+2", "%localappdata%", LocalAppData )
!+2::LocalAppData()
LocalAppData()
{
  SendInput( "%localappdata%" )
}



;^+8::Unused()
;Unused()
;{
;  SendInput( "<unused>" )
;}

;^+9::Unused()
;Unused()
;{
;  SendInput( "<unused>" )
;}

; Ctrl + Shift + 0 => NightCafe Creator URL
RegisterAction( "Ctrl+Shift+0", "NightCafe Creator URL", NCCreationPrefix )
^+0::NCCreationPrefix()
NCCreationPrefix()
{
  SendInput( "https://creator.nightcafe.studio/creation/" )
}

RegisterAction( "Ctrl+Alt+M", "Maximize window to work area (manual resize)", MaximizeWindowToWorkArea )
^!m::
{
	MaximizeWindowToWorkArea()
}
MaximizeWindowToWorkArea()
{
  hwnd := WinActive( "A" )
  if( hwnd == 0 )
  {
    return
  }

  WinGetPos( &wx, &wy, &ww, &wh, hwnd )
  monitorCount := MonitorGetCount()
  monitorIdx   := 0

  ; Find the monitor containing the window's center
  winCenterX := (wx + ww) // 2
  winCenterY := (wy + wh) // 2
  Loop monitorCount
  {
    MonitorGet( A_Index, &L, &T, &R, &B )
    if( (winCenterX >= L) &&
        (winCenterX <  R) &&
        (winCenterY >= T) &&
        (winCenterY <  B) )
    {
      monitorIdx := A_Index
      break
    }
  }
  if( monitorIdx == 0 )
  {
    monitorIdx := 1 ; fallback to primary
  }

  MonitorGetWorkArea( monitorIdx, &mx, &my, &mr, &mb )
  mw := mr - mx
  mh := mb - my

  ; Try to get extended frame bounds (includes shadow/border for DWM windows)
  rect := Buffer( 16, 0 )
  hasDwm := DllCall( "GetModuleHandle", "str", "dwmapi", "ptr" ) != 0
  isStandardBorders := false
  if( hasDwm )
  {
    result := DllCall( "dwmapi\DwmGetWindowAttribute",
                       "ptr", hwnd, "uint", 9,
                       "ptr", rect, "uint", 16 )
    if( result == 0 )
    {
      left    := NumGet( rect, 0,  "int" )
      top     := NumGet( rect, 4,  "int" )
      right   := NumGet( rect, 8,  "int" )
      bottom  := NumGet( rect, 12, "int" )
      borderL := left - wx
      borderT := top  - wy
      borderR := (wx + ww) - right
      borderB := (wy + wh) - bottom
      ; If borders are small and symmetric, likely standard
      if( (Abs( borderL - borderR ) <=  2) &&
          (Abs( borderT - borderB ) <=  2) &&
          (Abs( borderL )           <=  8) &&
          (Abs( borderT )           <= 32) )
      {
        isStandardBorders := true
      }
      ; Adjust for borders
      mx -= borderL
      my -= borderT
      mw += (borderL + borderR)
      mh += (borderT + borderB)
    }
  }

  if WinExist( "ahk_id " hwnd )
  {
    WinMove( mx, my, mw, mh )
  }
  ; Optionally, you can use isStandardBorders for further logic or debugging
}
