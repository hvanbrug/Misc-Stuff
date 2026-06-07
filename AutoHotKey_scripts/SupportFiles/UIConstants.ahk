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

g_wndX          := 0
g_wndY          := 0
g_fullW         := 0
g_fullH         := 0
g_frmSize       := 8
g_toggleSizeBtn := ""
g_snappedToTop  := false
g_snappedToFav  := false
g_dragOffsetX   := 0
g_dragOffsetY   := 0
g_favX          := ""
g_favY          := ""
g_useClipSend   := false
g_clipIndicator := ""

g_stripSendEmojis      := false
g_stripEmojisIndicator := ""

; Set by OnButtonDoubleClick (BN_DBLCLK) and consumed by SymbolClick after the
; symbol's text has been sent, so the newline always lands AFTER the text.
g_pendingNewline := false

; Cached physical-pixel X of the tab scrollbar in GUI client coords. The GUI
; width never changes, so this stays constant after ShowWindow sets it. Used
; by RelayoutForHeight when repositioning the scrollbar after a vertical
; resize.
g_tabScrollX := 0

; One-shot flag set by ToggleWindowSize around the programmatic Show() that
; collapses or expands the window. While true, OnGetMinMaxInfo skips its
; width-clamping so the new width can take effect; user-driven resize is
; unaffected.
g_allowWidthChange := false

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
