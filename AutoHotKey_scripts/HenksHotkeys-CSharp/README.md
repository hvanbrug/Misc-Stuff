# Henk's Hotkeys — C# / WinForms port

A faithful C# (.NET 9, WinForms) rewrite of the AutoHotkey v2 application in the
parent folder (`HenksHotkeys.ahk` + `SupportFiles\*.ahk`). It is an always-on-top,
frameless, dark-aware helper window with tabs of buttons that send symbols,
emojis, ready-made comments, prompt fragments, Greek/Russian letters and other
text to whatever window was active, plus global hotkeys and a tray icon.

## Build & run

```
cd HenksHotkeys-CSharp
dotnet build -c Release
dotnet run -c Release
```

The build copies the Twemoji PNG set (`..\Images\Twemoji\*.png`) and the app icon
next to the executable; the Emojis tab loads those images at runtime.

## What's implemented (1:1 with the AHK version)

- **Tabs**: Symbols, Emojis, Comments, Prompt Helpers, Greek, Russian, Misc,
  Tools, Sensitive — same buttons, layout maths, hotkeys and tooltips.
- **Emojis tab**: Twemoji PNG images composited on a grey backdrop, with a glyph
  fallback when an image is missing.
- **Sending**: types to the previously-active window via `SendInput` (Unicode),
  honours clipboard-send mode, interprets the AHK send subset
  (`{!}` `{#}` `{@}` escapes, `{Enter}`/`{Left}`/… keys, `` `n `` newlines,
  `` `b `` backspaces). Double-clicking a button appends a newline.
- **Dark mode**: follows the Windows app theme; owner-drawn dark buttons, dark
  tab strip, DWM dark frame, painted window border.
- **Window behaviour**: collapse/expand, manual top/bottom edge resize,
  drag-to-move with top-of-screen and favourite-spot snapping, smooth
  wheel scrolling (low-level mouse hook), INI-persisted position/size/state.
- **Global hotkeys**: every per-button hotkey plus `^+x` (toggle window),
  `^+a` (list hotkeys), `^+s` (SREF → full prompt), `^!v`/`^!e`/`^!w` tools.
- **Tray icon** with the same menu, single-instance enforcement.

## Layout of the source

| Area        | Files |
|-------------|-------|
| Entry point | `Program.cs`, `HotkeyAppContext.cs` |
| Core        | `Core/` — `AppState`, `IniFile`, `TextSender`, `HotkeyParser`, `GlobalHotkeyManager`, `HotkeyRegistry`, `AppActions` |
| Interop     | `Native/NativeMethods.cs` |
| UI          | `UI/` — `HotkeyWindow`, `TabModel`, `TabPanelControl`, `ThemedButton`, `DarkTabControl`, `Theme`, `SymbolElement`, `UiText`, `HotkeyListForm` |
| Emoji       | `Emoji/EmojiImageProvider.cs` |
| Tab data    | `Tabs/` — one class per tab (`EmojisTab.cs` is generated 1:1 from `SupportFiles\Emojis.ahk`) |

## Notes

- The original embedded the Twemoji PNGs as `.exe` resources via Ahk2Exe; this
  port loads them from the `Images\Twemoji` folder copied beside the executable
  (the same source the AHK debug build used).
- `TabModel` is the C# name for the AHK `TabPage` class, renamed to avoid
  colliding with `System.Windows.Forms.TabPage`.
