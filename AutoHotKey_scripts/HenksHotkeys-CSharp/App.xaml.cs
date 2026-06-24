using System.IO;
using System.Windows;
using System.Windows.Controls;
using HenksHotkeys.Core;
using HenksHotkeys.Tabs;
using HenksHotkeys.UI;
using WF = System.Windows.Forms;
using Drawing = System.Drawing;

namespace HenksHotkeys;

/// <summary>
/// Application entry point and wiring (the role played by HenksHotkeys.ahk +
/// Startup.ahk): builds the tab models, creates the WPF helper window, registers
/// global hotkeys, installs the tray menu, and restores the previous state.
/// The tray icon uses WinForms NotifyIcon (no first-class WPF equivalent).
/// </summary>
public partial class App
{
  private Mutex?               m_mutex;
  private GlobalHotkeyManager? m_hotkeys;
  private WF.NotifyIcon?       m_tray;
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
    AppState.Tabs.Add( new SymbolsTab() );
    AppState.Tabs.Add( new EmojisTab() );
    AppState.Tabs.Add( new CommentsTab() );
    AppState.Tabs.Add( new PromptsTab() );
    AppState.Tabs.Add( new GreekTab() );
    AppState.Tabs.Add( new RussianTab() );
    AppState.Tabs.Add( new MiscTab() );
    AppState.Tabs.Add( new ToolsTab() );
    AppState.Tabs.Add( new SensitiveTab() );
  }

  private void RegisterHotkeys()
  {
    m_hotkeys = new GlobalHotkeyManager();
    m_hotkeys.RegisterCollected();

    m_hotkeys.Register( "^+x", () => m_window!.ToggleUi() );
    m_hotkeys.Register( "^+a", AppActions.ListHotkeys );
    m_hotkeys.Register( "^+s", AppActions.SrefToFullPrompt );
  }

  // One menu definition drives both the tray menu (WinForms) and the window's
  // right-click context menu (WPF).
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

  private void BuildMenusAndTray()
  {
    var defs = MenuItems();

    // Tray (WinForms NotifyIcon + ContextMenuStrip).
    var strip = new WF.ContextMenuStrip();
    foreach( var (label, action) in defs )
    {
      if( label is null )
      {
        strip.Items.Add( new WF.ToolStripSeparator() );
      }
      else
      {
        strip.Items.Add( label, null, ( _, _ ) => action!() );
      }
    }

    m_tray = new WF.NotifyIcon
    {
      Text             = "Henk's Hotkeys",
      Icon             = LoadTrayIcon(),
      Visible          = true,
      ContextMenuStrip = strip,
    };
    m_tray.DoubleClick += ( _, _ ) => m_window!.ShowUi();

    // Window right-click menu (WPF).
    var menu = new ContextMenu();
    foreach( var (label, action) in defs )
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
    m_window!.ContextMenu = menu;
  }

  private static Drawing.Icon LoadTrayIcon()
  {
    try
    {
      using Stream? s = System.Reflection.Assembly.GetExecutingAssembly()
        .GetManifestResourceStream( "app.ico" );
      if( s is not null )
      {
        return new Drawing.Icon( s );
      }
    }
    catch { /* fall through */ }
    return Drawing.SystemIcons.Application;
  }

  private void ExitApp() => Shutdown();

  protected override void OnExit( ExitEventArgs e )
  {
    if( m_tray is not null )
    {
      m_tray.Visible = false;
      m_tray.Dispose();
    }
    m_hotkeys?.Dispose();
    base.OnExit( e );
  }
}
