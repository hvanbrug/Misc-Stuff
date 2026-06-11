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

; Send a key/combo via Send, bypassing clipboard mode.
; Use for keys like {Enter} that must be input events, not pasted text.
DoSendInput( msg )
{
  if( IsSet( g_activeWindow ) )
  {
    WinActivate( g_activeWindow )
    Sleep( 100 )
  }
  Send( msg )
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
  INI_SetClipSendMode( g_useClipSend )
  ToolTip( "Clipboard send mode: " state )
  SetTimer( (*) => ToolTip(), -2000 )
}

SetToggleSizeBtnState( collapsed )
{
  global g_toggleSizeBtn
  global g_tipMap

  OutputDebug( "Setting toggle-size button state: " (collapsed ? "COLLAPSED" : "EXPANDED") )
  if( !IsObject( g_toggleSizeBtn ) )
  {
    OutputDebug( "Toggle-size button not initialized yet." )
    return
  }

  if( collapsed )
  {
    ; Window is collapsed: clicking will expand it.
    g_toggleSizeBtn.Text := "▲"
    g_tipMap[g_toggleSizeBtn.Hwnd] := "Expand window"
  }
  else
  {
    ; Window is expanded: clicking will collapse it.
    g_toggleSizeBtn.Text := "▼"
    g_tipMap[g_toggleSizeBtn.Hwnd] := "Shrink window"
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

; Remove emoji codepoints from a string and tidy up the spaces left behind
; before any trailing punctuation. Used by the Comments tab when strip-emoji
; mode is enabled.
; AHK's PCRE build is already UTF-aware, so codepoints > U+FFFF can be matched
; directly with \x{...} — no (*UTF) verb needed (and it would be rejected).
StripEmojis( text )
{
  emojiPattern := "[\x{1F000}-\x{1FFFF}\x{2600}-\x{27BF}\x{200D}\x{FE0F}\x{20E3}]"
  result := RegExReplace( text,   emojiPattern,         ""  )
  result := RegExReplace( result, "\s+([\.,;:!?])",     "$1" )
  result := RegExReplace( result, "\s{2,}",             " "  )
  return Trim( result )
}

ToggleStripSendEmojis()
{
  global g_stripSendEmojis
  g_stripSendEmojis := !g_stripSendEmojis
  state := g_stripSendEmojis ? "ON" : "OFF"
  SetStripEmojisIndicatorState( g_stripSendEmojis )
  INI_SetStripCommentEmojis( g_stripSendEmojis )
  ToolTip( "Strip emojis from comments: " state )
  SetTimer( (*) => ToolTip(), -2000 )
}

SetStripEmojisIndicatorState( enabled )
{
  global g_stripEmojisIndicator
  global g_tipMap

  OutputDebug( "Setting strip-emojis indicator state: " (enabled ? "ON" : "OFF") )
  if( !IsObject( g_stripEmojisIndicator ) )
  {
    OutputDebug( "Strip-emojis indicator control not initialized yet." )
    return
  }
  g_stripEmojisIndicator.Text := enabled ? "☻" : "☺"
  g_tipMap[g_stripEmojisIndicator.Hwnd] := "Strip emojis from comments: " (enabled ? "ON" : "OFF")
}

IsClipControl( hwnd )
{
  global g_clipIndicator
  return IsObject( g_clipIndicator ) && (hwnd = g_clipIndicator.Hwnd)
}

IsStripEmojisControl( hwnd )
{
  global g_stripEmojisIndicator
  return IsObject( g_stripEmojisIndicator ) && (hwnd = g_stripEmojisIndicator.Hwnd)
}

IsToggleSizeBtn( hwnd )
{
  global g_toggleSizeBtn
  return IsObject( g_toggleSizeBtn ) && (hwnd = g_toggleSizeBtn.Hwnd)
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
  DisableButtonWrap( btn )
  btn.OnEvent( "Click", func )
  g_tipMap[btn.Hwnd] := tip
  g_gui.SetFont( g_fontSize " norm", g_fontName )

  return btn
}

; Remove BS_MULTILINE from a button so long text never wraps onto a second
; line. Buttons are created with this flag by default; we always want
; single-line behaviour so layouts stay predictable.
DisableButtonWrap( btn )
{
  if( !IsObject( btn ) || !btn.HasProp( "Hwnd" ) || !btn.Hwnd )
  {
    return
  }
  hwnd := btn.Hwnd
  if( !HasWindowStyle( hwnd, BS_MULTILINE ) )
  {
    return
  }
  RemoveWindowStyle( hwnd, BS_MULTILINE, false )
  DllCall( "InvalidateRect", "Ptr", hwnd, "Ptr", 0, "Int", 1 )
}

; Add BS_NOTIFY to a button so its parent receives BN_DBLCLK (and BN_SETFOCUS,
; BN_KILLFOCUS) WM_COMMAND notifications. We use this on tab-page buttons so
; double-click can append a newline after the normal single-click text send.
EnableButtonDoubleClick( btn )
{
  if( !IsObject( btn ) || !btn.HasProp( "Hwnd" ) || !btn.Hwnd )
  {
    return
  }
  hwnd := btn.Hwnd
  if( HasWindowStyle( hwnd, BS_NOTIFY ) )
  {
    return
  }
  AddWindowStyle( hwnd, BS_NOTIFY, false )
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

; Measure the rendered width (in device pixels) of `text` when drawn into `hdc`
; with the font currently selected into that DC.
MeasureTextWidth( hdc, text )
{
  size := Buffer( 8, 0 )
  DllCall( "GetTextExtentPoint32W",
           "Ptr",  hdc,
           "WStr", text,
           "Int",  StrLen( text ),
           "Ptr",  size )
  return NumGet( size, 0, "Int" )
}

; Build a word-boundary truncation of `fullText` that fits within `maxWidth`
; when rendered with the font selected into `hdc`, appending `ellipsis`.
; Falls back to per-character truncation when no full word fits. Any leading
; whitespace in `fullText` is preserved so left-aligned padding survives.
TruncateTextWithEllipsis( hdc, fullText, maxWidth, ellipsis := "..." )
{
  leading := ""
  if( RegExMatch( fullText, "^\s+", &leadMatch ) )
  {
    leading := leadMatch[0]
  }
  body := SubStr( fullText, StrLen( leading ) + 1 )

  bestFit  := ""
  fitFound := false

  words     := StrSplit( body, " " )
  candidate := leading
  loop words.Length
  {
    word := words[A_Index]
    next := (candidate = leading) ? candidate . word
                                  : candidate . " " . word
    if( MeasureTextWidth( hdc, next . ellipsis ) <= maxWidth )
    {
      candidate := next
      bestFit   := candidate
      fitFound  := true
    }
    else
    {
      break
    }
  }

  if( !fitFound )
  {
    candidate := leading
    chars     := StrSplit( body )
    loop chars.Length
    {
      ch   := chars[A_Index]
      next := candidate . ch
      if( MeasureTextWidth( hdc, next . ellipsis ) <= maxWidth )
      {
        candidate := next
        bestFit   := candidate
      }
      else
      {
        break
      }
    }
  }

  if( bestFit = "" )
  {
    bestFit := leading
  }
  return bestFit . ellipsis
}

; Clip a button's text at a word boundary, appending "..." when it doesn't fit
; within the button's client width. Assumes BS_MULTILINE has already been
; removed at button creation (see DisableButtonWrap). No-op for empty text.
ApplyEllipsisToButton( btn, padding := 10 )
{
  static WM_GETFONT := 0x31

  if( !IsObject( btn ) || !btn.HasProp( "Hwnd" ) )
  {
    return
  }
  hwnd := btn.Hwnd
  if( !hwnd )
  {
    return
  }

  fullText := btn.Text
  if( fullText = "" )
  {
    return
  }

  hdc := DllCall( "GetDC", "Ptr", hwnd, "Ptr" )
  if( !hdc )
  {
    return
  }

  hFont   := DllCall( "SendMessageW",
                      "Ptr",  hwnd,
                      "UInt", WM_GETFONT,
                      "Ptr",  0,
                      "Ptr",  0,
                      "Ptr" )
  oldFont := hFont ? DllCall( "SelectObject", "Ptr", hdc, "Ptr", hFont, "Ptr" ) : 0

  try
  {
    rect := Buffer( 16, 0 )
    DllCall( "GetClientRect", "Ptr", hwnd, "Ptr", rect )
    clientWidth := NumGet( rect, 8, "Int" ) - NumGet( rect, 0, "Int" )
    maxWidth    := clientWidth - padding
    if( maxWidth < 1 )
    {
      return
    }

    if( MeasureTextWidth( hdc, fullText ) <= maxWidth )
    {
      return
    }

    truncated := TruncateTextWithEllipsis( hdc, fullText, maxWidth )
    DllCall( "SetWindowTextW", "Ptr", hwnd, "WStr", truncated )
  }
  finally
  {
    if( oldFont )
    {
      DllCall( "SelectObject", "Ptr", hdc, "Ptr", oldFont )
    }
    DllCall( "ReleaseDC", "Ptr", hwnd, "Ptr", hdc )
  }
}

BtnPos( btnIdx, btnWidth, btnGap )
{
  ;gapAfterFirst := btnIdx > 0 ? btnWidth / 2 : 0
  gapAfterFirst := 0
  return btnWidth +
         (btnWidth * btnIdx) +
         (btnGap   * btnIdx) +
         gapAfterFirst
}

HotkeyLabel( hotkey )
{
  hotkey := StrUpper(   hotkey )
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

; Force the OS to recalculate the window's non-client area after style changes.
; Called by AddWindowStyle and RemoveWindowStyle when recalcFrame is true.
RecalcWindowFrame( hwnd )
{
  static SWP_NOSIZE       := 0x0001
  static SWP_NOMOVE       := 0x0002
  static SWP_NOZORDER     := 0x0004
  static SWP_FRAMECHANGED := 0x0020

  if( !hwnd )
  {
    return
  }

  DllCall( "SetWindowPos", "Ptr", hwnd, "Ptr", 0,
           "Int", 0, "Int", 0, "Int", 0, "Int", 0,
           "UInt", SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED )
}

; Add a window style flag to the specified window.
; styleFlag: the WS_* or other style flag to add (e.g., 0x00040000 for WS_THICKFRAME)
; recalcFrame: if true, forces OS to recalculate the non-client area
AddWindowStyle( hwnd, styleFlag, recalcFrame := true )
{
  static GWL_STYLE := -16

  if( !hwnd )
  {
    return
  }

  currentStyle := DllCall( "GetWindowLong", "Ptr", hwnd, "Int", GWL_STYLE, "Int" )
  newStyle := currentStyle | styleFlag
  DllCall( "SetWindowLong", "Ptr", hwnd, "Int", GWL_STYLE, "Int", newStyle )

  if( recalcFrame )
  {
    RecalcWindowFrame( hwnd )
  }
}

; Remove a window style flag from the specified window.
; styleFlag: the WS_* or other style flag to remove (e.g., 0x00040000 for WS_THICKFRAME)
; recalcFrame: if true, forces OS to recalculate the non-client area
RemoveWindowStyle( hwnd, styleFlag, recalcFrame := true )
{
  static GWL_STYLE := -16

  if( !hwnd )
  {
    return
  }

  currentStyle := DllCall( "GetWindowLong", "Ptr", hwnd, "Int", GWL_STYLE, "Int" )
  newStyle := currentStyle & ~styleFlag
  DllCall( "SetWindowLong", "Ptr", hwnd, "Int", GWL_STYLE, "Int", newStyle )

  if( recalcFrame )
  {
    RecalcWindowFrame( hwnd )
  }
}

; ── Window Style Constants ──────────────────────────────────────────────
; Common WS_* and button style flags for use with AddWindowStyle/RemoveWindowStyle
global GWL_STYLE       := -16
global WS_CLIPCHILDREN := 0x02000000
global WS_THICKFRAME   := 0x00040000
global WS_CLIPSIBLINGS := 0x04000000
global BS_MULTILINE    := 0x2000
global BS_NOTIFY       := 0x4000
global BS_BITMAP       := 0x80

; Test if all specified style flags are set on a window.
; Returns true only if ALL bits in styleFlags are set in the window's style.
HasWindowStyle( hwnd, styleFlags )
{
  if( !hwnd )
  {
    return false
  }

  currentStyle := DllCall( "GetWindowLong", "Ptr", hwnd, "Int", GWL_STYLE, "Int" )
  return (currentStyle & styleFlags) = styleFlags
}

; Test if any specified style flags are set on a window.
; Returns true if ANY bit in styleFlags is set in the window's style.
HasAnyWindowStyle( hwnd, styleFlags )
{
  if( !hwnd )
  {
    return false
  }

  currentStyle := DllCall( "GetWindowLong", "Ptr", hwnd, "Int", GWL_STYLE, "Int" )
  return (currentStyle & styleFlags) != 0
}
