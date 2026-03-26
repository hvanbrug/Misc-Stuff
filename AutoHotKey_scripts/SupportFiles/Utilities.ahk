; Utilities.ahk
; A collection of general utility functions that can be used in various contexts.

; ── Global registries for the Help Menu UI ──
g_HelpActions := []
g_symbols     := []

; Global map for tab names
if !IsSet( g_tabNames )
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
  SendInput( msg )
}

GetSelectedTextThroughClipboard()
{
  backup := ClipboardAll()
  A_Clipboard := ''
  Send( '^c' )
  CLIP_WAIT_TIMEOUT_SEC := 1
  CLIP_WAIT_ANY_DATA    := 1
  if !ClipWait( CLIP_WAIT_TIMEOUT_SEC, CLIP_WAIT_ANY_DATA )
  {
    return (A_Clipboard := backup)
  }
  txt := A_Clipboard
  A_Clipboard := backup
  return txt
}
