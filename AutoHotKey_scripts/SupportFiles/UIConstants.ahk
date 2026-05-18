; UIConstants.ahk - Constants and globals for UI

; ── Globals for the Help Menu ──
g_gui          := ""
g_guiVisible   := false
g_guiHwndRaw   := 0
g_tipMap       := Map()
g_tabs         := ""
g_activeWindow := unset

g_wheelPendingSteps   := 0
g_wheelFlushScheduled := false
g_mouseWheelHook      := 0
g_mouseWheelHookProc  := 0

g_fontSize := "s10"
g_fontName := "Segoe UI"

g_wndX      := 0
g_wndY      := 0
g_fullW     := 0
g_fullH     := 0
g_shrinkBtn := ""
g_expandBtn := ""

g_iniPath := A_ScriptDir "\HenksHotkeys.ini"

g_uiTabs := [ SymbolsTabPage(),
              EmojisTabPage(),
              CommentsTabPage(),
              PromptsTabPage(),
              GreekTabPage(),
              RussianTabPage(),
              SensitiveTabPage() ]
