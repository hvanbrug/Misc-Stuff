class TabPage
{
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
    this.m_viewportHeight := 0

    this.m_guiHwnd          := 0
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

  AddControls( gui, tabs, tabIdx, tipMap )
  {
    this.m_scrollY := 0
    this.m_guiHwnd := gui.Hwnd

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
        Hotkey( sym.hotkey, sym.hotkeyAction )
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

    this.ApplyScrollPosition()
  }

  SetViewportHeight( viewportHeight )
  {
    this.m_viewportHeight := viewportHeight
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
    newScrollY := this.ClampScrollY( this.m_scrollY + deltaY )
    if( newScrollY = this.m_scrollY )
    {
      return false
    }

    this.m_scrollY := newScrollY
    this.ApplyScrollPosition()
    this.QueueRedrawWatchdog()
    return true
  }

  ResetScroll()
  {
    this.m_scrollY := 0
    this.ApplyScrollPosition()
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
      if( this.IsSymbolVisibleInViewport( y, sym.h ) )
      {
        ctrl.Move( sym.x,
                   y,
                   sym.w,
                   sym.h )
        ctrl.Opt( "-Hidden" )
      }
      else
      {
        ctrl.Opt( "Hidden" )
      }
    }
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
             "ptr", this.m_guiHwnd,
             "ptr", 0,
             "ptr", 0,
             "uint", redrawFlags )
  }

  NextLine()
  {
    this.m_nextLine++
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
                   action := unset )
  {
    if( IsSet( action ) )
    {
      this.RegisterSymbol( this.m_nextLine,
                           this.m_nextSlot,
                           width,
                           char,
                           desc   ?? char,
                           hotkey ?? "",
                           action )
    }
    else
    {
      this.RegisterSymbol( this.m_nextLine,
                           this.m_nextSlot,
                           width,
                           char,
                           desc   ?? char,
                           hotkey ?? "" )
    }
  }

  RegisterSymbol( line,
                  slot,
                  width,
                  char,
                  desc   := unset,
                  hotkey := unset,
                  action := unset )
  {
    x := this.CalcSymbolX( line, slot )
    y := this.CalcSymbolY( line, slot )
    w := this.m_symBtnSizeX * width + this.m_symBtnGap * (width - 1)
    h := this.m_symBtnSizeY

    ; Visual width does not affect logical fill order.
    this.AdvanceSlot( 1 )

    ; Button click action: closure captures char by value, always callable with no args
    charCopy := char
    if( IsSet( action ) )
    {
      clickAction := action
    }
    else
    {
      clickAction := () => DoSendText( charCopy )
    }

    ; Hotkey action: must be Func or BoundFunc (Hotkey() does not accept Closures)
    if( IsSet( action ) && (Type( action ) = "Func" ||
                            Type( action ) = "BoundFunc") )
    {
      hotkeyAction := action
    }
    else
    {
      hotkeyAction := TabPage.SendCharFunc.Bind( charCopy )
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
                 action:       clickAction,
                 hotkeyAction: hotkeyAction }
    this.m_symbols.Push( element )
  }

  static SendCharFunc( char )
  {
    DoSendText( char )
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
    ;HelpMenu_Close()
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
