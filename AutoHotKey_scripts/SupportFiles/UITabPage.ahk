class TabPage
{
  static m_globalHotkeyMap := Map()

  __New( name )
  {
    this.m_name           := name
    this.m_symbols        := []
    this.m_fontSize       := "s14" ; "s10"
    this.m_fontName       := "Segoe UI"
    this.m_hdrHeight      := 24
    this.m_symOrgX        := 15
    this.m_symOrgY        := 35
    this.m_symBtnSizeX    := 35
    this.m_symBtnSizeY    := 35
    this.m_symBtnGap      := 3
    this.m_contentWidth   := 0
    this.m_contentHeight  := 0
    this.m_destroyed      := false
    this.m_nextLine       := 1
    this.m_nextSlot       := 1
    this.m_maxSlots       := 0
    this.m_lineIsRow      := true
    this.m_lineShift      := 0
    this.m_scrollY        := 0
    this.m_scrollTargetY  := 0
    this.m_viewportHeight := 0

    this.m_guiHwnd          := 0
    this.m_tabHwnd          := 0
    this.m_clipPanelHwnd    := 0
    this.m_contentPanelHwnd := 0
    this.m_subclassCallback := 0
    this.m_scrollFlushMs    := 16
    this.m_scrollFlushFn    := ObjBindMethod( this, "OnScrollFlush" )
    this.m_useEmojiImages   := false
  }

  SetColsOf( maxRows )
  {
    ; Column-primary: line -> X axis, slot -> Y axis.
    this.m_maxSlots  := maxRows
    this.m_lineIsRow := false
  }

  SetRowsOf( maxCols )
  {
    ; Row-primary: line -> Y axis, slot -> X axis.
    this.m_maxSlots  := maxCols
    this.m_lineIsRow := true
  }

  RecalcSizes()
  {
    this.SetContentSize()
  }

  MaxPerLine()
  {
    return this.m_maxSlots
  }

  SetContentSize()
  {
    maxRight  := 0
    maxBottom := 0
    for sym in this.m_symbols
    {
      rightEdge := sym.x + sym.w
      if( rightEdge > maxRight )
      {
        maxRight := rightEdge
      }
      bottomEdge := sym.y + sym.h
      if( bottomEdge > maxBottom )
      {
        maxBottom := bottomEdge
      }
    }

    if( maxRight = 0 )
    {
      this.m_contentWidth := this.m_symBtnSizeX + this.m_symBtnGap + 10
    }
    else
    {
      this.m_contentWidth := maxRight + 1
    }

    if( maxBottom = 0 )
    {
      this.m_contentHeight := this.m_symBtnSizeY + this.m_symBtnGap + 10
    }
    else
    {
      this.m_contentHeight := (maxBottom - this.m_symOrgY) + this.m_symBtnGap + 10
    }
  }

  RowHeight()
  {
    return this.m_symBtnSizeY + this.m_symBtnGap
  }

  ColWidth()
  {
    return this.m_symBtnSizeX + this.m_symBtnGap
  }

  CalcSymbolX( line, slot )
  {
    if( this.m_lineIsRow )
    {
      return this.m_symOrgX + (slot - 1) * this.ColWidth()
    }

    return this.m_symOrgX + ((line - 1) + this.m_lineShift) * this.ColWidth()
  }

  CalcSymbolY( line, slot )
  {
    if( this.m_lineIsRow )
    {
      return this.m_symOrgY + ((line - 1) + this.m_lineShift) * this.RowHeight()
    }

    return this.m_symOrgY + (slot - 1) * this.RowHeight()
  }

  GetContentWidth( curWidth )
  {
    return (curWidth > this.m_contentWidth)
           ? curWidth
           : this.m_contentWidth
  }

  GetContentHeight( curHeight )
  {
    return (curHeight > this.m_contentHeight)
           ? curHeight
           : this.m_contentHeight
  }

  IsAFunction( func )
  {
    return (Type( func ) = "Func")      ||
           (Type( func ) = "BoundFunc") ||
           (Type( func ) = "Closure")
  }

  NormalizeDisplayText( text )
  {
    return StrReplace( text, "`b", "⌫" )
  }

  RegisterHotkeyBinding( hk, hotkeyAction, fallbackChar := "" )
  {
    if( hk = "" )
    {
      return
    }

    if( TabPage.m_globalHotkeyMap.Has( hk ) )
    {
      return
    }

    if( (Type( hotkeyAction ) != "Func")      &&
        (Type( hotkeyAction ) != "BoundFunc") &&
        (Type( hotkeyAction ) != "Closure") )
    {
      fb := fallbackChar
      hotkeyAction := ( * ) => DoSendText( fb )
    }

    try
    {
      Hotkey( hk, hotkeyAction )
      TabPage.m_globalHotkeyMap[hk] := this.m_name
      return
    }
    catch Error as err
    {
      MsgBox( "Hotkey registration failed [" hk "] in tab [" this.m_name "]: " err.Message )
    }
  }

  AddControls( gui, tabs, tabIdx, tipMap )
  {
    this.m_scrollY       := 0
    this.m_scrollTargetY := 0
    this.m_guiHwnd       := gui.Hwnd
    this.m_tabHwnd       := tabs.Hwnd

    gui.SetFont( this.m_fontSize, this.m_fontName )

    ; Detach from tab association so AHK's Tab3 never shows/hides our buttons.
    ; We manage visibility entirely through the clip panel.
    tabs.UseTab( 0 )

    ; Determine the tab display area offset (below the tab strip) in tab-client coords.
    ; TCM_ADJUSTRECT with wParam=FALSE shrinks the client rect to the display area.
    displayRect := Buffer( 16, 0 )
    DllCall( "GetClientRect", "Ptr", tabs.Hwnd, "Ptr", displayRect.Ptr )
    SendMessage( 0x1328, 0, displayRect.Ptr, tabs.Hwnd )
    dispLeft   := NumGet( displayRect, 0,  "Int" )
    dispTop    := NumGet( displayRect, 4,  "Int" )
    dispRight  := NumGet( displayRect, 8,  "Int" )
    dispBottom := NumGet( displayRect, 12, "Int" )

    dpi := DllCall( "GetDpiForWindow", "Ptr", gui.Hwnd, "UInt" )
    dpiScale := dpi / 96

    ; Create a clip panel that fills the tab display area to clip scrolled content.
    ; WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN = 0x52000000
    clipW := dispRight - dispLeft
    clipH := dispBottom - dispTop

    ; Override viewport height with the actual visible area in logical pixels.
    this.m_viewportHeight := Round( clipH / dpiScale )

    this.m_clipPanelHwnd := DllCall( "CreateWindowEx",
                                     "UInt", 0,
                                     "Str",  "Static",
                                     "Ptr",  0,
                                     "UInt", 0x52000000,
                                     "Int",  dispLeft,
                                     "Int",  dispTop,
                                     "Int",  clipW,
                                     "Int",  clipH,
                                     "Ptr",  tabs.Hwnd,
                                     "Ptr",  0,
                                     "Ptr",  0,
                                     "Ptr",  0,
                                     "Ptr" )

    ; Create a content panel (full content height) inside the clip panel.
    ; All buttons will be children of this panel.  Scrolling moves this panel.
    ; WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN = 0x52000000
    contentH := Round( this.m_contentHeight * dpiScale )
    this.m_contentPanelHwnd := DllCall( "CreateWindowEx",
                                        "UInt", 0,
                                        "Str",  "Static",
                                        "Ptr",  0,
                                        "UInt", 0x52000000,
                                        "Int",  0,
                                        "Int",  0,
                                        "Int",  clipW,
                                        "Int",  contentH,
                                        "Ptr",  this.m_clipPanelHwnd,
                                        "Ptr",  0,
                                        "Ptr",  0,
                                        "Ptr",  0,
                                        "Ptr" )

    ; Subclass the content panel so WM_COMMAND (button click) notifications
    ; are forwarded back to the AHK GUI window.  Without this, SetParent
    ; causes click events to be swallowed by the Static control.
    this.m_subclassCallback := CallbackCreate( _ContentPanelForwardCmd, , 6 )
    DllCall( "comctl32\SetWindowSubclass",
             "Ptr",  this.m_contentPanelHwnd,
             "Ptr",  this.m_subclassCallback,
             "UPtr", 1,
             "UPtr", gui.Hwnd )

    for sym in this.m_symbols
    {
      tip         := sym.desc
      tip         := this.NormalizeDisplayText( tip )
      localAction := sym.action
      if( !IsSet( localAction ) || !this.IsAFunction( localAction ) )
      {
        charToSend  := sym.char
        localAction := () => DoSendText( charToSend )
      }
      if( sym.hotkey != "" )
      {
        if( tip != "" )
        {
          tip .= "`n"
        }
        tip .= HotkeyLabel( sym.hotkey )
      }

      x   := sym.x
      y   := sym.y + 5
      w   := sym.w
      h   := sym.h
      tip := tip
      opt := "x" x " y" y " w" w " h" h " Hidden"
      if( sym.align = "left" )
      {
        opt .= " Left"
      }
      btn := gui.AddButton( opt, this.NormalizeDisplayText( sym.showChar ? sym.char : sym.desc ) )
      btn.SetFont( this.m_fontSize, this.m_fontName )
      filename := ""
      if( this.m_useEmojiImages )
      {
        filename := ApplyEmojiBitmapToButton( btn, sym.char, Round( this.m_symBtnSizeX * dpiScale * 0.8 ) )
        if( filename != "" )
        {
          tip := tip "`nU+" filename
        }
      }
      sym.ctrl := btn
      if( IsSet( tip ) )
      {
        tipMap[btn.Hwnd] := tip
      }
      handler := (( a ) => ( ctrl, * ) => this.SymbolClick( a, ctrl ))( localAction )
      btn.OnEvent( "Click", handler )

      ; Re-parent the button into the content panel so it scrolls with the panel.
      ; After SetParent, reposition it at the correct coords relative to the
      ; content panel.  Use SWP_NOSIZE to keep AHK's DPI-scaled button dimensions.
      ; Subtract m_symOrgY so buttons start at the top of the content panel.
      DllCall( "SetParent", "Ptr", btn.Hwnd, "Ptr", this.m_contentPanelHwnd )
      scaledX := Round( x * dpiScale )
      scaledY := Round( (y - this.m_symOrgY) * dpiScale )
      DllCall( "SetWindowPos", "Ptr", btn.Hwnd, "Ptr", 0,
               "Int", scaledX, "Int", scaledY, "Int", 0, "Int", 0,
               "UInt", 0x0001 | 0x0004 | 0x0010 | 0x0040 )  ; SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW
    }

    this.ApplyScrollPosition()
  }

  SetViewportHeight( viewportHeight )
  {
    this.m_viewportHeight := viewportHeight
    this.m_scrollTargetY  := this.ClampScrollY( this.m_scrollTargetY )
    this.m_scrollY        := this.ClampScrollY( this.m_scrollY )
  }

  ShowClipPanel()
  {
    if( this.m_clipPanelHwnd )
    {
      DllCall( "ShowWindow", "Ptr", this.m_clipPanelHwnd, "Int", 5 )  ; SW_SHOW
      ; AHK's Tab3 may have hidden buttons on inactive tabs.
      ; Explicitly show every button so they paint in the content panel.
      for sym in this.m_symbols
      {
        if( sym.HasOwnProp( "ctrl" ) )
        {
          DllCall( "ShowWindow", "Ptr", sym.ctrl.Hwnd, "Int", 5 )  ; SW_SHOW
        }
      }
    }
  }

  HideClipPanel()
  {
    if( this.m_clipPanelHwnd )
    {
      DllCall( "ShowWindow", "Ptr", this.m_clipPanelHwnd, "Int", 0 )  ; SW_HIDE
    }
  }

  InvalidatePanel()
  {
    if( this.m_clipPanelHwnd )
    {
      ; RDW_INVALIDATE | RDW_ERASE | RDW_UPDATENOW | RDW_ALLCHILDREN = 0x0185
      DllCall( "RedrawWindow", "Ptr", this.m_clipPanelHwnd,
               "Ptr", 0, "Ptr", 0, "UInt", 0x0185 )
    }
  }

  MaxScrollY()
  {
    if( this.m_viewportHeight <= 0 )
    {
      return 0
    }

    maxScroll := this.m_contentHeight - this.m_viewportHeight
    if( maxScroll < 0 )
    {
      return 0
    }

    return maxScroll
  }

  GetScrollY()
  {
    return this.m_scrollY
  }

  GetScrollTargetY()
  {
    return this.m_scrollTargetY
  }

  GetViewportHeight()
  {
    return this.m_viewportHeight
  }

  SetScrollY( scrollY, immediate := true )
  {
    newScrollY := this.ClampScrollY( scrollY )
    if( newScrollY = this.m_scrollTargetY )
    {
      return false
    }

    this.m_scrollTargetY := newScrollY
    if( immediate )
    {
      this.FlushScrollNow()
    }
    else
    {
      this.QueueScrollFlush()
    }

    return true
  }

  ClampScrollY( scrollY )
  {
    maxScroll := this.MaxScrollY()
    if( scrollY < 0 )
    {
      return 0
    }
    if( scrollY > maxScroll )
    {
      return maxScroll
    }

    return scrollY
  }

  ScrollByPixels( deltaY )
  {
    newScrollY := this.ClampScrollY( this.m_scrollTargetY + deltaY )
    if( newScrollY = this.m_scrollTargetY )
    {
      return false
    }

    this.m_scrollTargetY := newScrollY
    this.QueueScrollFlush()
    return true
  }

  ResetScroll()
  {
    this.m_scrollY := 0
    this.m_scrollTargetY := 0
    this.ApplyScrollPosition()
  }

  QueueScrollFlush()
  {
    if( this.m_destroyed )
    {
      return
    }

    ; One-shot flush: repeated wheel events coalesce into one repaint.
    SetTimer( this.m_scrollFlushFn, -this.m_scrollFlushMs )
  }

  FlushScrollNow()
  {
    SetTimer( this.m_scrollFlushFn, 0 )
    if( this.m_scrollY != this.m_scrollTargetY )
    {
      this.m_scrollY := this.m_scrollTargetY
      this.ApplyScrollPosition()
    }
  }

  OnScrollFlush()
  {
    if( this.m_destroyed )
    {
      return
    }

    if( this.m_scrollY = this.m_scrollTargetY )
    {
      return
    }

    ; Interpolate toward the target for smooth scrolling.
    diff := this.m_scrollTargetY - this.m_scrollY
    LERP_FACTOR := 0.35
    SNAP_THRESHOLD := 1
    if( Abs( diff ) <= SNAP_THRESHOLD )
    {
      this.m_scrollY := this.m_scrollTargetY
    }
    else
    {
      this.m_scrollY := Round( this.m_scrollY + diff * LERP_FACTOR )
    }

    this.ApplyScrollPosition()

    ; Continue animating if we haven't reached the target yet.
    if( this.m_scrollY != this.m_scrollTargetY )
    {
      SetTimer( this.m_scrollFlushFn, -this.m_scrollFlushMs )
    }
  }

  ApplyScrollPosition()
  {
    ; Move the content panel inside the clip panel.  The clip panel's bounds
    ; handle all clipping automatically — no per-button visibility management.
    if( this.m_contentPanelHwnd )
    {
      dpi := DllCall( "GetDpiForWindow", "Ptr", this.m_guiHwnd, "UInt" )
      dpiScale := dpi / 96
      DllCall( "SetWindowPos",
               "Ptr",  this.m_contentPanelHwnd,
               "Ptr",  0,
               "Int",  0,
               "Int",  Round( -this.m_scrollY * dpiScale ),
               "Int",  0,
               "Int",  0,
               "UInt", 0x0001 | 0x0004 | 0x0010 )  ; SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE
    }
  }

  NextLine( testForEOL := false )
  {
    if( !testForEOL || (this.m_nextSlot > 1) )
    {
      this.m_nextLine++
    }
    this.m_nextSlot := 1
  }

  ShiftLineByHalf( num := 1 )
  {
    this.ShiftLineByFraction( num, 2 )
  }

  ShiftLineByThird( num := 1 )
  {
    this.ShiftLineByFraction( num, 3 )
  }

  ShiftLineByFraction( numerator := 1, denominator := 2 )
  {
    if( denominator != 0 )
    {
      this.m_lineShift += numerator / denominator
    }
  }

  RegisterSpace( slots := 1 )
  {
    this.AdvanceSlot( slots )
  }

  RegisterSymbolX( width,
                   char,
                   desc     := unset,
                   hotkey   := unset,
                   action   := unset,
                   align    := "center",
                   showChar := unset )
  {
    if( IsSet( action ) )
    {
      this.RegisterSymbol( this.m_nextLine,
                           this.m_nextSlot,
                           width,
                           char,
                           desc     ?? char,
                           hotkey   ?? "",
                           action,
                           align,
                           showChar ?? 1 )
    }
    else
    {
      this.RegisterSymbol( this.m_nextLine,
                           this.m_nextSlot,
                           width,
                           char,
                           desc     ?? char,
                           hotkey   ?? "",
                           unset,
                           align,
                           showChar ?? 1 )
    }
  }

  RegisterSymbol( line,
                  slot,
                  width,
                  char,
                  desc     := unset,
                  hotkey   := unset,
                  action   := unset,
                  align    := "center",
                  showChar := unset )
  {
    x := this.CalcSymbolX( line, slot )
    y := this.CalcSymbolY( line, slot )
    w := this.m_symBtnSizeX * width + this.m_symBtnGap * (width - 1)
    h := this.m_symBtnSizeY

    ; Visual width does not affect logical fill order.
    this.AdvanceSlot( 1 )

    ; Resolve optional actions only when already callable.
    resolvedAction := unset
    if( IsSet( action ) )
    {
      if( this.IsAFunction( action ) )
      {
        resolvedAction := action
      }
    }

    ; Button click action: closure captures char by value, always callable with no args.
    charCopy := char
    if( IsSet( resolvedAction ) && this.IsAFunction( resolvedAction ) )
    {
      clickAction := resolvedAction
    }
    else
    {
      clickAction := () => DoSendText( charCopy )
    }

    ; Hotkey action: closure captures char, accepts the hotkey-name arg via *.
    if( IsSet( resolvedAction ) && this.IsAFunction( resolvedAction ) )
    {
      hotkeyAction := resolvedAction
    }
    else
    {
      cc := charCopy
      hotkeyAction := ( * ) => DoSendText( cc )
    }

    element := { line:         line,
                 slot:         slot,
                 width:        width,
                 x:            x,
                 y:            y,
                 w:            w,
                 h:            h,
                 char:         char,
                 desc:         desc     ?? char,
                 showChar:     showChar ?? 1,
                 hotkey:       hotkey   ?? "",
                 align:        align,
                 action:       clickAction,
                 hotkeyAction: hotkeyAction }
    this.m_symbols.Push( element )

    this.RegisterHotkeyBinding( hotkey ?? "", hotkeyAction, charCopy )
  }

  DbgWrapRowOrCol( msg, slots )
  {
    ;OutputDebug( "AdvanceSlot " msg this.m_name " - lineIsRow: " this.m_lineIsRow " - maxSlots: " this.m_maxSlots " - step: " slots " - line: " this.m_nextLine " - slot: " this.m_nextSlot )
  }

  AdvanceSlot( slots := 1 )
  {
    this.DbgWrapRowOrCol( "In:  ", slots )
    if( this.m_maxSlots <= 0 )
    {
      this.DbgWrapRowOrCol( "Out: ", slots )
      return
    }

    if( slots <= 0 )
    {
      this.DbgWrapRowOrCol( "Out: ", slots )
      return
    }

    this.m_nextSlot += slots
    while( this.m_nextSlot > this.m_maxSlots )
    {
      this.m_nextSlot -= this.m_maxSlots
      this.m_nextLine++
      this.DbgWrapRowOrCol( "Rst: ", slots )
    }
    this.DbgWrapRowOrCol( "Out: ", slots )
  }

  SymbolClick( action, ctrl, * )
  {
    ;Close()
    Sleep( 150 )
    action.Call()
  }

  Destroy()
  {
    if( this.m_destroyed )
    {
      return
    }
    this.m_destroyed := true

    SetTimer( this.m_scrollFlushFn, 0 )

    if( this.m_subclassCallback && this.m_contentPanelHwnd )
    {
      DllCall( "comctl32\RemoveWindowSubclass",
               "Ptr",  this.m_contentPanelHwnd,
               "Ptr",  this.m_subclassCallback,
               "UPtr", 1 )
      CallbackFree( this.m_subclassCallback )
      this.m_subclassCallback := 0
    }

    if( this.m_contentPanelHwnd )
    {
      DllCall( "DestroyWindow", "Ptr", this.m_contentPanelHwnd )
      this.m_contentPanelHwnd := 0
    }
    if( this.m_clipPanelHwnd )
    {
      DllCall( "DestroyWindow", "Ptr", this.m_clipPanelHwnd )
      this.m_clipPanelHwnd := 0
    }

    if( IsObject( this.m_symbols ) )
    {
      this.m_symbols := unset
    }
  }

  __Delete()
  {
    this.Destroy()
  }
}

; Subclass callback: forwards WM_COMMAND from the content panel to the AHK GUI
; so that button click events are routed through AHK's event system.
_ContentPanelForwardCmd( hWnd, uMsg, wParam, lParam, uIdSubclass, dwRefData )
{
  if( uMsg = 0x0111 )  ; WM_COMMAND
  {
    return DllCall( "SendMessageW",
                    "Ptr",  dwRefData,
                    "UInt", uMsg,
                    "Ptr",  wParam,
                    "Ptr",  lParam,
                    "Ptr" )
  }

  return DllCall( "comctl32\DefSubclassProc",
                  "Ptr",  hWnd,
                  "UInt", uMsg,
                  "Ptr",  wParam,
                  "Ptr",  lParam,
                  "Ptr" )
}
