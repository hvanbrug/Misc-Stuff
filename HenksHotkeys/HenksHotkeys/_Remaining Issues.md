# Still on the list

1. **DONE** — Content as data, not code.
The seven text/content tabs (Symbols, Comments, Prompt Helpers, Greek, Russian, Misc, Sensitive) are now data in `%LocalAppData%\HenksHotkeys\tabs.json` (seeded from an embedded default) and rendered by `DataTabModel` — edit JSON + **Reload configuration** instead of recompiling. Emojis and Tools stay built-in code tabs, referenced from the JSON via `{"builtin": "..."}`.
*Still optional:* move the Emojis catalog (EmojisTab.cs is still ~3,400 lines) to data, and add a named-action registry so the Tools action buttons could be data too.

2. Layout via panels instead of arithmetic (coupled to #1).
Partly modernised: data tabs lay out from `rows` + `columns` + `gapBefore`/`indent`/`blank`, so the old `ShiftLineByThird/Half` cursor hacks are gone for them (they survive only in the two remaining code tabs and the geometry tests). But buttons are still placed at absolute X/Y on a `Canvas` (`DataTabModel` computes `col*ColWidth` / `rowOffset*RowHeight`). An `ItemsControl` + `UniformGrid`/`WrapPanel` + `DataTemplate` would hand layout to WPF. **Still open.**

3. Drop the static globals → DI + MVVM-lite.
**Unchanged / open.** `AppState` is still a static bag and the window news up everything. A small service container + a `MainViewModel` (inject `SettingsStore`, hotkey manager, sender) would make it testable and cleaner. Medium effort.

4. **DONE** — Async sending.
`TextSender`'s activate→send dance now `await`s `Task.Delay` instead of `Thread.Sleep`, so the message pump keeps running and the window stays responsive (draggable/scrollable) mid-send. The entry points (`SendText`/`SendInputKeys`/`GetSelectedTextThroughClipboard`) return `Task`; buttons and hotkeys fire-and-forget them. Continuations stay on the UI/STA thread (no `ConfigureAwait(false)`), so `Clipboard` access remains valid. A `SemaphoreSlim` gate serialises sends — the synchronous version got that for free by blocking the UI thread, so concurrent clicks can't interleave keystrokes now. Verified end-to-end (UI-Automation invoke → text lands once in Notepad).

5. **DONE** — Unit tests for the pure logic.
`HenksHotkeys.Tests` exists (52 tests): HotkeyParser, EmojiImageProvider.ToTwemojiStem, AppState.StripEmojis, the `{Enter}`/brace send tokenizer, and TabModel geometry — plus the newer Secrets (passphrase/DPAPI), versioning/merge (CRDT, reconcile, collapse), and layout (gap / blank / normalize) tests.

6. Quarantine the Win32 interop.
**Open.** `NativeMethods` centralises the P/Invokes and the elevated window-fit is wrapped (`ElevatedFit`), but the window still calls Win32 directly; there's no interface boundary yet. Wrapping the OS-integration bits (hotkeys, input send, foreground tracking, snap) behind a small interface would isolate the unavoidable Win32.

## New items that emerged after the original list

7. **DONE** — Go fully WPF by dropping UseWindowsForms.
The tray NotifyIcon (Shell_NotifyIcon), Clipboard (System.Windows.Clipboard), and Screen/monitor (Win32) were all replaced; no dual-framework dependency remains.

8. **DONE** — Button-gap constant, starting at 2px (gap between buttons, horizontal and vertical).
`Layout.ButtonGap` (2px) feeds `ColWidth`/`RowHeight` and the multi-cell button width. The per-tab `gap` field was removed.

9. **DONE** — Edge-gap constant, starting at 2px (gap between edges/containers and any controls — window edges, the tab control, etc.).
`Layout.EdgeGap` (2px): buttons start at `(EdgeGap, EdgeGap)`, `ContentWidth`/`Height` add a trailing edge-gap, and the window border carries `EdgeGap` padding. The per-tab `originX`/`originY` fields were removed, and the dead `TabExporter` was deleted.

10. **DONE** — Calculated corner controls + calculated collapsed size (both corners).
The strip's hand-tuned margins are gone: both the left cluster (○ ▲ ☺) and the right cluster (🔄 ⌫. ⇚, ↩ ▲) sit at `Layout.EdgeGap` from the window edges via the border padding, the strip auto-sizes to its controls (no fixed `Height = 26`), and the indicator spacing uses `EdgeGap`. The collapsed window is now derived — `CollapsedSize()` measures the left cluster and adds the border + edge-gap chrome (≈ 79×30) instead of the old hardcoded 84×28.
*Note:* the right cluster's per-glyph `RaiseTop` baseline nudges and the fixed inter-icon gap remain — they're font-metric cosmetics, not edge positioning.

11. Full-on UI configuration — no more error-prone manual JSON editing.
**In progress (end goal).** Manual editing is much safer now — embedded `_readme` cheat-sheet, **Reload**, **Export/Import** with per-button merge + **Repair duplicate tabs**, encrypted secrets, and blank/gap spacers.
*Stage 1 done — per-button right-click menu:* **Edit button…** opens a dialog for every `ButtonDef` property (text/value, desc, hotkey, width, gap, left-align, show-text, tip-text, blank, sensitive), and **Delete button** (with confirm). Both mutate the live model, persist via `TabStore.SaveCurrent()`/`DeleteButton()` (which keep the crypto header + tombstones intact and re-stamp for merge), then rebuild the UI. Built on a back-link from `SymbolElement` → `ButtonDef`; code tabs (Emojis/Tools) get no menu.
*Add done — open-area right-click:* **Add button here…** (on the tab's empty area, menu hung off the ScrollViewer so the whole tab is reachable) opens the same dialog and inserts the new button next to where you clicked — after/before the nearest button by click position (`TabStore.InsertButton`), or as a first row in an empty tab (`TabStore.AddButton`).
*Blanks are first-class now:* blank spacer cells are placed/rendered as invisible, hit-testable cells that show a faint border on hover and carry the same Edit/Delete menu — so you turn a blank into a real button in place (uncheck "Blank" in the dialog) instead of inserting next to it. The dialog clears + greys-out the content fields while "Blank" is ticked, and `InsertButton` now *consumes* an adjacent blank rather than pushing the row wider (fixes the spurious wrap-row a positional insert used to create).
*Next stages:* tab-level menu (rename / add / delete / reorder), then live single-button regeneration and #2 (panels) for true WYSIWYG.

12. **DONE** — Make the buttons in a tab control originate at top left instead of center.
The button `Canvas` is now `HorizontalAlignment.Left` + `VerticalAlignment.Top`, so a tab narrower than the (locked) window width sits at the top-left edge-gap instead of being centred. (Done alongside #8/#9.)

13. Add a favourites section at the top of the Emojis tab that is configurable by the user.

14. **DONE** — Label sections in a tab.
Implemented as a *header row* (not a new container): a row with a `section` key is a header that labels the rows beneath it until the next header — `{"section": "My group"}` for a labelled divider, `{"section": ""}` for a plain line, optional `headerHeight` (px, default `Layout.SectionHeaderHeight` = 24). Backward compatible (existing flat `rows` files are unchanged; a tab with no header rows is one implicit unnamed section) and the merge needed no new nesting — header rows carry through `TabSig`/`NormalizeRows`/merge like blank rows, with their buttons cleared on load. Renders as a bold label on a separator line spanning the columns (`DataTabModel` emits `TabModel.SectionHeader`; `HotkeyWindow.BuildSectionHeader` draws it). Cheat-sheet updated.
*Bonus fix:* the merge row-rebuild used to drop the `blank` flag — it now preserves `blank`/`section`/`headerHeight` via `CloneRow`.

15. It would be pretty cool if the user could drag a tab to a new position in the tab list, and have that new order saved to the json. This would be a nice feature for the user to be able to customize their experience.


### Oddities
- In the `WireSymbolButton` function, why aren't we just catching the `MouseDoubleClick` event instead of the mess we are doing now?
- In the `PtToDip` function, what do the magic numbers mean: 4.0, 3.0?
- In the `ComputeFullSize` function, what do the following magic numbers mean: 330, 320, 64?
- I'm not sure the SettingsStore is initialized yet in the HotkeyWindow constructor. When config says collapsed, the window starts up uncollapsed, and then immediately collapses.
- 