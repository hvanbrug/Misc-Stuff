; ═══════════════════════════════════════════════════════════════
; Ctrl + Shift + / => Show Help Menu with all available hotkeys
; ═══════════════════════════════════════════════════════════════
^+/::ShowHelpMenu()

; ── Globals for the Help Menu ──
g_HelpGui        := ""
g_TipMap         := Map()
g_LV             := ""
g_HeaderHotkey   := ""
g_HeaderDesc     := ""

ShowHelpMenu()
{
  global g_HelpGui, g_HelpActions, g_TipMap, g_Symbols, g_LV, g_HeaderHotkey, g_HeaderDesc

  WINDOW_TITLE      := "HenksScripts - Hotkey Reference"
  FONT_SIZE         := "s10"
  FONT_SYMBOL_SIZE  := "s14" ; "s18"
  FONT_NAME         := "Segoe UI"
  COL_HOTKEY_WIDTH  := 150
  COL_DESC_WIDTH    := 260
  RESIZE_H_MARGIN   := 20
  LV_WIDTH          := COL_HOTKEY_WIDTH + COL_DESC_WIDTH + RESIZE_H_MARGIN
  LV_ROW_COUNT      := 12
  HEADER_BG_COLOR   := "4B3621"
  HEADER_TEXT_COLOR := "FFFFFF"
  HEADER_HEIGHT     := 24
  SYMBOL_BTN_SIZE_X := 35
  SYMBOL_BTN_SIZE_Y := 35

  if WinExist( WINDOW_TITLE )
  {
    WinActivate
    return
  }

  ; ── Create GUI ──
  g_HelpGui := Gui( "+AlwaysOnTop +Resize", WINDOW_TITLE )

  ; ── Symbol Buttons (positioned by row/col from RegisterSymbol data) ──
  ; Tab stub: when tabs are added, uncomment and use sym.tab to assign buttons to tabs
  ; tabs := g_HelpGui.Add( "Tab3", "w" LV_WIDTH " h" (MAX_ROWS * (SYMBOL_BTN_SIZE + SYMBOL_BTN_GAP) + 40), ["Symbols"] )
  ; tabs.UseTab( sym.tab )
  ; ... after all buttons: tabs.UseTab( 0 )

  g_TipMap := Map()
  SYMBOL_BTN_GAP  := 3
  SYMBOL_X_ORIGIN := 10
  SYMBOL_Y_ORIGIN := 10
  g_HelpGui.SetFont( FONT_SYMBOL_SIZE, FONT_NAME )

  maxRow := 1
  for i, sym in g_Symbols
  {
    x := SYMBOL_X_ORIGIN + (sym.col - 1) * (SYMBOL_BTN_SIZE_X + SYMBOL_BTN_GAP)
    y := SYMBOL_Y_ORIGIN + (sym.row - 1) * (SYMBOL_BTN_SIZE_Y + SYMBOL_BTN_GAP)
    tip := sym.desc "`n" sym.hotkey
    btn := g_HelpGui.Add( "Button", "x" x " y" y " w" SYMBOL_BTN_SIZE_X " h" SYMBOL_BTN_SIZE_Y, sym.char )
    g_TipMap[btn.Hwnd] := tip
    btn.OnEvent( "Click", HelpMenu_SymbolClick.Bind( sym.action ) )
    if sym.row > maxRow
    {
      maxRow := sym.row
    }
  }
  g_HelpGui.SetFont( FONT_SIZE " norm", FONT_NAME )

  symbolsBottomY := SYMBOL_Y_ORIGIN + maxRow * (SYMBOL_BTN_SIZE_Y + SYMBOL_BTN_GAP)

  SetTimer( HelpMenu_HoverCheck, 100 )

  ; ── Hotkey List ──
  g_HelpGui.SetFont( FONT_SIZE " bold", FONT_NAME )

  g_HeaderHotkey := g_HelpGui.Add( "Text",
               "x10 y" symbolsBottomY " w" COL_HOTKEY_WIDTH " h" HEADER_HEIGHT
               " Background" HEADER_BG_COLOR " c" HEADER_TEXT_COLOR " 0x200 vHeaderHotkey",
               "  Hotkey" )
  g_HeaderDesc := g_HelpGui.Add( "Text",
               "x+0 yp w" (COL_DESC_WIDTH + RESIZE_H_MARGIN) " h" HEADER_HEIGHT
               " Background" HEADER_BG_COLOR " c" HEADER_TEXT_COLOR " 0x200 vHeaderDesc",
               "  Description" )
  g_HelpGui.SetFont( FONT_SIZE " norm", FONT_NAME )

  g_LV := g_HelpGui.Add( "ListView",
                     "x10 y+0 r" LV_ROW_COUNT " w" LV_WIDTH " Grid -Hdr",
                     ["Hotkey", "Description"] )
  g_LV.ModifyCol( 1, COL_HOTKEY_WIDTH )
  g_LV.ModifyCol( 2, COL_DESC_WIDTH )

  for item in g_HelpActions
  {
    g_LV.Add( , item.hotkey, item.desc )
  }

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
  global g_LV, g_HeaderHotkey, g_HeaderDesc
  if minMax = -1  ; minimized
  {
    return
  }
  MARGIN := 10
  lvWidth := w - MARGIN * 2
  ; Get the current Y position of the ListView so we know the fixed offset
  g_LV.GetPos( , &lvY )
  lvHeight := h - lvY - MARGIN
  if lvHeight < 50
  {
    lvHeight := 50
  }
  g_LV.Move( , , lvWidth, lvHeight )
  ; Resize headers to match — split proportionally
  g_HeaderHotkey.GetPos( , , &hkW )
  descWidth := lvWidth - hkW
  if descWidth < 50
  {
    descWidth := 50
  }
  g_HeaderDesc.Move( , , descWidth )
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
