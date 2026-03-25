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

RegisterSymbol( row, col, width, tab,
                char, desc := "", hotkey := "",
                action := "" )
{
  global g_symbols
  g_symbols.Push( { char: char, desc: (desc != "") ? desc : char,
                    hotkey: hotkey,
                    action: (action != "") ? action : () => DoSendText( char ),
                    row: row, col: col, width: width, tab: tab } )
}

RegisterEmoji( row, col, width, tab,
               char, desc, hotkey,
               imageFile,
               action := "" )
{
  OutputDebug( "Registering emoji: " char " - " desc " - " hotkey " - " imageFile )
  global g_symbols
  g_symbols.Push( { char: char, desc: desc, hotkey: hotkey,
                    imageFile: imageFile,
                    action: (action != "") ? action : () => DoSendText( char ),
                    row: row, col: col, width: width, tab: tab } )

  ;; Build full image path
  ;imagePath := "SupportFiles/twemoji-assets/" imageFile
  ;if (action = "")
  ;{
  ;  action := () => DoSendText( hotkey )
  ;}
  ;; Store symbol info (example: add to g_Symbols array or similar)
  ;; Add button with image
  ;x := (col - 1) * 40 + 10
  ;y := (row - 1) * 40 + 10
  ;btn := Gui().Add("Button", "x" x " y" y " w35 h35 +0x200", "")
  ;btn.SetImage(imagePath)
  ;btn.OnEvent("Click", action)
  ;; Optionally store btn, desc, etc. for tooltips or later use
}

SetTabName( tabIndex, tabName )
{
  global g_tabNames
  if !IsSet( g_tabNames )
  {
    g_tabNames := Map()
  }
  g_tabNames[tabIndex] := tabName
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
