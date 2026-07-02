using System.Windows;
using System.Windows.Controls;
using HenksHotkeys.Core;
using HenksHotkeys.UI;

namespace HenksHotkeys;

/// <summary>
/// Application entry point and wiring (the role played by HenksHotkeys.ahk +
/// Startup.ahk): builds the tab models, creates the WPF helper window, registers
/// global hotkeys, installs the tray icon/menu, and restores the previous state.
/// </summary>
public partial class App
{
  private const string MutexName = "HenksHotkeys_SingleInstance_2A6F";

  private Mutex?               m_mutex;
  private GlobalHotkeyManager? m_hotkeys;
  private TrayIcon?            m_tray;
  private HotkeyWindow?        m_window;

  protected override void OnStartup( StartupEventArgs e )
  {
    base.OnStartup( e );

    // Elevated helper mode: just serve window-fit requests over the pipe — no
    // single-instance lock, settings, tabs, UI or tray.
    if( ElevatedFit.IsHelperArg( e.Args ) )
    {
      new Thread( () => ElevatedFit.RunHelper( e.Args ) ) { IsBackground = true }.Start();
      return;
    }

    // #SingleInstance Force — only one running copy.
    m_mutex = new Mutex( true, MutexName, out bool createdNew );
    if( !createdNew )
    {
      Shutdown();
      return;
    }

    AppState.InitSettings();
    AppState.UseClipSend     = AppState.Settings.IsClipSendMode;
    AppState.StripSendEmojis = AppState.Settings.IsStripCommentEmojis;

    // Publish the light/dark palette into the app resources before any window or
    // style is built, so the App.xaml styles resolve their DynamicResource brushes.
    Theme.Apply();

    // Place context menus / tooltips to the right of the cursor even when Windows'
    // "left-handed" menu setting is on.
    //MenuAlignment.ForceRightHanded();

    // Lets TabStore prompt for the master secrets passphrase when needed.
    PassphrasePrompt.Provider = PassphraseDialog.Ask;

    BuildTabModels();

    m_window = new HotkeyWindow();
    AppState.Window = m_window;

    RegisterHotkeys();
    BuildMenusAndTray();

    // Lets the per-button right-click menu refresh the UI after an edit / delete.
    AppState.RequestReload = ReloadConfig;

    // The window is always shown — there is no hidden / "no UI" mode.
    m_window.ShowUi();

    if( TabStore.LockedSecretCount > 0 )
    {
      MessageBox.Show(
        $"{TabStore.LockedSecretCount} secret button(s) couldn't be decrypted on this machine " +
        "(sealed with a different passphrase/salt). They show a 🔒 and won't send until you " +
        "re-enter them: right-click → Edit → re-type the value.\n\n" +
        "Importing your other machine's config will also re-seal shared secrets so this stops happening.",
        "Secrets locked", MessageBoxButton.OK, MessageBoxImage.Warning );
    }
  }

  private static void BuildTabModels()
  {
    // Tab content is data now (tabs.json in %LocalAppData%, seeded from the
    // embedded default). Built-in tabs (Emojis, Tools) are referenced by name.
    foreach( TabModel tab in TabStore.Load() )
    {
      AppState.Tabs.Add( tab );
    }

    // The window width locks to the widest tab. Size it from every tab's natural
    // width, then let the proportional tabs expand their cells to fill that width.
    int targetWidth = 0;
    foreach( TabModel tab in AppState.Tabs )
    {
      targetWidth = Math.Max( targetWidth, tab.SizingWidth );
    }
    foreach( TabModel tab in AppState.Tabs )
    {
      if( tab is DataTabModel data )
      {
        data.ApplyProportionalLayout( targetWidth );
      }
    }
  }

  private void RegisterHotkeys()
  {
    m_hotkeys = new GlobalHotkeyManager();
    m_hotkeys.RegisterCollected();

    m_hotkeys.Register( "^+x", () => m_window!.ToggleCollapsed() );
    m_hotkeys.Register( "^+a", AppActions.ListHotkeys );
    m_hotkeys.Register( "^+s", () => { _ = AppActions.SrefToFullPrompt(); } );
  }

  // One definition drives both the tray menu and the window's right-click menu.
  private (string? Label, Action? Action)[] MenuItems() =>
  [
    ("Set favourite spot",     () => m_window!.SetFavouriteSpot()),
    ("Move to favourite spot", () => m_window!.MoveToFavouriteSpot()),
    (null, null),
    ("Reload configuration",   ReloadConfig),
    ("Import configuration",   ImportConfig),
    ("Export configuration",   ExportConfig),
    (null, null),
    ("Repair duplicate tabs",  RepairDuplicates),
    (null, null),
    ("Test Function",          AppActions.TestFunction),
    (null, null),
    ("Exit",                   ExitApp),
  ];

  // Write the current config to a file you can copy to the other machine.
  private void ExportConfig()
  {
    var dlg = new Microsoft.Win32.SaveFileDialog
    {
      Title    = "Export Henk's Hotkeys config",
      FileName = "tabs.json",
      Filter   = "Config (*.json)|*.json|All files (*.*)|*.*",
    };
    if( dlg.ShowDialog() != true )
    {
      return;
    }
    bool ok = TabStore.Export( dlg.FileName );
    MessageBox.Show( ok ? "Config exported." : "Export failed.", "Export config",
                     MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Warning );
  }

  // Collapse duplicate (same-name) tabs into one — recovery for a bad merge.
  private void RepairDuplicates()
  {
    int removed = TabStore.RepairDuplicates();
    if( removed > 0 )
    {
      ReloadConfig();
    }
    MessageBox.Show( removed > 0 ? $"Merged {removed} duplicate tab(s)." : "No duplicate tabs found.",
                     "Repair duplicates", MessageBoxButton.OK, MessageBoxImage.Information );
  }

  // Merge a shared config file into this machine's (last-writer-wins per tab/button).
  private void ImportConfig()
  {
    var dlg = new Microsoft.Win32.OpenFileDialog
    {
      Title  = "Import && merge config",
      Filter = "Config (*.json)|*.json|All files (*.*)|*.*",
    };
    if( dlg.ShowDialog() != true )
    {
      return;
    }
    if( TabStore.Import( dlg.FileName ) )
    {
      ReloadConfig(); // rebuild the UI from the merged config
      MessageBox.Show( "Config imported and merged.", "Import config",
                       MessageBoxButton.OK, MessageBoxImage.Information );
    }
    else
    {
      MessageBox.Show( "Couldn't read that file as a config.", "Import config",
                       MessageBoxButton.OK, MessageBoxImage.Warning );
    }
  }

  // Re-read tabs.json and rebuild the tabs, buttons and global hotkeys in place,
  // so edits to the config take effect without restarting.
  private void ReloadConfig()
  {
    if( m_window is null )
    {
      return;
    }

    // Reset the accumulators the tab builder feeds, then rebuild from disk.
    HotkeyRegistry.Clear();
    AppState.HotkeyHelp.Clear();
    AppState.Tabs.Clear();
    FavouritesStore.Invalidate(); // re-read favourites.json too (Emojis tab rebuilds from it)
    BuildTabModels();

    m_window.ReloadTabs();

    // Re-register global hotkeys (per-button bindings + the app-level ones).
    m_hotkeys?.Dispose();
    RegisterHotkeys();

    if( !TabStore.LastParseOk )
    {
      MessageBox.Show(
        "tabs.json could not be parsed — the built-in defaults were loaded instead.\n" +
        "Check the file for JSON errors, then reload again.",
        "Reload configuration", MessageBoxButton.OK, MessageBoxImage.Warning );
    }
  }

  private ContextMenu BuildMenu()
  {
    var menu = new ContextMenu();
    foreach( var (label, action) in MenuItems() )
    {
      if( (label  is null) &&
          (action is null) )
      {
        menu.Items.Add( new Separator() );
      }
      else
      {
        var item = new MenuItem { Header = label };
        Action a = action!;
        item.Click += ( _, _ ) => a();
        menu.Items.Add( item );
      }
    }
    return menu;
  }

  private void BuildMenusAndTray()
  {
    // A fresh menu instance for each surface so they never share open state.
    m_window!.ContextMenu = BuildMenu();
    m_tray = new TrayIcon( BuildMenu(), () => m_window!.Summon(), "Henk's Hotkeys" );
  }

  private void ExitApp() => Shutdown();

  protected override void OnExit( ExitEventArgs e )
  {
    ElevatedFit.Shutdown();
    m_tray?.Dispose();
    m_hotkeys?.Dispose();
    base.OnExit( e );
  }
}
