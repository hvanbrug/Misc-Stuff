^+a::ListHotkeys
^+x::ToggleUI



Startup()

Startup()
{
  A_TrayMenu.Delete()
  A_TrayMenu.Add( "Open UI",  ShowUI )
  A_TrayMenu.Add( "Close UI", HideUI )
  A_TrayMenu.Add()
  A_TrayMenu.Add( "Set favourite spot",     (*) => SetFavouriteSpot()    )
  A_TrayMenu.Add( "Move to favourite spot", (*) => MoveToFavouriteSpot() )
  A_TrayMenu.Add()
  A_TrayMenu.Add( "Exit", (*) => ExitApp() )
  A_TrayMenu.Default := "Open UI"

  global g_iniPath
  isWndOpen := IniRead( g_iniPath, "Window", "WndOpen", 0 )
  if( isWndOpen = "1" )
  {
    ShowUI()
  }
}




ToggleUI( * )
{
  global g_gui
  if( IsObject( g_gui ) )
  {
    g_gui.Show()
    WinActivate( "ahk_id " g_gui.Hwnd )
  }
  else
  {
    ShowWindow( 2 ) ; Start on the emojis tab
  }
}

ShowUI( * )
{
  global g_gui
  if( IsObject( g_gui ) )
  {
    g_gui.Show()
    WinActivate( "ahk_id " g_gui.Hwnd )
  }
  else
  {
    ShowWindow( 2 ) ; Start on the emojis tab
  }
  IniWrite( 1, g_iniPath, "Window", "WndOpen" )
}

HideUI( * )
{
  global g_gui
  g_gui.Hide()
  IniWrite( 0, g_iniPath, "Window", "WndOpen" )
}


ShowWindow( startTab )
{
  global g_uiTabs
  global g_activeWindow

  global g_gui
  global g_tipMap
  global g_symbols
  global g_tabNames
  global g_tabs
  global g_tabScrollHwnd
  global g_guiHwndRaw
  global g_fontSize
  global g_fontName

  global g_HelpActions
  global g_RESIZE_H_MARGIN
  global g_LV
  global g_LV_WIDTH
  global g_LV_ROW_COUNT
  global g_HEADER_BG_COLOR
  global g_HEADER_TEXT_COLOR
  global g_hdrHeight
  global g_COL_HOTKEY_WIDTH
  global g_COL_DESC_WIDTH
  global g_HeaderHotkey
  global g_HeaderDesc
  global g_HeaderBg
  global g_fullW
  global g_fullH
  global g_shrinkBtn
  global g_expandBtn
  global g_iniPath

  g_activeWindow := WinActive( "A" )

  windowTitle := "Henks Hotkeys"
  if WinExist( windowTitle )
  {
    WinActivate
    return
  }

  g_gui := Gui( "+AlwaysOnTop +ToolWindow -Caption -Resize -MinimizeBox -MaximizeBox", windowTitle )

  IniWrite( 1, g_iniPath, "Window", "WndOpen" )

  ; Prevent the GUI from painting its background over child button areas during scroll.
  WS_CLIPCHILDREN := 0x02000000
  guiStyle := DllCall( "GetWindowLong", "ptr", g_gui.Hwnd, "int", -16, "int" )
  DllCall( "SetWindowLong", "ptr", g_gui.Hwnd, "int", -16, "int", guiStyle | WS_CLIPCHILDREN )

  tabList := []
  tabContentWidth  := 0
  tabContentHeight := 0
  for tab in g_uiTabs
  {
    tabList.Push( tab.m_name )
    tabContentWidth  := tab.GetContentWidth(  tabContentWidth  )
    tabContentHeight := tab.GetContentHeight( tabContentHeight )
  }

  ; Keep symbol tabs to a bounded viewport so overflow can scroll.
  TAB_VIEWPORT_SCREEN_MARGIN := 320
  TAB_VIEWPORT_MAX_HEIGHT    := 330
  TAB_VIEWPORT_MIN_HEIGHT    := 320

  maxTabContentHeight := A_ScreenHeight - TAB_VIEWPORT_SCREEN_MARGIN
  if( maxTabContentHeight > TAB_VIEWPORT_MAX_HEIGHT )
  {
    maxTabContentHeight := TAB_VIEWPORT_MAX_HEIGHT
  }
  if( maxTabContentHeight < TAB_VIEWPORT_MIN_HEIGHT )
  {
    maxTabContentHeight := TAB_VIEWPORT_MIN_HEIGHT
  }
  if( tabContentHeight > maxTabContentHeight )
  {
    tabContentHeight := maxTabContentHeight
  }

  TAB_SCROLL_W := 18

  g_tabs := g_gui.AddTab3( "x5 y24 w" (tabContentWidth + TAB_SCROLL_W + 14) " h" (tabContentHeight + 30), tabList )

  ; WS_CLIPSIBLINGS: prevents the tab control from painting over sibling windows
  ; (the utility buttons) that sit above it in z-order.  Set once here so we
  ; never need to fiddle with z-order during shrink/expand.
  WS_CLIPSIBLINGS := 0x04000000
  tabStyle := DllCall( "GetWindowLong", "Ptr", g_tabs.Hwnd, "Int", -16, "Int" )
              DllCall( "SetWindowLong", "Ptr", g_tabs.Hwnd, "Int", -16, "Int", tabStyle | WS_CLIPSIBLINGS )
  ; Start from the tab's client rect, then shrink to the display area.
  displayRect := Buffer( 16, 0 )
  DllCall( "GetClientRect", "Ptr", g_tabs.Hwnd, "Ptr", displayRect.Ptr )
  ; TCM_ADJUSTRECT with wParam=FALSE shrinks the client rect to the display area.
  SendMessage( 0x1328, 0, displayRect.Ptr, g_tabs.Hwnd )
  dispLeft   := NumGet( displayRect, 0,  "Int" )
  dispTop    := NumGet( displayRect, 4,  "Int" )
  dispRight  := NumGet( displayRect, 8,  "Int" )
  dispBottom := NumGet( displayRect, 12, "Int" )

  ; Map the display-area corners from tab-client coords to GUI-client coords.
  ptTopLeft := Buffer( 8, 0 )
  NumPut( "Int", dispLeft, ptTopLeft, 0 )
  NumPut( "Int", dispTop,  ptTopLeft, 4 )
  DllCall( "ClientToScreen", "Ptr", g_tabs.Hwnd, "Ptr", ptTopLeft.Ptr )
  DllCall( "ScreenToClient", "Ptr", g_gui.Hwnd,  "Ptr", ptTopLeft.Ptr )

  ptBottomRight := Buffer( 8, 0 )
  NumPut( "Int", dispRight,  ptBottomRight, 0 )
  NumPut( "Int", dispBottom, ptBottomRight, 4 )
  DllCall( "ClientToScreen", "Ptr", g_tabs.Hwnd, "Ptr", ptBottomRight.Ptr )
  DllCall( "ScreenToClient", "Ptr", g_gui.Hwnd,  "Ptr", ptBottomRight.Ptr )

  dispGuiLeft   := NumGet( ptTopLeft, 0, "Int" )
  dispGuiTop    := NumGet( ptTopLeft, 4, "Int" )
  dispGuiRight  := NumGet( ptBottomRight, 0, "Int" )
  dispGuiBottom := NumGet( ptBottomRight, 4, "Int" )

  tabScrollX := dispGuiRight - TAB_SCROLL_W
  tabScrollY := dispGuiTop
  tabScrollH := dispGuiBottom - dispGuiTop

  ; Detach from tab context so the scrollbar is a window-level control visible on all tabs.
  g_tabs.UseTab( 0 )

  ; Create a native vertical scroll bar.  WS_VISIBLE|WS_CHILD|SBS_VERT = 0x50000001
  g_tabScrollHwnd := DllCall( "CreateWindowEx",
                              "UInt", 0,
                              "Str",  "SCROLLBAR",
                              "Ptr",  0,
                              "UInt", 0x50000001,
                              "Int",  tabScrollX,
                              "Int",  tabScrollY,
                              "Int",  TAB_SCROLL_W,
                              "Int",  tabScrollH,
                              "Ptr",  g_gui.Hwnd,
                              "Ptr",  0,
                              "Ptr",  0,
                              "Ptr",  0,
                              "Ptr" )

  ; Force classic scrollbar appearance so it stays visible instead of auto-hiding.
  DllCall( "uxtheme\SetWindowTheme", "Ptr", g_tabScrollHwnd, "Str", "", "Str", "" )

  OnMessage( 0x0115, VScroll )  ; WM_VSCROLL

  for tabIndex, tab in g_uiTabs
  {
    tab.SetViewportHeight( tabContentHeight )
    tab.AddControls( g_gui, g_tabs, tabIndex, g_tipMap )
  }

  ; All clip panels start visible (WS_VISIBLE); hide them all for now.
  ; The correct one will be shown after startTab is determined.
  for tabIndex, tab in g_uiTabs
  {
    tab.HideClipPanel()
  }

  g_gui.SetFont( g_fontSize " norm", g_fontName )

  SetTimer( HoverCheck, 100 )
  SetTimer( TrackActiveWindow, 100 )

  OnMessage( 0x201, OnLButtonDown )
  OnMessage( 0x205, OnRButtonUp   )  ; WM_RBUTTONUP
  g_gui.OnEvent(  "Escape", (*) => Close() )
  g_gui.OnEvent(  "Close",  (*) => Close() )
  g_tabs.OnEvent( "Change", TabChanged     )

  g_guiHwndRaw := g_gui.Hwnd
  InstallWheelHook()

  if( (startTab < 1) ||
      (startTab > tabList.Length) )
  {
    startTab := 1
  }
  g_tabs.Value := startTab

  ; Show the clip panel for the active start tab.
  if( (startTab >= 1) && (startTab <= g_uiTabs.Length) )
  {
    g_uiTabs[startTab].ShowClipPanel()
  }

  UpdateScrollInfo()

  ; Bring scrollbar to top of z-order so tab buttons don't steal mouse events from it.
  SWP_NOMOVE := 0x0002, SWP_NOSIZE := 0x0001, SWP_NOACTIVATE := 0x0010
  DllCall( "SetWindowPos", "Ptr", g_tabScrollHwnd, "Ptr", 0,
           "Int", 0, "Int", 0, "Int", 0, "Int", 0,
           "UInt", SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE )

  ; Utility buttons in top-right corner (outside tab context).
  g_tabs.UseTab( 0 )
  rightEdge := tabContentWidth + TAB_SCROLL_W + 16
  btnGap    := 2
  btnWth    := 40
  btnHgt    := 24

  CreateButton( "⌫.", "Back 3, Replace with period",
                "Segoe UI Symbol", "s10",
                rightEdge - BtnPos( 3, btnWth, btnGap ), 0, btnWth, btnHgt,
                (*) => DoSendText( "`b`b`b. " ) )

  CreateBtnWithStyle( "⇚,", "Back 3, Insert Comma",
                      "Segoe UI Symbol", "s16",
                      0x0F00, 0x0800, ; BS_BOTTOM (0x0800) — push baseline up so the tall glyph isn't clipped.
                      rightEdge - BtnPos( 2, btnWth, btnGap ), 0, btnWth, btnHgt,
                      (*) => DoSendText( "{Left}{Left}{Left}, " ) )

  CreateButton( "↩", "Enter / Newline",
                "Segoe UI Symbol", "s14",
                rightEdge - BtnPos( 1, btnWth, btnGap ), 0, btnWth, btnHgt,
                (*) => DoSendText( "`n" ) )

; 🔄⏎
  CreateButton( "", "Repaint / Refresh",
                "Segoe UI Symbol", "s10",
                rightEdge - BtnPos( 0, btnWth, btnGap ), 0, btnWth, btnHgt,
                (*) => ForceRepaint() )

  g_shrinkBtn := CreateButton( "▼", "Shrink window",
                               "Segoe UI Symbol", "s14",
                               15, 0, btnWth, btnHgt,
                               (*) => ShrinkWindow() )
  g_shrinkBtn.Opt( "-Hidden" )

  g_expandBtn := CreateButton( "▲", "Expand window",
                               "Segoe UI Symbol", "s14",
                               15, 0, btnWth, btnHgt,
                               (*) => ExpandWindow() )
  g_expandBtn.Opt( "Hidden" )

  ; Explicit window size based on tab control dimensions.
  ; Prevents AHK from auto-sizing to include hidden buttons at large Y offsets.
  showW   := tabContentWidth + TAB_SCROLL_W + 14 + 14
  showH   := tabContentHeight + 30 + 14 + 14
  g_fullW := showW
  g_fullH := showH
  ;savedPos := LoadWindowPos()
  ;g_gui.Show( "w" showW " h" showH " " savedPos )
  ;RedrawScrollbar()

  if( IsCollapsed() )
  {
    ShrinkWindow()
  }
  else
  {
    ExpandWindow()
  }

  OnMessage( 0x0003, OnWindowMove   )  ; WM_MOVE
  OnMessage( 0x0216, OnWindowMoving )  ; WM_MOVING

  ; Restore collapsed state last, after everything is laid out.
  if( IniRead( g_iniPath, "Window", "Collapsed", "0" ) = "1" )
  {
    ShrinkWindow()
  }
}



OnLButtonDown( wParam, lParam, msg, hwnd )
{
  global g_gui
  global g_dragOffsetX
  global g_dragOffsetY

  if( hwnd != g_gui.Hwnd )
  {
    return
  }

  ; Record how far the cursor is from the window top-left at grab time.
  pt := Buffer( 8 )
  DllCall( "GetCursorPos", "Ptr", pt )
  cursorX := NumGet( pt, 0, "Int" )
  cursorY := NumGet( pt, 4, "Int" )
  WinGetPos( &winX, &winY, , , g_gui )
  g_dragOffsetX := cursorX - winX
  g_dragOffsetY := cursorY - winY

  PostMessage( 0xA1, 2,,, "ahk_id " hwnd )
}

OnRButtonUp( wParam, lParam, msg, hwnd )
{
  global g_gui

  if( hwnd != g_gui.Hwnd )
  {
    return
  }

  contextMenu := Menu()
  contextMenu.Add( "Open UI",  ShowUI )
  contextMenu.Add( "Close UI", HideUI )
  contextMenu.Add()
  contextMenu.Add( "Set favourite spot",     (*) => SetFavouriteSpot()    )
  contextMenu.Add( "Move to favourite spot", (*) => MoveToFavouriteSpot() )
  contextMenu.Add()
  contextMenu.Add( "Exit", (*) => ExitApp() )
  contextMenu.Show()
}

OnWindowMove( wParam, lParam, msg, hwnd )
{
  ; Debounce: coalesce rapid move messages into one write 500 ms after the last one.
  SetTimer( SaveWindowPos, -500 )
}

OnWindowMoving( wParam, lParam, msg, hwnd )
{
  global g_gui
  global g_snappedToTop
  global g_snappedToFav
  global g_dragOffsetX
  global g_dragOffsetY
  global g_favX
  global g_favY

  static SNAP_THRESHOLD    := 5
  static RELEASE_THRESHOLD := 20

  if( hwnd != g_gui.Hwnd )
  {
    return
  }

  left := NumGet( lParam,  0, "Int" )
  top  := NumGet( lParam,  4, "Int" )
  w    := NumGet( lParam,  8, "Int" ) - left
  h    := NumGet( lParam, 12, "Int" ) - top

  ; ── Favourite spot snap (highest priority) ──
  haveFav := (g_favX != "" && g_favY != "")
  if( haveFav )
  {
    favX := Integer( g_favX )
    favY := Integer( g_favY )

    if( g_snappedToFav )
    {
      ; Already snapped to fav: release when cursor implies position far enough away.
      pt := Buffer( 8 )
      DllCall( "GetCursorPos", "Ptr", pt )
      cursorX     := NumGet( pt, 0, "Int" )
      cursorY     := NumGet( pt, 4, "Int" )
      impliedLeft := cursorX - g_dragOffsetX
      impliedTop  := cursorY - g_dragOffsetY
      if( Abs( impliedLeft - favX ) >= RELEASE_THRESHOLD ||
          Abs( impliedTop  - favY ) >= RELEASE_THRESHOLD )
      {
        g_snappedToFav := false
        NumPut( "Int", impliedLeft,     lParam,  0 )
        NumPut( "Int", impliedTop,      lParam,  4 )
        NumPut( "Int", impliedLeft + w, lParam,  8 )
        NumPut( "Int", impliedTop  + h, lParam, 12 )
        return 1
      }
      ; Keep snapped to fav.
      NumPut( "Int", favX,     lParam,  0 )
      NumPut( "Int", favY,     lParam,  4 )
      NumPut( "Int", favX + w, lParam,  8 )
      NumPut( "Int", favY + h, lParam, 12 )
      return 1
    }

    ; Not snapped to fav: check if within snap zone of favourite.
    if( Abs( left - favX ) <= SNAP_THRESHOLD && Abs( top - favY ) <= SNAP_THRESHOLD )
    {
      g_snappedToFav := true
      g_snappedToTop := false
      NumPut( "Int", favX,       lParam,  0 )
      NumPut( "Int", favY,       lParam,  4 )
      NumPut( "Int", favX + w,   lParam,  8 )
      NumPut( "Int", favY + h,   lParam, 12 )
      return 1
    }
  }

  ; ── Top-of-screen snap (y=0) ──
  if( g_snappedToTop )
  {
    ; Already snapped: release only when the implied window top moves far enough below 0.
    pt := Buffer( 8 )
    DllCall( "GetCursorPos", "Ptr", pt )
    cursorY := NumGet( pt, 4, "Int" )
    impliedTop := cursorY - g_dragOffsetY
    if( impliedTop >= RELEASE_THRESHOLD )
    {
      g_snappedToTop := false
      NumPut( "Int", impliedTop,     lParam,  4 ) ; top    = impliedTop
      NumPut( "Int", impliedTop + h, lParam, 12 ) ; bottom = impliedTop + h
      return 1
    }
    ; Keep snapped.
    NumPut( "Int", 0, lParam,  4 ) ; top    = 0
    NumPut( "Int", h, lParam, 12 ) ; bottom = h
    return 1
  }

  ; Not snapped: snap if the window top is within the snap zone.
  if( top <= SNAP_THRESHOLD )
  {
    g_snappedToTop := true
    NumPut( "Int", 0, lParam,  4 ) ; top    = 0
    NumPut( "Int", h, lParam, 12 ) ; bottom = h
    return 1
  }
}

ShrinkWindow()
{
  global g_gui
  global g_tabs
  global g_fullW
  global g_fullH
  global g_shrinkBtn
  global g_expandBtn
  global g_iniPath

  if( !IsObject( g_gui ) )
  {
    return
  }

  g_tabs     .Opt( "Hidden" )
  g_shrinkBtn.Opt( "Hidden" )
  g_expandBtn.Opt( "-Hidden" )

  savedPos := LoadWindowPos()
  g_gui.Show( "w70 h24 NoActivate" savedPos )
  IniWrite( 1, g_iniPath, "Window", "Collapsed" )
}

ExpandWindow()
{
  global g_gui
  global g_tabs
  global g_uiTabs
  global g_fullW
  global g_fullH
  global g_shrinkBtn
  global g_expandBtn
  global g_iniPath

  if( !IsObject( g_gui ) )
  {
    return
  }

  g_tabs     .Opt( "-Hidden" )
  g_shrinkBtn.Opt( "-Hidden" )
  g_expandBtn.Opt( "Hidden" )

  ; Hiding the tab control also hides its child clip panels.
  ; Re-show the active tab's clip panel so buttons reappear.
  if( IsObject( g_uiTabs ) )
  {
    tabIndex := g_tabs.Value
    for idx, tab in g_uiTabs
    {
      if( idx = tabIndex )
      {
        tab.ShowClipPanel()
      }
      else
      {
        tab.HideClipPanel()
      }
    }
  }

  savedPos := LoadWindowPos()
  g_gui.Show( "w" g_fullW " h" g_fullH " NoActivate" savedPos )
  IniWrite( 0, g_iniPath, "Window", "Collapsed" )
  RedrawScrollbar()
}

IsCollapsed()
{
  global g_iniPath
  return IniRead( g_iniPath, "Window", "Collapsed", "0" ) = "1"
}

ForceRepaint()
{
  global g_gui
  global g_tabs
  global g_uiTabs

  if( !IsSet( g_gui ) || !IsObject( g_gui ) )
  {
    return
  }

  if( IsSet( g_tabs ) && IsObject( g_tabs ) && IsSet( g_uiTabs ) )
  {
    tabIndex := g_tabs.Value
    if( (tabIndex >= 1) && (tabIndex <= g_uiTabs.Length) )
    {
      tab := g_uiTabs[tabIndex]
      tab.ShowClipPanel()
      tab.ApplyScrollPosition()
    }
  }

  RedrawScrollbar()
}

Close()
{
  global g_gui
  global g_guiHwndRaw
  global g_tabScrollHwnd
  global g_activeWindow
  global g_wheelPendingSteps
  global g_wheelFlushScheduled
  global g_shrinkBtn
  global g_expandBtn

  g_activeWindow  := unset
  g_tabScrollHwnd := 0
  g_shrinkBtn     := ""
  g_expandBtn     := ""

  SaveWindowPos()
  OnMessage( 0x0003, OnWindowMove,   0 )
  OnMessage( 0x0216, OnWindowMoving, 0 )
  OnMessage( 0x0115, VScroll,        0 )
  OnMessage( 0x0201, OnLButtonDown,  0 )
  OnMessage( 0x0205, OnRButtonUp,    0 )
  RemoveWheelHook()

  g_guiHwndRaw := 0
  g_wheelPendingSteps   := 0
  g_wheelFlushScheduled := false

  SetTimer( HoverCheck,        0 )
  SetTimer( TrackActiveWindow, 0 )
  ToolTip()
  if g_gui
  {
    g_gui.Destroy()
  }
  g_gui := ""
}

HoverCheck()
{
  global g_tipMap
  static prevHwnd := 0
  MouseGetPos( , , &winHwnd, &ctrlHwnd, 2 )
  if g_tipMap.Has( ctrlHwnd )
  {
    if ctrlHwnd != prevHwnd
    {
      ToolTip( g_tipMap[ctrlHwnd] )
      prevHwnd := ctrlHwnd
    }
  }
  else if prevHwnd != 0
  {
    ToolTip()
    prevHwnd := 0
  }
}

TrackActiveWindow()
{
  global g_activeWindow
  global g_gui

  hwnd := WinActive( "A" )
  if( hwnd = 0 )
  {
    return
  }

  if( IsObject( g_gui ) && (hwnd = g_gui.Hwnd) )
  {
    return
  }

  g_activeWindow := hwnd
}

; ── Window position persistence ─────────────────────────────────

LoadWindowPos()
{
  global g_wndX
  global g_wndY
  global g_favX
  global g_favY
  global g_iniPath

  g_wndX := IniRead( g_iniPath, "Window", "X", "0" )
  g_wndY := IniRead( g_iniPath, "Window", "Y", "0" )
  g_favX := IniRead( g_iniPath, "Window", "FavX", "" )
  g_favY := IniRead( g_iniPath, "Window", "FavY", "" )
  if( g_wndX = "" || g_wndY = "" )
  {
    return ""
  }
  return "x" g_wndX " y" g_wndY
}

SaveWindowPos()
{
  global g_gui
  global g_wndX
  global g_wndY
  global g_iniPath

  if( !IsObject( g_gui ) )
  {
    return
  }
  WinGetPos( &g_wndX, &g_wndY, , , g_gui )
  IniWrite( g_wndX, g_iniPath, "Window", "X" )
  IniWrite( g_wndY, g_iniPath, "Window", "Y" )
}

SetFavouriteSpot()
{
  global g_gui
  global g_favX
  global g_favY
  global g_iniPath

  if( !IsObject( g_gui ) )
  {
    return
  }

  WinGetPos( &x, &y, , , g_gui )
  g_favX := x
  g_favY := y
  IniWrite( g_favX, g_iniPath, "Window", "FavX" )
  IniWrite( g_favY, g_iniPath, "Window", "FavY" )
}

MoveToFavouriteSpot()
{
  global g_gui
  global g_favX
  global g_favY

  if( !IsObject( g_gui ) )
  {
    return
  }
  if( g_favX = "" || g_favY = "" )
  {
    return
  }
  WinMove( Integer( g_favX ), Integer( g_favY ), , , g_gui )
  SaveWindowPos()
}
