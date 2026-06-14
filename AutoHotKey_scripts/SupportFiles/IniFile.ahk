

INI_IsWndOpen()
{
  global g_iniPath
  isOpen := IniRead( g_iniPath, "Window", "WndOpen", 0 ) = 1
  OutputDebug( "Checked INI file for window open state: " (isOpen ? "OPEN" : "CLOSED") )
  return isOpen
}

INI_SetWndOpen( isOpen )
{
  global g_iniPath
  IniWrite( isOpen ? 1 : 0, g_iniPath, "Window", "WndOpen" )
  OutputDebug( "Updated INI file to indicate window is: " (isOpen ? "OPEN" : "CLOSED") )
}


INI_IsCollapsed()
{
  global g_iniPath
  isCollapsed := IniRead( g_iniPath, "Window", "Collapsed", 0 ) = 1
  OutputDebug( "Checked INI file for window is: " (isCollapsed ? "COLLAPSED" : "EXPANDED") )
  return isCollapsed
}

INI_SetCollapsed( isCollapsed )
{
  global g_iniPath
  IniWrite( isCollapsed ? 1 : 0, g_iniPath, "Window", "Collapsed" )
  OutputDebug( "Updated INI file to indicate window is: " (isCollapsed ? "COLLAPSED" : "EXPANDED") )
}

INI_IsClipSendMode()
{
  global g_iniPath
  isOn := IniRead( g_iniPath, "Window", "ClipSendMode", 0 ) = 1
  OutputDebug( "Checked INI file for clipboard send mode: " (isOn ? "ON" : "OFF") )
  return isOn
}

INI_SetClipSendMode( isOn )
{
  global g_iniPath
  IniWrite( isOn ? 1 : 0, g_iniPath, "Window", "ClipSendMode" )
  OutputDebug( "Updated INI file to indicate clipboard send mode: " (isOn ? "ON" : "OFF") )
}

INI_IsStripCommentEmojis()
{
  global g_iniPath
  isOn := IniRead( g_iniPath, "Window", "StripCommentEmojis", 0 ) = 1
  OutputDebug( "Checked INI file for strip-comment-emojis mode: " (isOn ? "ON" : "OFF") )
  return isOn
}

INI_SetStripCommentEmojis( isOn )
{
  global g_iniPath
  IniWrite( isOn ? 1 : 0, g_iniPath, "Window", "StripCommentEmojis" )
  OutputDebug( "Updated INI file to indicate strip-comment-emojis mode: " (isOn ? "ON" : "OFF") )
}

INI_WndHeight()
{
  global g_iniPath
  height := IniRead( g_iniPath, "Window", "Height", "" )
  OutputDebug( "Read window height from INI file: " (height != "" ? height : "not set") )
  return height
}

INI_SetWndHeight( height )
{
  global g_iniPath
  IniWrite( height, g_iniPath, "Window", "Height" )
  OutputDebug( "Updated INI file with window height: " height )
}

INI_WndPosX()
{
  global g_iniPath
  x := IniRead( g_iniPath, "Window", "X", "" )
  OutputDebug( "Read window X position from INI file: " (x != "" ? x : "not set") )
  return x
}

INI_WndPosY()
{
  global g_iniPath
  y := IniRead( g_iniPath, "Window", "Y", "" )
  OutputDebug( "Read window Y position from INI file: " (y != "" ? y : "not set") )
  return y
}

INI_SetWndPosX( x )
{
  global g_iniPath
  IniWrite( x, g_iniPath, "Window", "X" )
  OutputDebug( "Updated INI file with window X position: " x )
}

INI_SetWndPosY( y )
{
  global g_iniPath
  IniWrite( y, g_iniPath, "Window", "Y" )
  OutputDebug( "Updated INI file with window Y position: " y )
}

INI_WndFavX()
{
  global g_iniPath
  x := IniRead( g_iniPath, "Window", "FavX", "" )
  OutputDebug( "Read window favourite X position from INI file: " (x != "" ? x : "not set") )
  return x
}

INI_WndFavY()
{
  global g_iniPath
  y := IniRead( g_iniPath, "Window", "FavY", "" )
  OutputDebug( "Read window favourite Y position from INI file: " (y != "" ? y : "not set") )
  return y
}

INI_SetWndFavX( x )
{
  global g_iniPath
  IniWrite( x, g_iniPath, "Window", "FavX" )
  OutputDebug( "Updated INI file with window favourite X position: " x )
}

INI_SetWndFavY( y )
{
  global g_iniPath
  IniWrite( y, g_iniPath, "Window", "FavY" )
  OutputDebug( "Updated INI file with window favourite Y position: " y )
}

INI_LastTab()
{
  global g_iniPath
  global g_hotkeyWnd

  TCM_GETITEMCOUNT := 0x1304
  hwnd  := g_hotkeyWnd.m_tabs.Hwnd
  count := DllCall("SendMessageW", "Ptr", hwnd, "UInt", TCM_GETITEMCOUNT, "Ptr", 0, "Ptr", 0, "Int")

  lastTabIdx := IniRead( g_iniPath, "Window", "LastTab", 1 )
  OutputDebug( "Read last active tab from INI file: " (lastTabIdx != "" ? lastTabIdx : "not set") )
  if( (lastTabIdx < 1) ||
      (lastTabIdx > count) )
  {
    OutputDebug( "Last active tab index from INI file is out of bounds (" lastTabIdx " > " count "). Defaulting to 1." )
    lastTabIdx := 1
  }

  return lastTabIdx
}

INI_SetLastTab( tabIdx )
{
  global g_iniPath
  IniWrite( tabIdx, g_iniPath, "Window", "LastTab" )
  OutputDebug( "Updated INI file with last active tab: " tabIdx )
}
