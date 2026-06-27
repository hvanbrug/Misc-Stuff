# Still on the list

1. **DONE** — Content as data, not code.
The seven text/content tabs (Symbols, Comments, Prompt Helpers, Greek, Russian, Misc, Sensitive) are now data in `%LocalAppData%\HenksHotkeys\tabs.json` (seeded from an embedded default) and rendered by `DataTabModel` — edit JSON + **Reload configuration** instead of recompiling. Emojis and Tools stay built-in code tabs, referenced from the JSON via `{"builtin": "..."}`.
*Still optional:* move the Emojis catalog (EmojisTab.cs is still ~3,400 lines) to data, and add a named-action registry so the Tools action buttons could be data too.

2. Layout via panels instead of arithmetic (coupled to #1).
Partly modernised: data tabs lay out from `rows` + `columns` + `gapBefore`/`indent`/`blank`, so the old `ShiftLineByThird/Half` cursor hacks are gone for them (they survive only in the two remaining code tabs and the geometry tests). But buttons are still placed at absolute X/Y on a `Canvas` (`DataTabModel` computes `col*ColWidth` / `rowOffset*RowHeight`). An `ItemsControl` + `UniformGrid`/`WrapPanel` + `DataTemplate` would hand layout to WPF. **Still open.**

3. Drop the static globals → DI + MVVM-lite.
**Unchanged / open.** `AppState` is still a static bag and the window news up everything. A small service container + a `MainViewModel` (inject `SettingsStore`, hotkey manager, sender) would make it testable and cleaner. Medium effort.

4. Async sending.
**Unchanged / open.** `TextSender` still does `Thread.Sleep(100)` on the UI thread during the activate→send dance. `async`/`await` with `Task.Delay` would keep the UI responsive. Small, isolated.

5. **DONE** — Unit tests for the pure logic.
`HenksHotkeys.Tests` exists (52 tests): HotkeyParser, EmojiImageProvider.ToTwemojiStem, AppState.StripEmojis, the `{Enter}`/brace send tokenizer, and TabModel geometry — plus the newer Secrets (passphrase/DPAPI), versioning/merge (CRDT, reconcile, collapse), and layout (gap / blank / normalize) tests.

6. Quarantine the Win32 interop.
**Open.** `NativeMethods` centralises the P/Invokes and the elevated window-fit is wrapped (`ElevatedFit`), but the window still calls Win32 directly; there's no interface boundary yet. Wrapping the OS-integration bits (hotkeys, input send, foreground tracking, snap) behind a small interface would isolate the unavoidable Win32.

## New items that emerged after the original list

7. **DONE** — Go fully WPF by dropping UseWindowsForms.
The tray NotifyIcon (Shell_NotifyIcon), Clipboard (System.Windows.Clipboard), and Screen/monitor (Win32) were all replaced; no dual-framework dependency remains.

8. Button-gap constant, starting at 2px (gap between buttons, horizontal and vertical).
**Open.** There is a per-tab `gap` field (default 3) feeding `ColWidth`/`RowHeight`, but not a single 2px constant.

9. Edge-gap constant, starting at 2px (gap between edges/containers and any controls — window edges, the tab control, etc.).
**Open.** Spacing is still ad-hoc (the `SymOrgX`/`SymOrgY` origins and assorted margins).

10. Calculated corner controls + calculated collapsed size. I would like to include the right corner as well.
**Open.** The top-left indicators/collapse button are still positioned by hand, and the collapsed strip is hardcoded (84×30) rather than derived from those controls.

11. Full-on UI configuration — no more error-prone manual JSON editing.
**Open (end goal).** Manual editing is much safer now — embedded `_readme` cheat-sheet, **Reload**, **Export/Import** with per-button merge + **Repair duplicate tabs**, encrypted secrets, and blank/gap spacers — but a real in-app editor is still the target.

12. Make the buttons in a tab control originate at top left instead of center.

13. Add a favourites section at the top of the Emojis tab that is configurable by the user.
