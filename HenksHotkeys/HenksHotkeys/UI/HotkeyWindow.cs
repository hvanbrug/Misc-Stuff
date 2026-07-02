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

  private int SelectionCount => m_selBtns.Count + m_selHeads.Count;

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
      m_tabCanvases.Add( canvas ); // aligned with AppState.Tabs / m_tabs.Items order
      // Headings render *under* the buttons: add them first so a button that overlaps a
      // heading still receives the mouse (stays draggable) and paints on top of it.
      foreach( TabModel.SectionHeader hdr in model.Headers )
      {
        FrameworkElement label = BuildSectionHeader( hdr, model );
        Canvas.SetLeft( label, hdr.X );
        Canvas.SetTop( label, hdr.Y );
        canvas.Children.Add( label );

        // Headings are selectable (Ctrl+click) and move with the block when dragged.
        if( model is DataTabModel headModel && hdr.Source is { } sdef )
        {
          WireHeadingSelect( label, sdef, new Rect( hdr.X, hdr.Y, hdr.Width, hdr.Height ), canvas, headModel );
        }
      }

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
    var text = new TextBlock
    {
      Text                = UiText.NormalizeDisplayText( hdr.Name ),
      FontFamily          = new FontFamily( model.FontName ),
      FontSize            = PtToDip( model.FontSize ),
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
      Height          = hdr.Height,
      Background      = Brushes.Transparent, // hit-testable so the right-click menu opens
      BorderBrush     = Palette.Brush( "AccentBarBorder" ),
      BorderThickness = new Thickness( 0, 0, 0, 1 ), // separator line
      Child           = text,
    };

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
    AppState.ActiveWindow = NativeMethods.GetForegroundWindow();

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
