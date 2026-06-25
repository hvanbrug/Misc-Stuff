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
  private Mutex?               m_mutex;
  private GlobalHotkeyManager? m_hotkeys;
  private TrayIcon?            m_tray;
  private HotkeyWindow?        m_window;

  protected override void OnStartup( StartupEventArgs e )
  {
    base.OnStartup( e );

    // #SingleInstance Force — only one running copy.
    m_mutex = new Mutex( true, "HenksHotkeys_SingleInstance_2A6F", out bool createdNew );
    if( !createdNew )
    {
      Shutdown();
      return;
    }

    AppState.InitSettings();
    AppState.UseClipSend     = AppState.Settings.IsClipSendMode;
    AppState.StripSendEmojis = AppState.Settings.IsStripCommentEmojis;

    BuildTabModels();

    m_window = new HotkeyWindow();
    AppState.Window = m_window;

    RegisterHotkeys();
    BuildMenusAndTray();

    if( AppState.Settings.IsWndOpen )
    {
      m_window.ShowUi();
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
  }

  private void RegisterHotkeys()
  {
    m_hotkeys = new GlobalHotkeyManager();
    m_hotkeys.RegisterCollected();

    m_hotkeys.Register( "^+x", () => m_window!.ToggleUi() );
    m_hotkeys.Register( "^+a", AppActions.ListHotkeys );
    m_hotkeys.Register( "^+s", AppActions.SrefToFullPrompt );
  }

  // One definition drives both the tray menu and the window's right-click menu.
  private (string? Label, Action? Action)[] MenuItems() => new (string?, Action?)[]
  {
    ( "Open UI",                () => m_window!.ShowUi() ),
    ( "Close UI",               () => m_window!.HideUi() ),
    ( null, null ),
    ( "Set favourite spot",     () => m_window!.SetFavouriteSpot() ),
    ( "Move to favourite spot", () => m_window!.MoveToFavouriteSpot() ),
    ( null, null ),
    ( "Test Function",          AppActions.TestFunction ),
    ( null, null ),
    ( "Exit",                   ExitApp ),
  };

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
    m_tray = new TrayIcon( BuildMenu(), () => m_window!.ShowUi(), "Henk's Hotkeys" );
  }

  private void ExitApp() => Shutdown();

  protected override void OnExit( ExitEventArgs e )
  {
    m_tray?.Dispose();
    m_hotkeys?.Dispose();
    base.OnExit( e );
  }
}
