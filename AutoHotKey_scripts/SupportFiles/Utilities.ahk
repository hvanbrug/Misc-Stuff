; Utilities.ahk
; A collection of general utility functions that can be used in various contexts.

; ── Global registries for the Help Menu UI ──
g_HelpActions := []
g_Symbols     := []

RegisterAction( displayHotkey, desc, action )
{
  global g_HelpActions
  g_HelpActions.Push( { hotkey: displayHotkey, desc: desc, action: action } )
}

RegisterSymbol( char, desc, displayHotkey, action, row, col, tab := 1 )
{
  global g_Symbols
  g_Symbols.Push( { char: char, desc: desc, hotkey: displayHotkey, action: action, row: row, col: col, tab: tab } )
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
