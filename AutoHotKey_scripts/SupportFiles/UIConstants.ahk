; UIConstants.ahk - Constants and globals for UI

; ── Globals for the Help Menu ──
g_gui            := ""
g_tipMap         := Map()
g_LV             := ""
g_HeaderHotkey   := ""
g_HeaderDesc     := ""
g_HeaderBg       := ""
g_tabs           := ""

; ── Constants from ShowHelpMenu ──
g_fontSize      := "s10"
;g_fontSymSize   := "s14"
g_fontName      := "Segoe UI"
;g_fontEmojiName := "Segoe UI Emoji"
;g_symOrgX       := 15
;g_symOrgY       := 35
;g_symBtnSizeX   := 35
;g_symBtnSizeY   := 35
;g_symBtnGap     := 3

g_hdrHeight         := 24
g_COL_HOTKEY_WIDTH  := 150
g_COL_DESC_WIDTH    := 280
g_RESIZE_H_MARGIN   := 20
g_LV_WIDTH          := g_COL_HOTKEY_WIDTH + g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN
g_LV_ROW_COUNT      := 12
g_HEADER_BG_COLOR   := "4B3621"
g_HEADER_TEXT_COLOR := "FFFFFF"

g_uiTabs := [ SymbolsTabPage(),
              EmojisTabPage(),
              CommentsTabPage(),
              GreekTabPage(),
              RussianTabPage() ]
