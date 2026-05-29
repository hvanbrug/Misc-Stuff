
RedrawScrollbar()
{
  global g_tabScrollHwnd
  if( !g_tabScrollHwnd )
  {
    return
  }

  ; RDW_INVALIDATE | RDW_FRAME | RDW_ERASE | RDW_UPDATENOW = 0x0109
  DllCall( "RedrawWindow",
           "Ptr",  g_tabScrollHwnd,
           "Ptr",  0,
           "Ptr",  0,
           "UInt", 0x0109 )
}

TabChanged( ctrl, * )
{
  global g_uiTabs
  global g_tabScrollHwnd

  tabIndex := ctrl.Value
  INI_SetLastTab( tabIndex )

  ; Show only the active tab's clip panel; hide all others.
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

  if( tabIndex <= g_uiTabs.Length )
  {
    g_uiTabs[tabIndex].FlushScrollNow()
  }

  UpdateScrollInfo()

  ; Re-raise scrollbar above tab content that was just repainted.
  ; Use a short deferred timer because the tab control repaints asynchronously
  ; after the Change event, which can bury the scrollbar again.
  if( g_tabScrollHwnd )
  {
    SetTimer( DeferredScrollbarRaise, -30 )
  }
}

DeferredScrollbarRaise()
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
  RedrawScrollbar()
}

; Lightweight z-order raise without forced redraw.
; Used during wheel scrolling to avoid flash loops.
ScrollbarRaiseOnly()
{
  global g_tabScrollHwnd
  if( !g_tabScrollHwnd )
  {
    return
  }

  SWP_NOMOVE := 0x0002, SWP_NOSIZE := 0x0001, SWP_NOACTIVATE := 0x0010, SWP_NOREDRAW := 0x0008
  DllCall( "SetWindowPos", "Ptr", g_tabScrollHwnd, "Ptr", 0,
           "Int", 0, "Int", 0, "Int", 0, "Int", 0,
           "UInt", SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOREDRAW )
}

VScroll( wParam, lParam, msg, hwnd )
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

  UpdateScrollInfo()
  ToolTip()
  return 0
}

UpdateScrollInfo()
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
  RedrawScrollbar()
}

QueueWheel( stepDelta )
{
  global g_wheelPendingSteps
  global g_wheelFlushScheduled

  g_wheelPendingSteps += stepDelta
  if( g_wheelFlushScheduled )
  {
    return
  }

  g_wheelFlushScheduled := true
  ; Run scroll work outside the hotkey hook callback to avoid hook timeouts.
  SetTimer( FlushQueuedWheel, -1 )
}

FlushQueuedWheel()
{
  global g_wheelPendingSteps
  global g_wheelFlushScheduled

  stepDelta := g_wheelPendingSteps
  g_wheelPendingSteps := 0
  g_wheelFlushScheduled := false
  if( stepDelta = 0 )
  {
    return
  }

  DoWheel( stepDelta )
}

InstallWheelHook()
{
  global g_mouseWheelHook
  global g_mouseWheelHookProc

  if( g_mouseWheelHook )
  {
    return
  }

  if( !g_mouseWheelHookProc )
  {
    g_mouseWheelHookProc := CallbackCreate( LowLevelMouseProc, "Fast" )
  }

  WH_MOUSE_LL := 14
  hModule := DllCall( "GetModuleHandle", "Ptr", 0, "Ptr" )
  g_mouseWheelHook := DllCall( "SetWindowsHookEx",
                               "Int",  WH_MOUSE_LL,
                               "Ptr",  g_mouseWheelHookProc,
                               "Ptr",  hModule,
                               "UInt", 0,
                               "Ptr" )
}

RemoveWheelHook()
{
  global g_mouseWheelHook
  global g_mouseWheelHookProc

  if( g_mouseWheelHook )
  {
    DllCall( "UnhookWindowsHookEx", "Ptr", g_mouseWheelHook )
    g_mouseWheelHook := 0
  }

  if( g_mouseWheelHookProc )
  {
    CallbackFree( g_mouseWheelHookProc )
    g_mouseWheelHookProc := 0
  }
}

LowLevelMouseProc( nCode, wParam, lParam )
{
  global g_guiHwndRaw

  if( nCode < 0 )
  {
    return DllCall( "CallNextHookEx", "Ptr", 0, "Int", nCode, "UPtr", wParam, "UPtr", lParam, "Ptr" )
  }

  WM_MOUSEWHEEL := 0x020A
  if( !g_guiHwndRaw || (wParam != WM_MOUSEWHEEL) )
  {
    return DllCall( "CallNextHookEx", "Ptr", 0, "Int", nCode, "UPtr", wParam, "UPtr", lParam, "Ptr" )
  }

  ; MSLLHOOKSTRUCT starts with POINT {x, y} in screen coords.
  mx := NumGet( lParam, 0, "Int" )
  my := NumGet( lParam, 4, "Int" )

  ; Treat any wheel event inside the GUI window rect as belonging to our UI.
  ; This avoids WindowFromPoint/root resolution edge cases on child controls.
  rect := Buffer( 16, 0 )
  if( !DllCall( "GetWindowRect", "Ptr", g_guiHwndRaw, "Ptr", rect, "Int" ) )
  {
    return DllCall( "CallNextHookEx", "Ptr", 0, "Int", nCode, "UPtr", wParam, "UPtr", lParam, "Ptr" )
  }

  left   := NumGet( rect, 0,  "Int" )
  top    := NumGet( rect, 4,  "Int" )
  right  := NumGet( rect, 8,  "Int" )
  bottom := NumGet( rect, 12, "Int" )
  if( (mx < left) || (mx >= right) || (my < top) || (my >= bottom) )
  {
    return DllCall( "CallNextHookEx", "Ptr", 0, "Int", nCode, "UPtr", wParam, "UPtr", lParam, "Ptr" )
  }

  mouseData := NumGet( lParam, 8, "UInt" )
  delta := (mouseData >> 16) & 0xFFFF
  if( delta >= 0x8000 )
  {
    delta -= 0x10000
  }

  if( delta )
  {
    QueueWheel( -(delta / 120) )
  }

  ; Non-zero return swallows this mouse event system-wide.
  return 1
}

DoWheel( direction )
{
  global g_tabs
  global g_uiTabs
  global g_tabScrollHwnd

  ; Acceleration config
  SCROLL_PIXELS_BASE := 4    ; pixels per notch at rest
  SCROLL_PIXELS_MAX  := 40   ; pixels per notch at full speed
  ACCEL_WINDOW_MS    := 150  ; consecutive event window to build speed
  ACCEL_STEP         := 4    ; extra pixels added per successive notch within window

  ; Acceleration state
  static lastEventMs := 0
  static accelPixels := 0

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

  tab            := g_uiTabs[tabIndex]
  pixelsPerNotch := SCROLL_PIXELS_BASE + accelPixels
  scrollBy       := direction * pixelsPerNotch
  if( tab.ScrollByPixels( scrollBy ) )
  {
    UpdateScrollInfo()
    if( g_tabScrollHwnd )
    {
      ; Re-raise scrollbar z-order without forced redraw to avoid flash loops.
      ScrollbarRaiseOnly()
    }
    ToolTip()
  }
}
