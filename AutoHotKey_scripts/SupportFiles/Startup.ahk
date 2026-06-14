^+a::ListHotkeys
^+x::ToggleUI



Startup()

Startup()
{
  OutputDebug( "Running startup routine." )

  A_TrayMenu.Delete()
  FillTrayMenu( A_TrayMenu )
  A_TrayMenu.Default := "Open UI"
  OutputDebug( "Tray menu initialized." )

  if( INI_IsWndOpen() )
  {
    OutputDebug( "Previous session ended with window open. Showing UI." )
    ShowUI()
  }
  else
  {
    OutputDebug( "Previous session ended with window closed. Keeping UI hidden." )
  }
}

FillTrayMenu( menu )
{
  global g_hotkeyWnd

  menu.Add( "Open UI",  ShowUI )
  menu.Add( "Close UI", HideUI )
  menu.Add()
  menu.Add( "Set favourite spot",     (*) => g_hotkeyWnd.SetFavouriteSpot()    )
  menu.Add( "Move to favourite spot", (*) => g_hotkeyWnd.MoveToFavouriteSpot() )
  menu.Add()
  menu.Add( "Test Function", (*) => TestFunction() )
  menu.Add( "Theme diagnostics", (*) => ThemeDiagnostics() )   ; TEMP
  menu.Add()
  menu.Add( "Exit", (*) => ExitApp() )
}



ToggleUI( * )
{
  OutputDebug( "Toggling UI." )
  global g_hotkeyWnd
  OutputDebug( "Window is " (g_hotkeyWnd.IsCreated() ? "created" : "not created") "." )
  if( !g_hotkeyWnd.IsCreated() || !IsWindowVisible( g_hotkeyWnd.Hwnd ) )
  {
    OutputDebug( "Showing UI." )
    ShowUI()
  }
  else
  {
    OutputDebug( "Hiding UI." )
    HideUI()
  }
}

ShowUI( * )
{
  OutputDebug( "Showing UI." )
  global g_hotkeyWnd
  if( g_hotkeyWnd.IsCreated() )
  {
    OutputDebug( "UI object already exists. Restoring and activating window." )
    g_hotkeyWnd.Restore()
  }
  else
  {
    OutputDebug( "UI object does not exist. Creating new window." )
    g_hotkeyWnd.Show() ; Start on the emojis tab
  }
  OutputDebug( "UI is now visible." )
  INI_SetWndOpen( true )
  OutputDebug( "Updated INI file to indicate window is open." )
}

HideUI( * )
{
  OutputDebug( "Hiding UI." )
  global g_hotkeyWnd
  g_hotkeyWnd.Hide()
  INI_SetWndOpen( false )
  OutputDebug( "Updated INI file to indicate window is closed." )
}
