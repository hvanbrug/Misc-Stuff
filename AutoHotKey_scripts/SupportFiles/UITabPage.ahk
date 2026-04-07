class TabPage
{
  __New( name )
  {
    this.m_name          := name
    this.m_symbols       := []
    this.m_fontSize      := "s14" ; "s10"
    this.m_fontName      := "Segoe UI"
    this.m_hdrHeight     := 24
    this.m_symOrgX       := 15
    this.m_symOrgY       := 35
    this.m_symBtnSizeX   := 35
    this.m_symBtnSizeY   := 35
    this.m_symBtnGap     := 3
    this.m_contentWidth  := 0
    this.m_contentHeight := 0
    this.m_destroyed     := false
    this.m_nextRow       := 1
    this.m_nextCol       := 1
    this.m_maxPerLine    := 0
    this.m_fillHoriz     := false
  }

  SetByRow( maxRows )
  {
    this.m_maxPerLine := maxRows
    this.m_fillHoriz  := false
  }

  SetByCol( maxCols )
  {
    this.m_maxPerLine := maxCols
    this.m_fillHoriz  := true
  }

  RecalcSizes()
  {
    this.SetContentSize()
  }

  MaxPerLine()
  {
    return this.m_maxPerLine
  }

  SetContentSize()
  {
    maxCol := 1
    maxRow := 1
    for sym in this.m_symbols
    {
      if( sym.col > maxCol )
      {
        maxCol := sym.col
      }
      if( sym.row > maxRow )
      {
        maxRow := sym.row
      }
    }

    this.m_contentWidth  := maxCol * (this.m_symBtnSizeX + this.m_symBtnGap) + this.m_symBtnGap + 10
    this.m_contentHeight := maxRow * (this.m_symBtnSizeY + this.m_symBtnGap) + this.m_symBtnGap + 10
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

  AddControls( gui, tabs, tabIdx, tipMap )
  {
    gui.SetFont( this.m_fontSize, this.m_fontName )

    tabs.UseTab( tabIdx )
    for sym in this.m_symbols
    {
      tip         := sym.desc
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

      x   := this.m_symOrgX + (sym.col - 1) * (this.m_symBtnSizeX + this.m_symBtnGap)
      y   := this.m_symOrgY + (sym.row - 1) * (this.m_symBtnSizeY + this.m_symBtnGap)
      w   := this.m_symBtnSizeX * sym.width + this.m_symBtnGap * (sym.width - 1)
      h   := this.m_symBtnSizeY
      tip := tip
      opt := "x" x " y" y " w" w " h" h
      btn := gui.AddButton( opt, sym.char )
      btn.SetFont( this.m_fontSize, this.m_fontName )
      if( IsSet( tip ) )
      {
        tipMap[btn.Hwnd] := tip
      }
      handler := (( a ) => ( ctrl, * ) => this.SymbolClick( a, ctrl ))( localAction )
      btn.OnEvent( "Click", handler )
    }
  }

  NextLine()
  {
    this.RegisterSpace( this.m_maxPerLine )
  }

  RegisterSpace( width := 1 )
  {
    this.WrapRowOrCol( width )
  }

  RegisterSymbolX( width,
                   char,
                   desc   := unset,
                   hotkey := unset,
                   action := unset )
  {
    if( IsSet( action ) )
    {
      this.RegisterSymbol( this.m_nextRow,
                           this.m_nextCol,
                           width,
                           char,
                           desc   ?? char,
                           hotkey ?? "",
                           action )
    }
    else
    {
      this.RegisterSymbol( this.m_nextRow,
                           this.m_nextCol,
                           width,
                           char,
                           desc   ?? char,
                           hotkey ?? "" )
    }
  }

  RegisterSymbol( row,
                  col,
                  width,
                  char,
                  desc   := unset,
                  hotkey := unset,
                  action := unset )
  {
    this.WrapRowOrCol( width )

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
    if( IsSet( action ) && (Type( action ) = "Func" || Type( action ) = "BoundFunc") )
    {
      hotkeyAction := action
    }
    else
    {
      hotkeyAction := TabPage.SendCharFunc.Bind( charCopy )
    }

    element := { row:         row,
                 col:         col,
                 width:       width,
                 char:        char,
                 desc:        desc   ?? char,
                 hotkey:      hotkey ?? "",
                 action:      clickAction,
                 hotkeyAction: hotkeyAction }
    this.m_symbols.Push( element )
  }

  static SendCharFunc( char )
  {
    DoSendText( char )
  }

  DbgWrapRowOrCol( msg, width )
  {
    ;OutputDebug( "WrapRowOrCol " msg this.m_name " - fillH: " this.m_fillHoriz " - max: " this.m_maxRowsOrCols " - width: " width " - lastRow: " this.m_lastRow " - lastCol: " this.m_lastCol )
  }

  WrapRowOrCol( width )
  {
    this.DbgWrapRowOrCol( "In:  ", width )
    if( this.m_fillHoriz )
    {
      this.m_nextCol += width
      if( this.m_nextCol > this.m_maxPerLine )
      {
        this.m_nextCol := 1
        this.m_nextRow++
        this.DbgWrapRowOrCol( "Rst: ", width )
      }
    }
    else
    {
      this.m_nextRow += 1
      if( this.m_nextRow > this.m_maxPerLine )
      {
        this.m_nextRow := 1
        this.m_nextCol++
        this.DbgWrapRowOrCol( "Rst: ", width )
      }
    }
    this.DbgWrapRowOrCol( "Out: ", width )
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
