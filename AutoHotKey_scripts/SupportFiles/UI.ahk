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
  TAB_VIEWPORT_SCREEN_MARGIN := 420
  TAB_VIEWPORT_MAX_HEIGHT    := 430
  TAB_VIEWPORT_MIN_HEIGHT    := 420

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

  g_tabs := g_gui.AddTab3( "x5 y5 w" (tabContentWidth + 10) " h" (tabContentHeight + 30), tabList )

  for tabIndex, tab in g_uiTabs
  {
    tab.SetViewportHeight( tabContentHeight )
    tab.AddControls( g_gui, g_tabs, tabIndex, g_tipMap )
  }

  g_gui.SetFont( g_fontSize " norm", g_fontName )

  SetTimer( HelpMenu_HoverCheck, 100 )


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

  g_gui.Show()
}

HelpMenu_Close()
{
  global g_gui
  global g_activeWindow
  g_activeWindow := unset
  OnMessage( 0x020A, HelpMenu_MouseWheel, 0 )
  SetTimer( HelpMenu_HoverCheck, 0 )
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

  tabIndex := ctrl.Value
  if( tabIndex <= g_uiTabs.Length )
  {
    g_uiTabs[tabIndex].ApplyScrollPosition()
  }
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
    return
  }

  tabIndex := g_tabs.Value
  if( (tabIndex < 1) ||
      (tabIndex > g_uiTabs.Length) )
  {
    return
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
    ToolTip()
    return 0
  }
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
