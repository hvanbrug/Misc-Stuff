; ═══════════════════════════════════════════════════════════════
; Ctrl + Shift + / => Show Help Menu with all available hotkeys
; ═══════════════════════════════════════════════════════════════
^+a::ListHotkeys
^+z::ShowHelpMenu( 1 ) ; Start on the special characters tab
^+x::ShowHelpMenu( 2 ) ; Start on the emojis tab
^+c::ShowHelpMenu( 3 ) ; Start on the comments tab


ShowHelpMenu( startTab )
{
  global g_uiTabs
  global g_activeWindow

  global g_gui
  global g_tipMap
  global g_symbols
  global g_tabNames
  global g_tabs
  global g_tabScrollHwnd
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

  g_activeWindow := WinActive( "A" )

  windowTitle := "Henks Hotkey Reference"
  if WinExist( windowTitle )
  {
    WinActivate
    return
  }

  g_gui := Gui( "+AlwaysOnTop -Resize", windowTitle )

  ; Prevent the GUI from painting its background over child button areas during scroll.
  WS_CLIPCHILDREN := 0x02000000
  guiStyle := DllCall( "GetWindowLong", "ptr", g_gui.Hwnd, "int", -16, "int" )
  DllCall( "SetWindowLong", "ptr", g_gui.Hwnd, "int", -16, "int", guiStyle | WS_CLIPCHILDREN )

  tabList := []
  tabContentWidth  := g_LV_WIDTH
  tabContentHeight := 0
  for tab in g_uiTabs
  {
    tabList.Push( tab.m_name )
    tabContentWidth  := tab.GetContentWidth(  tabContentWidth  )
    tabContentHeight := tab.GetContentHeight( tabContentHeight )
  }

  ; Keep symbol tabs to a bounded viewport so overflow can scroll.
  TAB_VIEWPORT_SCREEN_MARGIN := 220
  TAB_VIEWPORT_MAX_HEIGHT    := 230
  TAB_VIEWPORT_MIN_HEIGHT    := 220

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

  HOTKEYS_TAB := tabList.Length + 1
  tabList.Push( "Hotkey Help" )

  ; Use the larger of symbol area or hotkey list area
  LV_AREA_HEIGHT := g_hdrHeight + (g_LV_ROW_COUNT * 20) + 10
  if LV_AREA_HEIGHT > tabContentHeight
  {
    tabContentHeight := LV_AREA_HEIGHT
  }
  if( tabContentHeight > maxTabContentHeight )
  {
    tabContentHeight := maxTabContentHeight
  }

  TAB_SCROLL_W := 18

  g_tabs := g_gui.AddTab3( "x5 y5 w" (tabContentWidth + TAB_SCROLL_W + 14) " h" (tabContentHeight + 30), tabList )

  ; Use TCM_ADJUSTRECT to get the tab's display area (content region below the tab strip).
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

  OnMessage( 0x0115, HelpMenu_VScroll )  ; WM_VSCROLL

  for tabIndex, tab in g_uiTabs
  {
    tab.SetViewportHeight( tabContentHeight )
    tab.AddControls( g_gui, g_tabs, tabIndex, g_tipMap )
  }

  g_gui.SetFont( g_fontSize " norm", g_fontName )

  SetTimer( HelpMenu_HoverCheck, 100 )
  SetTimer( HelpMenu_TrackActiveWindow, 100 )


  ; Colored header bar using a Progress control as background
  g_tabs.UseTab( HOTKEYS_TAB )
  HEADER_FULL_WIDTH := g_COL_HOTKEY_WIDTH + g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN
  g_HeaderBg := g_gui.AddProgress( "x15 y35 w" HEADER_FULL_WIDTH " h" g_hdrHeight
                                   " Background" g_HEADER_BG_COLOR " c" g_HEADER_BG_COLOR, 100 )

  g_gui.SetFont( g_fontSize " bold", g_fontName )
  g_HeaderHotkey := g_gui.AddText( "x15 y35 w" g_COL_HOTKEY_WIDTH " h" g_hdrHeight
                                   " BackgroundTrans c" g_HEADER_TEXT_COLOR " +0x200",
                                   "  Hotkey" )
  g_HeaderDesc := g_gui.AddText( "x+0 yp w" (g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN) " h" g_hdrHeight
                                 " BackgroundTrans c" g_HEADER_TEXT_COLOR " +0x200",
                                 "  Description" )
  g_gui.SetFont( g_fontSize " norm", g_fontName )

  g_LV := g_gui.AddListView( "x15 y+0 r" g_LV_ROW_COUNT " w" g_LV_WIDTH " Grid -Hdr",
                             ["Hotkey", "Description"] )
  g_LV.ModifyCol( 1, g_COL_HOTKEY_WIDTH )
  g_LV.ModifyCol( 2, g_COL_DESC_WIDTH )

  for item in g_HelpActions
  {
    g_LV.Add( , item.hotkey, item.desc )
  }


  g_LV.OnEvent(   "DoubleClick", HelpMenu_RowAction      )
  g_gui.OnEvent(  "Escape",      (*) => HelpMenu_Close() )
  g_gui.OnEvent(  "Close",       (*) => HelpMenu_Close() )
  g_tabs.OnEvent( "Change",      HelpMenu_TabChanged     )
  OnMessage(      0x020A,        HelpMenu_MouseWheel     ) ; WM_MOUSEWHEEL

  if( (startTab < 1) ||
      (startTab > tabList.Length) )
  {
    startTab := 1
  }
  g_tabs.Value := startTab
  HelpMenu_UpdateScrollInfo()

  ; Bring scrollbar to top of z-order so tab buttons don't steal mouse events from it.
  SWP_NOMOVE := 0x0002, SWP_NOSIZE := 0x0001, SWP_NOACTIVATE := 0x0010
  DllCall( "SetWindowPos", "Ptr", g_tabScrollHwnd, "Ptr", 0,
           "Int", 0, "Int", 0, "Int", 0, "Int", 0,
           "UInt", SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE )

  ; Enter button in top-right corner (outside tab context).
  g_tabs.UseTab( 0 )
  rightEdge := tabContentWidth + TAB_SCROLL_W + 16

  CreateButton( "⏎", "Enter / Newline",
                rightEdge - 40, 0, 40, 24,
                (*) => DoSendText( "`n" ) )

  g_gui.Show()
  HelpMenu_RedrawScrollbar()
}

HelpMenu_RedrawScrollbar()
{
  global g_tabScrollHwnd
  if( !g_tabScrollHwnd )
  {
    return
  }

  ; RDW_INVALIDATE | RDW_FRAME | RDW_ERASE | RDW_UPDATENOW = 0x0109
  DllCall( "RedrawWindow", "Ptr", g_tabScrollHwnd, "Ptr", 0, "Ptr", 0, "UInt", 0x0109 )
}

HelpMenu_Close()
{
  global g_gui
  global g_tabScrollHwnd
  global g_activeWindow
  g_activeWindow  := unset
  g_tabScrollHwnd := 0
  OnMessage( 0x0115, HelpMenu_VScroll,    0 )
  OnMessage( 0x020A, HelpMenu_MouseWheel, 0 )
  SetTimer( HelpMenu_HoverCheck, 0 )
  SetTimer( HelpMenu_TrackActiveWindow, 0 )
  ToolTip()
  if g_gui
  {
    g_gui.Destroy()
  }
  g_gui := ""
}

HelpMenu_TabChanged( ctrl, * )
{
  global g_uiTabs
  global g_tabScrollHwnd

  tabIndex := ctrl.Value
  if( tabIndex <= g_uiTabs.Length )
  {
    g_uiTabs[tabIndex].FlushScrollNow()
  }

  HelpMenu_UpdateScrollInfo()

  ; Re-raise scrollbar above tab content that was just repainted.
  ; Use a short deferred timer because the tab control repaints asynchronously
  ; after the Change event, which can bury the scrollbar again.
  if( g_tabScrollHwnd )
  {
    SetTimer( HelpMenu_DeferredScrollbarRaise, -30 )
  }
}

HelpMenu_DeferredScrollbarRaise()
{
  global g_tabScrollHwnd
  if( !g_tabScrollHwnd )
  {
    return
  }

  SWP_NOMOVE := 0x0002, SWP_NOSIZE := 0x0001, SWP_NOACTIVATE := 0x0010
  DllCall( "SetWindowPos", "Ptr", g_tabScrollHwnd, "Ptr", 0,
           "Int", 0, "Int", 0, "Int", 0, "Int", 0,
           "UInt", SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE )
  HelpMenu_RedrawScrollbar()
}

HelpMenu_VScroll( wParam, lParam, msg, hwnd )
{
  global g_tabs
  global g_uiTabs
  global g_tabScrollHwnd

  if( !g_tabScrollHwnd || (lParam != g_tabScrollHwnd) )
  {
    return
  }

  if( !IsSet( g_tabs ) || !IsObject( g_tabs ) )
  {
    return
  }

  tabIndex := g_tabs.Value
  if( (tabIndex < 1) ||
      (tabIndex > g_uiTabs.Length) )
  {
    return
  }

  tab   := g_uiTabs[tabIndex]
  nCode := wParam & 0xFFFF

  SB_LINEUP        := 0
  SB_LINEDOWN      := 1
  SB_PAGEUP        := 2
  SB_PAGEDOWN      := 3
  SB_THUMBPOSITION := 4
  SB_THUMBTRACK    := 5
  SB_TOP           := 6
  SB_BOTTOM        := 7
  SB_ENDSCROLL     := 8

  LINE_PIXELS := 20

  if( nCode = SB_LINEUP )
  {
    tab.SetScrollY( tab.GetScrollTargetY() - LINE_PIXELS )
  }
  else if( nCode = SB_LINEDOWN )
  {
    tab.SetScrollY( tab.GetScrollTargetY() + LINE_PIXELS )
  }
  else if( nCode = SB_PAGEUP )
  {
    tab.SetScrollY( tab.GetScrollTargetY() - tab.GetViewportHeight() )
  }
  else if( nCode = SB_PAGEDOWN )
  {
    tab.SetScrollY( tab.GetScrollTargetY() + tab.GetViewportHeight() )
  }
  else if( (nCode = SB_THUMBTRACK) || (nCode = SB_THUMBPOSITION) )
  {
    ; Use GetScrollInfo for precise track position (avoids 16-bit HIWORD truncation).
    SIF_TRACKPOS := 0x0010
    si := Buffer( 28, 0 )
    NumPut( "UInt", 28,           si, 0 )
    NumPut( "UInt", SIF_TRACKPOS, si, 4 )
    DllCall( "GetScrollInfo", "Ptr", g_tabScrollHwnd, "Int", 2, "Ptr", si.Ptr )  ; SB_CTL = 2
    trackPos := NumGet( si, 24, "Int" )
    tab.SetScrollY( trackPos )
  }
  else if( nCode = SB_TOP )
  {
    tab.SetScrollY( 0 )
  }
  else if( nCode = SB_BOTTOM )
  {
    tab.SetScrollY( tab.MaxScrollY() )
  }
  else if( nCode = SB_ENDSCROLL )
  {
    return
  }

  HelpMenu_UpdateScrollInfo()
  ToolTip()
  return 0
}

HelpMenu_UpdateScrollInfo()
{
  global g_tabs
  global g_uiTabs
  global g_tabScrollHwnd

  if( !g_tabScrollHwnd )
  {
    return
  }

  ; SIF_RANGE|SIF_PAGE|SIF_POS|SIF_DISABLENOSCROLL
  fMask     := 0x000F
  maxScroll := 0
  viewH     := 1
  scrollY   := 0
  hasScroll := false

  if( IsSet( g_tabs ) && IsObject( g_tabs ) && IsSet( g_uiTabs ) )
  {
    tabIndex := g_tabs.Value
    if( (tabIndex >= 1) && (tabIndex <= g_uiTabs.Length) )
    {
      tab       := g_uiTabs[tabIndex]
      maxScroll := tab.MaxScrollY()
      viewH     := Max( 1, tab.GetViewportHeight() )
      scrollY   := tab.GetScrollTargetY()
      hasScroll := (maxScroll > 0)
    }
  }

  si := Buffer( 28, 0 )
  NumPut( "UInt", 28,     si, 0  )  ; cbSize
  NumPut( "UInt", fMask,  si, 4  )  ; fMask

  if( hasScroll )
  {
    contentH := maxScroll + viewH
    NumPut( "Int",  0,        si, 8  )  ; nMin
    NumPut( "Int",  contentH, si, 12 )  ; nMax
    NumPut( "UInt", viewH,    si, 16 )  ; nPage  (sets proportional thumb size)
    NumPut( "Int",  scrollY,  si, 20 )  ; nPos
  }
  else
  {
    NumPut( "Int",  0, si, 8  )  ; nMin
    NumPut( "Int",  0, si, 12 )  ; nMax = nMin → scrollbar visually disabled
    NumPut( "UInt", 1, si, 16 )  ; nPage
    NumPut( "Int",  0, si, 20 )  ; nPos
  }

  ; SB_CTL = 2 (required for standalone scrollbar controls)
  DllCall( "SetScrollInfo", "Ptr", g_tabScrollHwnd, "Int", 2, "Ptr", si.Ptr, "Int", true )
  HelpMenu_RedrawScrollbar()
}

HelpMenu_MouseWheel( wParam, lParam, msg, hwnd )
{
  global g_gui
  global g_tabs
  global g_uiTabs

  ; Acceleration config
  WHEEL_DELTA              := 120
  SCROLL_PIXELS_BASE       := 4    ; pixels per notch at rest
  SCROLL_PIXELS_MAX        := 40   ; pixels per notch at full speed
  ACCEL_WINDOW_MS          := 150  ; consecutive event window to build speed
  ACCEL_STEP               := 4    ; extra pixels added per successive notch within window

  ; Acceleration state
  static lastEventMs   := 0
  static accelPixels   := 0

  if( !IsSet( g_gui  ) || !IsObject( g_gui  ) ||
      !IsSet( g_tabs ) || !IsObject( g_tabs ) )
  {
    return
  }

  MouseGetPos( , , &winHwnd )
  if( winHwnd != g_gui.Hwnd )
  {
    return 0
  }

  tabIndex := g_tabs.Value
  if( (tabIndex < 1) ||
      (tabIndex > g_uiTabs.Length) )
  {
    return 0
  }

  ; Update acceleration based on elapsed time since last event.
  nowMs := A_TickCount
  if( (nowMs - lastEventMs) <= ACCEL_WINDOW_MS )
  {
    accelPixels := Min( accelPixels + ACCEL_STEP, SCROLL_PIXELS_MAX - SCROLL_PIXELS_BASE )
  }
  else
  {
    accelPixels := 0
  }
  lastEventMs := nowMs

  tab   := g_uiTabs[tabIndex]
  delta := (wParam >> 16) & 0xFFFF
  if( delta >= 0x8000 )
  {
    delta -= 0x10000
  }

  pixelsPerNotch := SCROLL_PIXELS_BASE + accelPixels
  scrollBy       := -(delta / WHEEL_DELTA) * pixelsPerNotch
  if( tab.ScrollByPixels( scrollBy ) )
  {
    HelpMenu_UpdateScrollInfo()
    ToolTip()
  }
  return 0
}

HelpMenu_HoverCheck()
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

HelpMenu_TrackActiveWindow()
{
  global g_activeWindow
  global g_gui

  hwnd := WinActive( "A" )
  if( hwnd = 0 )
  {
    return
  }

  if( IsObject( g_gui ) && hwnd = g_gui.Hwnd )
  {
    return
  }

  g_activeWindow := hwnd
}

HelpMenu_SymbolClick( action, ctrl, * )
{
  HelpMenu_Close()
  Sleep( 150 )
  action.Call()
}

HelpMenu_RowAction( ctrl, rowNum )
{
  global g_HelpActions
  if rowNum = 0
  {
    return
  }
  entry := g_HelpActions[rowNum]
  if entry.action = ""
  {
    MsgBox( "This action can only be triggered via its hotkey.",
            "Not available from menu",
            "Icon!" )
    return
  }
  HelpMenu_Close()
  Sleep( 150 )
  entry.action.Call()
}
