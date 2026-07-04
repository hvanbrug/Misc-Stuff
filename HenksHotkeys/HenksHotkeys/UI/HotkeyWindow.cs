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
using HenksHotkeys.Tabs;
using PInvoke;

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

  // Default expanded height (used only when there is no saved height): the tallest tab's
  // content is clamped into [min, max] DIPs, then the non-scrolling chrome is added.
  private const double DefaultViewportMin = 320; // don't make the default window shorter than this
  private const double DefaultViewportMax = 330; // ...nor taller — taller tabs scroll instead
  private const double VerticalChrome     = 64;  // toolbar strip + tab-header row + top/bottom borders

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

  // A drag in flight — either a single unselected button or the whole selection as a block.
  // Both capture the (persistent) tab control and paint their ghost + drop cue on a window-level
  // overlay, so the drag survives — and stays visible — when it crosses to another tab.
  private enum DragMode { Single, Block }
  private DragMode         m_dragMode;
  private bool             m_dragArmed;    // a press landed; a move past the threshold starts the drag
  private bool             m_dragActive;   // past the threshold — a drag is in flight
  private DataTabModel?    m_dragSrcModel; // the tab the dragged item(s) belong to
  private Canvas?          m_dragSrcCanvas;
  private Point            m_dragStartSrc; // press point, in source-canvas coordinates
  private Point            m_dragStartLayer; // press point, in overlay coordinates (block ghosts)
  private Vector           m_dragGrab;     // press − grabbed-item top-left (single ghost offset)
  private Rect             m_dragGrabRect; // grabbed item's rect (single ghost size)
  private string           m_dragLabel = "";
  private int              m_dragAnchorRow, m_dragAnchorCol; // grabbed item's cell (the move anchor)
  private Core.ButtonDef?  m_dragButton;   // (single) the button being moved (for ResolveDrop)
  private SymbolElement?   m_dragSym;       // (single) the on-screen button + its click action
  private SymbolElement?   m_lastClickSym;  // double-click tracking for the data-button send
  private long             m_lastClickTime;

  // The always-visible overlay the ghost + drop cue live on (never clipped, above everything).
  private Canvas  m_dragLayer = null!;
  private Border? m_dragGhost;  // (single) the label ghost following the cursor
  private Border? m_dragHi;     // filled "place here" cell highlight
  private Border? m_dragCaret;  // thin insertion caret (single-button same-tab insert)
  private readonly List<(Border box, Point p0)> m_blockGhosts = new(); // (block) one per selected item

  // Multi-selection (Ctrl+click any buttons/headings) and moving the whole set as a block,
  // possibly to another tab (drag over a tab header to switch, then drop).
  private readonly HashSet<Core.ButtonDef>    m_selBtns     = new();
  private readonly HashSet<Core.SectionDef>   m_selHeads    = new();
  private readonly Dictionary<object, Border> m_selOverlays = new();
  private readonly List<Canvas> m_tabCanvases = new(); // one per tab, aligned with AppState.Tabs
  private Canvas?       m_selCanvas;  // canvas the selection + its overlays live on (source tab)
  private DataTabModel? m_selModel;   // the tab the selection belongs to (source tab)
  private int     m_hoverTab = -1;                 // tab header being dwelled over (to switch to)
  private readonly DispatcherTimer m_tabDwell = new() { Interval = TimeSpan.FromMilliseconds( 450 ) };

  // Drag a tab header to reorder the tabs (#15). Separate from the button drag above.
  private int     m_tabDragFrom = -1;   // index of the header being dragged, or −1
  private Point   m_tabDragStart;       // press point, in tab-control coordinates
  private bool    m_tabDragging;
  private Border? m_tabDropCaret;       // insertion caret on the overlay, between headers

  // Drag an Emojis-tab favourite to reorder it within the Favourites section (#13). It stays on the
  // one tab, so it captures the button itself (no tab-switching) and its caret lives on the canvas.
  private SymbolElement? m_favDragSym;
  private Point          m_favDragStart;
  private bool           m_favDragging;
  private Canvas?        m_favDragCanvas;
  private Border?        m_favCaret;

  private int SelectionCount => m_selBtns.Count + m_selHeads.Count;

  private IntPtr m_hwnd;
  private readonly DispatcherTimer m_activeWinTimer = new() { Interval = TimeSpan.FromMilliseconds( 200 ) };

  private IntPtr m_wheelHook;
  private Win32.HookProc? m_wheelProc;

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

    // Settings are already loaded here (FavX/FavY above come from the same store), so apply
    // the saved collapsed state now — before the first Show() — instead of letting the
    // window flash open at full size and snap shut in ShowUi.
    if( AppState.Settings.IsCollapsed )
    {
      SetCollapsed( true, persist: false );
    }

    m_activeWinTimer.Tick += ( _, _ ) => TrackActiveWindow();
    m_activeWinTimer.Start();

    // A drag (single button or a selection) captures the (persistent) tab control up front, so
    // switching tabs mid-drag can't kill the capture. All the drag logic is driven from here.
    // Because a data button's press captures the tab control (not the button), the plain click that
    // sends its text is also delivered here (a press-release with no drag) — see OnDragUp / the
    // no-drag branch below.
    m_tabs.PreviewMouseMove += ( _, e ) => { if( m_dragArmed ) OnDragMove( e ); };
    m_tabs.PreviewMouseLeftButtonUp += ( _, e ) =>
    {
      if( m_dragActive )     { OnDragUp( e );    e.Handled = true; } // real drag → drop
      else if( m_dragArmed ) { ClickNoDrag();    e.Handled = true; } // plain press-release → send (single)
    };
    m_tabs.LostMouseCapture += ( _, _ ) => CancelDrag();
    m_tabDwell.Tick += ( _, _ ) =>
    {
      m_tabDwell.Stop();
      if( m_dragActive && m_hoverTab >= 0 && m_hoverTab != m_tabs.SelectedIndex && m_hoverTab < m_tabs.Items.Count )
      {
        m_tabs.SelectedIndex = m_hoverTab; // hovered a different tab long enough → switch to it
      }
    };

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

    // A transparent, click-through overlay spanning the whole window. The drag ghost + drop cue
    // are painted here (not on a tab's canvas) so they stay visible when a drag crosses tabs.
    m_dragLayer = new Canvas { IsHitTestVisible = false };
    Grid.SetRow( m_dragLayer, 0 );
    Grid.SetRowSpan( m_dragLayer, 2 );
    Panel.SetZIndex( m_dragLayer, 10000 );
    grid.Children.Add( m_dragLayer );

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
    m_tabCanvases.Clear();
    ClearSelection(); // the rebuild makes fresh model instances, so drop any stale selection

    foreach( TabModel model in AppState.Tabs )
    {
      (Canvas canvas, SmoothScroller scroller, object content) = BuildTabContent( model );
      m_tabCanvases.Add( canvas );   // aligned with AppState.Tabs / m_tabs.Items order
      m_scrollers.Add( scroller );

      var item = new TabItem
      {
        Header = model.Name,
        Content = content,
        Style = (Style)Application.Current.FindResource( "AppTabItem" )!,
      };
      WireTabHeader( item, model );
      m_tabs.Items.Add( item );
    }

    m_tabs.SelectedIndex = (targetTab >= 0) && (targetTab < m_tabs.Items.Count) ? targetTab : 0;
    m_suppressTabPersist = false;
  }

  // Build one tab's content (canvas of buttons/headers, its scroller, and the outer content — the
  // Emojis tab wraps in a Grid with the pinned tone picker). Kept separate from the aligned
  // m_tabCanvases/m_scrollers/m_tabs.Items bookkeeping so a single tab can be rebuilt (see
  // RebuildEmojiTab) without touching the others.
  private (Canvas canvas, SmoothScroller scroller, object content) BuildTabContent( TabModel model )
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
    // Headings render *under* the buttons: add them first so a button that overlaps a
    // heading still receives the mouse (stays draggable) and paints on top of it.
    var headerEls = new List<FrameworkElement>();
    foreach( TabModel.SectionHeader hdr in model.Headers )
    {
      FrameworkElement label = BuildSectionHeader( hdr, model );
      Canvas.SetLeft( label, hdr.X );
      Canvas.SetTop( label, hdr.Y );
      canvas.Children.Add( label );
      headerEls.Add( label );

      // Headings are selectable (Ctrl+click) and move with the block when dragged.
      if( model is DataTabModel headModel && hdr.Source is { } sdef )
      {
        WireHeadingSelect( label, sdef, new Rect( hdr.X, hdr.Y, hdr.Width, hdr.Height ), canvas, headModel );
      }
    }
    if( model is EmojisTab ) m_emojiHeaderEls = headerEls; // tracked so a reconcile can refresh them

    foreach( SymbolElement sym in model.Symbols )
    {
      FrameworkElement btn = BuildButton( sym, model, canvas );
      Canvas.SetLeft( btn, sym.X );
      Canvas.SetTop( btn, sym.Y );
      canvas.Children.Add( btn );

      // Data-tab buttons can be dragged to a new cell.
      if( model is DataTabModel dragModel && sym.Source is not null )
      {
        WireDrag( btn, sym, canvas, dragModel );
      }
      else if( model is EmojisTab emo )
      {
        WireEmojiButton( btn, sym, canvas, emo ); // favourites: mark / unmark + reorder drag
      }
    }

    var sv = new ScrollViewer
    {
      VerticalScrollBarVisibility   = ScrollBarVisibility.Auto,
      HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
      Background = Theme.WindowBackground,
      Focusable  = false,
      Content    = canvas,
    };
    var scroller = new SmoothScroller( sv );

    if( model is DataTabModel dataModel )
    {
      // Every empty cell is a drop target: hovering one shows its outline (over the whole
      // tab width — including columns past the content — via the ScrollViewer). Right-click
      // the open area adds a button/heading; buttons keep their own Edit/Delete menu.
      AttachEmptyCellHover( sv, canvas, dataModel );
      AttachAddHereMenu( sv, canvas, dataModel );

      // A plain click on the empty area clears the selection (Ctrl+click keeps building it).
      canvas.MouseLeftButtonDown += ( _, _ ) =>
      {
        if( ( Keyboard.Modifiers & ModifierKeys.Control ) == 0 && SelectionCount > 0 ) ClearSelection();
      };
    }

    // The Emojis tab gets a pinned skin-tone picker in its top-right corner (#27).
    object content = sv;
    if( model is EmojisTab tonedModel )
    {
      var stack = new Grid();
      stack.Children.Add( sv );
      stack.Children.Add( BuildTonePicker( tonedModel ) );
      content = stack;
    }
    return ( canvas, scroller, content );
  }

  /// <summary>Apply the current skin tone to the Emojis tab **in place** (#27) — swap the image and
  /// sent char on the existing buttons, without rebuilding the tab. Rebuilding re-measures all
  /// ~1,200 emoji buttons (~2 s); this only re-decodes the toneable ones and reuses everything else,
  /// so a tone change is near-instant. The click reads the symbol's live Char, so no re-wiring.</summary>
  public void RetintEmojiInPlace()
  {
    if( AppState.Tabs.FirstOrDefault( t => t is EmojisTab ) is not EmojisTab model )
    {
      return;
    }
    string tone = AppState.SkinTone;
    int    px   = (int)Math.Round( model.SymBtnSizeX * Layout.SymbolScale );
    foreach( SymbolElement sym in model.Symbols )
    {
      if( sym.IsFavourite )
      {
        continue; // favourites keep whatever the user stored
      }
      string want = EmojiSkin.Apply( sym.BaseChar, tone );
      if( want == sym.Char )
      {
        continue; // not toneable, or already this tone
      }
      sym.Char = want; // the dynamic click action sends this
      if( sym.Ctrl is Button btn && btn.Content is Image img && EmojiImageProvider.Get( want, px ).Image is { } src )
      {
        img.Source = src;
      }
    }
    RefreshTonePicker();
  }

  private List<FrameworkElement> m_emojiHeaderEls = new(); // the Emojis tab's section-header elements

  /// <summary>Apply an Emojis-tab change (favourite add/remove/reorder, section collapse/expand) by
  /// **reconciling** the existing buttons against a freshly-computed layout, instead of rebuilding
  /// the whole tab. Rebuilding re-measures all ~1,200 templated buttons (~2 s); here the existing
  /// buttons are kept on the same canvas and merely repositioned (arrange is cheap — only *measure*
  /// is slow), and only the handful that appeared/disappeared are created/removed. So these actions
  /// go from ~3 s to a fraction of that.</summary>
  public void ReconcileEmojiTab()
  {
    int idx = AppState.Tabs.FindIndex( t => t is EmojisTab );
    if( idx < 0 || AppState.Tabs[idx] is not EmojisTab model || idx >= m_tabCanvases.Count )
    {
      return;
    }
    Canvas canvas = m_tabCanvases[idx];
    var    target = new EmojisTab { Backing = model.Backing }; // the desired layout (fresh state)
    int    px     = (int)Math.Round( model.SymBtnSizeX * Layout.SymbolScale );

    // Index the existing buttons by identity so we can reuse them.
    var pool = new Dictionary<(bool Fav, string Base), SymbolElement>();
    foreach( SymbolElement s in model.Symbols ) pool.TryAdd( ( s.IsFavourite, s.BaseChar ), s );

    var kept = new List<SymbolElement>( target.Symbols.Count );
    var used = new HashSet<(bool, string)>();
    foreach( SymbolElement t in target.Symbols )
    {
      var key = ( t.IsFavourite, t.BaseChar );
      if( used.Add( key ) && pool.TryGetValue( key, out SymbolElement? old ) && old.Ctrl is FrameworkElement el )
      {
        // Reuse: move the existing button; refresh its image only if the emoji actually changed.
        old.X = t.X; old.Y = t.Y; old.Line = t.Line; old.Slot = t.Slot;
        Canvas.SetLeft( el, t.X );
        Canvas.SetTop(  el, t.Y );
        if( old.Char != t.Char )
        {
          old.Char = t.Char;
          if( el is Button b && b.Content is Image im && EmojiImageProvider.Get( t.Char, px ).Image is { } src ) im.Source = src;
        }
        kept.Add( old );
      }
      else
      {
        // New emoji (e.g. a just-added favourite, or an expanded section): build a fresh button.
        FrameworkElement btn = BuildButton( t, model, canvas );
        Canvas.SetLeft( btn, t.X );
        Canvas.SetTop(  btn, t.Y );
        canvas.Children.Add( btn );
        WireEmojiButton( btn, t, canvas, model );
        kept.Add( t );
      }
    }

    // Remove buttons whose emoji is gone (unfavourited, or a collapsed section).
    foreach( SymbolElement s in model.Symbols )
    {
      if( !used.Contains( ( s.IsFavourite, s.BaseChar ) ) && s.Ctrl is UIElement el ) canvas.Children.Remove( el );
    }
    model.Symbols.Clear();
    model.Symbols.AddRange( kept );

    // Headers are few — just replace them (handles the triangle flip and Favourites appearing/going).
    foreach( FrameworkElement h in m_emojiHeaderEls ) canvas.Children.Remove( h );
    m_emojiHeaderEls = new List<FrameworkElement>( target.Headers.Count );
    foreach( TabModel.SectionHeader hdr in target.Headers )
    {
      FrameworkElement label = BuildSectionHeader( hdr, model );
      Canvas.SetLeft( label, hdr.X );
      Canvas.SetTop(  label, hdr.Y );
      canvas.Children.Add( label );
      m_emojiHeaderEls.Add( label );
    }
    model.Headers.Clear();
    model.Headers.AddRange( target.Headers );

    canvas.Height = target.ContentHeight; // keep the scroll extent correct
    canvas.Width  = target.ContentWidth;
  }

  // The tab strip is editable (#11) and reorderable (#15): every header carries a right-click menu
  // (add / edit / move / delete) and can be dragged to a new position. Both act on the backing
  // TabEntry via TabStore, then reload. The tab's index is read live from m_tabs.Items, so it stays
  // correct as the strip changes.
  private void WireTabHeader( TabItem item, TabModel model )
  {
    item.ContextMenu = BuildTabMenu( item, model );

    item.PreviewMouseLeftButtonDown += ( _, e ) =>
    {
      // A TabItem hosts its whole tab body, so this fires for presses in the content too. Only a
      // press on the header itself should arm a reorder — ignore presses inside the content.
      if( IsWithin( e.OriginalSource, item.Content as DependencyObject ) ) return;
      m_tabDragFrom  = m_tabs.Items.IndexOf( item );
      m_tabDragStart = e.GetPosition( m_tabs );
      m_tabDragging  = false;
      // Not handled: let the tab control select this tab on press as usual.
    };

    item.PreviewMouseMove += ( _, e ) =>
    {
      if( m_tabDragFrom < 0 || e.LeftButton != MouseButtonState.Pressed ) return;
      Point p = e.GetPosition( m_tabs );
      if( !m_tabDragging )
      {
        if( !PastDragThreshold( p, m_tabDragStart ) ) return;
        m_tabDragging = true;
        item.CaptureMouse();
        BuildTabDropCaret();
      }
      UpdateTabDropCaret( p );
      e.Handled = true;
    };

    item.PreviewMouseLeftButtonUp += ( _, e ) =>
    {
      if( !m_tabDragging )
      {
        m_tabDragFrom = -1;   // a plain click — selection already handled on press
        return;
      }
      int before = TabInsertIndexAt( e.GetPosition( m_tabs ) );
      EndTabDrag();
      if( before >= 0 && model.Backing is { } entry ) TabCommands.Move( entry, before );
      e.Handled = true;
    };

    item.LostMouseCapture += ( _, _ ) => { if( m_tabDragging ) EndTabDrag(); };
  }

  private ContextMenu BuildTabMenu( TabItem item, TabModel model )
  {
    var menu = new ContextMenu();

    var add = new MenuItem { Header = "Add tab…" };
    add.Click += ( _, _ ) => TabCommands.Add( m_tabs.Items.IndexOf( item ) + 1 ); // after this one
    menu.Items.Add( add );

    if( model.Backing is { Builtin: null or "" } )
    {
      var edit = new MenuItem { Header = "Edit tab…" };
      edit.Click += ( _, _ ) => { if( model.Backing is { } t ) TabCommands.Edit( t ); };
      menu.Items.Add( edit );
    }

    menu.Items.Add( new Separator() );
    var left  = new MenuItem { Header = "Move left"  };
    var right = new MenuItem { Header = "Move right" };
    left.Click  += ( _, _ ) => { int i = m_tabs.Items.IndexOf( item ); if( model.Backing is { } t && i > 0 ) TabCommands.Move( t, i - 1 ); };
    right.Click += ( _, _ ) => { int i = m_tabs.Items.IndexOf( item ); if( model.Backing is { } t && i < m_tabs.Items.Count - 1 ) TabCommands.Move( t, i + 2 ); };
    menu.Items.Add( left );
    menu.Items.Add( right );

    menu.Items.Add( new Separator() );
    var del = new MenuItem { Header = "Delete tab" };
    del.Click += ( _, _ ) => { if( model.Backing is { } t ) TabCommands.Delete( t ); };
    menu.Items.Add( del );

    menu.Opened += ( _, _ ) =>
    {
      int i = m_tabs.Items.IndexOf( item );
      left.IsEnabled  = i > 0;
      right.IsEnabled = i >= 0 && i < m_tabs.Items.Count - 1;
    };
    return menu;
  }

  private void BuildTabDropCaret()
  {
    m_tabDropCaret = new Border
    {
      Width            = 3,
      Background       = Palette.Brush( "SwitchOn" ),
      CornerRadius     = new CornerRadius( 1.5 ),
      IsHitTestVisible = false,
    };
    Panel.SetZIndex( m_tabDropCaret, 3 );
    m_dragLayer.Children.Add( m_tabDropCaret );
  }

  // Place the reorder caret at the insertion boundary nearest the pointer and return the index the
  // dragged tab would land before (−1 = not over a header).
  private int TabInsertIndexAt( Point onTabs )
  {
    int t = TabUnderCursor( onTabs );
    if( t < 0 || t >= m_tabs.Items.Count || m_tabs.Items[t] is not TabItem ti )
    {
      return -1;
    }
    GeneralTransform toLayer;
    try   { toLayer = ti.TransformToVisual( m_dragLayer ); }
    catch { return -1; }

    bool  after = ti.TransformToVisual( m_tabs ).Transform( new Point( 0, 0 ) ).X + ti.ActualWidth / 2 < onTabs.X;
    Point edge  = toLayer.Transform( new Point( after ? ti.ActualWidth : 0, 0 ) );
    if( m_tabDropCaret is { } caret )
    {
      caret.Height = ti.ActualHeight;
      Canvas.SetLeft( caret, edge.X - 1 );
      Canvas.SetTop(  caret, edge.Y );
    }
    return after ? t + 1 : t;
  }

  private void UpdateTabDropCaret( Point onTabs ) => TabInsertIndexAt( onTabs );

  // True when <paramref name="src"/> (a mouse event's OriginalSource) sits inside <paramref
  // name="container"/> in the visual tree — used to tell a header press from a body press.
  private static bool IsWithin( object? src, DependencyObject? container )
  {
    if( container is null ) return false;
    DependencyObject? d = src as DependencyObject;
    while( d is not null )
    {
      if( ReferenceEquals( d, container ) ) return true;
      // GetParent throws on a non-Visual (e.g. a text Run); fall back to the logical tree there.
      d = d is Visual or System.Windows.Media.Media3D.Visual3D
            ? VisualTreeHelper.GetParent( d )
            : LogicalTreeHelper.GetParent( d );
    }
    return false;
  }

  private void EndTabDrag()
  {
    m_tabDragging = false;
    m_tabDragFrom = -1;
    if( Mouse.Captured is TabItem ti ) ti.ReleaseMouseCapture();
    if( m_tabDropCaret is { } c ) m_dragLayer.Children.Remove( c );
    m_tabDropCaret = null;
  }

  // ── Emojis-tab favourites (#13): mark / unmark, and reorder by drag ─
  // The emoji catalog itself is fixed; only the user's Favourites section is editable. A plain
  // click still sends the emoji (native Click), a right-click marks/unmarks it, and — for a
  // favourite — a drag reorders it. The favourites are a left-to-right flow, so a reorder reflows
  // every row after the drop (the end emoji of a row becomes the start of the next).
  private void WireEmojiButton( FrameworkElement el, SymbolElement sym, Canvas canvas, EmojisTab model )
  {
    var menu = new ContextMenu();
    if( sym.IsFavourite )
    {
      var un = new MenuItem { Header = "Unfavourite" };
      un.Click += ( _, _ ) => FavouriteCommands.Remove( sym.Char );
      menu.Items.Add( un );
    }
    else
    {
      var fav = new MenuItem { Header = "Mark as favourite" };
      fav.Click += ( _, _ ) => FavouriteCommands.Add( sym.Char );
      menu.Items.Add( fav );
    }
    el.ContextMenu = menu;

    if( !sym.IsFavourite )
    {
      return; // only favourites can be dragged
    }

    el.PreviewMouseLeftButtonDown += ( _, e ) =>
    {
      m_favDragSym    = sym;
      m_favDragStart  = e.GetPosition( canvas );
      m_favDragging   = false;
      m_favDragCanvas = canvas;
    };

    el.PreviewMouseMove += ( _, e ) =>
    {
      if( m_favDragSym != sym || e.LeftButton != MouseButtonState.Pressed ) return;
      Point p = e.GetPosition( canvas );
      if( !m_favDragging )
      {
        if( !PastDragThreshold( p, m_favDragStart ) ) return; // still a click — let the send proceed
        m_favDragging = true;
        el.CaptureMouse();
        m_activeWinTimer.Stop();
        el.Opacity = 0.35; // dim the one in flight
        BuildFavCaret( canvas );
      }
      UpdateFavCaret( model, p );
      e.Handled = true;
    };

    el.PreviewMouseLeftButtonUp += ( _, e ) =>
    {
      if( !m_favDragging || m_favDragSym != sym ) return;
      int idx = FavouriteInsertIndex( model, e.GetPosition( canvas ) );
      EndFavDrag( el );
      FavouriteCommands.Reorder( sym.Char, idx ); // reflows the section, then reloads
      e.Handled = true; // swallow the click so the drop doesn't also send
    };

    el.LostMouseCapture += ( _, _ ) => { if( m_favDragging ) EndFavDrag( el ); };
  }

  private void BuildFavCaret( Canvas canvas )
  {
    m_favCaret = new Border
    {
      Width            = 3,
      Background       = Palette.Brush( "SwitchOn" ),
      CornerRadius     = new CornerRadius( 1.5 ),
      IsHitTestVisible = false,
    };
    Panel.SetZIndex( m_favCaret, 1000 );
    canvas.Children.Add( m_favCaret );
  }

  // The favourites index the drop would insert before (0..count), from the pointer's row/column in
  // the flow. Clamped to the favourites area so a favourite can't be dragged out of the section.
  private static int FavouriteInsertIndex( EmojisTab model, Point p )
  {
    IReadOnlyList<SymbolElement> favs = model.Favourites;
    if( favs.Count == 0 ) return 0;

    int cols    = Math.Max( 1, model.FavouriteColumns );
    int firstY  = favs[0].Y;
    int col     = Math.Clamp( (int)Math.Floor( ( p.X - model.SymOrgX ) / (double)model.ColWidth ), 0, cols - 1 );
    int lastRow = ( favs.Count - 1 ) / cols;
    int row     = Math.Clamp( (int)Math.Floor( ( p.Y - firstY ) / (double)model.RowHeight ), 0, lastRow );
    return Math.Clamp( row * cols + col, 0, favs.Count );
  }

  private void UpdateFavCaret( EmojisTab model, Point p )
  {
    if( m_favCaret is not { } caret ) return;
    IReadOnlyList<SymbolElement> favs = model.Favourites;
    if( favs.Count == 0 ) return;

    int    idx = FavouriteInsertIndex( model, p );
    double x, y;
    if( idx < favs.Count ) { x = favs[idx].X;                  y = favs[idx].Y; } // before that favourite
    else                   { x = favs[^1].X + favs[^1].W;      y = favs[^1].Y; } // append → right of the last
    caret.Height = model.SymBtnSizeY;
    Canvas.SetLeft( caret, x - 1.5 );
    Canvas.SetTop(  caret, y );
    caret.Visibility = Visibility.Visible;
  }

  private void EndFavDrag( FrameworkElement el )
  {
    m_favDragging = false;
    m_favDragSym  = null;
    m_activeWinTimer.Start();
    el.Opacity = 1.0;
    if( ReferenceEquals( Mouse.Captured, el ) ) el.ReleaseMouseCapture();
    if( m_favCaret is { } c && m_favDragCanvas is { } cv ) cv.Children.Remove( c );
    m_favCaret = null;
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

  private FrameworkElement BuildButton( SymbolElement sym, TabModel model, Canvas canvas )
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
    if( sym.Align == "left" )   buttonText = " "  + buttonText; // small inset off the edge
    if( sym.Align == "right" )  buttonText = buttonText + " ";

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
      TextAlignment ta = sym.Align switch { "left"  => TextAlignment.Left,
                                            "right" => TextAlignment.Right,
                                            _       => TextAlignment.Center };
      btn.Content = new TextBlock
      {
        Text              = buttonText,
        TextAlignment     = ta,
        TextTrimming      = ta == TextAlignment.Center ? TextTrimming.None : TextTrimming.CharacterEllipsis,
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

    // A built-in code-tab button (Source == null) sends via its native Click. A data-tab button's
    // press instead captures the tab control (so it can drag / cross tabs), which suppresses that
    // Click — so its send is reproduced in ClickNoDrag when the press ends without a drag.
    if( sym.Source is not {} def )
    {
      WireSymbolButton( btn, sym.ClickAction );
    }
    else
    {
      // Data-tab buttons get a right-click menu to edit / delete them, plus the same add/insert
      // actions as the open area.
      btn.ContextMenu = BuildButtonMenu( def, model as DataTabModel, canvas, btn );
    }

    sym.Ctrl = btn;
    return btn;
  }

  private ContextMenu BuildButtonMenu( ButtonDef def, DataTabModel? model, Canvas canvas, FrameworkElement btn )
  {
    var menu = new ContextMenu();
    var edit = new MenuItem { Header = "Edit button…" };
    var del  = new MenuItem { Header = "Delete button" };
    edit.Click += ( _, _ ) => ButtonCommands.Edit(   def );
    del.Click  += ( _, _ ) => ButtonCommands.Delete( def );
    menu.Items.Add( edit );
    menu.Items.Add( del );

    // The same add/insert actions the open-area menu offers, anchored at the cell you
    // right-clicked — so you can insert without hunting for empty space.
    if( model is not null )
    {
      Point clickPoint = default;
      btn.PreviewMouseRightButtonDown += ( _, e ) => clickPoint = e.GetPosition( canvas );
      menu.Items.Add( new Separator() );
      AddInsertItems( menu, model, () => clickPoint );
      AddSelectionItems( menu, model );
    }
    return menu;
  }

  // The shared "add / insert" menu items (used by both the open-area menu and each button's
  // menu). <paramref name="at"/> supplies the right-click position in canvas coordinates.
  private static void AddInsertItems( ContextMenu menu, DataTabModel model, Func<Point> at )
  {
    var addB = new MenuItem { Header = "Add button here…" };
    addB.Click += ( _, _ ) => ButtonCommands.AddHere( model, at() );
    var addH = new MenuItem { Header = "Add heading here…" };
    addH.Click += ( _, _ ) => HeadingCommands.AddHere( model, at() );
    var insBlank = new MenuItem { Header = "Insert blank row" };
    insBlank.Click += ( _, _ ) => HeadingCommands.InsertBlankRow( model, at() );
    var insHead  = new MenuItem { Header = "Insert heading row…" };
    insHead.Click += ( _, _ ) => HeadingCommands.InsertHeadingRow( model, at() );
    menu.Items.Add( addB );
    menu.Items.Add( addH );
    menu.Items.Add( insBlank );
    menu.Items.Add( insHead );
  }

  // Selection actions (Delete / Clear) added to every data-tab menu; they only show when a
  // selection exists, and the count is refreshed each time the menu opens.
  private void AddSelectionItems( ContextMenu menu, DataTabModel model )
  {
    var sep   = new Separator();
    var del   = new MenuItem();
    var clear = new MenuItem { Header = "Clear selection" };
    del.Click += ( _, _ ) =>
    {
      // The selection belongs to its own tab (m_selModel), which may differ from this menu's.
      if( m_selModel is { } sm && TabStore.DeleteSelection( sm.Entry, m_selBtns.ToList(), m_selHeads.ToList() ) )
      {
        AppState.RequestReload?.Invoke();
      }
    };
    clear.Click += ( _, _ ) => ClearSelection();
    menu.Items.Add( sep );
    menu.Items.Add( del );
    menu.Items.Add( clear );

    menu.Opened += ( _, _ ) =>
    {
      int n = SelectionCount;
      Visibility v = n > 0 ? Visibility.Visible : Visibility.Collapsed;
      sep.Visibility = del.Visibility = clear.Visibility = v;
      del.Header = $"Delete selected ({n})";
    };
  }

  // ── Drag a button to a new slot (or another tab) ──────────────────
  // A data-tab button's press arms a drag. An *unselected* button drags on its own (Single); a
  // *selected* one drags the whole selection (Block). Either way the actual dragging is driven
  // from the tab control's captured events (see the ctor) so it survives a tab switch — and the
  // ghost + drop cue live on the window overlay, so they stay visible on whichever tab shows.
  private void WireDrag( FrameworkElement el, SymbolElement sym, Canvas canvas, DataTabModel model )
  {
    el.PreviewMouseLeftButtonDown += ( _, e ) =>
    {
      if( sym.Source is not Core.ButtonDef def ) return;
      var rect = new Rect( sym.X, sym.Y, sym.W, sym.H );

      if( ( Keyboard.Modifiers & ModifierKeys.Control ) != 0 )
      {
        ToggleSelect( def, rect, canvas, model );
        e.Handled = true; // Ctrl+click selects; don't send / start a drag
        return;
      }

      // Either way, capture the (persistent) tab control up front — before the button can capture
      // itself — so a mid-drag tab switch can't drop the capture. Handling the press suppresses the
      // button's own click; the send is reproduced in ClickNoDrag when the press ends without a drag.
      Point    p    = e.GetPosition( canvas );
      DragMode mode = m_selBtns.Contains( def ) ? DragMode.Block : DragMode.Single;
      ArmDrag( mode, model, canvas, p, rect, GhostLabel( sym ), def.Row, def.Col );
      m_dragSym    = sym;
      m_dragButton = mode == DragMode.Single ? def : null; // only a single drag uses ResolveDrop
      if( sym.Ctrl is UIElement pressed ) pressed.Opacity = 0.7; // press feedback (native pressed state is suppressed)
      Mouse.Capture( m_tabs );
      e.Handled = true;
    };
  }

  private static bool PastDragThreshold( Point p, Point start )
    => Math.Abs( p.X - start.X ) >= SystemParameters.MinimumHorizontalDragDistance ||
       Math.Abs( p.Y - start.Y ) >= SystemParameters.MinimumVerticalDragDistance;

  private static SolidColorBrush Translucent( Color c, double opacity )
  {
    var b = new SolidColorBrush( c ) { Opacity = opacity };
    b.Freeze();
    return b;
  }

  private static string GhostLabel( SymbolElement sym )
    => UiText.NormalizeDisplayText( sym.ShowChar ? sym.Char : sym.Desc );

  // Record the pending drag. Capture (and the threshold) are handled by the caller / OnDragMove.
  private void ArmDrag( DragMode mode, DataTabModel model, Canvas canvas, Point press,
                        Rect grab, string label, int anchorRow, int anchorCol )
  {
    m_dragMode      = mode;
    m_dragSrcModel  = model;
    m_dragSrcCanvas = canvas;
    m_dragStartSrc  = press;
    m_dragGrabRect  = grab;
    m_dragGrab      = press - grab.TopLeft;
    m_dragLabel     = label;
    m_dragAnchorRow = anchorRow;
    m_dragAnchorCol = anchorCol;
    m_dragArmed     = true;
    m_dragActive    = false;
    m_dragButton    = null;
    m_dragSym       = null;
    m_selModel      = model;   // the block drop / delete act on this tab
    m_selCanvas     = canvas;
  }

  // Driven by the captured tab control. Detect the threshold, then track the ghost(s) / drop cue
  // and dwell over tab headers to switch tabs.
  private void OnDragMove( MouseEventArgs e )
  {
    if( !m_dragActive )
    {
      if( m_dragSrcCanvas is not {} src ) return;
      if( !PastDragThreshold( e.GetPosition( src ), m_dragStartSrc ) ) return;
      BeginDrag();
    }
    UpdateDrag( e );
    e.Handled = true;
  }

  private void BeginDrag()
  {
    Mouse.Capture( m_tabs ); // already captured on press; re-assert defensively

    m_dragActive = true;
    m_activeWinTimer.Stop(); // don't let background ticks disturb the drag

    if( m_dragMode == DragMode.Single && m_dragSym?.Ctrl is UIElement dragged )
    {
      dragged.Opacity = 0.35; // dim the original while its ghost is in flight
    }

    BuildDragVisuals();
  }

  private void BuildDragVisuals()
  {
    if( m_dragSrcCanvas is {} src )
    {
      m_dragStartLayer = src.TransformToVisual( m_dragLayer ).Transform( m_dragStartSrc );
    }

    if( m_dragMode == DragMode.Single )
    {
      // One label ghost following the cursor.
      m_dragGhost = new Border
      {
        Width            = Math.Max( m_dragGrabRect.Width, Layout.SectionHeaderHeight ),
        Height           = m_dragGrabRect.Height,
        CornerRadius     = new CornerRadius( Layout.ButtonCornerRadius ),
        Background       = Palette.Brush( "ControlHover" ),
        BorderBrush      = Palette.Brush( "TextSecondary" ),
        BorderThickness  = new Thickness( 1 ),
        Opacity          = 0.85,
        IsHitTestVisible = false,
        Child = new TextBlock
        {
          Text                = m_dragLabel,
          Foreground          = Palette.Brush( "TextPrimary" ),
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment   = VerticalAlignment.Center,
          TextTrimming        = TextTrimming.CharacterEllipsis,
        },
      };
      Panel.SetZIndex( m_dragGhost, 2 );
      m_dragLayer.Children.Add( m_dragGhost );
    }
    else
    {
      // A block: a sliding ghost for *every* selected item, so the whole group is visible as it
      // moves (and it stays visible when the drag crosses tabs). Hide the static source overlays
      // while they're in flight so the group doesn't appear doubled on its own tab.
      m_blockGhosts.Clear();
      if( m_dragSrcCanvas is {} bsrc )
      {
        GeneralTransform toLayer = bsrc.TransformToVisual( m_dragLayer );
        foreach( Border ov in m_selOverlays.Values )
        {
          ov.Visibility = Visibility.Hidden;
          Point p0  = toLayer.Transform( new Point( Canvas.GetLeft( ov ), Canvas.GetTop( ov ) ) );
          var   box = MakeSelOverlay( new Rect( 0, 0, ov.Width, ov.Height ) );
          box.RenderTransform = null; // positioned directly on the overlay, not via a transform
          Canvas.SetLeft( box, p0.X );
          Canvas.SetTop(  box, p0.Y );
          Panel.SetZIndex( box, 2 );
          m_dragLayer.Children.Add( box );
          m_blockGhosts.Add( ( box, p0 ) );
        }
      }
    }

    // Drop onto an empty cell → a filled "place here" highlight.
    m_dragHi = new Border
    {
      BorderBrush      = Palette.Brush( "SwitchOn" ),
      BorderThickness  = new Thickness( 2 ),
      CornerRadius     = new CornerRadius( Layout.ButtonCornerRadius ),
      Background       = Translucent( Palette.Colour( "SwitchOn" ), 0.22 ),
      IsHitTestVisible = false,
      Visibility       = Visibility.Collapsed,
    };
    Panel.SetZIndex( m_dragHi, 3 ); // above the ghosts so the drop cue is never hidden
    m_dragLayer.Children.Add( m_dragHi );

    // Drop between two buttons (single, same-tab) → a thin insertion caret.
    m_dragCaret = new Border
    {
      Width            = 3,
      Background       = Palette.Brush( "SwitchOn" ),
      CornerRadius     = new CornerRadius( 1.5 ),
      IsHitTestVisible = false,
      Visibility       = Visibility.Collapsed,
    };
    Panel.SetZIndex( m_dragCaret, 3 );
    m_dragLayer.Children.Add( m_dragCaret );
  }

  private void UpdateDrag( MouseEventArgs e )
  {
    // The ghost(s) follow the cursor on the always-visible overlay.
    Point onLayer = e.GetPosition( m_dragLayer );
    if( m_dragMode == DragMode.Single )
    {
      if( m_dragGhost is {} g )
      {
        Canvas.SetLeft( g, onLayer.X - m_dragGrab.X );
        Canvas.SetTop(  g, onLayer.Y - m_dragGrab.Y );
      }
    }
    else
    {
      Vector d = onLayer - m_dragStartLayer;
      foreach( (Border box, Point p0) in m_blockGhosts )
      {
        Canvas.SetLeft( box, p0.X + d.X );
        Canvas.SetTop(  box, p0.Y + d.Y );
      }
    }

    // The drop cue is drawn against whichever tab is currently showing (which may differ from
    // the source after a header dwell switched tabs).
    int           idx        = m_tabs.SelectedIndex;
    DataTabModel? dest       = idx >= 0 && idx < AppState.Tabs.Count    ? AppState.Tabs[idx] as DataTabModel : null;
    Canvas?       destCanvas = idx >= 0 && idx < m_tabCanvases.Count    ? m_tabCanvases[idx]                 : null;
    if( dest is not null && destCanvas is not null )
    {
      ShowDropCue( dest, destCanvas, e.GetPosition( destCanvas ) );
    }
    else
    {
      HideDropCue(); // over a built-in (non-data) tab — nothing to drop onto
    }

    // Dwell over a *different* tab's header to switch to it.
    int t = TabUnderCursor( e.GetPosition( m_tabs ) );
    if( t >= 0 && t != m_tabs.SelectedIndex )
    {
      if( t != m_hoverTab ) { m_hoverTab = t; m_tabDwell.Stop(); m_tabDwell.Start(); }
    }
    else
    {
      m_hoverTab = -1;
      m_tabDwell.Stop();
    }
  }

  private void HideDropCue()
  {
    if( m_dragHi    is {} h ) h.Visibility = Visibility.Collapsed;
    if( m_dragCaret is {} c ) c.Visibility = Visibility.Collapsed;
  }

  // Paint the drop cue for the current tab, transforming its cell geometry into overlay space.
  private void ShowDropCue( DataTabModel dest, Canvas destCanvas, Point onDest )
  {
    GeneralTransform toLayer;
    try { toLayer = destCanvas.TransformToVisual( m_dragLayer ); }
    catch { HideDropCue(); return; } // canvas not yet realised right after a tab switch

    // A single button dropped on its *own* tab keeps the insert semantics (caret / place). Any
    // other case (block, or crossing to another tab) simply places at the pointed-at cell.
    if( m_dragMode == DragMode.Single && ReferenceEquals( dest, m_dragSrcModel ) && m_dragButton is {} moving )
    {
      DropSpot spot = dest.ResolveDrop( onDest, moving );
      if( spot.Kind is DropKind.InsertBefore or DropKind.InsertAfter )
      {
        if( m_dragCaret is {} caret )
        {
          Point tl = toLayer.Transform( new Point( spot.CaretX, spot.CellY ) );
          caret.Height = spot.CellH;
          Canvas.SetLeft( caret, tl.X );
          Canvas.SetTop(  caret, tl.Y );
          caret.Visibility = Visibility.Visible;
        }
        if( m_dragHi is {} hi ) hi.Visibility = Visibility.Collapsed;
        return;
      }
      ShowPlaceHi( toLayer, spot.CellX, spot.CellY, spot.CellW, spot.CellH );
      return;
    }

    int    row   = dest.RowAt( onDest.Y );
    int    col   = dest.ColAt( onDest.X );
    double cellX = dest.SymOrgX + col * dest.ColWidth;
    double cellY = dest.RowTop( row );
    ShowPlaceHi( toLayer, cellX, cellY, dest.SymBtnSizeX, dest.SymBtnSizeY );
  }

  private void ShowPlaceHi( GeneralTransform toLayer, double x, double y, double w, double h )
  {
    if( m_dragHi is not {} hi ) return;
    Point tl = toLayer.Transform( new Point( x, y ) );
    hi.Width  = w;
    hi.Height = h;
    Canvas.SetLeft( hi, tl.X );
    Canvas.SetTop(  hi, tl.Y );
    hi.Visibility = Visibility.Visible;
    if( m_dragCaret is {} c ) c.Visibility = Visibility.Collapsed;
  }

  // Drop: single-button same-tab → insert (shift the row); anything else → place at the pointed-at
  // cell, anchored so the grabbed item lands there and the rest keep their relative layout. Both
  // the same-tab block move and every cross-tab move are collision-checked (no inserting).
  private void OnDragUp( MouseEventArgs e )
  {
    DragMode        mode    = m_dragMode;
    DataTabModel?   src     = m_dragSrcModel;
    Core.ButtonDef? single  = m_dragButton;
    int             anchorR = m_dragAnchorRow, anchorC = m_dragAnchorCol;
    int             idx     = m_tabs.SelectedIndex;
    DataTabModel?   dest    = idx >= 0 && idx < AppState.Tabs.Count ? AppState.Tabs[idx] as DataTabModel : null;
    Canvas?         canvas  = idx >= 0 && idx < m_tabCanvases.Count ? m_tabCanvases[idx]                 : null;
    Point           onDest  = canvas is not null ? e.GetPosition( canvas ) : default;
    var             btns    = m_selBtns.ToList();
    var             heads   = m_selHeads.ToList();

    CancelDrag(); // release capture, remove the overlay visuals, restore opacity

    if( src is null || dest is null || canvas is null )
    {
      return; // dropped over a built-in (non-data) tab, or lost state
    }

    bool ok;
    if( mode == DragMode.Single )
    {
      if( single is null ) return;
      if( ReferenceEquals( dest, src ) )
      {
        DropSpot spot = dest.ResolveDrop( onDest, single );
        ok = TabStore.MoveButtonToCell( single, spot.Row, spot.Col );
      }
      else
      {
        int dRow = dest.RowAt( onDest.Y ) - anchorR;
        int dCol = dest.ColAt( onDest.X ) - anchorC;
        ok = TabStore.MoveSelectionToTab( src.Entry, dest.Entry,
                                          new[] { single }, System.Array.Empty<Core.SectionDef>(), dRow, dCol );
      }
    }
    else
    {
      int dRow = dest.RowAt( onDest.Y ) - anchorR;
      int dCol = dest.ColAt( onDest.X ) - anchorC;
      ok = ReferenceEquals( dest, src )
        ? TabStore.MoveSelection( src.Entry, btns, heads, dRow, dCol )
        : TabStore.MoveSelectionToTab( src.Entry, dest.Entry, btns, heads, dRow, dCol );
    }

    if( ok )
    {
      AppState.RequestReload?.Invoke(); // rebuild at the new layout (clears the selection)
    }
    // otherwise a collision / no-op: nothing moved, the selection is kept.
  }

  private void CancelDrag()
  {
    if( !m_dragArmed )
    {
      return; // not armed — nothing to unwind (avoids re-entrancy on a stray capture loss)
    }
    bool wasActive = m_dragActive;
    m_dragArmed  = false;
    m_dragActive = false;
    m_hoverTab   = -1;
    m_tabDwell.Stop();
    if( wasActive ) m_activeWinTimer.Start();

    if( ReferenceEquals( Mouse.Captured, m_tabs ) ) m_tabs.ReleaseMouseCapture();

    if( m_dragSym?.Ctrl is UIElement dragged ) dragged.Opacity = 1.0;

    if( m_dragGhost is {} g ) m_dragLayer.Children.Remove( g );
    if( m_dragHi    is {} h ) m_dragLayer.Children.Remove( h );
    if( m_dragCaret is {} c ) m_dragLayer.Children.Remove( c );
    foreach( (Border box, Point _) in m_blockGhosts ) m_dragLayer.Children.Remove( box );
    m_blockGhosts.Clear();
    foreach( Border ov in m_selOverlays.Values ) ov.Visibility = Visibility.Visible; // un-hide the source overlays
    m_dragGhost = null;
    m_dragHi    = null;
    m_dragCaret = null;
  }

  // A press on a data button that ended without a drag: reproduce the click send the button's own
  // Click would have raised (suppressed because the press captured the tab control). A selected
  // button / heading doesn't send. Mirrors the double-click → Enter behaviour of WireSymbolButton.
  private void ClickNoDrag()
  {
    DragMode       mode = m_dragMode;
    SymbolElement? sym  = m_dragSym;
    CancelDrag();

    if( mode != DragMode.Single || sym is null ) return; // a selected item: no send

    long now  = Environment.TickCount64;
    bool pair = ReferenceEquals( m_lastClickSym, sym ) && now - m_lastClickTime <= DoubleClickMs;
    m_lastClickTime = now;
    m_lastClickSym  = pair ? null : sym;
    if( pair )
    {
      _ = TextSender.SendInputKeys( "{Enter}" ); // rapid second click → Enter
      return;
    }
    try { sym.ClickAction(); } catch { /* never let a send failure kill the UI */ }
  }

  // ── Multi-selection (Ctrl+click) + block move (incl. to another tab) ─
  // Toggle an item (button/heading) in the selection, showing/removing an accent overlay. A
  // selection lives on one tab; Ctrl+clicking on a different tab starts a fresh one.
  private void ToggleSelect( object item, Rect rect, Canvas canvas, DataTabModel model )
  {
    if( SelectionCount > 0 && !ReferenceEquals( canvas, m_selCanvas ) ) ClearSelection();
    m_selCanvas = canvas;
    m_selModel  = model;
    bool add = item switch
    {
      Core.ButtonDef b  => !m_selBtns.Remove( b )  && m_selBtns.Add( b ),
      Core.SectionDef s => !m_selHeads.Remove( s ) && m_selHeads.Add( s ),
      _                 => false,
    };
    if( add )
    {
      Border ov = MakeSelOverlay( rect );
      Panel.SetZIndex( ov, 900 ); // above buttons, below the drag ghost
      canvas.Children.Add( ov );
      m_selOverlays[item] = ov;
    }
    else if( m_selOverlays.Remove( item, out Border? ov ) )
    {
      canvas.Children.Remove( ov );
    }
  }

  private void ClearSelection()
  {
    if( m_selCanvas is { } canvas )
    {
      foreach( Border ov in m_selOverlays.Values ) canvas.Children.Remove( ov );
    }
    m_selOverlays.Clear();
    m_selBtns.Clear();
    m_selHeads.Clear();
    m_selModel  = null;
    m_selCanvas = null;
  }

  private static Border MakeSelOverlay( Rect r )
  {
    var ov = new Border
    {
      Width            = r.Width,
      Height           = r.Height,
      BorderBrush      = Palette.Brush( "SwitchOn" ),
      BorderThickness  = new Thickness( 2 ),
      Background       = Translucent( Palette.Colour( "SwitchOn" ), 0.18 ),
      CornerRadius     = new CornerRadius( Layout.ButtonCornerRadius ),
      IsHitTestVisible = false,
      RenderTransform  = new TranslateTransform(),
    };
    Canvas.SetLeft( ov, r.X );
    Canvas.SetTop(  ov, r.Y );
    return ov;
  }

  // A selectable heading (Ctrl+click to toggle; drag it, when selected, to move the block).
  private void WireHeadingSelect( FrameworkElement el, Core.SectionDef def, Rect rect, Canvas canvas, DataTabModel model )
  {
    el.PreviewMouseLeftButtonDown += ( _, e ) =>
    {
      if( ( Keyboard.Modifiers & ModifierKeys.Control ) != 0 )
      {
        ToggleSelect( def, rect, canvas, model );
        e.Handled = true;
        return;
      }
      if( m_selHeads.Contains( def ) )
      {
        // Drag the block. Capture the (persistent) tab control now, before anything else grabs it,
        // so switching tabs mid-drag can't drop it.
        ArmDrag( DragMode.Block, model, canvas, e.GetPosition( canvas ), rect, def.Name, def.Row, def.Col );
        Mouse.Capture( m_tabs );
        e.Handled = true;
      }
    };
  }

  // The tab whose header/area is under the pointer (in tab-control coordinates), or −1.
  private int TabUnderCursor( Point onTabs )
  {
    DependencyObject? hit = VisualTreeHelper.HitTest( m_tabs, onTabs )?.VisualHit;
    while( hit is not null and not TabItem ) hit = VisualTreeHelper.GetParent( hit );
    return hit is TabItem ti ? m_tabs.Items.IndexOf( ti ) : -1;
  }

  // Faint outline that follows the pointer over the tab's empty cells, so every cell reads
  // as a place you can drop / add a button (the old materialised "blank" cells, computed).
  // The move is tracked on the ScrollViewer (which fills the whole tab, past the content
  // width) so cells in unused trailing columns highlight too; the outline lives on the canvas
  // and is not clipped to the content, so it shows there.
  private void AttachEmptyCellHover( ScrollViewer sv, Canvas canvas, DataTabModel model )
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

    sv.MouseMove += ( _, e ) =>
    {
      if( m_dragActive ) { hover.Visibility = Visibility.Collapsed; return; } // the drag has its own cues
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
    sv.MouseLeave += ( _, _ ) => hover.Visibility = Visibility.Collapsed;
  }

  // The open-area menu for a data tab: remembers where the right-click landed (in canvas
  // coordinates, so scrolling is accounted for) and adds a button there.
  private void AttachAddHereMenu( ScrollViewer sv, Canvas canvas, DataTabModel model )
  {
    Point clickPoint = default;
    sv.PreviewMouseRightButtonDown += ( _, e ) => clickPoint = e.GetPosition( canvas );

    var menu = new ContextMenu();
    AddInsertItems( menu, model, () => clickPoint );
    AddSelectionItems( menu, model );
    sv.ContextMenu = menu;
  }

  // A section/heading label: bold text sitting on a separator line. An empty name renders as
  // just the line (a plain divider). On a data tab it gets a right-click Edit/Delete menu.
  private FrameworkElement BuildSectionHeader( TabModel.SectionHeader hdr, TabModel model )
  {
    // A collapse triangle (Emojis tab, #26): ▾ open, ▸ collapsed, to the left of the label.
    string label = ( hdr.Collapsible ? ( hdr.Collapsed ? "▸ " : "▾ " ) : "" ) + UiText.NormalizeDisplayText( hdr.Name );
    var text = new TextBlock
    {
      Text                = label,
      // A heading is a label, not a button — a fixed, readable size, independent of the tab's
      // button font (which is huge on the Emojis tab). The family stays the tab's for consistency.
      FontFamily          = new FontFamily( model.FontName ),
      FontSize            = PtToDip( (float)HeadingFontPt ),
      FontWeight          = FontWeights.SemiBold,
      Foreground          = Palette.Brush( "AccentText" ),
      VerticalAlignment   = VerticalAlignment.Bottom,
      HorizontalAlignment = hdr.Align switch { "center" => HorizontalAlignment.Center,
                                               "right"  => HorizontalAlignment.Right,
                                               _        => HorizontalAlignment.Left },
      Margin              = new Thickness( Layout.EdgeGap, 0, Layout.EdgeGap, Layout.EdgeGap ),
      TextTrimming        = TextTrimming.CharacterEllipsis,
    };
    var border = new Border
    {
      Width           = hdr.Width,
      // Shrink a couple of pixels so the separator line lifts clear of the buttons in the row
      // below (the header band otherwise butts right up against the next row's cells).
      Height          = Math.Max( 1, hdr.Height - HeadingUnderlineLift ),
      Background      = Brushes.Transparent, // hit-testable so the right-click menu opens
      BorderBrush     = Palette.Brush( "AccentBarBorder" ),
      BorderThickness = new Thickness( 0, 0, 0, 1 ), // separator line
      Child           = text,
    };

    // A collapsible heading toggles its section on click (the whole bar is the hit target).
    if( hdr.Collapsible )
    {
      border.Cursor = System.Windows.Input.Cursors.Hand;
      border.MouseLeftButtonUp += ( _, e ) =>
      {
        EmojiSectionStore.Toggle( hdr.Name );
        ReconcileEmojiTab(); // in-place: reposition existing buttons, add/remove the section's
        e.Handled = true;
      };
    }

    if( model is DataTabModel dataModel && hdr.Source is { } def )
    {
      var menu = new ContextMenu();
      var edit = new MenuItem { Header = "Edit heading…" };
      edit.Click += ( _, _ ) => HeadingCommands.Edit( dataModel, def );
      var del  = new MenuItem { Header = "Delete heading" };
      del.Click += ( _, _ ) => HeadingCommands.Delete( def );
      menu.Items.Add( edit );
      menu.Items.Add( del );
      AddSelectionItems( menu, dataModel );
      border.ContextMenu = menu;
    }
    return border;
  }

  private const double HeadingUnderlineLift = 2;  // px the heading separator sits above the next row
  private const double HeadingFontPt        = 12.0; // fixed heading text size (points), any tab

  private readonly List<(Border swatch, string hex)> m_toneSwatches = new(); // for the active outline

  // The skin-tone picker (#27): six ✋ swatches pinned to the Emojis tab's top-right corner. The
  // active tone is outlined; clicking one persists it and re-tints the tab in place.
  private FrameworkElement BuildTonePicker( EmojisTab model )
  {
    const int px = 22;
    m_toneSwatches.Clear();
    var swatches = new StackPanel { Orientation = Orientation.Horizontal };
    foreach( (string hex, string label) in EmojiSkin.Tones )
    {
      string glyph = EmojiSkin.Swatch( hex );
      var    img   = EmojiImageProvider.Get( glyph, px );

      var swatch = new Border
      {
        Width           = px + 8,
        Height          = px + 8,
        Margin          = new Thickness( 3, 0, 0, 0 ),
        CornerRadius    = new CornerRadius( 4 ),
        Cursor          = System.Windows.Input.Cursors.Hand,
        ToolTip         = new TextBlock { Text = "Skin tone: " + label },
        Child = img.Image is not null
          ? new Image { Source = img.Image, Width = px, Height = px, Stretch = Stretch.Uniform }
          : new TextBlock { Text = glyph, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
      };
      swatch.MouseLeftButtonUp += ( _, e ) => { SkinToneCommands.Set( hex ); e.Handled = true; };
      swatches.Children.Add( swatch );
      m_toneSwatches.Add( ( swatch, hex ) );
    }
    RefreshTonePicker(); // set the active outline

    // A subtle panel so the swatches read cleanly over whatever emoji sit behind them.
    return new Border
    {
      HorizontalAlignment = HorizontalAlignment.Right,
      VerticalAlignment   = VerticalAlignment.Top,
      Margin              = new Thickness( 0, Layout.TabEdgeGap, Layout.TabEdgeGap + 14, 0 ), // clear the scrollbar
      Padding             = new Thickness( 4 ),
      CornerRadius        = new CornerRadius( 6 ),
      Background          = Theme.WindowBackground,
      BorderBrush         = Palette.Brush( "AccentBarBorder" ),
      BorderThickness     = new Thickness( 1 ),
      Child               = swatches,
    };
  }

  // Outline the swatch for the active tone (called after an in-place re-tint, which doesn't rebuild
  // the picker).
  private void RefreshTonePicker()
  {
    foreach( (Border swatch, string hex) in m_toneSwatches )
    {
      bool active = AppState.SkinTone == hex;
      swatch.Background      = active ? Translucent( Palette.Colour( "SwitchOn" ), 0.25 ) : Brushes.Transparent;
      swatch.BorderBrush     = active ? Palette.Brush( "SwitchOn" ) : Palette.Brush( "ControlBorder" );
      swatch.BorderThickness = new Thickness( active ? 2 : 1 );
    }
  }

  private static string Append( string tip, string line )
  {
    return tip.Length == 0 ? line : tip + "\n" + line;
  }

  // A typographic point is 1/72 inch; a WPF device-independent pixel is 1/96 inch. So a
  // font's point size becomes DIPs by scaling 96/72 (= 4/3). WPF's FontSize is in DIPs.
  private const double DipsPerInch   = 96.0;
  private const double PointsPerInch = 72.0;

  private static double PtToDip( float pointSize )
  {
    return pointSize * ( DipsPerInch / PointsPerInch );
  }

  // First click runs the action (sends the text); a rapid second click sends an Enter
  // instead of re-sending, reproducing the BN_DBLCLK behaviour in UI.ahk.
  //
  // Why not just handle MouseDoubleClick? Because a Button fires Click on *both* clicks of
  // a double-click, so MouseDoubleClick would sit on top of two Clicks — sending the text
  // twice and the Enter in the wrong order (text, Enter, text). Disambiguating the other
  // way (a timer that waits to see if a second click arrives) would delay the single-click
  // send, which we want to be instant. So instead we let the first Click send immediately
  // and, if a second Click lands within DoubleClickMs, turn *that* one into the Enter.
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

    // With no saved height, pick a default: fit the tallest tab's content, but clamp it to
    // a comfortable band (short tabs don't make a tiny window; tall tabs scroll instead of
    // filling the screen), then add the fixed vertical chrome around the scroll area
    // (toolbar strip + tab-header row + top/bottom borders).
    double viewport = Math.Min( DefaultViewportMax, Math.Max( DefaultViewportMin, maxContentH ) );
    double defaultH = viewport + VerticalChrome;

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
    AppState.ActiveWindow = AppState.Foreground.Current();

    if( !IsVisible )
    {
      Show();
    }
    EnsureHwnd();
    RestoreSavedPosition();
    BringToFront();

    // The saved collapsed state was already applied in the constructor (before the first
    // Show), so there's nothing to toggle here.
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
    Win32.SetWindowPos( m_hwnd, Win32.HWND_TOPMOST, 0, 0, 0, 0,
                        Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE );
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
    Win32.RECT area = Win32.GetPrimaryWorkArea();
    Win32.GetWindowRect( m_hwnd, out Win32.RECT rc );
    MoveTo( area.Left + (area.Width  - rc.Width)  / 2,
            area.Top  + (area.Height - rc.Height) / 2 );
  }

  private void MoveTo( int x, int y )
  {
    Win32.SetWindowPos( m_hwnd, IntPtr.Zero, x, y, 0, 0,
                        Win32.SWP_NOSIZE | Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE );
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
        // Remember the current expanded height so a later expand restores it — but only
        // once the window has actually been measured. At startup (collapsing before the
        // first Show) ActualHeight is 0, and we must keep the computed default instead.
        if( !m_collapsed && ActualHeight > 0 )
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
    Win32.SetWindowPos( m_hwnd, Win32.HWND_TOPMOST, 0, 0, 0, 0,
                        Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE );
    InvalidateVisual();
  }

  // ── Favourite spot (physical px) ─────────────────────────────────
  public void SetFavouriteSpot()
  {
    if( m_hwnd == IntPtr.Zero )
    {
      return;
    }
    Win32.GetWindowRect( m_hwnd, out Win32.RECT rc );
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
    nint h = AppState.Foreground.Current();
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
    Win32.AddExStyle( m_hwnd, Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW );

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
    Win32.RemoveStyle( m_hwnd, Win32.WS_MAXIMIZEBOX );
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
    else if( msg == (int)Win32.WM_GETMINMAXINFO && !m_collapsed && m_hwnd != IntPtr.Zero )
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

    var mmi = Marshal.PtrToStructure<Win32.MINMAXINFO>( lParam );
    Win32.RECT work = Win32.GetWorkArea( m_hwnd );

    // Lock to the intended full width (physical px), NOT the current window width:
    // during a collapse→expand this message can fire while the window is still at
    // the collapsed width, and clamping to that would trap the expand at the
    // narrow size.
    double scale = Win32.GetDpiForWindow( m_hwnd ) / 96.0;
    int width = (int)Math.Round( m_fullWidth * scale );
    int minH  = (int)Math.Round( Layout.WindowMinHeight * scale );
    int maxH  = Math.Max( minH, work.Height - margin );

    mmi.ptMaxPosition  = new Win32.POINT { X = work.Left, Y = work.Top };
    mmi.ptMaxSize      = new Win32.POINT { X = width,     Y = maxH };
    mmi.ptMinTrackSize = new Win32.POINT { X = width,     Y = minH };
    mmi.ptMaxTrackSize = new Win32.POINT { X = width,     Y = maxH };

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
    Win32.GetCursorPos( out Win32.POINT cur );
    Win32.GetWindowRect( m_hwnd, out Win32.RECT rc );
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

    Win32.GetCursorPos( out Win32.POINT cur );
    ( int left, int top ) = ApplyDragSnap( cur.X - m_dragOffsetX, cur.Y - m_dragOffsetY );
    Win32.SetWindowPos( m_hwnd, IntPtr.Zero,
                        left, top, 0, 0,
                        Win32.SWP_NOSIZE | Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE );
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
    Win32.POINT start = default;
    bool armed = false;
    bool dragged = false;

    el.PreviewMouseLeftButtonDown += ( _, e ) =>
    {
      Win32.GetCursorPos( out start );
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
      Win32.GetCursorPos( out Win32.POINT cur );
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
      Win32.GetWindowRect( m_hwnd, out Win32.RECT rc );
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
    m_wheelHook = Win32.SetWindowsHookEx( Win32.WH_MOUSE_LL, m_wheelProc,
                                          Win32.GetModuleHandle( null ), 0 );
  }

  private void RemoveWheelHook()
  {
    if( m_wheelHook != IntPtr.Zero )
    {
      Win32.UnhookWindowsHookEx( m_wheelHook );
      m_wheelHook = IntPtr.Zero;
    }
    m_wheelProc = null;
  }

  private nint WheelHookProc( int nCode, nint wParam, nint lParam )
  {
    if( (nCode >= 0) &&
        (wParam == (nint)Win32.WM_MOUSEWHEEL) &&
        IsVisible &&
        !m_collapsed )
    {
      var data = Marshal.PtrToStructure<Win32.MSLLHOOKSTRUCT>( lParam );
      if( Win32.GetWindowRect( m_hwnd, out Win32.RECT rc ) &&
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
    return Win32.CallNextHookEx( IntPtr.Zero, nCode, wParam, lParam );
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
