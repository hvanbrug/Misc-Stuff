using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using HenksHotkeys.Core;
using HenksHotkeys.Emoji;
using HenksHotkeys.Native;

namespace HenksHotkeys.UI;

/// <summary>
/// The always-on-top, frameless WPF helper window (UI.ahk / HotkeyWindow). Hosts
/// the tabbed grid of symbol buttons plus the corner indicators and helper
/// buttons; supports collapse/expand, vertical resize (WindowChrome), drag-to-move
/// with top/favourite snapping, smooth wheel scrolling and INI-persisted state.
///
/// Positions (X/Y/favourite) are kept in physical pixels because the snap logic
/// runs in the WM_MOVING message (physical), while sizes use WPF device-independent
/// units — WPF handles DPI scaling, so there is no manual pixel scaling.
/// </summary>
internal sealed class HotkeyWindow : Window
{
  private const int WM_EXITSIZEMOVE = 0x0232;
  private const int DoubleClickMs   = 400;

  private readonly TabControl              m_tabs = new();
  private readonly List<SmoothScroller>    m_scrollers = new();
  private readonly List<FrameworkElement>  m_helperButtons = new();

  private TextBlock  m_clipIndicator  = null!;
  private TextBlock  m_stripIndicator = null!;
  private Button     m_toggleBtn      = null!;
  private StackPanel m_leftStrip      = null!;
  private StackPanel m_rightCluster   = null!;

  private double m_fullWidth;
  private double m_fullHeight;
  private bool   m_collapsed;
  private bool   m_suppressResizePersist;
  private bool   m_suppressTabPersist;

  private int? m_favX;
  private int? m_favY;
  private int  m_dragOffsetX;
  private int  m_dragOffsetY;
  private bool m_snappedToTop;
  private bool m_snappedToFav;
  private bool m_dragging;

  private IntPtr m_hwnd;
  private readonly DispatcherTimer m_activeWinTimer = new() { Interval = TimeSpan.FromMilliseconds( 200 ) };

  private IntPtr m_wheelHook;
  private NativeMethods.LowLevelMouseProc? m_wheelProc;

  public HotkeyWindow()
  {
    Title                 = "Henk's Hotkeys";
    WindowStyle           = WindowStyle.None;
    ResizeMode            = ResizeMode.CanResize;
    AllowsTransparency    = false;
    ShowInTaskbar         = false;
    ShowActivated         = false;
    Topmost               = true;
    SizeToContent         = SizeToContent.Manual;
    Background            = Theme.WindowBackground;
    WindowStartupLocation = WindowStartupLocation.Manual;

    // Frameless but resizable on the top/bottom edges only; width is locked below.
    var chrome = new WindowChrome
    {
      CaptionHeight         = 0,
      GlassFrameThickness   = new Thickness( 0 ),
      ResizeBorderThickness = new Thickness( 0, 6, 0, 6 ),
      CornerRadius          = new CornerRadius( 0 ),
    };
    WindowChrome.SetWindowChrome( this, chrome );

    BuildContent();
    ComputeFullSize();

    MinWidth  = MaxWidth = m_fullWidth;
    MinHeight = 140;
    MaxHeight = SystemParameters.PrimaryScreenHeight;
    Width     = m_fullWidth;
    Height    = m_fullHeight;

    m_favX = AppState.Settings.FavX;
    m_favY = AppState.Settings.FavY;

    m_activeWinTimer.Tick += ( _, _ ) => TrackActiveWindow();
    m_activeWinTimer.Start();

    PreviewKeyDown += ( _, e ) => { if( e.Key == Key.Escape ) SetCollapsed( true ); };

    // The context menu (assigned by App) is restricted to the bare window strip,
    // the indicators and the left collapse button — never the tab control or the
    // helper buttons.
    ContextMenuOpening += OnContextMenuOpening;
  }

  // ── Construction ─────────────────────────────────────────────────
  private void BuildContent()
  {
    BuildTabs();
    BuildCornerControls();

    // The strip auto-sizes to its corner controls; the window border's EdgeGap
    // padding provides the gap to the window edges (no hand-tuned margins).
    var strip = new Grid { Background = Theme.WindowBackground };

    m_leftStrip = new StackPanel
    {
      Orientation         = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Left,
      VerticalAlignment   = VerticalAlignment.Center,
    };
    m_leftStrip.Children.Add( m_clipIndicator  );
    m_leftStrip.Children.Add( m_toggleBtn      );
    m_leftStrip.Children.Add( m_stripIndicator );

    strip.Children.Add( m_leftStrip );
    strip.Children.Add( m_rightCluster );
    strip.MouseLeftButtonDown += OnStripDragStart;

    var grid = new Grid();
    grid.RowDefinitions.Add( new RowDefinition { Height = GridLength.Auto } );
    grid.RowDefinitions.Add( new RowDefinition { Height = new GridLength( 1, GridUnitType.Star ) } );
    Grid.SetRow( strip, 0 );
    Grid.SetRow( m_tabs, 1 );
    grid.Children.Add( strip );
    grid.Children.Add( m_tabs );

    Content = new Border
    {
      BorderThickness = new Thickness( Theme.BorderThickness ),
      BorderBrush     = Theme.BorderColor,
      Background      = Theme.WindowBackground,
      Padding         = new Thickness( Layout.EdgeGap ), // gap between border and contents
      Child           = grid,
    };
  }

  private void BuildTabs()
  {
    if( Theme.IsDark )
    {
      m_tabs.Style = (Style)Application.Current.FindResource( "DarkTabControl" );
    }

    PopulateTabs();

    // Subscribe once; PopulateTabs may run again on reload. The guard stops the
    // auto-selection during a rebuild from overwriting the remembered tab.
    m_tabs.SelectionChanged += ( _, _ ) =>
    {
      if( !m_suppressTabPersist && m_tabs.SelectedIndex >= 0 )
      {
        AppState.Settings.SetLastTab( m_tabs.SelectedIndex + 1 );
      }
    };
  }

  // Build (or rebuild) the tab items from AppState.Tabs.
  private void PopulateTabs()
  {
    // Capture the current tab first — rebuilding the items auto-selects the first
    // one, which would otherwise clobber the remembered tab before we restore it.
    int targetTab = AppState.Settings.LastTab - 1;

    m_suppressTabPersist = true;
    m_tabs.Items.Clear();
    m_scrollers.Clear();

    foreach( TabModel model in AppState.Tabs )
    {
      var canvas = new Canvas
      {
        Width      = model.ContentWidth,
        Height     = model.ContentHeight,
        Background  = Theme.WindowBackground,
        // Sit at the top-left so the edge gap is a consistent 2px instead of the
        // buttons being centred in the (wider) locked-width window.
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment   = VerticalAlignment.Top,
      };
      foreach( SymbolElement sym in model.Symbols )
      {
        FrameworkElement btn = BuildButton( sym, model );
        Canvas.SetLeft( btn, sym.X );
        Canvas.SetTop( btn, sym.Y );
        canvas.Children.Add( btn );
      }

      var sv = new ScrollViewer
      {
        VerticalScrollBarVisibility   = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        Background = Theme.WindowBackground,
        Focusable  = false,
        Content    = canvas,
      };
      m_scrollers.Add( new SmoothScroller( sv ) );

      var item = new TabItem { Header = model.Name, Content = sv };
      if( Theme.IsDark )
      {
        item.Style = (Style)Application.Current.FindResource( "DarkTabItem" );
      }
      m_tabs.Items.Add( item );
    }

    m_tabs.SelectedIndex = targetTab >= 0 && targetTab < m_tabs.Items.Count ? targetTab : 0;
    m_suppressTabPersist = false;
  }

  /// <summary>Rebuild the tab UI after the tab models were reloaded from disk.</summary>
  public void ReloadTabs()
  {
    PopulateTabs();
    ComputeFullWidth();

    // Re-apply the locked width (the widest tab may have changed). Height is left
    // as-is so a user-resized window keeps its height. Collapsed stays collapsed;
    // the new width takes effect on the next expand.
    if( !m_collapsed )
    {
      m_suppressResizePersist = true;
      try
      {
        MinWidth = MaxWidth = m_fullWidth;
        Width    = m_fullWidth;
      }
      finally
      {
        m_suppressResizePersist = false;
      }
    }
  }

  private FrameworkElement BuildButton( SymbolElement sym, TabModel model )
  {
    var btn = new Button
    {
      Width      = sym.W,
      Height     = sym.H,
      FontFamily = new FontFamily( model.FontName ),
      FontSize   = PtToDip( model.FontSize ),
      HorizontalContentAlignment = HorizontalAlignment.Stretch,
      VerticalContentAlignment   = VerticalAlignment.Center,
    };

    string buttonText = UiText.NormalizeDisplayText( sym.ShowChar ? sym.Char : sym.Desc );
    if( sym.Hotkey.Length > 0 ) buttonText = "• " + buttonText;
    if( sym.Align == "left" )   buttonText = " "  + buttonText;

    string tip  = UiText.NormalizeDisplayText( sym.Desc );
    string stem = "";
    bool   isEmoji = false;

    if( model.UseEmojiImages )
    {
      int emojiPx = (int)Math.Round( model.SymBtnSizeX * 0.8 );
      EmojiImageProvider.Result res = EmojiImageProvider.Get( sym.Char, emojiPx );
      stem = res.Stem;
      if( res.Image is not null )
      {
        isEmoji = true;
        btn.Content = new Image
        {
          Source = res.Image,
          Width  = model.SymBtnSizeX * 0.8,
          Height = model.SymBtnSizeY * 0.8,
          Stretch = Stretch.Uniform,
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment   = VerticalAlignment.Center,
        };
      }
    }

    if( !isEmoji )
    {
      btn.Content = new TextBlock
      {
        Text         = buttonText,
        TextAlignment = sym.Align == "left" ? TextAlignment.Left : TextAlignment.Center,
        TextTrimming = sym.Align == "left" ? TextTrimming.CharacterEllipsis : TextTrimming.None,
        VerticalAlignment = VerticalAlignment.Center,
      };
    }

    if( Theme.IsDark )
    {
      btn.Style = (Style)Application.Current.FindResource( isEmoji ? "DarkEmojiButton" : "DarkButton" );
    }

    if( sym.Hotkey.Length > 0 ) tip = Append( tip, HotkeyParser.Label( sym.Hotkey ) );
    if( sym.TipChar )           tip = Append( tip, sym.Char );
    if( model.UseEmojiImages && stem.Length > 0 ) tip = Append( tip, "U+" + stem );
    if( tip.Length > 0 )
    {
      btn.ToolTip = new TextBlock { Text = tip };
    }

    WireSymbolButton( btn, sym.ClickAction );
    sym.Ctrl = btn;
    return btn;
  }

  private static string Append( string tip, string line )
  {
    return tip.Length == 0 ? line : tip + "\n" + line;
  }

  private static double PtToDip( float pointSize )
  {
    return pointSize * 4.0 / 3.0;
  }

  // First click runs the action (sends the text); a rapid second click sends an
  // Enter instead of re-sending, reproducing the BN_DBLCLK behaviour in UI.ahk.
  private static void WireSymbolButton( Button btn, Action click )
  {
    long last = 0;
    btn.Click += ( _, _ ) =>
    {
      long now = Environment.TickCount64;
      bool pair = now - last <= DoubleClickMs;
      last = now;
      if( pair )
      {
        last = 0;
        try { TextSender.SendInputKeys( "{Enter}" ); } catch { /* ignore */ }
        return;
      }
      try { click(); } catch { /* never let a send failure kill the UI */ }
    };
  }

  private void BuildCornerControls()
  {
    m_clipIndicator  = MakeIndicator( "○", AppActions.ToggleClipboardSendMode );
    m_stripIndicator = MakeIndicator( "☺", AppActions.ToggleStripSendEmojis );

    m_toggleBtn = MakeButton( "▲", "Shrink window", "Segoe UI Symbol", 11f, ToggleCollapsed, addToCluster: false );
    // The left collapse button also doubles as a drag handle. MakeDragOrClick
    // handles the press at the preview stage, so the button's own Click never
    // fires (no double toggle) — a plain click toggles, a press-drag moves.
    MakeDragOrClick( m_toggleBtn, ToggleCollapsed );

    m_rightCluster = new StackPanel
    {
      Orientation         = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Right,
      VerticalAlignment   = VerticalAlignment.Center,
    };
    RaiseTop( 1, MakeButton( "🔄",  "Repaint / Refresh",          "Segoe UI Symbol", 10f, ForceRepaint ) );
    RaiseTop( 0, MakeBtnGap( 38 ) );
    RaiseTop( 3, MakeButton( "⌫.", "Back 3, Replace with period", "Segoe UI Symbol", 12f, () => TextSender.SendInputKeys( "\b\b\b. " ) ) );
    RaiseTop( 9, MakeButton( "⇚,", "Back 3, Insert Comma",        "Segoe UI Symbol", 18f, () => TextSender.SendInputKeys( "{Left}{Left}{Left}, " ) ) );
    RaiseTop( 0, MakeButton( "↩",  "Enter / Newline",             "Segoe UI Symbol", 12f, () => TextSender.SendInputKeys( "{Enter}" ) ) );
    RaiseTop( 0, MakeButton( "▲",  "Shrink window",               "Segoe UI Symbol", 11f, ToggleCollapsed ) );

    UpdateClipIndicator(  AppState.UseClipSend     );
    UpdateStripIndicator( AppState.StripSendEmojis );
  }

  private void RaiseTop( int padValue, Control cc )
  {
    var pad = cc.Padding;
    pad.Top    -= padValue;
    cc.Padding =  pad;
  }

  private void RaiseTop( int unused1, FrameworkElement unused2 )
  {}

  private TextBlock MakeIndicator( string glyph, Action onClick )
  {
    var tb = new TextBlock
    {
      Text       = glyph,
      FontFamily = new FontFamily( "Segoe UI Symbol" ),
      FontSize   = PtToDip( 10f ),
      Foreground = Theme.TextColor,
      VerticalAlignment = VerticalAlignment.Center,
      Margin     = new Thickness( Layout.EdgeGap, 0, Layout.EdgeGap, 0 ),
      Cursor     = Cursors.Hand,
    };
    MakeDragOrClick( tb, onClick );
    return tb;
  }

  private Button MakeButton( string text, string tip, string fontName, float fontSize, Action onClick, bool addToCluster = true )
  {
    var btn = new Button
    {
      Content    = text,
      FontFamily = new FontFamily( fontName ),
      FontSize   = PtToDip( fontSize ),
      Width      = 38,
      Height     = 22,
      ToolTip    = tip,
      Focusable  = false,
    };
    if( Theme.IsDark )
    {
      btn.Style = (Style)Application.Current.FindResource( "DarkButton" );
    }
    btn.Click += ( _, _ ) => { try { onClick(); } catch { /* ignore */ } };

    if( addToCluster )
    {
      m_rightCluster.Children.Add( btn );
      m_helperButtons.Add( btn );
    }
    return btn;
  }

  private Border MakeBtnGap( int width = 24, int height = 0 )
  {
    var border = new Border { Width = width, Height = height };
    m_rightCluster.Children.Add( border );
    return border;
  }

  private void ComputeFullWidth()
  {
    double maxContentW = 0;
    foreach( TabModel m in AppState.Tabs )
    {
      maxContentW = Math.Max( maxContentW, m.ContentWidth );
    }
    // tab content + scrollbar + border padding (EdgeGap) + window border.
    m_fullWidth = maxContentW + Layout.ScrollBarWidth + 2 * Layout.EdgeGap + 2 * Theme.BorderThickness;
  }

  private void ComputeFullSize()
  {
    ComputeFullWidth();

    double maxContentH = 0;
    foreach( TabModel m in AppState.Tabs )
    {
      maxContentH = Math.Max( maxContentH, m.ContentHeight );
    }

    double viewport = Math.Min( 330, Math.Max( 320, maxContentH ) );
    double defaultH = viewport + 64;

    double screenH = SystemParameters.PrimaryScreenHeight;
    int    savedH  = AppState.Settings.WndHeight;
    m_fullHeight = savedH >= 140 && savedH <= screenH ? savedH : defaultH;
  }

  // ── Show / summon ────────────────────────────────────────────────
  // The window is always visible. ShowUi runs once at startup (restoring the
  // saved collapsed state); Summon brings it forward and expands it.
  public void ShowUi()
  {
    AppState.ActiveWindow = NativeMethods.GetForegroundWindow();

    if( !IsVisible )
    {
      Show();
    }
    EnsureHwnd();
    RestoreSavedPosition();
    BringToFront();

    if( AppState.Settings.IsCollapsed )
    {
      SetCollapsed( true, persist: false );
    }

    InstallWheelHook();
  }

  /// <summary>Bring the always-on window to the foreground and expand it.</summary>
  public void Summon()
  {
    if( !IsVisible )
    {
      Show();
    }
    EnsureHwnd();
    if( m_collapsed )
    {
      SetCollapsed( false );
    }
    BringToFront();
  }

  private void BringToFront()
  {
    NativeMethods.SetWindowPos( m_hwnd, new IntPtr( -1 ), 0, 0, 0, 0,
                                NativeMethods.SWP_NOMOVE |
                                NativeMethods.SWP_NOSIZE |
                                NativeMethods.SWP_NOACTIVATE );
  }

  private void RestoreSavedPosition()
  {
    if( m_hwnd == IntPtr.Zero )
    {
      return;
    }

    if( AppState.Settings.WndX is int px && AppState.Settings.WndY is int py )
    {
      MoveTo( px, py );
      return;
    }

    // No saved position: centre on the primary work area (physical px) instead of
    // landing at (0, 0), which sits in the top-of-screen snap zone.
    NativeMethods.RECT area = NativeMethods.GetPrimaryWorkArea();
    NativeMethods.GetWindowRect( m_hwnd, out NativeMethods.RECT rc );
    MoveTo( area.Left + ( area.Width - rc.Width ) / 2,
            area.Top  + ( area.Height - rc.Height ) / 2 );
  }

  private void MoveTo( int x, int y )
  {
    NativeMethods.SetWindowPos( m_hwnd, IntPtr.Zero, x, y, 0, 0,
                                NativeMethods.SWP_NOSIZE   |
                                NativeMethods.SWP_NOZORDER |
                                NativeMethods.SWP_NOACTIVATE );
  }

  // ── Collapse / expand ────────────────────────────────────────────
  public void ToggleCollapsed() => SetCollapsed( !m_collapsed );

  public void SetCollapsed( bool collapse, bool persist = true )
  {
    m_suppressResizePersist = true;
    try
    {
      if( collapse )
      {
        if( !m_collapsed )
        {
          m_fullHeight = ActualHeight;
        }
        m_collapsed               = true;
        m_tabs.Visibility         = Visibility.Collapsed;
        m_rightCluster.Visibility = Visibility.Collapsed;
        m_toggleBtn.Content       = "▼";
        m_toggleBtn.ToolTip       = "Expand window";

        // Collapsed strip is sized to its corner controls (not resizable).
        // Relax each Min before assigning the (smaller) value: WPF coerces a new
        // size up to the current Min, so the Min has to drop first or it sticks.
        ResizeMode = ResizeMode.NoResize;
        Size c     = CollapsedSize();
        MinWidth   = c.Width;
        MaxWidth   = c.Width;
        Width      = c.Width;
        MinHeight  = c.Height;
        MaxHeight  = c.Height;
        Height     = c.Height;
      }
      else
      {
        m_collapsed               = false;
        m_tabs.Visibility         = Visibility.Visible;
        m_rightCluster.Visibility = Visibility.Visible;
        m_toggleBtn.Content       = "▲";
        m_toggleBtn.ToolTip       = "Shrink window";

        ResizeMode = ResizeMode.CanResize;
        DisableMaximize(); // CanResize re-adds WS_MAXIMIZEBOX; strip it again
        MinWidth   = m_fullWidth;
        MaxWidth   = m_fullWidth;
        Width      = m_fullWidth;
        MinHeight  = 140;
        MaxHeight  = SystemParameters.PrimaryScreenHeight;
        Height     = m_fullHeight;
      }
    }
    finally
    {
      m_suppressResizePersist = false;
    }

    if( persist )
    {
      AppState.Settings.SetCollapsed( collapse );
    }
  }

  // The collapsed window is the left corner cluster plus the window border and
  // its edge-gap padding — derived, so it tracks the controls and the constants.
  private Size CollapsedSize()
  {
    m_leftStrip.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
    Size   s      = m_leftStrip.DesiredSize;
    double chrome = 2 * ( Theme.BorderThickness + Layout.EdgeGap );
    return new Size( Math.Ceiling( s.Width + chrome ), Math.Ceiling( s.Height + chrome ) );
  }

  private void ForceRepaint()
  {
    NativeMethods.SetWindowPos( m_hwnd, new IntPtr( -1 ), 0, 0, 0, 0,
                                NativeMethods.SWP_NOMOVE |
                                NativeMethods.SWP_NOSIZE |
                                NativeMethods.SWP_NOACTIVATE );
    InvalidateVisual();
  }

  // ── Favourite spot (physical px) ─────────────────────────────────
  public void SetFavouriteSpot()
  {
    if( m_hwnd == IntPtr.Zero )
    {
      return;
    }
    NativeMethods.GetWindowRect( m_hwnd, out NativeMethods.RECT rc );
    m_favX = rc.Left;
    m_favY = rc.Top;
    AppState.Settings.SetFav( rc.Left, rc.Top );
  }

  public void MoveToFavouriteSpot()
  {
    m_favX = AppState.Settings.FavX;
    m_favY = AppState.Settings.FavY;
    if( (m_favX is int fx) &&
        (m_favY is int fy) &&
        (m_hwnd != nint.Zero) )
    {
      MoveTo( fx, fy );
      AppState.Settings.SetWndPos( fx, fy );
    }
  }

  // ── Indicators ───────────────────────────────────────────────────
  public void UpdateClipIndicator( bool on )
  {
    m_clipIndicator.Text    = on ? "●" : "○";
    m_clipIndicator.ToolTip = "Clipboard send mode: " + ( on ? "ON" : "OFF" );
  }

  public void UpdateStripIndicator( bool on )
  {
    m_stripIndicator.Text    = on ? "☻" : "☺";
    m_stripIndicator.ToolTip = "Strip emojis from comments: " + ( on ? "ON" : "OFF" );
  }

  // ── Active-window tracking ───────────────────────────────────────
  private void TrackActiveWindow()
  {
    IntPtr h = NativeMethods.GetForegroundWindow();
    if( h == IntPtr.Zero || h == m_hwnd )
    {
      return;
    }
    AppState.ActiveWindow = h;
  }

  // ── Window source: ex-styles, dark frame, message hook ───────────
  protected override void OnSourceInitialized( EventArgs e )
  {
    base.OnSourceInitialized( e );
    EnsureHwnd();

    // Make the window no-activate + tool-window so it never steals focus and
    // stays out of the taskbar / alt-tab.
    IntPtr ex = NativeMethods.GetWindowLongPtr( m_hwnd, NativeMethods.GWL_EXSTYLE );
    long exNew = ex.ToInt64() | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW;
    NativeMethods.SetWindowLongPtr( m_hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr( exNew ) );

    DisableMaximize();
    Theme.ApplyDarkFrame( m_hwnd );

    HwndSource.FromHwnd( m_hwnd )?.AddHook( WndHook );
  }

  // ResizeMode.CanResize gives the window WS_MAXIMIZEBOX, which makes it eligible
  // for the Aero-Snap "drag to the top = maximize" gesture — that fills the height
  // and pushes the resize edges off-screen. Stripping WS_MAXIMIZEBOX disables that
  // gesture while leaving edge-resize (WS_THICKFRAME / WindowChrome) intact. WPF
  // re-adds the style whenever ResizeMode is set back to CanResize, so this is
  // re-applied after each expand.
  private void DisableMaximize()
  {
    if( m_hwnd == IntPtr.Zero )
    {
      return;
    }
    const long WS_MAXIMIZEBOX = 0x00010000;
    long style = NativeMethods.GetWindowLongPtr( m_hwnd, NativeMethods.GWL_STYLE ).ToInt64();
    NativeMethods.SetWindowLongPtr( m_hwnd, NativeMethods.GWL_STYLE, new IntPtr( style & ~WS_MAXIMIZEBOX ) );
  }

  private void EnsureHwnd()
  {
    if( m_hwnd == IntPtr.Zero )
    {
      m_hwnd = new WindowInteropHelper( this ).EnsureHandle();
    }
  }

  private IntPtr WndHook( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
  {
    // WM_EXITSIZEMOVE fires after an edge-resize (the resize still uses the OS
    // sizing loop via WindowChrome); moves are handled manually below.
    if( msg == WM_EXITSIZEMOVE )
    {
      PersistPositionAndSize();
    }
    else if( msg == NativeMethods.WM_GETMINMAXINFO && !m_collapsed && m_hwnd != IntPtr.Zero )
    {
      ClampMinMax( lParam );
      handled = true;
    }
    return IntPtr.Zero;
  }

  // Cap the window's size and the maximized placement to the work area (minus a
  // margin) and lock the width. This guarantees at least one edge stays on-screen
  // even if something (Aero-Snap, Win+Up) tries to maximize the window — so it can
  // always be grabbed and resized/dragged back.
  private void ClampMinMax( IntPtr lParam )
  {
    const int margin = 32;

    var mmi = Marshal.PtrToStructure<NativeMethods.MINMAXINFO>( lParam );
    NativeMethods.RECT work = NativeMethods.GetWorkAreaForWindow( m_hwnd );

    // Lock to the intended full width (physical px), NOT the current window width:
    // during a collapse→expand this message can fire while the window is still at
    // the collapsed width, and clamping to that would trap the expand at the
    // narrow size.
    double scale = NativeMethods.GetDpiForWindow( m_hwnd ) / 96.0;
    int width = (int)Math.Round( m_fullWidth * scale );
    int minH  = (int)Math.Round( 140 * scale );
    int maxH  = Math.Max( minH, work.Height - margin );

    mmi.ptMaxPosition  = new NativeMethods.POINT { X = work.Left, Y = work.Top };
    mmi.ptMaxSize      = new NativeMethods.POINT { X = width, Y = maxH };
    mmi.ptMinTrackSize = new NativeMethods.POINT { X = width, Y = minH };
    mmi.ptMaxTrackSize = new NativeMethods.POINT { X = width, Y = maxH };

    Marshal.StructureToPtr( mmi, lParam, false );
  }

  // ── Drag move + snap (physical px) ───────────────────────────────
  // A press on the bare strip background starts a window drag immediately.
  private void OnStripDragStart( object sender, MouseButtonEventArgs e )
  {
    BeginWindowDrag();
  }

  // Manual drag (deliberately NOT DragMove / the OS move loop): moving the window
  // ourselves with SetWindowPos means Windows' Aero-Snap never engages, so a drag
  // to the top edge can't maximize the window. We apply our own top/favourite snap.
  private void BeginWindowDrag()
  {
    EnsureHwnd();
    NativeMethods.GetCursorPos( out NativeMethods.POINT cur );
    NativeMethods.GetWindowRect( m_hwnd, out NativeMethods.RECT rc );
    m_dragOffsetX  = cur.X - rc.Left;
    m_dragOffsetY  = cur.Y - rc.Top;
    m_snappedToTop = false;
    m_snappedToFav = false;
    m_dragging     = true;
    CaptureMouse();
  }

  protected override void OnMouseMove( MouseEventArgs e )
  {
    base.OnMouseMove( e );
    if( !m_dragging )
    {
      return;
    }

    NativeMethods.GetCursorPos( out NativeMethods.POINT cur );
    ( int left, int top ) = ApplyDragSnap( cur.X - m_dragOffsetX, cur.Y - m_dragOffsetY );
    NativeMethods.SetWindowPos( m_hwnd, IntPtr.Zero, left, top, 0, 0,
      NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE );
  }

  protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
  {
    base.OnMouseLeftButtonUp( e );
    EndDrag();
  }

  protected override void OnLostMouseCapture( MouseEventArgs e )
  {
    base.OnLostMouseCapture( e );
    EndDrag();
  }

  private void EndDrag()
  {
    if( !m_dragging )
    {
      return;
    }
    m_dragging = false;
    ReleaseMouseCapture();
    PersistPositionAndSize();
  }

  // Top-of-screen and favourite-spot snapping with release hysteresis, applied to
  // the cursor-implied top-left (UI.ahk OnWindowMoving).
  private (int Left, int Top) ApplyDragSnap( int left, int top )
  {
    const int snapThreshold    = 20;
    const int releaseThreshold = 30;

    if( m_favX is int favX && m_favY is int favY )
    {
      if( m_snappedToFav )
      {
        if( Math.Abs( left - favX ) >= releaseThreshold || Math.Abs( top - favY ) >= releaseThreshold )
        {
          m_snappedToFav = false;
        }
        else
        {
          return ( favX, favY );
        }
      }
      else if( Math.Abs( left - favX ) <= snapThreshold && Math.Abs( top - favY ) <= snapThreshold )
      {
        m_snappedToFav = true;
        m_snappedToTop = false;
        return ( favX, favY );
      }
    }

    if( m_snappedToTop )
    {
      if( top >= releaseThreshold )
      {
        m_snappedToTop = false;
      }
      else
      {
        return ( left, 0 ); // stay snapped to the top edge, allow horizontal movement
      }
    }
    else if( top <= snapThreshold )
    {
      m_snappedToTop = true;
      return ( left, 0 );
    }

    return ( left, top );
  }

  // Make a corner control double as a drag handle: a plain click runs <paramref
  // name="onClick"/>, but pressing and moving past the drag threshold starts a
  // window move (mirrors the DragDetect handling in UI.ahk).
  private void MakeDragOrClick( UIElement el, Action onClick )
  {
    NativeMethods.POINT start = default;
    bool armed = false;
    bool dragged = false;

    el.PreviewMouseLeftButtonDown += ( _, e ) =>
    {
      NativeMethods.GetCursorPos( out start );
      armed   = true;
      dragged = false;
      el.CaptureMouse();
      e.Handled = true;
    };

    el.PreviewMouseMove += ( _, e ) =>
    {
      if( !armed || e.LeftButton != MouseButtonState.Pressed )
      {
        return;
      }
      NativeMethods.GetCursorPos( out NativeMethods.POINT cur );
      if( Math.Abs( cur.X - start.X ) >= 4 || Math.Abs( cur.Y - start.Y ) >= 4 )
      {
        dragged = true;
        armed   = false;
        el.ReleaseMouseCapture();
        BeginWindowDrag();
      }
    };

    el.PreviewMouseLeftButtonUp += ( _, e ) =>
    {
      if( armed && !dragged )
      {
        armed = false;
        el.ReleaseMouseCapture();
        onClick();
      }
      e.Handled = true;
    };
  }

  // Right-click menu only on the bare window / indicators / left collapse button;
  // suppress it over the tab control (and its contents) and the helper buttons.
  private void OnContextMenuOpening( object sender, ContextMenuEventArgs e )
  {
    if( e.OriginalSource is DependencyObject d && ( IsInside( d, m_tabs ) || IsInside( d, m_rightCluster ) ) )
    {
      e.Handled = true;
    }
  }

  private static bool IsInside( DependencyObject? node, DependencyObject ancestor )
  {
    while( node != null )
    {
      if( ReferenceEquals( node, ancestor ) )
      {
        return true;
      }
      DependencyObject? parent = node is Visual ? VisualTreeHelper.GetParent( node ) : null;
      parent ??= LogicalTreeHelper.GetParent( node );
      node = parent;
    }
    return false;
  }

  private void PersistPositionAndSize()
  {
    if( m_hwnd != IntPtr.Zero )
    {
      NativeMethods.GetWindowRect( m_hwnd, out NativeMethods.RECT rc );
      AppState.Settings.SetWndPos( rc.Left, rc.Top );
    }

    if( !m_collapsed && !m_suppressResizePersist )
    {
      m_fullHeight = ActualHeight;
      AppState.Settings.SetWndHeight( (int)Math.Round( ActualHeight ) );
    }
  }

  // ── Low-level mouse-wheel hook (scrolls the active tab) ──────────
  private void InstallWheelHook()
  {
    if( m_wheelHook != IntPtr.Zero )
    {
      return;
    }
    m_wheelProc = WheelHookProc;
    m_wheelHook = NativeMethods.SetWindowsHookEx( NativeMethods.WH_MOUSE_LL, m_wheelProc,
                                                  NativeMethods.GetModuleHandle( null ), 0 );
  }

  private void RemoveWheelHook()
  {
    if( m_wheelHook != IntPtr.Zero )
    {
      NativeMethods.UnhookWindowsHookEx( m_wheelHook );
      m_wheelHook = IntPtr.Zero;
    }
    m_wheelProc = null;
  }

  private nint WheelHookProc( int nCode, nint wParam, nint lParam )
  {
    if( (nCode >= 0) &&
        (wParam == (nint)NativeMethods.WM_MOUSEWHEEL) &&
        IsVisible &&
        !m_collapsed )
    {
      var data = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>( lParam );
      if( NativeMethods.GetWindowRect( m_hwnd, out NativeMethods.RECT rc ) &&
          (data.pt.X >= rc.Left) && (data.pt.X < rc.Right)  &&
          (data.pt.Y >= rc.Top)  && (data.pt.Y < rc.Bottom) )
      {
        int delta = unchecked((short)((data.mouseData >> 16) & 0xFFFF));
        var idx   = m_tabs.SelectedIndex;
        if( (delta != 0) && (idx >= 0) && (idx < m_scrollers.Count) )
        {
          m_scrollers[idx].ScrollBy( -(delta / 120) );
        }
        return new nint( 1 ); // swallow so the focused window doesn't also scroll
      }
    }
    return NativeMethods.CallNextHookEx( IntPtr.Zero, nCode, wParam, lParam );
  }

  protected override void OnClosed( EventArgs e )
  {
    RemoveWheelHook();
    m_activeWinTimer.Stop();
    base.OnClosed( e );
  }

  // ── Smooth, accelerating wheel scroll for one ScrollViewer ───────
  private sealed class SmoothScroller
  {
    private readonly ScrollViewer    m_sv;
    private readonly DispatcherTimer m_timer;
    private double m_target;
    private long   m_lastMs;
    private double m_accel;

    public SmoothScroller( ScrollViewer sv )
    {
      m_sv = sv;
      m_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds( 16 ) };
      m_timer.Tick += ( _, _ ) => Tick();
    }

    public void ScrollBy( int notches )
    {
      const double basePx   = 30;
      const double maxPx    = 150;
      const double step     = 20;
      const double windowMs = 150;

      if( !m_timer.IsEnabled )
      {
        m_target = m_sv.VerticalOffset;
      }

      var now = Environment.TickCount64;
      m_accel  = (now - m_lastMs <= windowMs)
                   ? Math.Min( m_accel + step, maxPx - basePx )
                   : 0;
      m_lastMs = now;

      var px = basePx + m_accel;
      m_target = Math.Max( 0, Math.Min( m_target + (notches * px), m_sv.ScrollableHeight ) );
      if( !m_timer.IsEnabled )
      {
        m_timer.Start();
      }
    }

    private void Tick()
    {
      var cur  = m_sv.VerticalOffset;
      var diff = m_target - cur;
      if( Math.Abs( diff ) <= 0.5 )
      {
        m_sv.ScrollToVerticalOffset( m_target );
        m_timer.Stop();
        return;
      }
      m_sv.ScrollToVerticalOffset( cur + (diff * 0.35) );
    }
  }
}
