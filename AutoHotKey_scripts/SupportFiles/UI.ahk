
ShowWindow()
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
  global g_fullW
  global g_fullH
  global g_frmSize
  global g_toggleSizeBtn
  global g_clipIndicator
  global g_stripEmojisIndicator
  global g_stripSendEmojis

  g_activeWindow := WinActive( "A" )

  windowTitle := "Henks Hotkeys"
  if WinExist( windowTitle )
  {
    WinActivate
    return
  }

  g_gui := Gui( "+AlwaysOnTop +ToolWindow -Caption -Resize -MinimizeBox -MaximizeBox", windowTitle )

  INI_SetWndOpen( true )

  g_frmSize := 8

  ; Add WS_CLIPCHILDREN so the GUI doesn't paint over child button areas during
  ; scroll, and WS_THICKFRAME so the OS will actually let us resize the window
  ; via WM_NCHITTEST. With -Caption, WS_THICKFRAME would normally add a small
  ; visible sizing border; we suppress that frame entirely in OnNcCalcSize so
  ; the client area stays flush with the window edges.
  AddWindowStyle( g_gui.Hwnd, WS_CLIPCHILDREN | WS_THICKFRAME, true )

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

  tabScrlW := 18
  tabLeft  := 5  + g_frmSize
  tabTop   := 24 + g_frmSize
  tabWth   := tabContentWidth  + tabScrlW + 14
  tabHgt   := tabContentHeight + 30

  g_tabs := g_gui.AddTab3( "x" tabLeft " y" tabTop " w" tabWth " h" tabHgt, tabList )

  ; WS_CLIPSIBLINGS: prevents the tab control from painting over sibling windows
  ; (the utility buttons) that sit above it in z-order.  Set once here so we
  ; never need to fiddle with z-order during shrink/expand.
  AddWindowStyle( g_tabs.Hwnd, WS_CLIPSIBLINGS, false )
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

  tabScrollX := dispGuiRight - tabScrlW
  tabScrollY := dispGuiTop
  tabScrollH := dispGuiBottom - dispGuiTop

  ; Cache the scrollbar's GUI-client X for use by RelayoutForHeight.
  global g_tabScrollX
  g_tabScrollX := tabScrollX

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
                              "Int",  tabScrlW,
                              "Int",  tabScrollH,
                              "Ptr",  g_gui.Hwnd,
                              "Ptr",  0,
                              "Ptr",  0,
                              "Ptr",  0,
                              "Ptr" )

  ; Force classic scrollbar appearance so it stays visible instead of auto-hiding.
  DllCall( "uxtheme\SetWindowTheme", "Ptr", g_tabScrollHwnd, "Str", "", "Str", "" )

  OnMessage( 0x0115, VScroll             )  ; WM_VSCROLL
  OnMessage( 0x0111, OnButtonDoubleClick )  ; WM_COMMAND       (for BN_DBLCLK)
  OnMessage( 0x0083, OnNcCalcSize        )  ; WM_NCCALCSIZE    (suppress frame)
  OnMessage( 0x0084, OnNcHitTest         )  ; WM_NCHITTEST     (vertical-only resize hit-test)
  OnMessage( 0x0024, OnGetMinMaxInfo     )  ; WM_GETMINMAXINFO (height bounds)
  OnMessage( 0x0005, OnWindowSize        )  ; WM_SIZE          (relayout on height change)

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

  startTab     := INI_LastTab()
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
  btnTop    := g_frmSize
  btnGap    := 2
  btnWth    := 40
  btnHgt    := 24
  rightEdge := tabContentWidth + tabScrlW + 16 + g_frmSize

  CreateButton( "⌫.", "Back 3, Replace with period",
                "Segoe UI Symbol", "s10",
                rightEdge - BtnPos( 2, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                (*) => DoSendInput( "`b`b`b. " ) )

  CreateBtnWithStyle( "⇚,", "Back 3, Insert Comma",
                      "Segoe UI Symbol", "s16",
                      0x0F00, 0x0800, ; BS_BOTTOM (0x0800) — push baseline up so the tall glyph isn't clipped.
                      rightEdge - BtnPos( 1, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                      (*) => DoSendInput( "{Left}{Left}{Left}, " ) )

  CreateButton( "↩", "Enter / Newline",
                "Segoe UI Symbol", "s14",
                rightEdge - BtnPos( 0, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                (*) => DoSendInput( "{Enter}" ) )

; 🔄⏎
  CreateButton( "", "Repaint / Refresh",
                "Segoe UI Symbol", "s10",
                rightEdge - BtnPos( 5, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                (*) => ForceRepaint() )

  g_toggleSizeBtn := CreateButton( "▼", "Shrink window",
                                   "Segoe UI Symbol", "s14",
                                   25, btnTop, btnWth, btnHgt,
                                   (*) => ToggleWindowSize() )

  ; Clipboard-mode indicator (click to toggle). Always visible to the left of
  ; the shrink/expand buttons so it's easy to see in either mode.
  clipW := 12
  clipH := 14
  clipX := 2 + g_frmSize
  clipY := 1 + g_frmSize
  g_gui.SetFont( "s10", "Segoe UI Symbol" )
  g_clipIndicator := g_gui.AddText( "x" clipX " y" clipY " w" clipW " h" clipH " +0x100", "○" )
  g_tipMap[g_clipIndicator.Hwnd] := "Clipboard send mode: OFF"
  g_gui.SetFont( g_fontSize " norm", g_fontName )
  g_useClipSend := INI_IsClipSendMode()
  SetShowClipBulletState( g_useClipSend )

  ; Strip-emojis-from-comments indicator (click to toggle): sits immediately
  ; to the right of the shrink/expand button, mirroring the clipboard-mode
  ; indicator on the left. State is persisted to INI and only affects the
  ; Comments tab via CommentsTabPage.TransformSendText.
  stripW := 14
  stripH := 16
  stripX := 14 + btnWth + 2 + g_frmSize
  stripY := 1 + g_frmSize
  g_gui.SetFont( "s10", "Segoe UI Symbol" )
  g_stripEmojisIndicator := g_gui.AddText( "x" stripX " y" stripY " w" stripW " h" stripH " +0x100", "☺" )
  g_tipMap[g_stripEmojisIndicator.Hwnd] := "Strip emojis from comments: OFF"
  g_gui.SetFont( g_fontSize " norm", g_fontName )
  g_stripSendEmojis := INI_IsStripCommentEmojis()
  SetStripEmojisIndicatorState( g_stripSendEmojis )

  ; Explicit window size based on tab control dimensions.
  ; Prevents AHK from auto-sizing to include hidden buttons at large Y offsets.
  showW   := tabContentWidth + tabScrlW + 14 + 14
  showH   := tabContentHeight + 30 + 14 + 14
  g_fullW := showW

  ; Apply the user's saved height (from a previous resize). Falls back to the
  ; computed default if no persisted value or the value is out of range.
  savedH := INI_WndHeight()
  if( savedH != "" && savedH >= 140 && savedH <= A_ScreenHeight )
  {
    g_fullH := Integer( savedH )
  }
  else
  {
    g_fullH := showH
  }
  ;savedPos := LoadWindowPos()
  ;g_gui.Show( "w" showW " h" showH " " savedPos )
  ;RedrawScrollbar()

  ToggleWindowSize( IsCollapsed() )

  OnMessage( 0x0003, OnWindowMove   )  ; WM_MOVE
  OnMessage( 0x0216, OnWindowMoving )  ; WM_MOVING

  ; Restore collapsed state last, after everything is laid out.
  ToggleWindowSize( INI_IsCollapsed() )
}



; WM_COMMAND handler that fires for any button with BS_NOTIFY set (we apply
; that style only to tab-page symbol buttons via EnableButtonDoubleClick).
; On a double-click the OS sends BN_CLICKED first (so the normal Click handler
; runs and the symbol's text is sent through DoSendText as usual) and then
; BN_DBLCLK on the second click. We can't send the newline directly here:
; the Click handler sleeps briefly before sending, so BN_DBLCLK arrives while
; that send is still pending and the Enter would race ahead. Instead we just
; raise a flag — SymbolClick sends the newline after action.Call() returns.
OnButtonDoubleClick( wParam, lParam, msg, hwnd )
{
  static BN_DBLCLK := 5

  notifyCode := (wParam >> 16) & 0xFFFF
  if( notifyCode != BN_DBLCLK )
  {
    return
  }

  global g_pendingNewline
  g_pendingNewline := true
}



; WM_NCCALCSIZE handler. WS_THICKFRAME normally inserts a small sizing border
; around the whole window. Returning 0 with wParam=TRUE tells the OS that the
; proposed window rect is the new client rect verbatim — i.e. no non-client
; area at all — so the visual layout stays exactly as it was before we added
; WS_THICKFRAME, while the OS still treats the window as resizable.
OnNcCalcSize( wParam, lParam, msg, hwnd )
{
  global g_gui
  if( !IsObject( g_gui ) || hwnd != g_gui.Hwnd )
  {
    return
  }
  return 0
}

; WM_NCHITTEST handler. Default hit-testing on a window with WS_THICKFRAME
; returns HTTOPLEFT / HTLEFT / HTRIGHT / etc. near the edges, which would
; show horizontal/diagonal resize cursors. We override every hit-test for
; this window: only the top and bottom edge zones return resize codes; all
; other positions are reported as HTCLIENT, which keeps the cursor as the
; normal arrow and prevents the OS from initiating a horizontal resize.
OnNcHitTest( wParam, lParam, msg, hwnd )
{
  static RESIZE_ZONE := 6
  static HTCLIENT    := 1
  static HTTOP       := 12
  static HTBOTTOM    := 15

  global g_gui
  if( !IsObject( g_gui ) || hwnd != g_gui.Hwnd )
  {
    return
  }
  if( INI_IsCollapsed() )
  {
    return HTCLIENT
  }

  ; lParam packs cursor screen coords as two signed 16-bit ints.
  cx := lParam & 0xFFFF
  if( cx & 0x8000 )
  {
    cx -= 0x10000
  }
  cy := (lParam >> 16) & 0xFFFF
  if( cy & 0x8000 )
  {
    cy -= 0x10000
  }

  rect := Buffer( 16, 0 )
  DllCall( "GetWindowRect", "Ptr", hwnd, "Ptr", rect )
  top    := NumGet( rect, 4,  "Int" )
  bottom := NumGet( rect, 12, "Int" )

  if( cy - top < RESIZE_ZONE )
  {
    return HTTOP
  }
  if( bottom - cy < RESIZE_ZONE )
  {
    return HTBOTTOM
  }
  return HTCLIENT
}

; WM_GETMINMAXINFO handler. Clamp the window to a sensible vertical range
; so the user can't drag-resize it to nothing or beyond the screen. Width is
; also clamped to the current physical width so a horizontal drag-resize is
; rejected by the OS even if hit-testing somehow let one through. The clamp
; is bypassed while ToggleWindowSize is calling Show() so collapse/expand
; can change the width.
OnGetMinMaxInfo( wParam, lParam, msg, hwnd )
{
  global g_gui
  global g_allowWidthChange
  if( !IsObject( g_gui ) || hwnd != g_gui.Hwnd )
  {
    return
  }
  if( INI_IsCollapsed() )
  {
    return
  }
  if( g_allowWidthChange )
  {
    return
  }

  static MIN_HEIGHT := 140

  ; Lock width: read current physical-pixel width and use it for both min and
  ; max track sizes so any horizontal change is rejected by the OS even if
  ; some future code path were to request one.
  rect := Buffer( 16, 0 )
  DllCall( "GetWindowRect", "Ptr", hwnd, "Ptr", rect )
  curWidth := NumGet( rect, 8, "Int" ) - NumGet( rect, 0, "Int" )

  ; MINMAXINFO layout (offsets in bytes):
  ;    0  POINT ptReserved
  ;    8  POINT ptMaxSize
  ;   16  POINT ptMaxPosition
  ;   24  POINT ptMinTrackSize
  ;   32  POINT ptMaxTrackSize
  NumPut( "Int", curWidth,         lParam, 24 )  ; ptMinTrackSize.x
  NumPut( "Int", MIN_HEIGHT,       lParam, 28 )  ; ptMinTrackSize.y
  NumPut( "Int", curWidth,         lParam, 32 )  ; ptMaxTrackSize.x
  NumPut( "Int", A_ScreenHeight,   lParam, 36 )  ; ptMaxTrackSize.y
  return 0
}

; WM_SIZE handler. Reflow the tab control, scrollbar and clip panels so the
; new height is actually used. The collapsed state is skipped because the
; tab control is hidden then and its size must stay at its full configured
; value so the next expand looks right.
OnWindowSize( wParam, lParam, msg, hwnd )
{
  global g_gui
  if( !IsObject( g_gui ) || hwnd != g_gui.Hwnd )
  {
    return
  }
  if( INI_IsCollapsed() )
  {
    return
  }

  static SIZE_MINIMIZED := 1
  if( wParam = SIZE_MINIMIZED )
  {
    return
  }

  RelayoutForHeight()

  ; Persist the new height (debounced) so it's restored next time the script
  ; runs. SaveWindowHeight reads g_fullH that RelayoutForHeight just updated.
  SetTimer( SaveWindowHeight, -500 )
}



OnLButtonDown( wParam, lParam, msg, hwnd )
{
  global g_gui
  global g_dragOffsetX
  global g_dragOffsetY

  if( hwnd != g_gui.Hwnd )
  {
    if( IsClipControl( hwnd ) )
    {
      ToggleClipboardSendMode()
    }
    else if( IsStripEmojisControl( hwnd ) )
    {
      ToggleStripSendEmojis()
    }
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
    if( !IsClipControl(        hwnd ) &&
        !IsToggleSizeBtn(      hwnd ) &&
        !IsStripEmojisControl( hwnd ) )
    {
      return
    }
  }

  contextMenu := Menu()
  FillTrayMenu( contextMenu )
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
    if( !IsClipControl(        hwnd ) &&
        !IsStripEmojisControl( hwnd ) )
    {
      return
    }
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

; Position the utility controls (indicators and toggle button) accounting for frame offset.
; includeFrame: true to add g_frmSize offset, false for no offset
AdjustControlPositions( includeFrame )
{
  global g_toggleSizeBtn
  global g_clipIndicator
  global g_stripEmojisIndicator
  global g_frmSize

  ; Get control dimensions
  g_clipIndicator.GetPos( , , &clipW, &clipH )
  g_toggleSizeBtn.GetPos( , , &btnW,  &btnH  )
  g_stripEmojisIndicator.GetPos( , , &stripW, &stripH )

  ; Y positions with frame offset
  yOffset := includeFrame ? g_frmSize : 0
  btnY    := yOffset

  ; Center the indicator controls vertically on the button
  btnCenterY := btnY + btnH / 2         ; 0 + (24/2) = 12
  clipY      := btnCenterY - clipH  / 2 - 3 ; 12 - (14/2) = 5 - 3 = 2 (there is an offset related to the font's character baseline on the statics)
  stripY     := btnCenterY - stripH / 2 - 2 ; 12 - (16/2) = 4 - 2 = 2 (there is an offset related to the font's character baseline on the statics)

  ; X positions: layout left-to-right with 2-pixel gaps
  xOffset    := includeFrame ? g_frmSize : 0
  edgeGap    := 2
  controlGap := 1

  clipX  := xOffset + edgeGap
  btnX   := clipX + clipW + controlGap
  stripX := btnX + btnW + controlGap

  g_clipIndicator.Move( clipX, clipY )
  g_toggleSizeBtn.Move( btnX,  btnY  )
  g_stripEmojisIndicator.Move( stripX, stripY )
}

; Single entry point for collapsing/expanding the window.
; Pass true to collapse, false to expand, or omit to flip the current state.
ToggleWindowSize( collapse := "" )
{
  global g_gui
  global g_tabs
  global g_uiTabs
  global g_fullW
  global g_fullH
  global g_allowWidthChange
  global g_frmSize
  global g_toggleSizeBtn
  global g_clipIndicator
  global g_stripEmojisIndicator

  if( !IsObject( g_gui ) )
  {
    return
  }

  if( collapse = "" )
  {
    collapse := !INI_IsCollapsed()
  }

  ; Allow OnGetMinMaxInfo to skip its width clamp for the duration of the
  ; Show() call — we are intentionally changing the width here.
  g_allowWidthChange := true
  try
  {
    if( collapse )
    {
      g_tabs.Opt( "Hidden" )
      SetToggleSizeBtnState( true )

      ; Remove WS_THICKFRAME to get the tool border back
      RemoveWindowStyle( g_gui.Hwnd, WS_THICKFRAME, true )

      ; Adjust control positions to remove frame offset when collapsed
      AdjustControlPositions( false )


      ; Update INI before Show() so the WM_SIZE handler sees the new state and
      ; can correctly skip the relayout while collapsing.
      INI_SetCollapsed( true )
      savedPos := LoadWindowPos()
      width    := 72
      height   := 24
      g_gui.Show( "w" width " h" height " NoActivate" savedPos )
    }
    else
    {
      g_tabs.Opt( "-Hidden" )
      SetToggleSizeBtnState( false )

      ; Re-add WS_THICKFRAME for the frame border
      AddWindowStyle( g_gui.Hwnd, WS_THICKFRAME, true )

      ; Restore control positions with frame offset when expanding
      AdjustControlPositions( true )

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

      ; Update INI before Show() so the WM_SIZE handler sees the expanded state
      ; and runs the relayout instead of skipping it.
      INI_SetCollapsed( false )
      savedPos := LoadWindowPos()
      g_gui.Show( "w" g_fullW " h" g_fullH " NoActivate" savedPos )
      RedrawScrollbar()
    }
  }
  finally
  {
    g_allowWidthChange := false
  }
}

IsCollapsed()
{
  return INI_IsCollapsed()
}

; Resize the tab control, scrollbar, and each tab's clip panel to match the
; current GUI client height. Called from OnWindowSize after the user drags
; the top or bottom edge.
RelayoutForHeight()
{
  global g_gui
  global g_tabs
  global g_uiTabs
  global g_tabScrollHwnd
  global g_tabScrollX
  global g_fullH

  if( !IsObject( g_gui ) || !IsObject( g_tabs ) )
  {
    return
  }

  ; Client size is in AHK-logical pixels (DPI-aware). Tab control top stays
  ; at y=24 (room for the helper buttons) and we leave a small bottom margin.
  g_gui.GetClientPos( , , &clientW, &clientH )
  TAB_TOP    := 24
  TAB_MARGIN := 14
  newTabH    := clientH - TAB_TOP - TAB_MARGIN
  if( newTabH < 60 )
  {
    newTabH := 60
  }

  g_tabs.Move( , , , newTabH )

  ; Recompute the tab display rect (physical px) so the scrollbar and clip
  ; panels can be repositioned to match the new tab height.
  displayRect := Buffer( 16, 0 )
  DllCall( "GetClientRect", "Ptr", g_tabs.Hwnd, "Ptr", displayRect.Ptr )
  SendMessage( 0x1328, 0, displayRect.Ptr, g_tabs.Hwnd )  ; TCM_ADJUSTRECT
  dispTop    := NumGet( displayRect, 4,  "Int" )
  dispBottom := NumGet( displayRect, 12, "Int" )

  ; Map dispTop / dispBottom from tab-client coords to GUI-client coords.
  ptTL := Buffer( 8, 0 )
  NumPut( "Int", 0,       ptTL, 0 )
  NumPut( "Int", dispTop, ptTL, 4 )
  DllCall( "ClientToScreen", "Ptr", g_tabs.Hwnd, "Ptr", ptTL.Ptr )
  DllCall( "ScreenToClient", "Ptr", g_gui.Hwnd,  "Ptr", ptTL.Ptr )
  guiTop := NumGet( ptTL, 4, "Int" )

  ptBL := Buffer( 8, 0 )
  NumPut( "Int", 0,          ptBL, 0 )
  NumPut( "Int", dispBottom, ptBL, 4 )
  DllCall( "ClientToScreen", "Ptr", g_tabs.Hwnd, "Ptr", ptBL.Ptr )
  DllCall( "ScreenToClient", "Ptr", g_gui.Hwnd,  "Ptr", ptBL.Ptr )
  guiBottom := NumGet( ptBL, 4, "Int" )

  if( g_tabScrollHwnd )
  {
    scrollRect := Buffer( 16, 0 )
    DllCall( "GetWindowRect", "Ptr", g_tabScrollHwnd, "Ptr", scrollRect )
    scrollW := NumGet( scrollRect, 8, "Int" ) - NumGet( scrollRect, 0, "Int" )

    DllCall( "SetWindowPos",
             "Ptr",  g_tabScrollHwnd, "Ptr", 0,
             "Int",  g_tabScrollX, "Int", guiTop,
             "Int",  scrollW,      "Int", guiBottom - guiTop,
             "UInt", 0x0004 | 0x0010 )  ; SWP_NOZORDER | SWP_NOACTIVATE
  }

  if( IsObject( g_uiTabs ) )
  {
    for tab in g_uiTabs
    {
      tab.ResizeClipPanel( g_gui, g_tabs )
    }
  }

  ; Track the new full height so collapse → expand round-trips honour the
  ; user's chosen size.
  g_gui.GetPos( , , , &winH )
  g_fullH := winH

  UpdateScrollInfo()
  RedrawScrollbar()
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
  global g_toggleSizeBtn

  g_activeWindow  := unset
  g_tabScrollHwnd := 0
  g_toggleSizeBtn := ""
  g_clipIndicator := ""

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

  g_wndX := INI_WndPosX()
  g_wndY := INI_WndPosY()
  g_favX := INI_WndFavX()
  g_favY := INI_WndFavY()
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

  if( !IsObject( g_gui ) )
  {
    return
  }
  WinGetPos( &g_wndX, &g_wndY, , , g_gui )
  INI_SetWndPosX( g_wndX )
  INI_SetWndPosY( g_wndY )
}

SaveWindowHeight()
{
  global g_fullH
  if( !g_fullH )
  {
    return
  }
  INI_SetWndHeight( g_fullH )
}

SetFavouriteSpot()
{
  global g_gui
  global g_favX
  global g_favY

  if( !IsObject( g_gui ) )
  {
    return
  }

  WinGetPos( &x, &y, , , g_gui )
  g_favX := x
  g_favY := y
  INI_SetWndFavX( g_favX )
  INI_SetWndFavY( g_favY )
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

  g_favX := INI_WndFavX()
  g_favY := INI_WndFavY()
  if( g_favX = "" || g_favY = "" )
  {
    return
  }

  WinMove( Integer( g_favX ), Integer( g_favY ), , , g_gui )
  SaveWindowPos()
}
