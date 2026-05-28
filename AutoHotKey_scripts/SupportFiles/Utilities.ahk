; Utilities.ahk
; A collection of general utility functions that can be used in various contexts.

; ── Global registries for the Help Menu UI ──
g_HelpActions := []
g_symbols     := []

; Global map for tab names
if( !IsSet( g_tabNames ) )
{
  global g_tabNames := Map()
}

RegisterAction( hotkey, desc, action )
{
  global g_HelpActions
  g_HelpActions.Push( { hotkey: hotkey, desc: desc, action: action } )
}

DoSendText( msg )
{
  global g_useClipSend
  if( g_useClipSend )
  {
    DoSendViaClipboard( msg )
    return
  }
  if( IsSet( g_activeWindow ) )
  {
    WinActivate( g_activeWindow )
    Sleep( 100 )
    Send( msg )
  }
  else
  {
    SendInput( msg )
  }
}

DoSendViaClipboard( rawText )
{
  backup := ClipboardAll()
  A_Clipboard := rawText
  if( IsSet( g_activeWindow ) )
  {
    WinActivate( g_activeWindow )
    Sleep( 100 )
  }
  Send( "^v" )
  Sleep( 150 )
  A_Clipboard := backup
}

ToggleClipboardSendMode()
{
  global g_useClipSend
  global g_tipMap
  g_useClipSend := !g_useClipSend
  state := g_useClipSend ? "ON" : "OFF"
  SetShowClipBulletState( g_useClipSend )
  ToolTip( "Clipboard send mode: " state )
  SetTimer( (*) => ToolTip(), -2000 )
}

SetShowShrinkBtnState( enabled )
{
  global g_shrinkBtn
  global g_expandBtn

  OutputDebug( "Setting shrink/expand button state: " enabled ? "SHRINK" : "EXPAND" )
  if( !IsObject( g_shrinkBtn ) ||
      !IsObject( g_expandBtn ) )
  {
    OutputDebug( "Shrink/expand buttons not initialized yet." )
    return
  }

  if( enabled )
  {
    g_shrinkBtn.Opt( "-Hidden" )
    g_expandBtn.Opt( "Hidden"  )
    OutputDebug( "Showing SHRINK button, hiding EXPAND button." )
  }
  else
  {
    g_shrinkBtn.Opt( "Hidden"  )
    g_expandBtn.Opt( "-Hidden" )
    OutputDebug( "Hiding SHRINK button, showing EXPAND button." )
  }
}

SetShowClipBulletState( enabled )
{
  global g_clipIndicator
  global g_tipMap

  OutputDebug( "Setting clip bullet state: " (enabled ? "ON" : "OFF" ) )
  if( !IsObject( g_clipIndicator ) )
  {
    OutputDebug( "Clip indicator control not initialized yet." )
    return
  }
  g_clipIndicator.Text := enabled ? "●" : "○"
  g_tipMap[g_clipIndicator.Hwnd]  := "Clipboard send mode: " (enabled ? "ON" : "OFF")
  OutputDebug( "Updated clip bullet state: " (enabled ? "ON" : "OFF") )
}

IsClipControl( hwnd )
{
  global g_clipIndicator
  return IsObject( g_clipIndicator ) && (hwnd = g_clipIndicator.Hwnd)
}

GetSelectedTextThroughClipboard()
{
  backup := ClipboardAll()
  A_Clipboard := ''
  Send( '^c' )
  CLIP_WAIT_TIMEOUT_SEC := 1
  CLIP_WAIT_ANY_DATA    := 1
  if( !ClipWait( CLIP_WAIT_TIMEOUT_SEC, CLIP_WAIT_ANY_DATA ) )
  {
    return (A_Clipboard := backup)
  }
  txt := A_Clipboard
  A_Clipboard := backup
  return txt
}

CreateButton( text, tip,
              fontName, fontSize,
              x, y, w, h,
              func )
{
  global g_gui
  global g_tipMap
  global g_fontSize
  global g_fontName

  g_gui.SetFont( fontSize, fontName )
  btn := g_gui.AddButton( "x" x " y" y " w" w " h" h, text )
  btn.OnEvent( "Click", func )
  g_tipMap[btn.Hwnd] := tip
  g_gui.SetFont( g_fontSize " norm", g_fontName )

  return btn
}

CreateBtnWithStyle( text, tip,
                    fontName, fontSize,
                    styleMask, styleBits,
                    x, y, w, h,
                    func )
{
  btn := CreateButton( text, tip,
                       fontName, fontSize,
                       x, y, w, h,
                       func )
  style := DllCall( "GetWindowLong", "Ptr", btn.Hwnd, "Int", -16, "Int" )
           DllCall( "SetWindowLong", "Ptr", btn.Hwnd, "Int", -16, "Int", (style & ~styleMask) | styleBits )
}

BtnPos( btnIdx, btnWidth, btnGap )
{
  gapAfterFirst := btnIdx > 0 ? btnWidth / 2 : 0
  return btnWidth +
         (btnWidth * btnIdx) +
         (btnGap   * btnIdx) +
         gapAfterFirst
}

HotkeyLabel( hotkey )
{
  hotkey := StrReplace( hotkey, "^", "Ctrl-"  )
  hotkey := StrReplace( hotkey, "+", "Shift-" )
  hotkey := StrReplace( hotkey, "#", "Win-"   )
  hotkey := StrReplace( hotkey, "!", "Alt-"   )
  return hotkey
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
  MoveWindowElevated( g_activeWindow, x, y, w, h )
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

IsWindowVisible( hwnd )
{
  state := GetWindowState( hwnd )
  isVisible := state.hwnd != 0 &&
               state.visible   &&
              !state.cloaked   &&
              !state.minimized
  OutputDebug( "IsWindowVisible check for hwnd " hwnd ": " isVisible )
  return isVisible
}

GetWindowState( hwnd := WinExist( "A" ) )
{
  OutputDebug( "Getting window state for hwnd: " hwnd )
  if( !hwnd )
  {
    OutputDebug( "Invalid hwnd, returning empty state." )
    return { hwnd: 0 }
  }

  visible   := !! DllCall( "IsWindowVisible", "Ptr", hwnd )
  minimized := !! DllCall( "IsIconic",        "Ptr", hwnd )
  maximized := !! DllCall( "IsZoomed",        "Ptr", hwnd )

  ; DWM cloaked (UWP / remote desktop windows)
  cloakedBuf := Buffer( 4, 0 )
  DllCall( "dwmapi\DwmGetWindowAttribute", "Ptr", hwnd, "UInt", 14, "Ptr", cloakedBuf.Ptr, "UInt", 4 )
  cloaked := NumGet( cloakedBuf, 0, "UInt" ) != 0
  OutputDebug( "Window state - Visible: " visible ", Minimized: " minimized ", Maximized: " maximized ", Cloaked: " cloaked )

  return { hwnd:      hwnd,
           visible:   visible,
           minimized: minimized,
           maximized: maximized,
           cloaked:   cloaked }
}
