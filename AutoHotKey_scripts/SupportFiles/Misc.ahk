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
