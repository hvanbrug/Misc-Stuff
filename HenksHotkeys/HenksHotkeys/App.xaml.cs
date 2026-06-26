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

    // Lets TabStore prompt for the master secrets passphrase when needed.
    PassphrasePrompt.Provider = PassphraseDialog.Ask;

    BuildTabModels();

    m_window = new HotkeyWindow();
    AppState.Window = m_window;

    RegisterHotkeys();
    BuildMenusAndTray();

    // The window is always shown — there is no hidden / "no UI" mode.
    m_window.ShowUi();
  }

  private static void BuildTabModels()
  {
    // Tab content is data now (tabs.json in %LocalAppData%, seeded from the
    // embedded default). Built-in tabs (Emojis, Tools) are referenced by name.
    foreach( TabModel tab in TabStore.Load() )
    {
      AppState.Tabs.Add( tab );
    }
  }

  private void RegisterHotkeys()
  {
    m_hotkeys = new GlobalHotkeyManager();
    m_hotkeys.RegisterCollected();

    m_hotkeys.Register( "^+x", () => m_window!.ToggleCollapsed() );
    m_hotkeys.Register( "^+a", AppActions.ListHotkeys );
    m_hotkeys.Register( "^+s", AppActions.SrefToFullPrompt );
  }

  // One definition drives both the tray menu and the window's right-click menu.
  private (string? Label, Action? Action)[] MenuItems() => new (string?, Action?)[]
  {
    ("Set favourite spot",     () => m_window!.SetFavouriteSpot()),
    ("Move to favourite spot", () => m_window!.MoveToFavouriteSpot()),
    (null, null),
    ("Reload configuration",   ReloadConfig),
    (null, null),
    ("Test Function",          AppActions.TestFunction),
    (null, null),
    ("Exit",                   ExitApp),
  };

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
      if( label is null )
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
