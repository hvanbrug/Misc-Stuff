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