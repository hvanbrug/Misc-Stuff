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
    this.m_symbolsByY     := []
    this.m_visibleStart   := 0
    this.m_visibleEnd     := 0

    this.m_guiHwnd          := 0
    this.m_tabHwnd          := 0
    this.m_scrollFlushMs    := 16
    this.m_scrollFlushFn    := ObjBindMethod( this, "OnScrollFlush" )
    this.m_redrawWatchdogMs := 20
    this.m_redrawWatchdogFn := ObjBindMethod( this, "OnRedrawWatchdog" )
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

  BuildSymbolsByY()
  {
    sortedSymbols := []

    for sym in this.m_symbols
    {
      insertAt := sortedSymbols.Length + 1
      idx      := 1
      while( idx <= sortedSymbols.Length )
      {
        if( sym.y < sortedSymbols[idx].y )
        {
          insertAt := idx
          break
        }

        idx++
      }

      sortedSymbols.InsertAt( insertAt, sym )
    }

    this.m_symbolsByY := sortedSymbols
  }

  AddControls( gui, tabs, tabIdx, tipMap )
  {
    this.m_scrollY       := 0
    this.m_scrollTargetY := 0
    this.m_guiHwnd       := gui.Hwnd
    this.m_tabHwnd       := tabs.Hwnd

    gui.SetFont( this.m_fontSize, this.m_fontName )

    tabs.UseTab( tabIdx )
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
        tip .= sym.hotkey
      }

      x   := sym.x
      y   := sym.y
      w   := sym.w
      h   := sym.h
      tip := tip
      opt := "x" x " y" y " w" w " h" h
      if( sym.align = "left" )
      {
        opt .= " Left"
      }
      btn := gui.AddButton( opt, this.NormalizeDisplayText( sym.char ) )
      btn.SetFont( this.m_fontSize, this.m_fontName )
      sym.ctrl := btn
      if( IsSet( tip ) )
      {
        tipMap[btn.Hwnd] := tip
      }
      handler := (( a ) => ( ctrl, * ) => this.SymbolClick( a, ctrl ))( localAction )
      btn.OnEvent( "Click", handler )
    }

    this.m_visibleStart := 0
    this.m_visibleEnd   := 0
    this.BuildSymbolsByY()

    this.ApplyScrollPosition()
  }

  SetViewportHeight( viewportHeight )
  {
    this.m_viewportHeight := viewportHeight
    this.m_scrollTargetY  := this.ClampScrollY( this.m_scrollTargetY )
    this.m_scrollY        := this.ClampScrollY( this.m_scrollY )
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
      this.RequestViewportRedraw()
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

  RequestViewportRedraw()
  {
    if( this.m_guiHwnd = 0 )
    {
      return
    }

    ; Invalidate the GUI so the parent background repaints between moved buttons.
    ; WS_CLIPCHILDREN on the GUI prevents background erase from covering buttons.
    ; RDW_INVALIDATE | RDW_ERASE | RDW_UPDATENOW = 0x0105
    DllCall( "RedrawWindow",
             "ptr", this.m_guiHwnd,
             "ptr", 0,
             "ptr", 0,
             "uint", 0x0105 )
  }

  IsSymbolVisibleInViewport( y, h )
  {
    if( this.m_viewportHeight <= 0 )
    {
      return true
    }

    viewportTop    := this.m_symOrgY
    viewportBottom := this.m_symOrgY + this.m_viewportHeight
    symbolBottom   := y + h

    return (symbolBottom > viewportTop) &&
           (y < viewportBottom)
  }

  ApplyScrollPosition()
  {
    ; No viewport clipping requested: keep all controls in sync.
    if( this.m_viewportHeight <= 0 )
    {
      for sym in this.m_symbols
      {
        if( !sym.HasOwnProp( "ctrl" ) )
        {
          continue
        }

        ctrl := sym.ctrl
        if( !IsSet( ctrl ) )
        {
          continue
        }

        y := sym.y - this.m_scrollY
        shouldMove := (!sym.HasOwnProp( "renderY" )) ||
                      (sym.renderY != y)
        if( shouldMove )
        {
          ctrl.Move( sym.x,
                     y,
                     sym.w,
                     sym.h )
          sym.renderY := y
        }

        if( (!sym.HasOwnProp( "visible" )) ||
            (!sym.visible) )
        {
          ctrl.Opt( "-Hidden" )
          sym.visible := true
        }
      }

      return
    }

    topBase    := this.m_symOrgY + this.m_scrollY
    bottomBase := this.m_symOrgY + this.m_viewportHeight + this.m_scrollY

    ; Include buttons that are partially visible at the top or bottom edge.
    ; A button is visible if any part of it overlaps the viewport, i.e.
    ;   sym.y + sym.h > topBase  AND  sym.y < bottomBase
    ; The startIdx/endIdx searches below use these relaxed bounds.

    n := this.m_symbolsByY.Length
    if( n = 0 )
    {
      return
    }

    ; Find first symbol whose bottom edge is past the viewport top.
    startIdx := 1
    while( (startIdx <= n) &&
           ((this.m_symbolsByY[startIdx].y + this.m_symbolsByY[startIdx].h) <= topBase) )
    {
      startIdx++
    }

    ; Find last symbol whose top edge is before the viewport bottom.
    endIdx := n
    while( (endIdx >= startIdx) &&
           (this.m_symbolsByY[endIdx].y >= bottomBase) )
    {
      endIdx--
    }

    ; --- Suppress repaints while repositioning all buttons ---
    ; Moving buttons one at a time with ctrl.Move() leaves ghost images
    ; because the parent repaints between individual moves. Suppressing
    ; WM_SETREDRAW on the GUI batches all visual changes into a single
    ; repaint at the end, eliminating artifacts in both scroll directions.
    ; We use AHK's native ctrl.Move()/ctrl.Opt() so DPI scaling is handled
    ; correctly (raw Win32 DeferWindowPos bypasses AHK's DPI conversion).

    guiHwnd := this.m_guiHwnd
    WM_SETREDRAW := 0x000B
    DllCall( "SendMessageW", "ptr", guiHwnd, "uint", WM_SETREDRAW, "ptr", 0, "ptr", 0 )

    ; --- Hide buttons that scrolled out of view ---
    oldStart := this.m_visibleStart
    oldEnd   := this.m_visibleEnd
    if( (oldStart >= 1) && (oldEnd >= oldStart) )
    {
      i := oldStart
      while( i <= oldEnd )
      {
        if( (i < startIdx) || (i > endIdx) )
        {
          sym := this.m_symbolsByY[i]
          if( sym.HasOwnProp( "ctrl" ) )
          {
            ctrl := sym.ctrl
            if( IsSet( ctrl ) &&
                ((!sym.HasOwnProp( "visible" )) || (sym.visible)) )
            {
              ctrl.Opt( "Hidden" )
              sym.visible := false
            }
          }
        }

        i++
      }
    }

    ; --- Move and show visible buttons ---
    if( (startIdx >= 1) && (startIdx <= endIdx) )
    {
      i := startIdx
      while( i <= endIdx )
      {
        sym := this.m_symbolsByY[i]
        if( sym.HasOwnProp( "ctrl" ) )
        {
          ctrl := sym.ctrl
          if( IsSet( ctrl ) )
          {
            y := sym.y - this.m_scrollY
            if( (!sym.HasOwnProp( "renderY" )) || (sym.renderY != y) )
            {
              ctrl.Move( sym.x, y, sym.w, sym.h )
              sym.renderY := y
            }

            if( (!sym.HasOwnProp( "visible" )) || (!sym.visible) )
            {
              ctrl.Opt( "-Hidden" )
              sym.visible := true
            }
          }
        }

        i++
      }
    }

    ; Re-enable drawing and invalidate for an async repaint.
    ; Avoid RDW_UPDATENOW here: a synchronous repaint of all children blocks
    ; AHK's message pump, which can cause the low-level mouse hook to time
    ; out and let wheel events leak through to the window beneath.
    DllCall( "SendMessageW", "ptr", guiHwnd, "uint", WM_SETREDRAW, "ptr", 1, "ptr", 0 )
    ; RDW_INVALIDATE | RDW_ERASE | RDW_ALLCHILDREN = 0x0085
    DllCall( "RedrawWindow",
             "ptr",  guiHwnd,
             "ptr",  0,
             "ptr",  0,
             "uint", 0x0085 )

    this.m_visibleStart := startIdx
    this.m_visibleEnd   := endIdx
  }

  QueueRedrawWatchdog()
  {
    if( this.m_destroyed )
    {
      return
    }

    ; One-shot watchdog: repeated calls reset the timer and coalesce redraws.
    SetTimer( this.m_redrawWatchdogFn, -this.m_redrawWatchdogMs )
  }

  OnRedrawWatchdog()
  {
    this.RequestFullRedraw()
  }

  RequestFullRedraw()
  {
    if( this.m_guiHwnd = 0 )
    {
      return
    }

    ; Invalidate only and let Windows batch repaint to reduce flashing.
    redrawFlags := 0x0001 | 0x0080
    DllCall( "RedrawWindow",
             "ptr",  this.m_guiHwnd,
             "ptr",  0,
             "ptr",  0,
             "uint", redrawFlags )
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
                   desc   := unset,
                   hotkey := unset,
                   action := unset,
                   align  := "center" )
  {
    if( IsSet( action ) )
    {
      this.RegisterSymbol( this.m_nextLine,
                           this.m_nextSlot,
                           width,
                           char,
                           desc   ?? char,
                           hotkey ?? "",
                           action,
                           align )
    }
    else
    {
      this.RegisterSymbol( this.m_nextLine,
                           this.m_nextSlot,
                           width,
                           char,
                           desc   ?? char,
                           hotkey ?? "",
                           unset,
                           align )
    }
  }

  RegisterSymbol( line,
                  slot,
                  width,
                  char,
                  desc   := unset,
                  hotkey := unset,
                  action := unset,
                  align  := "center" )
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
                 desc:         desc   ?? char,
                 hotkey:       hotkey ?? "",
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

    SetTimer( this.m_scrollFlushFn,    0 )
    SetTimer( this.m_redrawWatchdogFn, 0 )

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
