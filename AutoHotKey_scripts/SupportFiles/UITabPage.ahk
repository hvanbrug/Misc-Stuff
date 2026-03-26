

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

  AddControls( gui, tabs, tabIdx, tipMap )
  {
    gui.SetFont( this.m_fontSize, this.m_fontName )

    for sym in this.m_symbols
    {
      tabs.UseTab( tabIdx )
      x   := this.m_symOrgX + (sym.col - 1) * (this.m_symBtnSizeX + this.m_symBtnGap)
      y   := this.m_symOrgY + (sym.row - 1) * (this.m_symBtnSizeY + this.m_symBtnGap)
      w   := this.m_symBtnSizeX * sym.width + this.m_symBtnGap * (sym.width - 1)
      h   := this.m_symBtnSizeY
      tip := (sym.hotkey == "") ? sym.desc : (sym.desc "`n" sym.hotkey)
      opt := "x" x " y" y " w" w " h" h
      ;if sym.tab  = COMMENTS_TAB
      ;{
      ;  opt .= " left"
      ;}
      btn := gui.Add( "Button", opt, sym.char )
      btn.SetFont( this.m_fontSize, this.m_fontName )
      tipMap[btn.Hwnd] := tip
      localAction := sym.action
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
    this.RegisterSymbol( this.m_nextRow,
                         this.m_nextCol,
                         width,
                         char,
                         desc   ?? char,
                         hotkey ?? "",
                         action ?? () => DoSendText( char ) )
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

    element := { char:   char,
                 desc:   desc   ?? char,
                 hotkey: hotkey ?? "",
                 action: action ?? () => DoSendText( char ),
                 row:    row,
                 col:    col,
                 width:  width }
    this.m_symbols.Push( element )
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
