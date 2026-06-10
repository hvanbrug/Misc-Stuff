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
  menu.Add( "Open UI",  ShowUI )
  menu.Add( "Close UI", HideUI )
  menu.Add()
  menu.Add( "Set favourite spot",     (*) => SetFavouriteSpot()    )
  menu.Add( "Move to favourite spot", (*) => MoveToFavouriteSpot() )
  menu.Add()
  menu.Add( "Test Function", (*) => TestFunction() )
  menu.Add()
  menu.Add( "Exit", (*) => ExitApp() )
}



ToggleUI( * )
{
  OutputDebug( "Toggling UI." )
  global g_gui
  OutputDebug( "g_gui is " (IsObject( g_gui ) ? "an object" : "not an object") "." )
  if( !IsObject( g_gui ) || !IsWindowVisible( g_gui.Hwnd ) )
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
  global g_gui
  if( IsObject( g_gui ) )
  {
    OutputDebug( "UI object already exists. Restoring and activating window." )
    g_gui.Restore()
    ;g_gui.Show()
    WinActivate( "ahk_id " g_gui.Hwnd )
  }
  else
  {
    OutputDebug( "UI object does not exist. Creating new window." )
    ShowWindow() ; Start on the emojis tab
  }
  OutputDebug( "UI is now visible." )
  INI_SetWndOpen( true )
  OutputDebug( "Updated INI file to indicate window is open." )
}

HideUI( * )
{
  OutputDebug( "Hiding UI." )
  global g_gui
  g_gui.Hide()
  INI_SetWndOpen( false )
  OutputDebug( "Updated INI file to indicate window is closed." )
}

