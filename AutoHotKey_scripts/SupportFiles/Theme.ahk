; Theme.ahk — light/dark theming that follows the Windows app theme.
;
; Pragmatic, OS-level theming decided once at startup (see HotkeyWindow.Show):
; we read the system "AppsUseLightTheme" setting and, when it is dark, enable
; the undocumented per-window dark mode and apply the "DarkMode_Explorer"
; visual style to the standard controls (buttons, tab control, scrollbar). The
; window/panel backgrounds and indicator text are coloured from a small grey
; palette. In light mode every Theme call is a no-op, so the light appearance
; is left exactly as before.

class Theme
{
  static m_initialized := false
  static m_isDark      := false
  static m_panelBrush  := 0
  static m_darkColorCb := 0
  static m_darkTabCb   := 0
  static m_ownerBtnCb  := 0

  ; TEMP (diagnostics): counts how many times the tab control's NM_CUSTOMDRAW
  ; prepaint actually reached our handler. Read by ThemeDiagnostics().
  static m_tabCustomDrawHits := 0

  ; Dark palette. Greys are used so the RGB ordering Gui.BackColor wants and the
  ; BGR ordering COLORREF (Set*Color) wants are identical — no conversion.
  static DARK_BG   := 0x202020   ; window / panel background
  static DARK_TEXT := 0xDCDCDC   ; foreground text / glyphs

  ; Owner-drawn button palette (buttons sit slightly above the panel so they read
  ; as raised). Win32 push buttons have no dark visual style, so we draw them.
  static BTN_BG         := 0x3A3A3A
  static BTN_BG_PRESSED := 0x4A4A4A
  static BTN_BORDER     := 0x555555

  ; Cached GDI brushes keyed by colour, and per-button owner-draw info keyed by
  ; hwnd. Both live for the process lifetime.
  static m_brushes   := Map()
  static m_ownerDraw := Map()

  ; Read the current Windows app theme once and cache it. AppsUseLightTheme is
  ; 1 for light and 0 for dark; a missing value (older Windows) means light.
  static Init()
  {
    if( Theme.m_initialized )
    {
      return
    }
    Theme.m_initialized := true

    light := 1
    try
    {
      light := RegRead( "HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                        "AppsUseLightTheme", 1 )
    }
    Theme.m_isDark := (light = 0)
  }

  static IsDark()
  {
    Theme.Init()
    return Theme.m_isDark
  }

  static BackColorHex()
  {
    return Format( "{:06X}", Theme.DARK_BG )
  }

  ; A cached solid brush in the given colour (process-lifetime).
  static Brush( color )
  {
    if( !Theme.m_brushes.Has( color ) )
    {
      Theme.m_brushes[color] := DllCall( "gdi32\CreateSolidBrush", "UInt", color, "Ptr" )
    }
    return Theme.m_brushes[color]
  }

  ; A cached solid brush in the panel background colour, used by the dark
  ; WM_CTLCOLORSTATIC handler.
  static PanelBrush()
  {
    return Theme.Brush( Theme.DARK_BG )
  }

  ; A cached subclass callback that paints child static backgrounds dark.
  ; Reused for every window that needs it; freed implicitly at process exit.
  static DarkColorCallback()
  {
    if( !Theme.m_darkColorCb )
    {
      Theme.m_darkColorCb := CallbackCreate( _DarkStaticColor, , 6 )
    }
    return Theme.m_darkColorCb
  }

  ; Resolve an export of uxtheme.dll by ordinal. The dark-mode helpers are
  ; exported by ordinal only (no name), and AHK's DllCall("uxtheme\#NNN") form
  ; does NOT resolve those — it throws "nonexistent function". So we look the
  ; address up by ordinal via GetProcAddress and cache it. Returns 0 on failure.
  static UxthemeProc( ordinal )
  {
    static hMod  := 0
    static cache := Map()

    if( cache.Has( ordinal ) )
    {
      return cache[ordinal]
    }
    if( !hMod )
    {
      hMod := DllCall( "GetModuleHandle", "Str", "uxtheme", "Ptr" )
      if( !hMod )
      {
        hMod := DllCall( "LoadLibrary", "Str", "uxtheme.dll", "Ptr" )
      }
    }
    ; GetProcAddress treats a small integer lpProcName as an ordinal.
    addr := hMod ? DllCall( "GetProcAddress", "Ptr", hMod, "Ptr", ordinal, "Ptr" ) : 0
    cache[ordinal] := addr
    return addr
  }

  ; AllowDarkModeForWindow( hwnd, TRUE ) via ordinal #133.
  static AllowDarkForWindow( hwnd )
  {
    proc := Theme.UxthemeProc( 133 )
    if( proc )
    {
      try DllCall( proc, "Ptr", hwnd, "Int", true, "Int" )
    }
  }

  ; Put the whole app into dark mode. Must run before any control is themed,
  ; and is the reason DarkMode_Explorer actually renders dark: SetPreferredAppMode
  ; alone is not enough — RefreshImmersiveColorPolicyState has to be called too,
  ; or themed controls keep painting light. Runs its body only once.
  static EnableAppDarkMode()
  {
    static done := false
    if( done || !Theme.IsDark() )
    {
      return
    }
    done := true

    ; uxtheme exposes these only as ordinals (no public names):
    ;   #135 SetPreferredAppMode( AllowDark = 1 )
    ;   #104 RefreshImmersiveColorPolicyState()
    ;   #136 FlushMenuThemes()
    if( proc := Theme.UxthemeProc( 135 ) )
    {
      try DllCall( proc, "Int", 1, "Int" )
    }
    if( proc := Theme.UxthemeProc( 104 ) )
    {
      try DllCall( proc )
    }
    if( proc := Theme.UxthemeProc( 136 ) )
    {
      try DllCall( proc )
    }
  }

  ; Tell DWM to render the window frame/border dark. Without this the (thick)
  ; resize border shows as white, most visibly when the window is inactive.
  static ApplyDarkFrame( hwnd )
  {
    if( !Theme.IsDark() || !hwnd )
    {
      return
    }

    ; DWMWA_USE_IMMERSIVE_DARK_MODE = 20 (Win10 2004+/Win11): dark frame colours.
    enable := Buffer( 4, 0 )
    NumPut( "Int", 1, enable )
    try DllCall( "dwmapi\DwmSetWindowAttribute", "Ptr", hwnd, "UInt", 20, "Ptr", enable, "UInt", 4 )

    ; DWMWA_BORDER_COLOR = 34 (Win11): tint the border to the panel colour so it
    ; blends instead of showing as a white frame.
    border := Buffer( 4, 0 )
    NumPut( "UInt", Theme.DARK_BG, border )
    try DllCall( "dwmapi\DwmSetWindowAttribute", "Ptr", hwnd, "UInt", 34, "Ptr", border, "UInt", 4 )

    ; DWMWA_NCRENDERING_POLICY = 2, DWMNCRP_DISABLED = 1: stop DWM from drawing
    ; the non-client frame at all. WM_NCPAINT suppression didn't remove the thick
    ; border, which means DWM is compositing it — this targets that.
    ncPolicy := Buffer( 4, 0 )
    NumPut( "Int", 1, ncPolicy )
    try DllCall( "dwmapi\DwmSetWindowAttribute", "Ptr", hwnd, "UInt", 2, "Ptr", ncPolicy, "UInt", 4 )
  }

  ; Enable dark mode for the given top-level GUI and set its dark background.
  ; No-op in light mode.
  static EnableDarkMode( gui )
  {
    if( !Theme.IsDark() || !IsObject( gui ) )
    {
      return
    }

    Theme.EnableAppDarkMode()
    Theme.AllowDarkForWindow( gui.Hwnd )
    Theme.ApplyDarkFrame( gui.Hwnd )

    gui.BackColor := Theme.BackColorHex()
  }

  ; Apply the dark control theme so a standard control (button, tab, scrollbar)
  ; renders dark. No-op in light mode.
  static ThemeControl( hwnd )
  {
    if( !Theme.IsDark() || !hwnd )
    {
      return
    }
    Theme.EnableAppDarkMode()
    Theme.AllowDarkForWindow( hwnd )
    DllCall( "uxtheme\SetWindowTheme", "Ptr", hwnd, "Str", "DarkMode_Explorer", "Ptr", 0 )
    DllCall( "SendMessageW", "Ptr", hwnd, "UInt", 0x031A, "Ptr", 0, "Ptr", 0 )  ; WM_THEMECHANGED
  }

  ; A cached subclass callback that stops an owner-drawn button from erasing its
  ; (light) background before WM_DRAWITEM, which otherwise flickers white on
  ; resize. We paint the whole button in WM_DRAWITEM, so the erase is unneeded.
  static OwnerButtonCallback()
  {
    if( !Theme.m_ownerBtnCb )
    {
      Theme.m_ownerBtnCb := CallbackCreate( _OwnerButtonNoErase, , 6 )
    }
    return Theme.m_ownerBtnCb
  }

  ; Subclass a container static so its child statics paint on the dark panel
  ; background instead of the default light grey. No-op in light mode.
  static DarkenStaticBackground( hwnd )
  {
    if( !Theme.IsDark() || !hwnd )
    {
      return
    }
    DllCall( "comctl32\SetWindowSubclass",
             "Ptr",  hwnd,
             "Ptr",  Theme.DarkColorCallback(),
             "UPtr", 2,    ; subclass id (the TabPage content-panel forwarder uses 1)
             "UPtr", 0 )
  }

  ; A cached subclass callback that fully paints a tab control dark.
  static DarkTabCallback()
  {
    if( !Theme.m_darkTabCb )
    {
      Theme.m_darkTabCb := CallbackCreate( _DarkTabProc, , 6 )
    }
    return Theme.m_darkTabCb
  }

  ; Take over a tab control's painting so the whole control (tab strip, the
  ; gaps, and the labels) renders dark — the standard control has no dark style
  ; and its label text / frame stay light otherwise. No-op in light mode.
  static DarkenTabControl( hwnd )
  {
    if( !Theme.IsDark() || !hwnd )
    {
      return
    }
    DllCall( "comctl32\SetWindowSubclass",
             "Ptr",  hwnd,
             "Ptr",  Theme.DarkTabCallback(),
             "UPtr", 3,    ; subclass id distinct from the static-colour one (2)
             "UPtr", 0 )
    DllCall( "InvalidateRect", "Ptr", hwnd, "Ptr", 0, "Int", true )
  }

  ; Paint a tab control dark: fill the whole control, then for each tab draw a
  ; dark cell (the selected one lighter) with its label in the light foreground.
  ; Called from the tab subclass on WM_PAINT.
  static PaintTabControl( hwnd )
  {
    static WM_GETFONT       := 0x0031
    static TCM_GETITEMCOUNT := 0x1304
    static TCM_GETCURSEL    := 0x130B
    static TCM_GETITEMRECT  := 0x130A
    static TCM_GETITEMW     := 0x133C
    static TCIF_TEXT        := 0x0001
    static TRANSPARENT      := 1
    static DT_CENTER_VC     := 0x25   ; DT_CENTER|DT_VCENTER|DT_SINGLELINE

    ps  := Buffer( 72, 0 )
    hdc := DllCall( "user32\BeginPaint", "Ptr", hwnd, "Ptr", ps, "Ptr" )
    if( !hdc )
    {
      return
    }

    rc := Buffer( 16, 0 )
    DllCall( "GetClientRect", "Ptr", hwnd, "Ptr", rc )
    DllCall( "user32\FillRect", "Ptr", hdc, "Ptr", rc, "Ptr", Theme.Brush( Theme.DARK_BG ) )

    count := DllCall( "SendMessageW", "Ptr", hwnd, "UInt", TCM_GETITEMCOUNT, "Ptr", 0, "Ptr", 0, "Ptr" )
    cur   := DllCall( "SendMessageW", "Ptr", hwnd, "UInt", TCM_GETCURSEL,    "Ptr", 0, "Ptr", 0, "Ptr" )

    hFont   := DllCall( "SendMessageW", "Ptr", hwnd, "UInt", WM_GETFONT, "Ptr", 0, "Ptr", 0, "Ptr" )
    oldFont := hFont ? DllCall( "SelectObject", "Ptr", hdc, "Ptr", hFont, "Ptr" ) : 0
    DllCall( "gdi32\SetBkMode", "Ptr", hdc, "Int", TRANSPARENT )

    itemRc := Buffer( 16, 0 )
    tci    := Buffer( 40, 0 )
    txt    := Buffer( 256 * 2, 0 )

    i := 0
    while( i < count )
    {
      DllCall( "SendMessageW", "Ptr", hwnd, "UInt", TCM_GETITEMRECT, "Ptr", i, "Ptr", itemRc )

      NumPut( "UInt", TCIF_TEXT, tci, 0 )    ; mask
      NumPut( "Ptr",  txt.Ptr,   tci, 16 )   ; pszText
      NumPut( "Int",  256,       tci, 24 )   ; cchTextMax
      DllCall( "SendMessageW", "Ptr", hwnd, "UInt", TCM_GETITEMW, "Ptr", i, "Ptr", tci )

      isSel := (i = cur)
      DllCall( "user32\FillRect", "Ptr", hdc, "Ptr", itemRc,
               "Ptr", Theme.Brush( isSel ? Theme.BTN_BG_PRESSED : Theme.BTN_BG ) )
      DllCall( "gdi32\SetTextColor", "Ptr", hdc, "UInt", Theme.DARK_TEXT )
      DllCall( "user32\DrawTextW", "Ptr", hdc, "Ptr", txt, "Int", -1, "Ptr", itemRc, "UInt", DT_CENTER_VC )

      i += 1
    }

    if( oldFont )
    {
      DllCall( "SelectObject", "Ptr", hdc, "Ptr", oldFont )
    }
    DllCall( "user32\EndPaint", "Ptr", hwnd, "Ptr", ps )
  }

  ; Set a control's text colour to the dark-theme foreground. No-op in light.
  static ApplyTextColor( ctrl )
  {
    if( !Theme.IsDark() || !IsObject( ctrl ) )
    {
      return
    }
    ctrl.SetFont( "c" Format( "{:06X}", Theme.DARK_TEXT ) )
  }

  ; Convert a text button to owner-draw and register how to paint it, so we can
  ; draw a dark face + light text ourselves (Win32 push buttons have no dark
  ; visual style). No-op in light mode. `align` is "left" or "center"; it should
  ; match how the button's text was created. Not for image/bitmap buttons.
  static MakeOwnerDrawn( btn, align := "center" )
  {
    if( !Theme.IsDark() || !IsObject( btn ) || !btn.HasProp( "Hwnd" ) || !btn.Hwnd )
    {
      return
    }
    hwnd := btn.Hwnd
    ; Replace the button-type nibble with BS_OWNERDRAW, keeping the other flags
    ; (BS_NOTIFY for double-click, etc.).
    RemoveWindowStyle( hwnd, BS_TYPEMASK,  false )
    AddWindowStyle(    hwnd, BS_OWNERDRAW, false )

    ; Suppress the button's own background erase so it doesn't flash white before
    ; our WM_DRAWITEM paint during a resize.
    DllCall( "comctl32\SetWindowSubclass", "Ptr", hwnd, "Ptr", Theme.OwnerButtonCallback(), "UPtr", 4, "UPtr", 0 )

    Theme.m_ownerDraw[hwnd] := { align: align }
    DllCall( "InvalidateRect", "Ptr", hwnd, "Ptr", 0, "Int", true )
  }

  ; Forget all registered owner-drawn buttons. Called when the window closes so
  ; destroyed button hwnds can't be matched against if Windows reuses them.
  static ClearOwnerDraw()
  {
    Theme.m_ownerDraw.Clear()
  }

  ; Paint one owner-drawn button from a WM_DRAWITEM DRAWITEMSTRUCT pointer.
  ; Returns true if the button was one of ours (and was painted), false otherwise
  ; so the caller can fall through to default handling.
  static DrawOwnerButton( lParam )
  {
    static ODS_SELECTED := 0x0001
    static ODS_FOCUS    := 0x0010
    static TRANSPARENT  := 1
    ; DrawText flags: DT_SINGLELINE|DT_VCENTER|DT_END_ELLIPSIS plus DT_CENTER/DT_LEFT.
    static DT_CENTER_VC := 0x8025   ; DT_CENTER(1)|DT_VCENTER(4)|DT_SINGLELINE(0x20)|DT_END_ELLIPSIS(0x8000)
    static DT_LEFT_VC   := 0x8024   ; DT_LEFT(0) |DT_VCENTER(4)|DT_SINGLELINE(0x20)|DT_END_ELLIPSIS(0x8000)

    ; DRAWITEMSTRUCT (x64) offsets: itemState 16, hwndItem 24, hDC 32, rcItem 40.
    hwndItem := NumGet( lParam, 24, "Ptr" )
    if( !Theme.m_ownerDraw.Has( hwndItem ) )
    {
      return false
    }
    ; The light themed buttons rendered with an inset/rounded margin, so they
    ; looked smaller than their rect with a clear gap between them. We fill the
    ; whole cell with the panel colour (= the gap) and then paint the face inset
    ; by this margin, so the spacing matches what it was before.
    static MARGIN := 2

    info  := Theme.m_ownerDraw[hwndItem]
    state := NumGet( lParam, 16, "UInt" )
    hdc   := NumGet( lParam, 32, "Ptr" )
    rcPtr := lParam + 40
    pressed := state & ODS_SELECTED

    ; Gap (the cell margin) in the panel colour, then the inset button face.
    DllCall( "user32\FillRect", "Ptr", hdc, "Ptr", rcPtr, "Ptr", Theme.Brush( Theme.DARK_BG ) )

    face := Buffer( 16, 0 )
    NumPut( "Int", NumGet( lParam, 40, "Int" ) + MARGIN, face, 0 )
    NumPut( "Int", NumGet( lParam, 44, "Int" ) + MARGIN, face, 4 )
    NumPut( "Int", NumGet( lParam, 48, "Int" ) - MARGIN, face, 8 )
    NumPut( "Int", NumGet( lParam, 52, "Int" ) - MARGIN, face, 12 )

    DllCall( "user32\FillRect",  "Ptr", hdc, "Ptr", face,
             "Ptr", Theme.Brush( pressed ? Theme.BTN_BG_PRESSED : Theme.BTN_BG ) )
    DllCall( "user32\FrameRect", "Ptr", hdc, "Ptr", face, "Ptr", Theme.Brush( Theme.BTN_BORDER ) )

    ; Text, in the button's own font and colour.
    len := DllCall( "GetWindowTextLength", "Ptr", hwndItem, "Int" )
    if( len > 0 )
    {
      buf := Buffer( (len + 1) * 2, 0 )
      DllCall( "GetWindowTextW", "Ptr", hwndItem, "Ptr", buf, "Int", len + 1 )

      hFont   := DllCall( "SendMessageW", "Ptr", hwndItem, "UInt", 0x31, "Ptr", 0, "Ptr", 0, "Ptr" )  ; WM_GETFONT
      oldFont := hFont ? DllCall( "SelectObject", "Ptr", hdc, "Ptr", hFont, "Ptr" ) : 0

      DllCall( "gdi32\SetBkMode",    "Ptr", hdc, "Int",  TRANSPARENT )
      DllCall( "gdi32\SetTextColor", "Ptr", hdc, "UInt", Theme.DARK_TEXT )
      fmt := (info.align = "left") ? DT_LEFT_VC : DT_CENTER_VC
      DllCall( "user32\DrawTextW", "Ptr", hdc, "Ptr", buf, "Int", -1, "Ptr", face, "UInt", fmt )

      if( oldFont )
      {
        DllCall( "SelectObject", "Ptr", hdc, "Ptr", oldFont )
      }
    }

    if( state & ODS_FOCUS )
    {
      DllCall( "user32\DrawFocusRect", "Ptr", hdc, "Ptr", face )
    }
    return true
  }
}

; Subclass procedure: when a container receives WM_CTLCOLORSTATIC for a child
; static, hand back the dark panel brush and matching text/background colours so
; the child paints dark. Everything else falls through to the default handling.
_DarkStaticColor( hWnd, uMsg, wParam, lParam, uIdSubclass, dwRefData )
{
  static WM_CTLCOLORSTATIC := 0x0138
  if( uMsg = WM_CTLCOLORSTATIC )
  {
    DllCall( "gdi32\SetBkColor",   "Ptr", wParam, "UInt", Theme.DARK_BG   )
    DllCall( "gdi32\SetTextColor", "Ptr", wParam, "UInt", Theme.DARK_TEXT )
    return Theme.PanelBrush()
  }
  return DllCall( "comctl32\DefSubclassProc",
                  "Ptr",  hWnd,
                  "UInt", uMsg,
                  "Ptr",  wParam,
                  "Ptr",  lParam,
                  "Ptr" )
}

; Subclass procedure for owner-drawn buttons: swallow WM_ERASEBKGND (we paint
; the whole button in WM_DRAWITEM, so the default light erase is unneeded and
; only causes a white flash on resize). Everything else is left to default.
_OwnerButtonNoErase( hWnd, uMsg, wParam, lParam, uIdSubclass, dwRefData )
{
  static WM_ERASEBKGND := 0x0014
  if( uMsg = WM_ERASEBKGND )
  {
    return 1
  }
  return DllCall( "comctl32\DefSubclassProc",
                  "Ptr",  hWnd,
                  "UInt", uMsg,
                  "Ptr",  wParam,
                  "Ptr",  lParam,
                  "Ptr" )
}

; Subclass procedure for the tab control. Paints the control dark on WM_PAINT,
; swallows the default (light) background erase, and hands the dark brush to the
; child clip panels via WM_CTLCOLORSTATIC. Everything else is left to default.
_DarkTabProc( hWnd, uMsg, wParam, lParam, uIdSubclass, dwRefData )
{
  static WM_PAINT          := 0x000F
  static WM_ERASEBKGND     := 0x0014
  static WM_CTLCOLORSTATIC := 0x0138

  if( uMsg = WM_CTLCOLORSTATIC )
  {
    DllCall( "gdi32\SetBkColor",   "Ptr", wParam, "UInt", Theme.DARK_BG   )
    DllCall( "gdi32\SetTextColor", "Ptr", wParam, "UInt", Theme.DARK_TEXT )
    return Theme.PanelBrush()
  }
  if( uMsg = WM_ERASEBKGND )
  {
    return 1   ; background is painted in WM_PAINT; skip the default light erase
  }
  if( uMsg = WM_PAINT )
  {
    Theme.PaintTabControl( hWnd )
    return 0
  }
  return DllCall( "comctl32\DefSubclassProc",
                  "Ptr",  hWnd,
                  "UInt", uMsg,
                  "Ptr",  wParam,
                  "Ptr",  lParam,
                  "Ptr" )
}

; ── TEMP: dark-mode diagnostics ──────────────────────────────────────────────
; Run from the tray / window right-click menu AFTER the window is open. Probes
; each dark-mode mechanism live and reports whether the call resolved, what it
; returned, and whether the tab custom-draw fired. Remove once we know which
; layer is failing.

; Run a closure, returning its value or "ERR(...)" if the call threw (e.g. the
; uxtheme ordinal could not be resolved on this Windows build).
_Probe( fn )
{
  try
  {
    return fn.Call()
  }
  catch Error as e
  {
    return "ERR(" e.Message ")"
  }
}

; Probe a DwmSetWindowAttribute call, returning its HRESULT (0 = S_OK) or
; "ERR(...)" if the call itself failed to resolve.
_ProbeDwm( hwnd, attr, value )
{
  buf := Buffer( 4, 0 )
  NumPut( "UInt", value, buf )
  try
  {
    return DllCall( "dwmapi\DwmSetWindowAttribute", "Ptr", hwnd, "UInt", attr, "Ptr", buf, "UInt", 4, "Int" )
  }
  catch Error as e
  {
    return "ERR(" e.Message ")"
  }
}

ThemeDiagnostics()
{
  global g_hotkeyWnd

  if( !IsObject( g_hotkeyWnd ) || !g_hotkeyWnd.IsCreated() )
  {
    MsgBox( "Open the Henk's Hotkeys window first, then run diagnostics.", "Theme diagnostics" )
    return
  }

  gui     := g_hotkeyWnd.m_gui
  btnHwnd := IsObject( g_hotkeyWnd.m_toggleSizeBtn ) ? g_hotkeyWnd.m_toggleSizeBtn.Hwnd : 0

  raw := "(missing)"
  try raw := RegRead( "HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme" )

  ; HRESULT-returning calls: 0 = S_OK. Ordinal calls: a number means it resolved.
  setTheme := _Probe( () => DllCall( "uxtheme\SetWindowTheme",
                                     "Ptr", btnHwnd, "Str", "DarkMode_Explorer", "Ptr", 0, "Int" ) )

  imm    := _ProbeDwm( gui.Hwnd, 20, 1 )
  border := _ProbeDwm( gui.Hwnd, 34, Theme.DARK_BG )

  p132 := Theme.UxthemeProc( 132 )
  p135 := Theme.UxthemeProc( 135 )
  p104 := Theme.UxthemeProc( 104 )
  p133 := Theme.UxthemeProc( 133 )
  shouldDark := p132 ? _Probe( () => DllCall( p132, "Int" ) ) : "addr=0"
  appMode    := p135 ? _Probe( () => DllCall( p135, "Int", 1, "Int" ) ) : "addr=0"
  refresh    := p104 ? _Probe( () => DllCall( p104 ) ) : "addr=0"
  allowWin   := p133 ? _Probe( () => DllCall( p133, "Ptr", gui.Hwnd, "Int", true, "Int" ) ) : "addr=0"

  report := "OS version: " A_OSVersion "`n"
  report .= "AppsUseLightTheme = " raw "   (Theme.IsDark() = " (Theme.IsDark() ? "true" : "false") ")`n`n"
  report .= "uxtheme ordinals (a number = resolved & returned that; ERR = call failed):`n"
  report .= "  #132 ShouldAppsUseDarkMode  = " shouldDark "`n"
  report .= "  #135 SetPreferredAppMode(1) = " appMode "`n"
  report .= "  #104 RefreshImmersiveColor  = " refresh "`n"
  report .= "  #133 AllowDarkForWindow     = " allowWin "`n`n"
  report .= "SetWindowTheme(button, DarkMode_Explorer) HRESULT = " setTheme "   (0 = S_OK)`n"
  report .= "DwmSetWindowAttribute(20 immersive dark)  HRESULT = " imm "   (0 = S_OK)`n"
  report .= "DwmSetWindowAttribute(34 border color)    HRESULT = " border "   (0 = S_OK)`n`n"
  report .= "Tab NM_CUSTOMDRAW reached our handler: " Theme.m_tabCustomDrawHits " time(s)`n"
  report .= "  (> 0 means the tab sends custom-draw and our hook runs)"

  MsgBox( report, "Theme diagnostics" )
}
