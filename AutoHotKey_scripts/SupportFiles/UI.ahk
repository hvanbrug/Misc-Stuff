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

  global g_gui
  global g_tipMap
  global g_symbols
  global g_tabNames
  global g_tabs
  global g_fontSize
  global g_fontSymSize
  global g_fontName
  global g_fontEmojiName
  global g_symOrgX
  global g_symOrgY
  global g_symBtnSizeX
  global g_symBtnSizeY
  global g_symBtnGap

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

  windowTitle := "Henks Hotkey Reference"

  if WinExist( windowTitle )
  {
    WinActivate
    return
  }

  ; ── Create GUI ──
  ;g_gui := Gui( "+AlwaysOnTop +Resize", windowTitle )
  g_gui := Gui( "+AlwaysOnTop", windowTitle )

  ; ── Determine tab names from registered symbols ──
  ;tabNames := Map()
  ;for i, sym in g_symbols
  ;{
  ;  if !tabNames.Has( sym.tab )
  ;  {
  ;    tabNames[sym.tab] := g_tabNames.Has( sym.tab )
  ;                           ? g_tabNames[sym.tab]
  ;                           : "Symbols " sym.tab
  ;  }
  ;}

  tabList := []
  tabContentWidth  := g_LV_WIDTH
  tabContentHeight := 0
  for tabIndex, tab in g_uiTabs
  {
    ;tabNames[tabIndex] := tab.m_name
    tabList.Push( tab.m_name )
    tabContentWidth  := tab.GetContentWidth(  tabContentWidth  )
    tabContentHeight := tab.GetContentHeight( tabContentHeight )
  }

  ; Build ordered tab name array; last tab is always "Hotkeys"
  ;tabList  := []
  ;tabIndex := 1
  ;while tabNames.Has( tabIndex )
  ;{
  ;  tabList.Push( tabNames[tabIndex] )
  ;  tabIndex++
  ;}
  HOTKEYS_TAB := tabList.Length + 1
  tabList.Push( "Hotkey Help" )

  ; ── Create Tab control ──
  ; Calculate tab height from max symbol rows
  ;maxRow := 1
  ;for i, sym in g_symbols
  ;{
  ;  if sym.row > maxRow
  ;  {
  ;    maxRow := sym.row
  ;  }
  ;}
  ;tabContentWidth  := g_LV_WIDTH
  ;tabContentHeight := maxRow * (g_symBtnSizeY + g_symBtnGap) + g_symBtnGap + 10

  ;tabContentWidth  := emojiTab.GetContentWidth(  tabContentWidth  )
  ;tabContentHeight := emojiTab.GetContentHeight( tabContentHeight )

  ; Use the larger of symbol area or hotkey list area
  LV_AREA_HEIGHT := g_hdrHeight + (g_LV_ROW_COUNT * 20) + 10
  if LV_AREA_HEIGHT > tabContentHeight
  {
    tabContentHeight := LV_AREA_HEIGHT
  }

  g_tabs := g_gui.Add( "Tab", "x5 y5 w" (tabContentWidth + 10) " h" (tabContentHeight + 30), tabList )

  ; ── Symbol Buttons (on their respective tab) ──
  g_tipMap := Map()
  ;symXOrg  := g_symOrgX
  ;symYOrg  := g_symOrgY
  ;
  ;g_gui.SetFont( g_fontSymSize, g_fontName )
  ;
  ;for i, sym in g_symbols
  ;{
  ;  g_tabs.UseTab( sym.tab )
  ;  x   := symXOrg + (sym.col - 1) * (g_symBtnSizeX + g_symBtnGap)
  ;  y   := symYOrg + (sym.row - 1) * (g_symBtnSizeY + g_symBtnGap)
  ;  w   := g_symBtnSizeX * sym.width + g_symBtnGap * (sym.width - 1)
  ;  h   := g_symBtnSizeY
  ;  tip := (sym.hotkey == "") ? sym.desc : (sym.desc "`n" sym.hotkey)
  ;  opt := "x" x " y" y " w" w " h" h
  ;  if sym.tab  = COMMENTS_TAB
  ;  {
  ;    opt .= " left"
  ;  }
  ;  ;hasImageFile := sym.Has( "imageFile" ) && ( sym.imageFile != "" )
  ;  ;hasImageFile := sym.imageFile != ""
  ;  ;if hasImageFile
  ;  ;{
  ;  ;  imagePath := A_ScriptDir "\\SupportFiles\\twemoji-assets\\" sym.imageFile
  ;  ;}
  ;  ;else
  ;  ;{
  ;  ;  hasImageFile := false
  ;  ;}
  ;  ;if hasImageFile
  ;  ;{
  ;  ;  if FileExist( imagePath )
  ;  ;  {
  ;  ;    opt .= " +0x200"
  ;  ;    btn := g_gui.Add( "Button", opt, "" )
  ;  ;    btn.SetImage( imagePath )
  ;  ;  }
  ;  ;  else
  ;  ;  {
  ;  ;    btn := g_gui.Add( "Button", opt, sym.char )
  ;  ;  }
  ;  ;}
  ;  ;else
  ;  {
  ;    btn := g_gui.Add( "Button", opt, sym.char )
  ;  }
  ;  btn.SetFont( g_fontSymSize, g_fontEmojiName )
  ;  g_tipMap[btn.Hwnd] := tip
  ;  btn.OnEvent( "Click", HelpMenu_SymbolClick.Bind( sym.action ) )
  ;}

  for tabIndex, tab in g_uiTabs
  {
    tab.AddControls( g_gui, g_tabs, tabIndex, g_tipMap )
  }

  g_gui.SetFont( g_fontSize " norm", g_fontName )

  SetTimer( HelpMenu_HoverCheck, 100 )


  ; Colored header bar using a Progress control as background
  g_tabs.UseTab( HOTKEYS_TAB )
  HEADER_FULL_WIDTH := g_COL_HOTKEY_WIDTH + g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN
  g_HeaderBg := g_gui.Add( "Progress", "x15 y35 w" HEADER_FULL_WIDTH " h" g_hdrHeight
                           " Background" g_HEADER_BG_COLOR " c" g_HEADER_BG_COLOR, 100 )

  g_gui.SetFont( g_fontSize " bold", g_fontName )
  g_HeaderHotkey := g_gui.Add( "Text",
               "x15 y35 w" g_COL_HOTKEY_WIDTH " h" g_hdrHeight
               " BackgroundTrans c" g_HEADER_TEXT_COLOR " +0x200",
               "  Hotkey" )
  g_HeaderDesc := g_gui.Add( "Text",
               "x+0 yp w" (g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN) " h" g_hdrHeight
               " BackgroundTrans c" g_HEADER_TEXT_COLOR " +0x200",
               "  Description" )
  g_gui.SetFont( g_fontSize " norm", g_fontName )

  g_LV := g_gui.Add( "ListView",
                     "x15 y+0 r" g_LV_ROW_COUNT " w" g_LV_WIDTH " Grid -Hdr",
                     ["Hotkey", "Description"] )
  g_LV.ModifyCol( 1, g_COL_HOTKEY_WIDTH )
  g_LV.ModifyCol( 2, g_COL_DESC_WIDTH )

  for item in g_HelpActions
  {
    g_LV.Add( , item.hotkey, item.desc )
  }


  g_LV.OnEvent( "DoubleClick", HelpMenu_RowAction )
  g_gui.OnEvent( "Escape", (*) => HelpMenu_Close() )
  g_gui.OnEvent( "Close",  (*) => HelpMenu_Close() )
  ;g_gui.OnEvent( "Size", HelpMenu_OnResize )

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
  SetTimer( HelpMenu_HoverCheck, 0 )
  ToolTip()
  if g_gui
  {
    g_gui.Destroy()
  }
  g_gui := ""
}

HelpMenu_OnResize( thisGui, minMax, w, h )
{
  global g_LV, g_HeaderHotkey, g_HeaderDesc, g_HeaderBg, g_tabs
  if minMax = -1  ; minimized
  {
    return
  }
  MARGIN := 5
  ; Resize the tab control to fill the window
  g_tabs.Move( , , w - MARGIN * 2, h - MARGIN * 2 )
  ; Resize the ListView within the Hotkeys tab
  lvWidth := w - MARGIN * 2 - 20
  g_LV.GetPos( , &lvY )
  lvHeight := h - lvY - MARGIN - 10
  if lvHeight < 50
  {
    lvHeight := 50
  }
  g_LV.Move( , , lvWidth, lvHeight )
  ; Resize description header to match
  g_HeaderHotkey.GetPos( , , &hkW )
  descWidth := lvWidth - hkW
  if descWidth < 50
  {
    descWidth := 50
  }
  g_HeaderDesc.Move( , , descWidth )
  g_HeaderBg.Move( , , lvWidth )
  ; Resize the Description column to fill remaining width
  g_LV.ModifyCol( 2, descWidth )
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
