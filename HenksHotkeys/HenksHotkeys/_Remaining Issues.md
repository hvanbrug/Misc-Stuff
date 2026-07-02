# Still on the list

2. Layout via panels instead of arithmetic (coupled to #1).
Partly modernised: data tabs lay out from `rows` + `columns` + `gapBefore`/`indent`/`blank`, so the old `ShiftLineByThird/Half` cursor hacks are gone for them (they survive only in the two remaining code tabs and the geometry tests). But buttons are still placed at absolute X/Y on a `Canvas` (`DataTabModel` computes `col*ColWidth` / `rowOffset*RowHeight`). An `ItemsControl` + `UniformGrid`/`WrapPanel` + `DataTemplate` would hand layout to WPF. **Still open.**

3. Drop the static globals → DI + MVVM-lite.
**Unchanged / open.** `AppState` is still a static bag and the window news up everything. A small service container + a `MainViewModel` (inject `SettingsStore`, hotkey manager, sender) would make it testable and cleaner. Medium effort.

6. Quarantine the Win32 interop.
**Open.** `NativeMethods` centralises the P/Invokes and the elevated window-fit is wrapped (`ElevatedFit`), but the window still calls Win32 directly; there's no interface boundary yet. Wrapping the OS-integration bits (hotkeys, input send, foreground tracking, snap) behind a small interface would isolate the unavoidable Win32.

11. Full-on UI configuration — no more error-prone manual JSON editing.
**In progress (end goal).** Manual editing is much safer now — embedded `_readme` cheat-sheet, **Reload**, **Export/Import** with per-button merge + **Repair duplicate tabs**, encrypted secrets, and blank/gap spacers.
*Stage 1 done — per-button right-click menu:* **Edit button…** opens a dialog for every `ButtonDef` property (text/value, desc, hotkey, width, gap, left-align, show-text, tip-text, blank, sensitive), and **Delete button** (with confirm). Both mutate the live model, persist via `TabStore.SaveCurrent()`/`DeleteButton()` (which keep the crypto header + tombstones intact and re-stamp for merge), then rebuild the UI. Built on a back-link from `SymbolElement` → `ButtonDef`; code tabs (Emojis/Tools) get no menu.
*Add done — open-area right-click:* **Add button here…** (on the tab's empty area, menu hung off the ScrollViewer so the whole tab is reachable) opens the same dialog and inserts the new button next to where you clicked — after/before the nearest button by click position (`TabStore.InsertButton`), or as a first row in an empty tab (`TabStore.AddButton`).
*Blanks are first-class now:* blank spacer cells are placed/rendered as invisible, hit-testable cells that show a faint border on hover and carry the same Edit/Delete menu — so you turn a blank into a real button in place (uncheck "Blank" in the dialog) instead of inserting next to it. The dialog clears + greys-out the content fields while "Blank" is ticked, and `InsertButton` now *consumes* an adjacent blank rather than pushing the row wider (fixes the spurious wrap-row a positional insert used to create).
*Next stages:* tab-level menu (rename / add / delete / reorder), then live single-button regeneration and #2 (panels) for true WYSIWYG.

13. Add a favourites section at the top of the Emojis tab that is configurable by the user.

15. It would be pretty cool if the user could drag a tab to a new position in the tab list, and have that new order saved to the json. This would be a nice feature for the user to be able to customize their experience.

18. Need to be able to move buttons to different tabs.

22. Heading text should be placed left, or center, or right aligned. The default should be left aligned. Use some sort of mutually exclusive buttons for the setting selection

23. Button text should be placed left, or center, or right aligned. The default should be center aligned. Use some sort of mutually exclusive buttons for the setting selection

24. Add up/down to the all cell number, cell count, etc. settings edit boxes for easier changes.

# Completed

1. **DONE** — Content as data, not code.
The seven text/content tabs (Symbols, Comments, Prompt Helpers, Greek, Russian, Misc, Sensitive) are now data in `%LocalAppData%\HenksHotkeys\tabs.json` (seeded from an embedded default) and rendered by `DataTabModel` — edit JSON + **Reload configuration** instead of recompiling. Emojis and Tools stay built-in code tabs, referenced from the JSON via `{"builtin": "..."}`.
*Still optional:* move the Emojis catalog (EmojisTab.cs is still ~3,400 lines) to data, and add a named-action registry so the Tools action buttons could be data too.

4. **DONE** — Async sending.
`TextSender`'s activate→send dance now `await`s `Task.Delay` instead of `Thread.Sleep`, so the message pump keeps running and the window stays responsive (draggable/scrollable) mid-send. The entry points (`SendText`/`SendInputKeys`/`GetSelectedTextThroughClipboard`) return `Task`; buttons and hotkeys fire-and-forget them. Continuations stay on the UI/STA thread (no `ConfigureAwait(false)`), so `Clipboard` access remains valid. A `SemaphoreSlim` gate serialises sends — the synchronous version got that for free by blocking the UI thread, so concurrent clicks can't interleave keystrokes now. Verified end-to-end (UI-Automation invoke → text lands once in Notepad).

5. **DONE** — Unit tests for the pure logic.
`HenksHotkeys.Tests` exists (52 tests): HotkeyParser, EmojiImageProvider.ToTwemojiStem, AppState.StripEmojis, the `{Enter}`/brace send tokenizer, and TabModel geometry — plus the newer Secrets (passphrase/DPAPI), versioning/merge (CRDT, reconcile, collapse), and layout (gap / blank / normalize) tests.

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

12. **DONE** — Make the buttons in a tab control originate at top left instead of center.
The button `Canvas` is now `HorizontalAlignment.Left` + `VerticalAlignment.Top`, so a tab narrower than the (locked) window width sits at the top-left edge-gap instead of being centred. (Done alongside #8/#9.)

14. **DONE** — Label sections in a tab.
Implemented as a *header row* (not a new container): a row with a `section` key is a header that labels the rows beneath it until the next header — `{"section": "My group"}` for a labelled divider, `{"section": ""}` for a plain line, optional `headerHeight` (px, default `Layout.SectionHeaderHeight` = 24). Backward compatible (existing flat `rows` files are unchanged; a tab with no header rows is one implicit unnamed section) and the merge needed no new nesting — header rows carry through `TabSig`/`NormalizeRows`/merge like blank rows, with their buttons cleared on load. Renders as a bold label on a separator line spanning the columns (`DataTabModel` emits `TabModel.SectionHeader`; `HotkeyWindow.BuildSectionHeader` draws it). Cheat-sheet updated.
*Bonus fix:* the merge row-rebuild used to drop the `blank` flag — it now preserves `blank`/`section`/`headerHeight` via `CloneRow`.

16. **DONE** — It would be very nice to be able to create and move buttons more than one row past the last row.

17. **DONE** — Hotkey editor now uses **Ctrl / Alt / Win / Shift toggle buttons** + a narrow key box instead of raw `^+!#` typing. The four toggles are equal-sized and the same height as the key box, stay pressed (accent) when selected (a styled `ToggleButton` template added to `DialogChrome`), and the box holds just the unmodified key (e.g. `F9`). `HotkeyParser.Split`/`Compose` convert between the toggles+key and the stored AHK string (composed in Ctrl/Alt/Win/Shift order); the key is validated on OK (single letter/digit or F1–F24, and a modifier without a key is rejected). 5 parser tests added (split, compose, round-trip).

19. **DONE** — Allow sub-cell buttons: more than one button sharing a cell, split horizontally. Two fields on `ButtonDef` — `subcells` (how many share the cell, default 1, omitted from the file when 1) and `subcell` (0-based slot, left→right). `DataTabModel` sizes each to `(cellWidth − gaps)/subcells` and places it at its slot (full cell height). Guarding: the merge's collision resolver keys on `(row, col, subcell)`, so sub-cell siblings coexist while two buttons truly claiming the *same* slot are still separated; `ButtonSig` includes the slot; and `ShiftRight` now moves whole cell-groups (all buttons in a column together) so an insert-shift never splits a group. Edit dialog gained **Sub-cells** / **Sub-cell #** fields (validated 0 ≤ # < subcells). Old files default to 1 (no migration). Verified: 3-way and 2-way splits render at the right widths; 4 tests added.
*Follow-ups:* placing/dropping a sub-cell button into a cell that already holds one now **coexists** in a free slot instead of shifting the existing group aside — `Occupied` is conflict-aware (a whole-cell button fills the cell; mismatched divisions overlap; matching divisions clash only in the same slot) and a dragged sub-cell button targets the whole cell (no insert-shift). The edit dialog shows the **sub-cell # 1-based** while the stored value stays 0-based.

20. **DONE** — Popup-menu option to insert a heading. Right-click the open area → **Insert heading here…** opens a small dialog (label + start column + span) and drops the heading at the clicked cell. Built by extending `SectionDef` with `col` (start, default 0) and `span` (columns, default 0 = full width): a heading with `span > 0` is a partial-width label that is the **same height as a button row** (starts at its column, covers `span` cells), while the existing full-width `span 0` sections are unchanged (still `SectionHeaderHeight`). Headings carry a right-click **Edit heading… / Delete heading** menu (back-linked via `SectionHeader.Source`). Sections ride with the tab, so `TabSig`/`CloneAttrs` now include `col`/`span`, and `TabStore.AddHeading`/`DeleteHeading` persist. Verified: a col-1/span-3 button-row-height heading renders beside buttons and a classic full-width section; 2 tests added.

21. **DONE** — The open-area right-click menu now offers, below a separator: **Insert blank row** (opens an empty row at the click, pushing everything at/below down) and **Insert heading row…**. The old **Insert heading here…** (which places a heading in the clicked cell without moving anything) is renamed **Add heading here…**.
*Insert heading (refined):* nothing shifts until the heading dialog is saved. The heading starts at the clicked column and runs for its span; on save, **only the cells under the heading's columns** (at or below its row) shift down — content in other columns stays put. Because a multi-column entity (a wider heading, or a wide button) can't be split, `ShiftCellsDown` **staircases**: scanning downward it widens the affected column range at the row where such an entity overlaps the current range, and keeps it widened below (chaining). E.g. a 2–4 heading inserted above a 1–5 heading shifts cols 2–4 down from the top, then from the 1–5 heading's row the range widens to 1–5 (cols 1 & 5 shift from there down) — so nothing collides. A full-width divider below widens it to the whole row. **Insert blank row** stays a full-width row insert (`ShiftRowsDown`). 4 tests added (full-row shift, column-scoped shift, staircase-to-wider-heading, blank-row shift).

### Oddities
- **Answered** — `WireSymbolButton` vs `MouseDoubleClick`: a `Button` raises `Click` on *both* clicks of a double-click, so `MouseDoubleClick` would sit on top of two Clicks and send the text twice (text, Enter, text). A wait-and-see timer would instead delay the single-click send, which must be instant. So we send on the first Click and, if a second lands within `DoubleClickMs`, turn *that* one into the Enter. (Comment added.)
- **Answered** — `PtToDip` 4.0/3.0: a point is 1/72 inch, a WPF DIP is 1/96 inch, so points→DIPs scales by 96/72 = 4/3. Now written as named `DipsPerInch`/`PointsPerInch` constants.
- **Answered** — `ComputeFullSize` 330/320/64: the default (unsaved) height clamps the tallest tab's content into a [320, 330] DIP band, then adds 64 DIP of non-scrolling chrome (toolbar strip + tab-header row + top/bottom borders). Now named `DefaultViewportMin`/`DefaultViewportMax`/`VerticalChrome`.
- **DONE** — startup collapse flash: settings *are* loaded in the constructor (it already reads FavX/FavY), so the saved collapsed state is now applied there — before the first `Show()` — instead of flashing open at full size and snapping shut in `ShowUi`. The height-capture in `SetCollapsed` is guarded (`ActualHeight > 0`) so collapsing before the window is measured no longer zeroes the remembered expanded height. Verified: starts collapsed (101×35) and expands to a proper height.
- **DONE** — empty-cell hover skipped trailing columns: the hover was tracked on the `Canvas`, which is only as wide as the *content*, so cells in columns past the last button (e.g. the last column of a sparse tab) were outside it. The move is now tracked on the `ScrollViewer` (which fills the whole tab width), and the outline lives on the un-clipped canvas, so every column up to `columns` highlights. Verified: col 7 highlights on a tab whose content stops at col 1.
- **DONE** — a button overlapping a heading was un-draggable (and got trapped there): section headings are hit-testable (for their right-click menu) and were added *after* the buttons, so they painted on top and swallowed the button's mouse events. Headings now render *under* the buttons (added first), so an overlapping button stays on top — clickable and draggable back out. Verified: a button sharing a heading's cell drags away cleanly.