using System.Drawing;
using System.Windows.Forms;
using HenksHotkeys.Core;
using HenksHotkeys.Tabs;
using HenksHotkeys.UI;

namespace HenksHotkeys;

/// <summary>
/// Application context that wires everything together (the role played by
/// HenksHotkeys.ahk + Startup.ahk): builds the tab models, creates the helper
/// window, registers global hotkeys, installs the tray menu, and restores the
/// previous open/closed state.
/// </summary>
internal sealed class HotkeyAppContext : ApplicationContext
{
  private readonly GlobalHotkeyManager m_hotkeys = new();
  private readonly NotifyIcon          m_tray;
  private readonly HotkeyWindow        m_window;

  public HotkeyAppContext()
  {
    AppState.InitIni();
    AppState.UseClipSend     = AppState.Ini.IsClipSendMode;
    AppState.StripSendEmojis = AppState.Ini.IsStripCommentEmojis;

    BuildTabModels();

    m_window = new HotkeyWindow();
    AppState.Window = m_window;
    m_window.FormClosed += ( _, _ ) => ExitThread();

    RegisterHotkeys();

    m_tray = BuildTray();

    if( AppState.Ini.IsWndOpen )
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
    // Per-symbol bindings collected while the tabs were built.
    m_hotkeys.RegisterCollected();

    // App-level hotkeys (HenksHotkeys.ahk / PromptHelpers.ahk).
    m_hotkeys.Register( "^+x", () => m_window.ToggleUi() );
    m_hotkeys.Register( "^+a", AppActions.ListHotkeys );
    m_hotkeys.Register( "^+s", AppActions.SrefToFullPrompt );
  }

  private NotifyIcon BuildTray()
  {
    var menu = new ContextMenuStrip();
    menu.Items.Add( "Open UI",               null, ( _, _ ) => m_window.ShowUi() );
    menu.Items.Add( "Close UI",              null, ( _, _ ) => m_window.HideUi() );
    menu.Items.Add( new ToolStripSeparator() );
    menu.Items.Add( "Set favourite spot",    null, ( _, _ ) => m_window.SetFavouriteSpot() );
    menu.Items.Add( "Move to favourite spot",null, ( _, _ ) => m_window.MoveToFavouriteSpot() );
    menu.Items.Add( new ToolStripSeparator() );
    menu.Items.Add( "Test Function",         null, ( _, _ ) => AppActions.TestFunction() );
    menu.Items.Add( new ToolStripSeparator() );
    menu.Items.Add( "Exit",                  null, ( _, _ ) => ExitApp() );

    // The same menu doubles as the window's right-click context menu.
    m_window.AttachContextMenu( menu );

    var tray = new NotifyIcon
    {
      Text             = "Henk's Hotkeys",
      Icon             = LoadTrayIcon(),
      Visible          = true,
      ContextMenuStrip = menu
    };
    tray.DoubleClick += ( _, _ ) => m_window.ShowUi(); // default action: Open UI
    return tray;
  }

  private static Icon LoadTrayIcon()
  {
    try
    {
      string ico = Path.Combine( AppState.BaseDir, "Images", "HenksHotkeys.ico" );
      if( File.Exists( ico ) )
      {
        return new Icon( ico );
      }
    }
    catch { /* fall through */ }
    return SystemIcons.Application;
  }

  private void ExitApp()
  {
    m_window.Close();
  }

  protected override void Dispose( bool disposing )
  {
    if( disposing )
    {
      m_tray.Visible = false;
      m_tray.Dispose();
      m_hotkeys.Dispose();
    }
    base.Dispose( disposing );
  }
}
