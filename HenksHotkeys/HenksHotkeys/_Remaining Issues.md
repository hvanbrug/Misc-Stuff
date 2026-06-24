# Still on the list
1. Content as data, not code — the biggest remaining item.
The tabs are still code (RegisterSymbolX(...) calls), and EmojisTab.cs is 3,400 generated lines. Moving the catalog to an embedded JSON ({char, desc, shortcode, hotkey?, …}) + a renderer means editing data instead of recompiling, and a hybrid handler-registry for the action buttons (Tools/Prompts). High value, medium effort.

2. Layout via panels instead of arithmetic (coupled to #1).
We still position buttons on a Canvas with absolute X/Y from TabModel's CalcSymbolX/Y + line/slot + the ShiftLineByThird/Half hacks. An ItemsControl + UniformGrid/WrapPanel + DataTemplate would let WPF lay them out. Best done together with #1.

3. Drop the static globals → DI + MVVM-lite.
AppState is still a static bag and the window news up everything. A small service container + a MainViewModel (inject SettingsStore, hotkey manager, sender) would make it testable and cleaner. Medium effort.

4. Async sending.
TextSender still does Thread.Sleep(100) on the UI thread during the activate→send dance. async/await with Task.Delay would keep the UI responsive. Small, isolated.

5. Unit tests for the pure logic.
No test project yet. HotkeyParser, EmojiImageProvider.ToTwemojiStem, AppState.StripEmojis, the SendKeystrokes brace/{Enter} tokenizer, and TabModel geometry are all pure and bug-prone (the double-click and snap bugs were exactly this kind). Low effort, high safety payoff.

6. Quarantine the Win32 interop.
Partly there (NativeMethods), but the window still calls it directly. Wrapping the OS-integration bits (hotkeys, input send, foreground tracking, snap) behind a small interface would isolate the unavoidable Win32.

## New item that emerged from the migration (wasn't on the original list)

7. *completed* - Go fully WPF by dropping UseWindowsForms.
We kept it only for the tray NotifyIcon, Clipboard, and Screen. Replacing those (a WPF tray lib or Shell_NotifyIcon, System.Windows.Clipboard, Win32 monitor APIs) would remove the dual-framework dependency and the implicit-using juggling. Optional/cosmetic.