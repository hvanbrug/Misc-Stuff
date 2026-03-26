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

  HOTKEYS_TAB := tabList.Length + 1
  tabList.Push( "Hotkey Help" )

  ; Use the larger of symbol area or hotkey list area
  LV_AREA_HEIGHT := g_hdrHeight + (g_LV_ROW_COUNT * 20) + 10
  if LV_AREA_HEIGHT > tabContentHeight
  {
    tabContentHeight := LV_AREA_HEIGHT
  }

  g_tabs := g_gui.Add( "Tab", "x5 y5 w" (tabContentWidth + 10) " h" (tabContentHeight + 30), tabList )

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
  SetTimer( HelpMenu_HoverCheck, 0 )
  ToolTip()
  if g_gui
  {
    g_gui.Destroy()
  }
  g_gui := ""
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
