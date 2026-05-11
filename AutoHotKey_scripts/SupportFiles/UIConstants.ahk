; UIConstants.ahk - Constants and globals for UI

; ── Globals for the Help Menu ──
g_gui            := ""
g_guiHwndRaw     := 0
g_tipMap         := Map()
g_LV             := ""
g_HeaderHotkey   := ""
g_HeaderDesc     := ""
g_HeaderBg       := ""
g_tabs           := ""
g_activeWindow   := unset
g_wheelPendingSteps   := 0
g_wheelFlushScheduled := false
g_mouseWheelHook      := 0
g_mouseWheelHookProc  := 0

; ── Constants from ShowWindow ──
g_fontSize      := "s10"
g_fontName      := "Segoe UI"

g_hdrHeight         := 24
g_COL_HOTKEY_WIDTH  := 150
g_COL_DESC_WIDTH    := 280
g_RESIZE_H_MARGIN   := 20
g_LV_WIDTH          := g_COL_HOTKEY_WIDTH + g_COL_DESC_WIDTH + g_RESIZE_H_MARGIN
g_LV_ROW_COUNT      := 12
g_HEADER_BG_COLOR   := "4B3621"
g_HEADER_TEXT_COLOR := "FFFFFF"

g_fullW             := 0
g_fullH             := 0
g_shrinkBtn         := ""
g_expandBtn         := ""

g_iniPath           := A_ScriptDir "\HenksHotkeys.ini"

g_uiTabs := [ SymbolsTabPage(),
              EmojisTabPage(),
              CommentsTabPage(),
              PromptsTabPage(),
              GreekTabPage(),
              RussianTabPage() ]
