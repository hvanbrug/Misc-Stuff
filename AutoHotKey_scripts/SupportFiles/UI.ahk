; ═══════════════════════════════════════════════════════════════
; Ctrl + Shift + / => Show Help Menu with all available hotkeys
; ═══════════════════════════════════════════════════════════════
^+/::ShowHelpMenu()
ShowHelpMenu()
{
  global g_HelpGui,
         g_HelpActions,
         g_TipMap,
         g_Symbols,
         g_LV,
         g_HeaderHotkey,
         g_HeaderDesc,
         g_HeaderBg,
         g_Tabs
  global g_WINDOW_TITLE,
         g_FONT_SIZE,
         g_FONT_SYMBOL_SIZE,
         g_FONT_NAME,
         g_COL_HOTKEY_WIDTH,
         g_COL_DESC_WIDTH,
         g_RESIZE_H_MARGIN,
         g_LV_WIDTH,
         g_LV_ROW_COUNT,
         g_HEADER_BG_COLOR,
         g_HEADER_TEXT_COLOR,
         g_HEADER_HEIGHT,
         g_SYMBOL_BTN_SIZE_X,
         g_SYMBOL_BTN_SIZE_Y,
         g_SYMBOL_BTN_GAP,
         g_SYMBOL_X_ORIGIN,
         g_SYMBOL_Y_ORIGIN

  if WinExist( g_WINDOW_TITLE )
  {
    WinActivate
    return
  }

  ; ── Create GUI ──
  g_HelpGui := Gui( "+AlwaysOnTop +Resize", g_WINDOW_TITLE )

  ; ── Determine tab names from registered symbols ──
  tabNames := Map()
  global g_TabNames
  for i, sym in g_Symbols
  {
    if !tabNames.Has( sym.tab )
    {
      tabNames[sym.tab] := g_TabNames.Has( sym.tab ) ? g_TabNames[sym.tab] : "Symbols " sym.tab
    }
  }
  ; Build ordered tab name array; last tab is always "Hotkeys"
  tabList  := []
  tabIndex := 1
  while tabNames.Has( tabIndex )
  {
    tabList.Push( tabNames[tabIndex] )
    tabIndex++
  }
  HOTKEYS_TAB := tabList.Length + 1
  tabList.Push( "Hotkey Help" )

  ; ── Create Tab control ──
  ; Calculate tab height from max symbol rows
  maxRow := 1
  for i, sym in g_Symbols
  {
    if sym.row > maxRow
    {
      maxRow := sym.row
    }
  }
  TAB_CONTENT_HEIGHT := maxRow * (g_SYMBOL_BTN_SIZE_Y + g_SYMBOL_BTN_GAP) + g_SYMBOL_BTN_GAP + 10
  ; Use the larger of symbol area or hotkey list area
  LV_AREA_HEIGHT := g_HEADER_HEIGHT + (g_LV_ROW_COUNT * 20) + 10
  if LV_AREA_HEIGHT > TAB_CONTENT_HEIGHT
    TAB_CONTENT_HEIGHT := LV_AREA_HEIGHT

  g_Tabs := g_HelpGui.Add( "Tab3", "x5 y5 w" (g_LV_WIDTH + 10) " h" (TAB_CONTENT_HEIGHT + 30), tabList )

  ; ── Symbol Buttons (on their respective tab) ──
  g_TipMap := Map()
  SYMBOL_X_ORIGIN := g_SYMBOL_X_ORIGIN
  SYMBOL_Y_ORIGIN := g_SYMBOL_Y_ORIGIN

  g_HelpGui.SetFont( g_FONT_SYMBOL_SIZE, g_FONT_NAME )

  for i, sym in g_Symbols
  {
    g_Tabs.UseTab( sym.tab )
    x := SYMBOL_X_ORIGIN + (sym.col - 1) * (g_SYMBOL_BTN_SIZE_X + g_SYMBOL_BTN_GAP)
    y := SYMBOL_Y_ORIGIN + (sym.row - 1) * (g_SYMBOL_BTN_SIZE_Y + g_SYMBOL_BTN_GAP)
    tip := (sym.hotkey == "") ? sym.desc : (sym.desc "`n" sym.hotkey)
    btn := g_HelpGui.Add( "Button", "x" x " y" y " w" g_SYMBOL_BTN_SIZE_X " h" g_SYMBOL_BTN_SIZE_Y, sym.char )
    btn.SetFont( g_FONT_SYMBOL_SIZE, g_FONT_EMOJI_NAME )
    g_TipMap[btn.Hwnd] := tip
    btn.OnEvent( "Click", HelpMenu_SymbolClick.Bind( sym.action ) )
  }
  g_HelpGui.SetFont( g_FONT_SIZE " norm", g_FONT_NAME )

  SetTimer( HelpMenu_HoverCheck, 100 )

  ; ── Hotkey List (on last tab) ──
  ;g_Tabs.UseTab( "Hotkeys" )
  g_Tabs.UseTab( tabList[HOTKEYS_TAB] )

  ; Colored header bar using a Progress control as background
  HEADER_FULL_WIDTH := g_COL_HOTKEY_WIDTH + g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN
  g_HeaderBg := g_HelpGui.Add( "Progress", "x15 y35 w" HEADER_FULL_WIDTH " h" g_HEADER_HEIGHT
               " Background" g_HEADER_BG_COLOR " c" g_HEADER_BG_COLOR, 100 )

  g_HelpGui.SetFont( g_FONT_SIZE " bold", g_FONT_NAME )
  g_HeaderHotkey := g_HelpGui.Add( "Text",
               "x15 y35 w" g_COL_HOTKEY_WIDTH " h" g_HEADER_HEIGHT
               " BackgroundTrans c" g_HEADER_TEXT_COLOR " +0x200",
               "  Hotkey" )
  g_HeaderDesc := g_HelpGui.Add( "Text",
               "x+0 yp w" (g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN) " h" g_HEADER_HEIGHT
               " BackgroundTrans c" g_HEADER_TEXT_COLOR " +0x200",
               "  Description" )
  g_HelpGui.SetFont( g_FONT_SIZE " norm", g_FONT_NAME )

  g_LV := g_HelpGui.Add( "ListView",
                     "x15 y+0 r" g_LV_ROW_COUNT " w" g_LV_WIDTH " Grid -Hdr",
                     ["Hotkey", "Description"] )
  g_LV.ModifyCol( 1, g_COL_HOTKEY_WIDTH )
  g_LV.ModifyCol( 2, g_COL_DESC_WIDTH )

  for item in g_HelpActions
  {
    g_LV.Add( , item.hotkey, item.desc )
  }

  ; ── Done with tabbed controls ──
  g_Tabs.UseTab( 0 )

  g_LV.OnEvent( "DoubleClick", HelpMenu_RowAction )
  g_HelpGui.OnEvent( "Escape", (*) => HelpMenu_Close() )
  g_HelpGui.OnEvent( "Close",  (*) => HelpMenu_Close() )
  g_HelpGui.OnEvent( "Size", HelpMenu_OnResize )

  g_HelpGui.Show()
}

HelpMenu_Close()
{
  global g_HelpGui
  SetTimer( HelpMenu_HoverCheck, 0 )
  ToolTip()
  if g_HelpGui
  {
    g_HelpGui.Destroy()
  }
  g_HelpGui := ""
}

HelpMenu_OnResize( thisGui, minMax, w, h )
{
  global g_LV, g_HeaderHotkey, g_HeaderDesc, g_HeaderBg, g_Tabs
  if minMax = -1  ; minimized
  {
    return
  }
  MARGIN := 5
  ; Resize the tab control to fill the window
  g_Tabs.Move( , , w - MARGIN * 2, h - MARGIN * 2 )
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
  global g_TipMap
  static prevHwnd := 0
  MouseGetPos( , , &winHwnd, &ctrlHwnd, 2 )
  if g_TipMap.Has( ctrlHwnd )
  {
    if ctrlHwnd != prevHwnd
    {
      ToolTip( g_TipMap[ctrlHwnd] )
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
