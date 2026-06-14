; UIConstants.ahk - Constants and globals for UI

; ── App-wide globals ──
; Window state (gui, tabs, scrollbar, sizes, indicators, snap/drag state)
; lives on the g_hotkeyWnd instance created at the bottom of this file.
g_tipMap       := Map()
g_activeWindow := unset

g_wheelPendingSteps   := 0
g_wheelFlushScheduled := false
g_mouseWheelHook      := 0
g_mouseWheelHookProc  := 0

g_fontSize := "s10"
g_fontName := "Segoe UI"

g_useClipSend     := false
g_stripSendEmojis := false

; Set by HotkeyWindow.OnButtonDoubleClick (BN_DBLCLK) and consumed by
; SymbolClick after the symbol's text has been sent, so the newline always
; lands AFTER the text.
g_pendingNewline := false

g_iniPath := A_ScriptDir "\HenksHotkeys.ini"

g_uiTabs := [ SymbolsTabPage(),
              EmojisTabPage(),
              CommentsTabPage(),
              PromptsTabPage(),
              GreekTabPage(),
              RussianTabPage(),
              MiscTabPage(),
              ToolsTabPage(),
              SensitiveTabPage() ]

; The single helper-window instance. See the HotkeyWindow class in UI.ahk.
g_hotkeyWnd := HotkeyWindow()
