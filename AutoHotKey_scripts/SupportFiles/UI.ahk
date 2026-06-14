; UI.ahk - The always-on-top helper window, implemented as a class.
;
; All window state (gui, tab control, scrollbar, sizes, corner controls,
; snap/drag state) lives on the single global instance g_hotkeyWnd, created
; in UIConstants.ahk. App-wide state (g_uiTabs, g_tipMap, g_useClipSend,
; g_stripSendEmojis, g_pendingNewline, fonts, wheel-hook state) stays global.

class HotkeyWindow
{
  ; ── Window and control state ──
  m_gui                  := ""
  m_guiHwndRaw           := 0
  m_tabs                 := ""
  m_tabScrollHwnd        := 0
  m_fullW                := 0
  m_fullH                := 0
  m_frmSize              := 9
  m_toggleSizeBtn        := ""
  m_clipIndicator        := ""
  m_stripEmojisIndicator := ""

  ; Cached physical-pixel X of the tab scrollbar in GUI client coords. The GUI
  ; width never changes, so this stays constant after Show sets it. Used by
  ; RelayoutForHeight when repositioning the scrollbar after a vertical resize.
  m_tabScrollX := 0

  ; One-shot flag set by ToggleWindowSize around the programmatic Show() that
  ; collapses or expands the window. While true, OnGetMinMaxInfo skips its
  ; width-clamping so the new width can take effect; user-driven resize is
  ; unaffected.
  m_allowWidthChange := false

  ; ── Drag / snap state ──
  m_snappedToTop := false
  m_snappedToFav := false
  m_dragOffsetX  := 0
  m_dragOffsetY  := 0

  ; ── Window position persistence ──
  m_wndX := 0
  m_wndY := 0
  m_favX := ""
  m_favY := ""

  ; ── Bound handlers and timers ──
  ; OnMessage and SetTimer identify a callback by its function object, so each
  ; handler is bound exactly once here and the same object is reused for
  ; registration, deregistration, and timer debouncing.
  m_onButtonDoubleClick     := ""
  m_onNcCalcSize            := ""
  m_onNcHitTest             := ""
  m_onGetMinMaxInfo         := ""
  m_onWindowSize            := ""
  m_onLButtonDown           := ""
  m_onRButtonUp             := ""
  m_onWindowMove            := ""
  m_onWindowMoving          := ""
  m_hoverCheckTimer         := ""
  m_trackActiveWindowTimer  := ""
  m_saveWindowPosTimer      := ""
  m_saveWindowHeightTimer   := ""

  __New()
  {
    this.m_onButtonDoubleClick    := ObjBindMethod( this, "OnButtonDoubleClick" )
    this.m_onNcCalcSize           := ObjBindMethod( this, "OnNcCalcSize"        )
    this.m_onNcHitTest            := ObjBindMethod( this, "OnNcHitTest"         )
    this.m_onGetMinMaxInfo        := ObjBindMethod( this, "OnGetMinMaxInfo"     )
    this.m_onWindowSize           := ObjBindMethod( this, "OnWindowSize"        )
    this.m_onLButtonDown          := ObjBindMethod( this, "OnLButtonDown"       )
    this.m_onRButtonUp            := ObjBindMethod( this, "OnRButtonUp"         )
    this.m_onWindowMove           := ObjBindMethod( this, "OnWindowMove"        )
    this.m_onWindowMoving         := ObjBindMethod( this, "OnWindowMoving"      )
    this.m_hoverCheckTimer        := ObjBindMethod( this, "HoverCheck"          )
    this.m_trackActiveWindowTimer := ObjBindMethod( this, "TrackActiveWindow"   )
    this.m_saveWindowPosTimer     := ObjBindMethod( this, "SaveWindowPos"       )
    this.m_saveWindowHeightTimer  := ObjBindMethod( this, "SaveWindowHeight"    )
  }

  ; ── Lifecycle helpers ────────────────────────────────────────────

  IsCreated()
  {
    return IsObject( this.m_gui )
  }

  Hwnd
  {
    get
    {
      return IsObject( this.m_gui ) ? this.m_gui.Hwnd : 0
    }
  }

  Restore()
  {
    if( !IsObject( this.m_gui ) )
    {
      return
    }
    this.m_gui.Restore()
    WinActivate( "ahk_id " this.m_gui.Hwnd )
  }

  Hide()
  {
    if( !IsObject( this.m_gui ) )
    {
      return
    }
    this.m_gui.Hide()
  }

  ; ── Window construction ──────────────────────────────────────────

  Show()
  {
    global g_uiTabs
    global g_activeWindow
    global g_tipMap
    global g_fontSize
    global g_fontName
    global g_useClipSend
    global g_stripSendEmojis

    g_activeWindow := WinActive( "A" )

    windowTitle := "Henks Hotkeys"
    if WinExist( windowTitle )
    {
      WinActivate
      return
    }

    this.m_gui := Gui( "+AlwaysOnTop +ToolWindow -Caption -Resize -MinimizeBox -MaximizeBox", windowTitle )

    INI_SetWndOpen( true )

    this.m_frmSize := 9

    ; Add WS_CLIPCHILDREN so the GUI doesn't paint over child button areas during
    ; scroll, and WS_THICKFRAME so the OS will actually let us resize the window
    ; via WM_NCHITTEST. With -Caption, WS_THICKFRAME would normally add a small
    ; visible sizing border; we suppress that frame entirely in OnNcCalcSize so
    ; the client area stays flush with the window edges.
    AddWindowStyle( this.m_gui.Hwnd, WS_CLIPCHILDREN | WS_THICKFRAME, true )

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
    tabLeft  := 5  + this.m_frmSize
    tabTop   := 24 + this.m_frmSize
    tabWth   := tabContentWidth  + tabScrlW + 14
    tabHgt   := tabContentHeight + 30

    this.m_tabs := this.m_gui.AddTab3( "x" tabLeft " y" tabTop " w" tabWth " h" tabHgt, tabList )

    ; WS_CLIPSIBLINGS: prevents the tab control from painting over sibling windows
    ; (the utility buttons) that sit above it in z-order.  Set once here so we
    ; never need to fiddle with z-order during shrink/expand.
    AddWindowStyle( this.m_tabs.Hwnd, WS_CLIPSIBLINGS, false )
    ; Start from the tab's client rect, then shrink to the display area.
    displayRect := Buffer( 16, 0 )
    DllCall( "GetClientRect", "Ptr", this.m_tabs.Hwnd, "Ptr", displayRect.Ptr )
    ; TCM_ADJUSTRECT with wParam=FALSE shrinks the client rect to the display area.
    SendMessage( 0x1328, 0, displayRect.Ptr, this.m_tabs.Hwnd )
    dispLeft   := NumGet( displayRect, 0,  "Int" )
    dispTop    := NumGet( displayRect, 4,  "Int" )
    dispRight  := NumGet( displayRect, 8,  "Int" )
    dispBottom := NumGet( displayRect, 12, "Int" )

    ; Map the display-area corners from tab-client coords to GUI-client coords.
    ptTopLeft := Buffer( 8, 0 )
    NumPut( "Int", dispLeft, ptTopLeft, 0 )
    NumPut( "Int", dispTop,  ptTopLeft, 4 )
    DllCall( "ClientToScreen", "Ptr", this.m_tabs.Hwnd, "Ptr", ptTopLeft.Ptr )
    DllCall( "ScreenToClient", "Ptr", this.m_gui.Hwnd,  "Ptr", ptTopLeft.Ptr )

    ptBottomRight := Buffer( 8, 0 )
    NumPut( "Int", dispRight,  ptBottomRight, 0 )
    NumPut( "Int", dispBottom, ptBottomRight, 4 )
    DllCall( "ClientToScreen", "Ptr", this.m_tabs.Hwnd, "Ptr", ptBottomRight.Ptr )
    DllCall( "ScreenToClient", "Ptr", this.m_gui.Hwnd,  "Ptr", ptBottomRight.Ptr )

    dispGuiLeft   := NumGet( ptTopLeft, 0, "Int" )
    dispGuiTop    := NumGet( ptTopLeft, 4, "Int" )
    dispGuiRight  := NumGet( ptBottomRight, 0, "Int" )
    dispGuiBottom := NumGet( ptBottomRight, 4, "Int" )

    tabScrollX := dispGuiRight - tabScrlW
    tabScrollY := dispGuiTop
    tabScrollH := dispGuiBottom - dispGuiTop

    ; Cache the scrollbar's GUI-client X for use by RelayoutForHeight.
    this.m_tabScrollX := tabScrollX

    ; Detach from tab context so the scrollbar is a window-level control visible on all tabs.
    this.m_tabs.UseTab( 0 )

    ; Create a native vertical scroll bar.  WS_VISIBLE|WS_CHILD|SBS_VERT = 0x50000001
    this.m_tabScrollHwnd := DllCall( "CreateWindowEx",
                                     "UInt", 0,
                                     "Str",  "SCROLLBAR",
                                     "Ptr",  0,
                                     "UInt", 0x50000001,
                                     "Int",  tabScrollX,
                                     "Int",  tabScrollY,
                                     "Int",  tabScrlW,
                                     "Int",  tabScrollH,
                                     "Ptr",  this.m_gui.Hwnd,
                                     "Ptr",  0,
                                     "Ptr",  0,
                                     "Ptr",  0,
                                     "Ptr" )

    ; Force classic scrollbar appearance so it stays visible instead of auto-hiding.
    DllCall( "uxtheme\SetWindowTheme", "Ptr", this.m_tabScrollHwnd, "Str", "", "Str", "" )

    OnMessage( 0x0115, VScroll                    )  ; WM_VSCROLL
    OnMessage( 0x0111, this.m_onButtonDoubleClick )  ; WM_COMMAND       (for BN_DBLCLK)
    OnMessage( 0x0083, this.m_onNcCalcSize        )  ; WM_NCCALCSIZE    (suppress frame)
    OnMessage( 0x0084, this.m_onNcHitTest         )  ; WM_NCHITTEST     (vertical-only resize hit-test)
    OnMessage( 0x0024, this.m_onGetMinMaxInfo     )  ; WM_GETMINMAXINFO (height bounds)
    OnMessage( 0x0005, this.m_onWindowSize        )  ; WM_SIZE          (relayout on height change)

    for tabIndex, tab in g_uiTabs
    {
      tab.SetViewportHeight( tabContentHeight )
      tab.AddControls( this.m_gui, this.m_tabs, tabIndex, g_tipMap )
    }

    ; All clip panels start visible (WS_VISIBLE); hide them all for now.
    ; The correct one will be shown after startTab is determined.
    for tabIndex, tab in g_uiTabs
    {
      tab.HideClipPanel()
    }

    this.m_gui.SetFont( g_fontSize " norm", g_fontName )

    SetTimer( this.m_hoverCheckTimer,        100 )
    SetTimer( this.m_trackActiveWindowTimer, 100 )

    OnMessage( 0x201, this.m_onLButtonDown )
    OnMessage( 0x205, this.m_onRButtonUp   )  ; WM_RBUTTONUP
    this.m_gui.OnEvent(  "Escape", (*) => this.Close() )
    this.m_gui.OnEvent(  "Close",  (*) => this.Close() )
    this.m_tabs.OnEvent( "Change", TabChanged           )

    this.m_guiHwndRaw := this.m_gui.Hwnd
    InstallWheelHook()

    startTab          := INI_LastTab()
    this.m_tabs.Value := startTab

    ; Show the clip panel for the active start tab.
    if( (startTab >= 1) && (startTab <= g_uiTabs.Length) )
    {
      g_uiTabs[startTab].ShowClipPanel()
    }

    UpdateScrollInfo()

    ; Bring scrollbar to top of z-order so tab buttons don't steal mouse events from it.
    SWP_NOMOVE := 0x0002, SWP_NOSIZE := 0x0001, SWP_NOACTIVATE := 0x0010
    DllCall( "SetWindowPos", "Ptr", this.m_tabScrollHwnd, "Ptr", 0,
             "Int", 0, "Int", 0, "Int", 0, "Int", 0,
             "UInt", SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE )

    ; Utility buttons in top-right corner (outside tab context).
    this.m_tabs.UseTab( 0 )
    btnTop    := this.m_frmSize
    btnGap    := 2
    btnWth    := 40
    btnHgt    := 24
    rightEdge := tabContentWidth + tabScrlW + 16 + this.m_frmSize

    this.CreateButton( "⌫.", "Back 3, Replace with period",
                       "Segoe UI Symbol", "s10",
                       rightEdge - BtnPos( 3, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                       (*) => DoSendInput( "`b`b`b. " ) )

    this.CreateBtnWithStyle( "⇚,", "Back 3, Insert Comma",
                             "Segoe UI Symbol", "s16",
                             0x0F00, 0x0800, ; BS_BOTTOM (0x0800) — push baseline up so the tall glyph isn't clipped.
                             rightEdge - BtnPos( 2, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                             (*) => DoSendInput( "{Left}{Left}{Left}, " ) )

    this.CreateButton( "↩", "Enter / Newline",
                       "Segoe UI Symbol", "s14",
                       rightEdge - BtnPos( 1, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                       (*) => DoSendInput( "{Enter}" ) )

    this.CreateButton( "🔄", "Repaint / Refresh",
                       "Segoe UI Symbol", "s10",
                       rightEdge - BtnPos( 5, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                       (*) => this.ForceRepaint() )

    this.CreateButton( "▲", "Shrink window",
                       "Segoe UI Symbol", "s14",
                       rightEdge - BtnPos( 0, btnWth, btnGap ), btnTop, btnWth, btnHgt,
                       (*) => this.ToggleWindowSize() )

    this.m_toggleSizeBtn := this.CreateButton( "▲", "Shrink window",
                                               "Segoe UI Symbol", "s14",
                                               25, btnTop, btnWth, btnHgt,
                                               (*) => this.ToggleWindowSize() )

    ; Clipboard-mode indicator (click to toggle). Always visible to the left of
    ; the shrink/expand buttons so it's easy to see in either mode.
    clipW := 12
    clipH := 14
    clipX := 2 + this.m_frmSize
    clipY := 1 + this.m_frmSize
    this.m_gui.SetFont( "s10", "Segoe UI Symbol" )
    this.m_clipIndicator := this.m_gui.AddText( "x" clipX " y" clipY " w" clipW " h" clipH " +0x100", "○" )
    g_tipMap[this.m_clipIndicator.Hwnd] := "Clipboard send mode: OFF"
    this.m_gui.SetFont( g_fontSize " norm", g_fontName )
    g_useClipSend := INI_IsClipSendMode()
    this.SetShowClipBulletState( g_useClipSend )

    ; Strip-emojis-from-comments indicator (click to toggle): sits immediately
    ; to the right of the shrink/expand button, mirroring the clipboard-mode
    ; indicator on the left. State is persisted to INI and only affects the
    ; Comments tab via CommentsTabPage.TransformSendText.
    stripW := 14
    stripH := 16
    stripX := 14 + btnWth + 2 + this.m_frmSize
    stripY := 1 + this.m_frmSize
    this.m_gui.SetFont( "s10", "Segoe UI Symbol" )
    this.m_stripEmojisIndicator := this.m_gui.AddText( "x" stripX " y" stripY " w" stripW " h" stripH " +0x100", "☺" )
    g_tipMap[this.m_stripEmojisIndicator.Hwnd] := "Strip emojis from comments: OFF"
    this.m_gui.SetFont( g_fontSize " norm", g_fontName )
    g_stripSendEmojis := INI_IsStripCommentEmojis()
    this.SetStripEmojisIndicatorState( g_stripSendEmojis )

    ; Explicit window size based on tab control dimensions.
    ; Prevents AHK from auto-sizing to include hidden buttons at large Y offsets.
    showW        := tabContentWidth + tabScrlW + 14 + 14
    showH        := tabContentHeight + 30 + 14 + 14
    this.m_fullW := showW

    ; Apply the user's saved height (from a previous resize). Falls back to the
    ; computed default if no persisted value or the value is out of range.
    savedH := INI_WndHeight()
    if( savedH != "" && savedH >= 140 && savedH <= A_ScreenHeight )
    {
      this.m_fullH := Integer( savedH )
    }
    else
    {
      this.m_fullH := showH
    }

    this.ToggleWindowSize( this.IsCollapsed() )

    OnMessage( 0x0003, this.m_onWindowMove   )  ; WM_MOVE
    OnMessage( 0x0216, this.m_onWindowMoving )  ; WM_MOVING

    ; Restore collapsed state last, after everything is laid out.
    this.ToggleWindowSize( INI_IsCollapsed() )
  }

  Close()
  {
    global g_activeWindow
    global g_wheelPendingSteps
    global g_wheelFlushScheduled

    g_activeWindow              := unset
    this.m_tabScrollHwnd        := 0
    this.m_toggleSizeBtn        := ""
    this.m_clipIndicator        := ""
    this.m_stripEmojisIndicator := ""

    this.SaveWindowPos()
    OnMessage( 0x0003, this.m_onWindowMove,   0 )
    OnMessage( 0x0216, this.m_onWindowMoving, 0 )
    OnMessage( 0x0115, VScroll,               0 )
    OnMessage( 0x0201, this.m_onLButtonDown,  0 )
    OnMessage( 0x0205, this.m_onRButtonUp,    0 )
    RemoveWheelHook()

    this.m_guiHwndRaw     := 0
    g_wheelPendingSteps   := 0
    g_wheelFlushScheduled := false

    SetTimer( this.m_hoverCheckTimer,        0 )
    SetTimer( this.m_trackActiveWindowTimer, 0 )
    ToolTip()
    if( IsObject( this.m_gui ) )
    {
      this.m_gui.Destroy()
    }
    this.m_gui := ""
  }

  ; ── Collapse / expand ────────────────────────────────────────────

  ; Single entry point for collapsing/expanding the window.
  ; Pass true to collapse, false to expand, or omit to flip the current state.
  ToggleWindowSize( collapse := "" )
  {
    global g_uiTabs

    if( !IsObject( this.m_gui ) )
    {
      return
    }

    if( collapse = "" )
    {
      collapse := !INI_IsCollapsed()
    }

    ; Allow OnGetMinMaxInfo to skip its width clamp for the duration of the
    ; Show() call — we are intentionally changing the width here.
    this.m_allowWidthChange := true
    try
    {
      if( collapse )
      {
        this.m_tabs.Opt( "Hidden" )
        this.SetToggleSizeBtnState( true )

        ; Remove WS_THICKFRAME to get the tool border back
        RemoveWindowStyle( this.m_gui.Hwnd, WS_THICKFRAME, true )

        ; Adjust control positions to remove frame offset when collapsed
        this.AdjustControlPositions( false )

        ; Update INI before Show() so the WM_SIZE handler sees the new state and
        ; can correctly skip the relayout while collapsing.
        INI_SetCollapsed( true )
        savedPos := this.LoadWindowPos()
        width    := 72
        height   := 24
        this.m_gui.Show( "w" width " h" height " NoActivate" savedPos )
      }
      else
      {
        this.m_tabs.Opt( "-Hidden" )
        this.SetToggleSizeBtnState( false )

        ; Re-add WS_THICKFRAME for the frame border
        AddWindowStyle( this.m_gui.Hwnd, WS_THICKFRAME, true )

        ; Restore control positions with frame offset when expanding
        this.AdjustControlPositions( true )

        ; Hiding the tab control also hides its child clip panels.
        ; Re-show the active tab's clip panel so buttons reappear.
        if( IsObject( g_uiTabs ) )
        {
          tabIndex := this.m_tabs.Value
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
        savedPos := this.LoadWindowPos()
        this.m_gui.Show( "w" this.m_fullW " h" this.m_fullH " NoActivate" savedPos )
        RedrawScrollbar()
      }
    }
    finally
    {
      this.m_allowWidthChange := false
    }
  }

  IsCollapsed()
  {
    return INI_IsCollapsed()
  }

  ; ── Layout ───────────────────────────────────────────────────────

  ; Position the utility controls (indicators and toggle button) accounting for frame offset.
  ; includeFrame: true to add m_frmSize offset, false for no offset
  AdjustControlPositions( includeFrame )
  {
    ; Get control dimensions
    this.m_clipIndicator.GetPos( , , &clipW, &clipH )
    this.m_toggleSizeBtn.GetPos( , , &btnW,  &btnH  )
    this.m_stripEmojisIndicator.GetPos( , , &stripW, &stripH )

    ; Y positions with frame offset
    yOffset := includeFrame ? this.m_frmSize : 0
    btnY    := yOffset

    ; Center the indicator controls vertically on the button
    btnCenterY := btnY + btnH / 2             ; 0 + (24/2) = 12
    clipY      := btnCenterY - clipH  / 2 - 3 ; 12 - (14/2) = 5 - 3 = 2 (there is an offset related to the font's character baseline on the statics)
    stripY     := btnCenterY - stripH / 2 - 2 ; 12 - (16/2) = 4 - 2 = 2 (there is an offset related to the font's character baseline on the statics)

    ; X positions: layout left-to-right with 2-pixel gaps
    xOffset    := includeFrame ? this.m_frmSize : 0
    edgeGap    := 2
    controlGap := 1

    clipX  := xOffset + edgeGap
    btnX   := clipX + clipW + controlGap
    stripX := btnX + btnW + controlGap

    this.m_clipIndicator.Move( clipX, clipY )
    this.m_toggleSizeBtn.Move( btnX,  btnY  )
    this.m_stripEmojisIndicator.Move( stripX, stripY )

    ; Moving a child control repaints it at its new spot but leaves the parent
    ; pixels it vacated untouched, so the old position ghosts as an artifact.
    ; Invalidate the GUI client area with an erase so the exposed background
    ; repaints. WS_CLIPCHILDREN (set in Show) keeps the erase off the controls
    ; themselves, so this clears the ghosts without flicker.
    DllCall( "InvalidateRect", "Ptr", this.m_gui.Hwnd, "Ptr", 0, "Int", true )
  }

  ; Resize the tab control, scrollbar, and each tab's clip panel to match the
  ; current GUI client height. Called from OnWindowSize after the user drags
  ; the top or bottom edge.
  RelayoutForHeight()
  {
    global g_uiTabs

    if( !IsObject( this.m_gui ) || !IsObject( this.m_tabs ) )
    {
      return
    }

    ; Client size is in AHK-logical pixels (DPI-aware). Tab control top stays
    ; at y=24 (room for the helper buttons) and we leave a small bottom margin.
    this.m_gui.GetClientPos( , , &clientW, &clientH )
    TAB_TOP    := 24
    TAB_MARGIN := 14
    newTabH    := clientH - TAB_TOP - TAB_MARGIN
    if( newTabH < 60 )
    {
      newTabH := 60
    }

    this.m_tabs.Move( , , , newTabH )

    ; Recompute the tab display rect (physical px) so the scrollbar and clip
    ; panels can be repositioned to match the new tab height.
    displayRect := Buffer( 16, 0 )
    DllCall( "GetClientRect", "Ptr", this.m_tabs.Hwnd, "Ptr", displayRect.Ptr )
    SendMessage( 0x1328, 0, displayRect.Ptr, this.m_tabs.Hwnd )  ; TCM_ADJUSTRECT
    dispTop    := NumGet( displayRect, 4,  "Int" )
    dispBottom := NumGet( displayRect, 12, "Int" )

    ; Map dispTop / dispBottom from tab-client coords to GUI-client coords.
    ptTL := Buffer( 8, 0 )
    NumPut( "Int", 0,       ptTL, 0 )
    NumPut( "Int", dispTop, ptTL, 4 )
    DllCall( "ClientToScreen", "Ptr", this.m_tabs.Hwnd, "Ptr", ptTL.Ptr )
    DllCall( "ScreenToClient", "Ptr", this.m_gui.Hwnd,  "Ptr", ptTL.Ptr )
    guiTop := NumGet( ptTL, 4, "Int" )

    ptBL := Buffer( 8, 0 )
    NumPut( "Int", 0,          ptBL, 0 )
    NumPut( "Int", dispBottom, ptBL, 4 )
    DllCall( "ClientToScreen", "Ptr", this.m_tabs.Hwnd, "Ptr", ptBL.Ptr )
    DllCall( "ScreenToClient", "Ptr", this.m_gui.Hwnd,  "Ptr", ptBL.Ptr )
    guiBottom := NumGet( ptBL, 4, "Int" )

    if( this.m_tabScrollHwnd )
    {
      scrollRect := Buffer( 16, 0 )
      DllCall( "GetWindowRect", "Ptr", this.m_tabScrollHwnd, "Ptr", scrollRect )
      scrollW := NumGet( scrollRect, 8, "Int" ) - NumGet( scrollRect, 0, "Int" )

      DllCall( "SetWindowPos",
               "Ptr",  this.m_tabScrollHwnd, "Ptr", 0,
               "Int",  this.m_tabScrollX, "Int", guiTop,
               "Int",  scrollW,           "Int", guiBottom - guiTop,
               "UInt", 0x0004 | 0x0010 )  ; SWP_NOZORDER | SWP_NOACTIVATE
    }

    if( IsObject( g_uiTabs ) )
    {
      for tab in g_uiTabs
      {
        tab.ResizeClipPanel( this.m_gui, this.m_tabs )
      }
    }

    ; Track the new full height so collapse → expand round-trips honour the
    ; user's chosen size.
    this.m_gui.GetPos( , , , &winH )
    this.m_fullH := winH

    UpdateScrollInfo()
    RedrawScrollbar()
  }

  ForceRepaint()
  {
    global g_uiTabs

    if( !IsObject( this.m_gui ) )
    {
      return
    }

    if( IsObject( this.m_tabs ) && IsSet( g_uiTabs ) )
    {
      tabIndex := this.m_tabs.Value
      if( (tabIndex >= 1) && (tabIndex <= g_uiTabs.Length) )
      {
        tab := g_uiTabs[tabIndex]
        tab.ShowClipPanel()
        tab.ApplyScrollPosition()
      }
    }

    RedrawScrollbar()
  }

  ; ── Control creation ─────────────────────────────────────────────

  CreateButton( text, tip,
                fontName, fontSize,
                x, y, w, h,
                func )
  {
    global g_tipMap
    global g_fontSize
    global g_fontName

    this.m_gui.SetFont( fontSize, fontName )
    btn := this.m_gui.AddButton( "x" x " y" y " w" w " h" h, text )
    DisableButtonWrap( btn )
    btn.OnEvent( "Click", func )
    g_tipMap[btn.Hwnd] := tip
    this.m_gui.SetFont( g_fontSize " norm", g_fontName )

    return btn
  }

  CreateBtnWithStyle( text, tip,
                      fontName, fontSize,
                      styleMask, styleBits,
                      x, y, w, h,
                      func )
  {
    btn   := this.CreateButton( text, tip,
                                fontName, fontSize,
                                x, y, w, h,
                                func )
    style := DllCall( "GetWindowLong", "Ptr", btn.Hwnd, "Int", GWL_STYLE, "Int" )
    DllCall( "SetWindowLong", "Ptr", btn.Hwnd, "Int", GWL_STYLE, "Int", (style & ~styleMask) | styleBits )

    return btn
  }

  ; ── Corner-control state ─────────────────────────────────────────

  SetToggleSizeBtnState( collapsed )
  {
    global g_tipMap

    OutputDebug( "Setting toggle-size button state: " (collapsed ? "COLLAPSED" : "EXPANDED") )
    if( !IsObject( this.m_toggleSizeBtn ) )
    {
      OutputDebug( "Toggle-size button not initialized yet." )
      return
    }

    if( collapsed )
    {
      ; Window is collapsed: clicking will expand it.
      this.m_toggleSizeBtn.Text := "▼"
      g_tipMap[this.m_toggleSizeBtn.Hwnd] := "Expand window"
    }
    else
    {
      ; Window is expanded: clicking will collapse it.
      this.m_toggleSizeBtn.Text := "▲"
      g_tipMap[this.m_toggleSizeBtn.Hwnd] := "Shrink window"
    }
  }

  SetShowClipBulletState( enabled )
  {
    global g_tipMap

    OutputDebug( "Setting clip bullet state: " (enabled ? "ON" : "OFF" ) )
    if( !IsObject( this.m_clipIndicator ) )
    {
      OutputDebug( "Clip indicator control not initialized yet." )
      return
    }
    this.m_clipIndicator.Text := enabled ? "●" : "○"
    g_tipMap[this.m_clipIndicator.Hwnd]  := "Clipboard send mode: " (enabled ? "ON" : "OFF")
    OutputDebug( "Updated clip bullet state: " (enabled ? "ON" : "OFF") )
  }

  SetStripEmojisIndicatorState( enabled )
  {
    global g_tipMap

    OutputDebug( "Setting strip-emojis indicator state: " (enabled ? "ON" : "OFF") )
    if( !IsObject( this.m_stripEmojisIndicator ) )
    {
      OutputDebug( "Strip-emojis indicator control not initialized yet." )
      return
    }
    this.m_stripEmojisIndicator.Text := enabled ? "☻" : "☺"
    g_tipMap[this.m_stripEmojisIndicator.Hwnd] := "Strip emojis from comments: " (enabled ? "ON" : "OFF")
  }

  IsClipControl( hwnd )
  {
    return IsObject( this.m_clipIndicator ) && (hwnd = this.m_clipIndicator.Hwnd)
  }

  IsStripEmojisControl( hwnd )
  {
    return IsObject( this.m_stripEmojisIndicator ) && (hwnd = this.m_stripEmojisIndicator.Hwnd)
  }

  IsToggleSizeBtn( hwnd )
  {
    return IsObject( this.m_toggleSizeBtn ) && (hwnd = this.m_toggleSizeBtn.Hwnd)
  }

  ; ── Message handlers ─────────────────────────────────────────────

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
    if( !IsObject( this.m_gui ) || hwnd != this.m_gui.Hwnd )
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

    if( !IsObject( this.m_gui ) || hwnd != this.m_gui.Hwnd )
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
    if( !IsObject( this.m_gui ) || hwnd != this.m_gui.Hwnd )
    {
      return
    }
    if( INI_IsCollapsed() )
    {
      return
    }
    if( this.m_allowWidthChange )
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
    if( !IsObject( this.m_gui ) || hwnd != this.m_gui.Hwnd )
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

    this.RelayoutForHeight()

    ; Persist the new height (debounced) so it's restored next time the script
    ; runs. SaveWindowHeight reads m_fullH that RelayoutForHeight just updated.
    SetTimer( this.m_saveWindowHeightTimer, -500 )
  }

  OnLButtonDown( wParam, lParam, msg, hwnd )
  {
    ; Pressing the bare window background starts a drag immediately — there is
    ; no click action to disambiguate from.
    if( hwnd = this.m_gui.Hwnd )
    {
      this.BeginWindowDrag()
      return
    }

    ; The clipboard / strip-emoji indicators and the shrink-expand button each
    ; do something on a plain click, but they should double as drag handles:
    ; when collapsed they cover almost the whole window, leaving no bare strip
    ; to grab. DragDetect tells a click apart from a press-and-drag.
    isCornerControl := this.IsClipControl(        hwnd )
                    || this.IsStripEmojisControl( hwnd )
                    || this.IsToggleSizeBtn(      hwnd )
    if( !isCornerControl )
    {
      return
    }

    if( this.StartDragIfMoved( hwnd ) )
    {
      ; User dragged: the window move has begun, so skip the click action.
      ; Returning a value also suppresses the button's default press/BN_CLICKED.
      return 0
    }

    ; Plain click: run the control's action.
    if( this.IsClipControl( hwnd ) )
    {
      ToggleClipboardSendMode()
    }
    else if( this.IsStripEmojisControl( hwnd ) )
    {
      ToggleStripSendEmojis()
    }
    else
    {
      this.ToggleWindowSize()
    }
    return 0
  }

  ; Record how far the cursor is from the window top-left at grab time (so the
  ; snap logic in OnWindowMoving can recover the cursor-implied position), then
  ; post the message that starts the OS window-move loop. Shared by the
  ; background-drag path and the corner-control drag handles.
  BeginWindowDrag()
  {
    pt := Buffer( 8 )
    DllCall( "GetCursorPos", "Ptr", pt )
    cursorX := NumGet( pt, 0, "Int" )
    cursorY := NumGet( pt, 4, "Int" )
    WinGetPos( &winX, &winY, , , this.m_gui )
    this.m_dragOffsetX := cursorX - winX
    this.m_dragOffsetY := cursorY - winY

    PostMessage( 0xA1, 2,,, "ahk_id " this.m_gui.Hwnd )  ; WM_NCLBUTTONDOWN, HTCAPTION
  }

  ; True if the user pressed and dragged on the given control — in which case the
  ; window move has been started. False on a plain click. DragDetect captures the
  ; mouse and pumps messages itself until the button is released (click) or the
  ; cursor moves past the system drag threshold (drag).
  StartDragIfMoved( hwnd )
  {
    pt := Buffer( 8 )
    DllCall( "GetCursorPos", "Ptr", pt )
    if( !DllCall( "DragDetect", "Ptr", hwnd, "Int64", NumGet( pt, 0, "Int64" ) ) )
    {
      return false
    }
    this.BeginWindowDrag()
    return true
  }

  OnRButtonUp( wParam, lParam, msg, hwnd )
  {
    if( hwnd != this.m_gui.Hwnd )
    {
      if( !this.IsClipControl(        hwnd ) &&
          !this.IsToggleSizeBtn(      hwnd ) &&
          !this.IsStripEmojisControl( hwnd ) )
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
    SetTimer( this.m_saveWindowPosTimer, -500 )
  }

  OnWindowMoving( wParam, lParam, msg, hwnd )
  {
    static SNAP_THRESHOLD    := 20
    static RELEASE_THRESHOLD := 30

    if( hwnd != this.m_gui.Hwnd )
    {
      if( !this.IsClipControl(        hwnd ) &&
          !this.IsStripEmojisControl( hwnd ) )
      {
        return
      }
    }

    left := NumGet( lParam,  0, "Int" )
    top  := NumGet( lParam,  4, "Int" )
    w    := NumGet( lParam,  8, "Int" ) - left
    h    := NumGet( lParam, 12, "Int" ) - top

    ; ── Favourite spot snap (highest priority) ──
    haveFav := (this.m_favX != "" && this.m_favY != "")
    if( haveFav )
    {
      favX := Integer( this.m_favX )
      favY := Integer( this.m_favY )

      if( this.m_snappedToFav )
      {
        ; Already snapped to fav: release when cursor implies position far enough away.
        pt := Buffer( 8 )
        DllCall( "GetCursorPos", "Ptr", pt )
        cursorX     := NumGet( pt, 0, "Int" )
        cursorY     := NumGet( pt, 4, "Int" )
        impliedLeft := cursorX - this.m_dragOffsetX
        impliedTop  := cursorY - this.m_dragOffsetY
        if( Abs( impliedLeft - favX ) >= RELEASE_THRESHOLD ||
            Abs( impliedTop  - favY ) >= RELEASE_THRESHOLD )
        {
          this.m_snappedToFav := false
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
        this.m_snappedToFav := true
        this.m_snappedToTop := false
        NumPut( "Int", favX,       lParam,  0 )
        NumPut( "Int", favY,       lParam,  4 )
        NumPut( "Int", favX + w,   lParam,  8 )
        NumPut( "Int", favY + h,   lParam, 12 )
        return 1
      }
    }

    ; ── Top-of-screen snap (y=0) ──
    if( this.m_snappedToTop )
    {
      ; Already snapped: release only when the implied window top moves far enough below 0.
      pt := Buffer( 8 )
      DllCall( "GetCursorPos", "Ptr", pt )
      cursorY := NumGet( pt, 4, "Int" )
      impliedTop := cursorY - this.m_dragOffsetY
      if( impliedTop >= RELEASE_THRESHOLD )
      {
        this.m_snappedToTop := false
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
      this.m_snappedToTop := true
      NumPut( "Int", 0, lParam,  4 ) ; top    = 0
      NumPut( "Int", h, lParam, 12 ) ; bottom = h
      return 1
    }
  }

  ; ── Timers ───────────────────────────────────────────────────────

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

    hwnd := WinActive( "A" )
    if( hwnd = 0 )
    {
      return
    }

    if( IsObject( this.m_gui ) && (hwnd = this.m_gui.Hwnd) )
    {
      return
    }

    g_activeWindow := hwnd
  }

  ; ── Window position persistence ──────────────────────────────────

  LoadWindowPos()
  {
    this.m_wndX := INI_WndPosX()
    this.m_wndY := INI_WndPosY()
    this.m_favX := INI_WndFavX()
    this.m_favY := INI_WndFavY()
    if( this.m_wndX = "" || this.m_wndY = "" )
    {
      return ""
    }
    return "x" this.m_wndX " y" this.m_wndY
  }

  SaveWindowPos()
  {
    if( !IsObject( this.m_gui ) )
    {
      return
    }
    WinGetPos( &wndX, &wndY, , , this.m_gui )
    this.m_wndX := wndX
    this.m_wndY := wndY
    INI_SetWndPosX( this.m_wndX )
    INI_SetWndPosY( this.m_wndY )
  }

  SaveWindowHeight()
  {
    if( !this.m_fullH )
    {
      return
    }
    INI_SetWndHeight( this.m_fullH )
  }

  SetFavouriteSpot()
  {
    if( !IsObject( this.m_gui ) )
    {
      return
    }

    WinGetPos( &x, &y, , , this.m_gui )
    this.m_favX := x
    this.m_favY := y
    INI_SetWndFavX( this.m_favX )
    INI_SetWndFavY( this.m_favY )
  }

  MoveToFavouriteSpot()
  {
    if( !IsObject( this.m_gui ) )
    {
      return
    }

    this.m_favX := INI_WndFavX()
    this.m_favY := INI_WndFavY()
    if( this.m_favX = "" || this.m_favY = "" )
    {
      return
    }

    WinMove( Integer( this.m_favX ), Integer( this.m_favY ), , , this.m_gui )
    this.SaveWindowPos()
  }
}
