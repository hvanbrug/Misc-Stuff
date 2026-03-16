; UIConstants.ahk - Constants and globals for UI

; ── Globals for the Help Menu ──
g_HelpGui        := ""
g_TipMap         := Map()
g_LV             := ""
g_HeaderHotkey   := ""
g_HeaderDesc     := ""
g_HeaderBg       := ""
g_Tabs           := ""

; ── Constants from ShowHelpMenu ──
g_WINDOW_TITLE      := "HenksScripts - Hotkey Reference"
g_FONT_SIZE         := "s10"
g_FONT_SYMBOL_SIZE  := "s14" ; "s18"
g_FONT_NAME         := "Segoe UI"
g_FONT_EMOJI_NAME   := "Segoe UI Emoji"
g_COL_HOTKEY_WIDTH  := 150
g_COL_DESC_WIDTH    := 280
g_RESIZE_H_MARGIN   := 20
g_LV_WIDTH          := g_COL_HOTKEY_WIDTH + g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN
g_LV_ROW_COUNT      := 12
g_HEADER_BG_COLOR   := "4B3621"
g_HEADER_TEXT_COLOR := "FFFFFF"
g_HEADER_HEIGHT     := 24
g_SYMBOL_BTN_SIZE_X := 35
g_SYMBOL_BTN_SIZE_Y := 35
g_SYMBOL_BTN_GAP    := 3
g_SYMBOL_X_ORIGIN   := 15
g_SYMBOL_Y_ORIGIN   := 35
