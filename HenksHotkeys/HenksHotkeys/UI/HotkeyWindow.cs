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
  private StackPanel m_leftCluster      = null!;
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

  // Button drag-to-reposition (distinct from the window-move drag above).
  private SymbolElement? m_btnDragSym;
  private Canvas?        m_btnDragCanvas;
  private DataTabModel?  m_btnDragModel;
  private Point          m_btnDragStart;
  private Vector         m_btnDragGrab;
  private bool           m_btnDragging;
  private Border?        m_btnDragGhost;
  private Border?        m_btnDropHi;       // filled cell highlight (drop on an empty cell)
  private Border?        m_btnDropCaret;    // thin insertion caret (drop between buttons)
  private FrameworkElement? m_btnDragEl;   // the element holding the mouse capture

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
    MinHeight = Layout.WindowMinHeight;
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
    BuildLeftCornerControls();
    BuildRightCornerControls();

    // The strip auto-sizes to its corner controls; the window border's EdgeGap
    // padding provides the gap to the window edges (no hand-tuned margins). The accent
    // background turns it into a subtle toolbar (size unchanged, so collapse still fits).
    var strip = new Grid { Background = Palette.Brush( "AccentBarBg" ) };
    strip.Children.Add( m_leftCluster  );
    strip.Children.Add( m_rightCluster );
    strip.MouseLeftButtonDown += OnStripDragStart;

    var grid = new Grid();
    grid.RowDefinitions.Add( new RowDefinition { Height = GridLength.Auto } );
    grid.RowDefinitions.Add( new RowDefinition { Height = new GridLength( 1, GridUnitType.Star ) } );
    Grid.SetRow( strip,  0 );
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
    m_tabs.Style = (Style)Application.Current.FindResource( "AppTabControl" );

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
        Width       = model.ContentWidth,
        Height      = model.ContentHeight,
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

        // Data-tab buttons can be dragged to a new cell.
        if( model is DataTabModel dragModel && sym.Source is not null )
        {
          WireDrag( btn, sym, canvas, dragModel );
        }
      }

      // Every empty cell behaves like a drop target: hovering one shows its outline.
      if( model is DataTabModel hoverModel )
      {
        AttachEmptyCellHover( canvas, hoverModel );
      }

      foreach( TabModel.SectionHeader hdr in model.Headers )
      {
        FrameworkElement label = BuildSectionHeader( hdr, model );
        Canvas.SetLeft( label, hdr.X );
        Canvas.SetTop( label, hdr.Y );
        canvas.Children.Add( label );
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

      // Right-click the open area of a data tab → "Add button here" (placed near the
      // click). Buttons keep their own Edit/Delete menu, which takes precedence.
      if( model is DataTabModel dataModel )
      {
        AttachAddHereMenu( sv, canvas, dataModel );
      }

      var item = new TabItem
      {
        Header = model.Name,
        Content = sv,
        Style = (Style)Application.Current.FindResource( "AppTabItem" )!,
      };
      m_tabs.Items.Add( item );
    }

    m_tabs.SelectedIndex = (targetTab >= 0) && (targetTab < m_tabs.Items.Count) ? targetTab : 0;
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

    // A locked secret (couldn't be decrypted) wears a padlock and says why.
    bool locked = sym.Source is { IsSecret: true, Locked: true };
    if( locked ) buttonText = "🔒 " + buttonText;

    string tip  = UiText.NormalizeDisplayText( sym.Desc );
    if( locked ) tip = Append( tip, "Locked — couldn't decrypt; right-click → Edit to re-enter." );
    string stem = "";
    bool   isEmoji = false;

    if( model.UseEmojiImages )
    {
      int emojiPx = (int)Math.Round( model.SymBtnSizeX * Layout.SymbolScale );
      EmojiImageProvider.Result res = EmojiImageProvider.Get( sym.Char, emojiPx );
      stem = res.Stem;
      if( res.Image is not null )
      {
        isEmoji = true;
        btn.Content = new Image
        {
          Source = res.Image,
          Width  = model.SymBtnSizeX * Layout.SymbolScale,
          Height = model.SymBtnSizeY * Layout.SymbolScale,
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
        Text              = buttonText,
        TextAlignment     = sym.Align == "left" ? TextAlignment.Left : TextAlignment.Center,
        TextTrimming      = sym.Align == "left" ? TextTrimming.CharacterEllipsis : TextTrimming.None,
        VerticalAlignment = VerticalAlignment.Center,
      };
    }

    btn.Style = (Style)Application.Current.FindResource( isEmoji ? "GridEmojiButton" : "GridButton" )!;

    if( sym.Hotkey.Length > 0 ) tip = Append( tip, HotkeyParser.Label( sym.Hotkey ) );
    if( sym.TipChar )           tip = Append( tip, sym.Char );
    if( model.UseEmojiImages && stem.Length > 0 ) tip = Append( tip, "U+" + stem );
    if( tip.Length > 0 )
    {
      btn.ToolTip = new TextBlock { Text = tip };
    }

    WireSymbolButton( btn, sym.ClickAction );

    // Data-tab buttons get a right-click menu to edit / delete them. Built-in code
    // tabs (Source == null) aren't JSON-editable, so they get none.
    if( sym.Source is {} def )
    {
      btn.ContextMenu = BuildButtonMenu( def );
    }

    sym.Ctrl = btn;
    return btn;
  }

  private static ContextMenu BuildButtonMenu( ButtonDef def )
  {
    var menu = new ContextMenu();
    var edit = new MenuItem { Header = "Edit button…" };
    var del  = new MenuItem { Header = "Delete button" };
    edit.Click += ( _, _ ) => ButtonCommands.Edit(   def );
    del.Click  += ( _, _ ) => ButtonCommands.Delete( def );
    menu.Items.Add( edit );
    menu.Items.Add( del );
    return menu;
  }

  // ── Drag a button to a new slot (swaps with the cell dropped on) ───
  // The whole drag is driven from the dragged element's own preview events: it keeps the
  // mouse capture it grabbed on press, so move/up fire reliably even off the element (the
  // earlier approach handed capture to the canvas mid-press, which often didn't take — so
  // the ghost/highlight were placed once and never moved).
  private void WireDrag( FrameworkElement el, SymbolElement sym, Canvas canvas, DataTabModel model )
  {
    el.PreviewMouseLeftButtonDown += ( _, e ) =>
    {
      m_btnDragSym    = sym;
      m_btnDragCanvas = canvas;
      m_btnDragModel  = model;
      m_btnDragStart  = e.GetPosition( canvas );
      m_btnDragging   = false;
    };

    el.PreviewMouseMove += ( _, e ) =>
    {
      if( m_btnDragSym != sym || e.LeftButton != MouseButtonState.Pressed )
      {
        return;
      }
      Point p = e.GetPosition( canvas );
      if( !m_btnDragging )
      {
        if( Math.Abs( p.X - m_btnDragStart.X ) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs( p.Y - m_btnDragStart.Y ) < SystemParameters.MinimumVerticalDragDistance )
        {
          return; // still a click, not a drag — let the send proceed
        }
        BeginButtonDrag( p );
        m_btnDragEl = el;
        el.CaptureMouse();   // ensure capture (esp. for blank Borders, which don't self-capture)
      }
      UpdateButtonDrag( p );
      e.Handled = true;
    };

    el.PreviewMouseLeftButtonUp += ( _, e ) =>
    {
      if( m_btnDragging && m_btnDragSym == sym )
      {
        DropButtonDrag( e.GetPosition( canvas ) );
        e.Handled = true; // swallow the click so the drop doesn't also send
      }
    };

    el.LostMouseCapture += ( _, _ ) => CancelButtonDrag();
  }

  private void BeginButtonDrag( Point at )
  {
    if( m_btnDragSym    is not {} sym ||
        m_btnDragCanvas is not {} canvas )
    {
      return;
    }

    m_activeWinTimer.Stop(); // don't let background ticks disturb the drag
    m_btnDragging = true;
    m_btnDragGrab = at - new Point( sym.X, sym.Y );

    if( sym.Ctrl is UIElement dragged )
    {
      dragged.Opacity = 0.35; // dim the original while its ghost is in flight
    }

    m_btnDragGhost = MakeGhost( sym );
    Panel.SetZIndex( m_btnDragGhost, 1000 );
    canvas.Children.Add( m_btnDragGhost );

    // Drop on an empty cell → a filled, outlined cell highlight.
    m_btnDropHi = new Border
    {
      BorderBrush      = Palette.Brush( "SwitchOn" ),
      BorderThickness  = new Thickness( 2 ),
      CornerRadius     = new CornerRadius( Layout.ButtonCornerRadius ),
      Background       = Translucent( Palette.Colour( "SwitchOn" ), 0.22 ),
      IsHitTestVisible = false,
      Visibility       = Visibility.Collapsed,
    };
    Panel.SetZIndex( m_btnDropHi, 1001 ); // above the ghost so the drop cue is never hidden
    canvas.Children.Add( m_btnDropHi );

    // Drop between two buttons → a thin insertion caret on the before/after edge.
    m_btnDropCaret = new Border
    {
      Width            = 3,
      Background       = Palette.Brush( "SwitchOn" ),
      CornerRadius     = new CornerRadius( 1.5 ),
      IsHitTestVisible = false,
      Visibility       = Visibility.Collapsed,
    };
    Panel.SetZIndex(     m_btnDropCaret, 1001 );
    canvas.Children.Add( m_btnDropCaret );

    UpdateButtonDrag( at );
  }

  private static SolidColorBrush Translucent( Color c, double opacity )
  {
    var b = new SolidColorBrush( c ) { Opacity = opacity };
    b.Freeze();
    return b;
  }

  private void UpdateButtonDrag( Point at )
  {
    if( m_btnDragGhost is {} ghost )
    {
      Canvas.SetLeft( ghost, at.X - m_btnDragGrab.X );
      Canvas.SetTop(  ghost, at.Y - m_btnDragGrab.Y );
    }

    if( m_btnDragModel is not {} model ||
        m_btnDropHi    is not {} hi    ||
        m_btnDropCaret is not {} caret )
    {
      return;
    }

    DropSpot spot = model.ResolveDrop( at, m_btnDragSym?.Source );
    if( spot.Kind is DropKind.InsertBefore or DropKind.InsertAfter )
    {
      // A thin caret on the insertion edge (which of rule a/b will fire is visible).
      caret.Height = spot.CellH;
      Canvas.SetLeft( caret, spot.CaretX );
      Canvas.SetTop(  caret, spot.CellY  );
      caret.Visibility = Visibility.Visible;
      hi.Visibility    = Visibility.Collapsed;
    }
    else
    {
      // Empty cell (or a brand-new row) — a filled cell highlight ("place here").
      hi.Width  = spot.CellW;
      hi.Height = spot.CellH;
      Canvas.SetLeft( hi, spot.CellX );
      Canvas.SetTop(  hi, spot.CellY );
      hi.Visibility    = Visibility.Visible;
      caret.Visibility = Visibility.Collapsed;
    }
  }

  private void DropButtonDrag( Point drop )
  {
    if( !m_btnDragging )
    {
      return;
    }
    SymbolElement? source = m_btnDragSym;
    DataTabModel?  model  = m_btnDragModel;
    CancelButtonDrag(); // release capture, remove ghost/highlight, restore opacity

    if( source?.Source is not {} moving || model is null )
    {
      return;
    }

    DropSpot spot = model.ResolveDrop( drop, moving );
    bool ok = spot.Kind == DropKind.NewRow
                ? TabStore.MoveButtonToNewRow( moving, spot.Col )
                : TabStore.MoveButtonToCell(   moving, spot.Row, spot.Col ); // empty → place; on a button → insert (shift)

    if( ok )
    {
      AppState.RequestReload?.Invoke(); // rebuilds the tab at the new layout
    }
  }

  private void CancelButtonDrag()
  {
    if( !m_btnDragging )
    {
      return;
    }
    m_btnDragging = false;
    m_activeWinTimer.Start();

    if( m_btnDragEl is {} el && ReferenceEquals( Mouse.Captured, el ) )
    {
      el.ReleaseMouseCapture();
    }
    m_btnDragEl = null;

    if( m_btnDragSym?.Ctrl is UIElement dragged )
    {
      dragged.Opacity = 1.0;
    }
    if( m_btnDragCanvas is { } canvas )
    {
      if( m_btnDragGhost  is {} g ) canvas.Children.Remove( g );
      if( m_btnDropHi     is {} h ) canvas.Children.Remove( h );
      if( m_btnDropCaret  is {} c ) canvas.Children.Remove( c );
    }
    m_btnDragGhost = null;
    m_btnDropHi    = null;
    m_btnDropCaret = null;
  }

  private static Border MakeGhost( SymbolElement sym )
  {
    string label = UiText.NormalizeDisplayText( sym.ShowChar ? sym.Char : sym.Desc );
    return new Border
    {
      Width            = sym.W,
      Height           = sym.H,
      CornerRadius     = new CornerRadius( Layout.ButtonCornerRadius ),
      Background       = Palette.Brush( "ControlHover" ),
      BorderBrush      = Palette.Brush( "TextSecondary" ), // neutral, so blue = the drop cue only
      BorderThickness  = new Thickness( 1 ),
      Opacity          = 0.8,
      IsHitTestVisible = false,
      Child = new TextBlock
      {
        Text                = label,
        Foreground          = Palette.Brush( "TextPrimary" ),
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment   = VerticalAlignment.Center,
        TextTrimming        = TextTrimming.CharacterEllipsis,
      },
    };
  }

  // Faint outline that follows the pointer over the tab's empty cells, so every cell reads
  // as a place you can drop / add a button (the old materialised "blank" cells, computed).
  private void AttachEmptyCellHover( Canvas canvas, DataTabModel model )
  {
    var hover = new Border
    {
      BorderBrush      = Theme.BlankBorderColor,
      BorderThickness  = new Thickness( Theme.BorderThickness ),
      CornerRadius     = new CornerRadius( 4 ),
      IsHitTestVisible = false,
      Visibility       = Visibility.Collapsed,
    };
    Panel.SetZIndex( hover, 1 ); // above the canvas, below the buttons
    canvas.Children.Add( hover );

    canvas.MouseMove += ( _, e ) =>
    {
      if( m_btnDragging ) { hover.Visibility = Visibility.Collapsed; return; } // the drag has its own cues
      Point p    = e.GetPosition( canvas );
      int   row  = model.RowAt( p.Y );
      int   col  = model.ColAt( p.X );
      bool  free = row <= model.MaxRow                       &&
                   col <  Math.Max( 1, model.Entry.Columns ) &&
                   model.SymbolAt( row, col, null ) is null;
      if( free )
      {
        hover.Width  = model.SymBtnSizeX;
        hover.Height = model.SymBtnSizeY;
        Canvas.SetLeft( hover, model.SymOrgX + col * model.ColWidth );
        Canvas.SetTop(  hover, model.RowTop( row ) );
        hover.Visibility = Visibility.Visible;
      }
      else
      {
        hover.Visibility = Visibility.Collapsed;
      }
    };
    canvas.MouseLeave += ( _, _ ) => hover.Visibility = Visibility.Collapsed;
  }

  // The open-area menu for a data tab: remembers where the right-click landed (in canvas
  // coordinates, so scrolling is accounted for) and adds a button there.
  private static void AttachAddHereMenu( ScrollViewer sv, Canvas canvas, DataTabModel model )
  {
    Point clickPoint = default;
    sv.PreviewMouseRightButtonDown += ( _, e ) => clickPoint = e.GetPosition( canvas );

    var menu = new ContextMenu();
    var add  = new MenuItem { Header = "Add button here…" };
    add.Click += ( _, _ ) => ButtonCommands.AddHere( model, clickPoint );
    menu.Items.Add( add );
    sv.ContextMenu = menu;
  }

  // A section label: bold text sitting on a separator line spanning the columns.
  // An empty name renders as just the line (a plain divider between groups).
  private static FrameworkElement BuildSectionHeader( TabModel.SectionHeader hdr, TabModel model )
  {
    var text = new TextBlock
    {
      Text                = UiText.NormalizeDisplayText( hdr.Name ),
      FontFamily          = new FontFamily( model.FontName ),
      FontSize            = PtToDip( model.FontSize ),
      FontWeight          = FontWeights.SemiBold,
      Foreground          = Palette.Brush( "AccentText" ),
      VerticalAlignment   = VerticalAlignment.Bottom,
      HorizontalAlignment = HorizontalAlignment.Left,
      Margin              = new Thickness( Layout.EdgeGap, 0, 0, Layout.EdgeGap ),
      TextTrimming        = TextTrimming.CharacterEllipsis,
    };
    return new Border
    {
      Width           = hdr.Width,
      Height          = hdr.Height,
      BorderBrush     = Palette.Brush( "AccentBarBorder" ),
      BorderThickness = new Thickness( 0, 0, 0, 1 ), // separator line
      Child           = text,
    };
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
        _ = TextSender.SendInputKeys( "{Enter}" ); // queues after the first click's send
        return;
      }
      try { click(); } catch { /* never let a send failure kill the UI */ }
    };
  }

  private void BuildLeftCornerControls()
  {
    m_clipIndicator  = MakeIndicator( "○", AppActions.ToggleClipboardSendMode );
    m_stripIndicator = MakeIndicator( "☺", AppActions.ToggleStripSendEmojis );

    m_toggleBtn = MakeButton( "▲", "Shrink window", "Segoe UI Symbol", 11f, ToggleCollapsed, addToCluster: false );
    m_toggleBtn.Margin = new Thickness( Layout.EdgeGap, 0, Layout.EdgeGap, 0 );
    // The left collapse button also doubles as a drag handle. MakeDragOrClick
    // handles the press at the preview stage, so the button's own Click never
    // fires (no double toggle) — a plain click toggles, a press-drag moves.
    MakeDragOrClick( m_toggleBtn, ToggleCollapsed );

    m_leftCluster = new StackPanel
    {
      Orientation         = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Left,
      VerticalAlignment   = VerticalAlignment.Center,
    };
    m_leftCluster.Children.Add( m_clipIndicator  );
    m_leftCluster.Children.Add( m_toggleBtn      );
    m_leftCluster.Children.Add( m_stripIndicator );

    UpdateClipIndicator(  AppState.UseClipSend     );
    UpdateStripIndicator( AppState.StripSendEmojis );
  }

  private void BuildRightCornerControls()
  {
    m_rightCluster = new StackPanel
    {
      Orientation         = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Right,
      VerticalAlignment   = VerticalAlignment.Center,
    };
    RaiseTop( 1, MakeButton( "🔄",  "Repaint / Refresh",          "Segoe UI Symbol", 10f, ForceRepaint ) );
    RaiseTop( 0, MakeBtnGap( Layout.CornerButtonWidth / 2 ) );
    RaiseTop( 3, MakeButton( "⌫.", "Back 3, Replace with period", "Segoe UI Symbol", 12f, () => { _ = TextSender.SendInputKeys( "\b\b\b. " ); } ) );
    RaiseTop( 9, MakeButton( "⇚,", "Back 3, Insert Comma",        "Segoe UI Symbol", 18f, () => { _ = TextSender.SendInputKeys( "{Left}{Left}{Left}, " ); } ) );
    RaiseTop( 0, MakeButton( "↩",  "Enter / Newline",             "Segoe UI Symbol", 12f, () => { _ = TextSender.SendInputKeys( "{Enter}" ); } ) );
    RaiseTop( 0, MakeButton( "▲",  "Shrink window",               "Segoe UI Symbol", 11f, ToggleCollapsed ) );
  }

  private void RaiseTop( int padValue, Control cc )
  {
    var pad = cc.Padding;
    pad.Top    -= padValue;
    cc.Padding =  pad;
  }

  private void RaiseTop( int unused1, FrameworkElement unused2 )
  {
     // This exists literally so that it can keep the button gap row
     // looking the same as the rest. It has no functional purpose.
  }

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
      Width      = Layout.CornerButtonWidth,
      Height     = Layout.CornerButtonHeight,
      ToolTip    = tip,
      Margin     = new Thickness( Layout.ButtonGap, 0, 0, 0 ),
      Focusable  = false,
      Style      = (Style)Application.Current?.FindResource( "GridButton" )!,
    };
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
    // tab content (already includes the buttons' TabEdgeGap inset) + the themed
    // scrollbar + the window-edge gap (EdgeGap) + window border.
    m_fullWidth = maxContentW +
                  Layout.ScrollBarWidth +
                  Layout.EdgeGap        * 2 +
                  Theme.BorderThickness * 2;
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
    bool   hgtLim  = savedH >= Layout.WindowMinHeight &&
                     savedH <= screenH;
    m_fullHeight = hgtLim ? savedH : defaultH;
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

    if( AppState.Settings.WndX is {} px &&
        AppState.Settings.WndY is {} py )
    {
      MoveTo( px, py );
      return;
    }

    // No saved position: centre on the primary work area (physical px) instead of
    // landing at (0, 0), which sits in the top-of-screen snap zone.
    NativeMethods.RECT area = NativeMethods.GetPrimaryWorkArea();
    NativeMethods.GetWindowRect( m_hwnd, out NativeMethods.RECT rc );
    MoveTo( area.Left + (area.Width  - rc.Width)  / 2,
            area.Top  + (area.Height - rc.Height) / 2 );
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
        MinHeight  = Layout.WindowMinHeight;
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
    m_leftCluster.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
    Size   s      = m_leftCluster.DesiredSize;
    double chrome = (Theme.BorderThickness + Layout.EdgeGap) * 2;
    return new Size( Math.Ceiling( s.Width  + chrome ),
                     Math.Ceiling( s.Height + chrome ) );
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
    if( (m_favX is {} fx) &&
        (m_favY is {} fy) &&
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
    m_clipIndicator.ToolTip = "Clipboard send mode: " + (on ? "ON" : "OFF");
  }

  public void UpdateStripIndicator( bool on )
  {
    m_stripIndicator.Text    = on ? "☻" : "☺";
    m_stripIndicator.ToolTip = "Strip emojis from comments: " + (on ? "ON" : "OFF");
  }

  // ── Active-window tracking ───────────────────────────────────────
  private void TrackActiveWindow()
  {
    nint h = NativeMethods.GetForegroundWindow();
    if( h == nint.Zero ||
        h == m_hwnd )
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
    Theme.ApplyRoundedCorners( m_hwnd );

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
    if( m_hwnd == nint.Zero )
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
    int minH  = (int)Math.Round( Layout.WindowMinHeight * scale );
    int maxH  = Math.Max( minH, work.Height - margin );

    mmi.ptMaxPosition  = new NativeMethods.POINT { X = work.Left, Y = work.Top };
    mmi.ptMaxSize      = new NativeMethods.POINT { X = width,     Y = maxH };
    mmi.ptMinTrackSize = new NativeMethods.POINT { X = width,     Y = minH };
    mmi.ptMaxTrackSize = new NativeMethods.POINT { X = width,     Y = maxH };

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
    NativeMethods.SetWindowPos( m_hwnd, IntPtr.Zero,
                                left, top, 0, 0,
                                NativeMethods.SWP_NOSIZE   |
                                NativeMethods.SWP_NOZORDER |
                                NativeMethods.SWP_NOACTIVATE );
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

    if( m_favX is {} favX &&
        m_favY is {} favY )
    {
      if( m_snappedToFav )
      {
        if( (Math.Abs( left - favX ) >= releaseThreshold) ||
            (Math.Abs( top  - favY ) >= releaseThreshold) )
        {
          m_snappedToFav = false;
        }
        else
        {
          return (favX, favY);
        }
      }
      else if( (Math.Abs( left - favX ) <= snapThreshold) &&
               (Math.Abs( top  - favY ) <= snapThreshold) )
      {
        m_snappedToFav = true;
        m_snappedToTop = false;
        return (favX, favY);
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
        return (left, 0); // stay snapped to the top edge, allow horizontal movement
      }
    }
    else if( top <= snapThreshold )
    {
      m_snappedToTop = true;
      return (left, 0);
    }

    return (left, top);
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
      if( Math.Abs( cur.X - start.X ) >= 4 ||
          Math.Abs( cur.Y - start.Y ) >= 4 )
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
  // suppress the window's app menu over the tab control (and its contents) and the
  // helper buttons — but let a button's own edit/delete menu through.
  private void OnContextMenuOpening( object sender, ContextMenuEventArgs e )
  {
    if( e.OriginalSource is not DependencyObject d )
    {
      return;
    }
    if( HasOwnContextMenu( d ) )
    {
      return; // a data-tab button's edit/delete menu — open it normally
    }
    if( IsInside( d, m_tabs ) ||
        IsInside( d, m_rightCluster ) )
    {
      e.Handled = true;
    }
  }

  // True if the right-clicked element (or an ancestor below the window) carries its
  // own ContextMenu — i.e. a per-button menu, not the window's app menu.
  private bool HasOwnContextMenu( DependencyObject? node )
  {
    while( node != null )
    {
      if( ReferenceEquals( node, this ) )
      {
        return false; // reached the window itself → only the app menu remains
      }
      if( node is FrameworkElement fe && fe.ContextMenu != null )
      {
        return true;
      }
      DependencyObject? parent = node is Visual ? VisualTreeHelper.GetParent( node ) : null;
      parent ??= LogicalTreeHelper.GetParent( node );
      node = parent;
    }
    return false;
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
