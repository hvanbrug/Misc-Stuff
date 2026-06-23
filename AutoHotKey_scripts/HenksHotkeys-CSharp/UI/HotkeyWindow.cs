using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HenksHotkeys.Core;
using HenksHotkeys.Native;

namespace HenksHotkeys.UI;

/// <summary>
/// The always-on-top, frameless helper window (UI.ahk / HotkeyWindow class).
/// Hosts the tabbed grid of symbol buttons plus the corner indicators and helper
/// buttons; supports collapse/expand, manual vertical resize, drag-to-move with
/// top/favourite snapping, smooth wheel scrolling and INI-persisted state.
/// </summary>
internal sealed class HotkeyWindow : Form
{
  private const int WM_MOVING        = 0x0216;
  private const int WM_EXITSIZEMOVE  = 0x0232;
  private const int HTCAPTION = 2;
  private const int WS_CLIPCHILDREN = 0x02000000;
  private const int ResizeEdge = 6;

  private readonly DarkTabControl       m_tabs   = new();
  private readonly List<TabPanelControl> m_panels = new();
  private readonly ToolTip               m_tip    = new() { ShowAlways = true, InitialDelay = 300 };

  private Label  m_clipIndicator  = null!;
  private Label  m_stripIndicator = null!;
  private Button m_toggleBtn      = null!;
  private readonly List<Control> m_helperButtons = new();

  private int  m_fullWidth;
  private int  m_fullHeight;
  private bool m_collapsed;
  private bool m_suppressResizePersist;

  // Manual vertical-resize state (UI.ahk BeginResize / OnMouseMove).
  private bool   m_resizing;
  private string m_resizeEdge = "";
  private int    m_resizeGrabY;
  private int    m_resizeStartTop;
  private int    m_resizeStartHeight;

  private int? m_favX;
  private int? m_favY;

  // Drag-move snap state (UI.ahk OnWindowMoving): the offset of the cursor from
  // the window's top-left captured when the move starts, plus latched snap flags
  // with release hysteresis so a snapped window can be pulled free.
  private int  m_dragOffsetX;
  private int  m_dragOffsetY;
  private bool m_snappedToTop;
  private bool m_snappedToFav;

  private readonly System.Windows.Forms.Timer m_activeWinTimer = new() { Interval = 200 };

  private IntPtr m_wheelHook;
  private NativeMethods.LowLevelMouseProc? m_wheelProc;

  public HotkeyWindow()
  {
    AutoScaleMode   = AutoScaleMode.None; // we scale by DPI ourselves (see Dpi)
    FormBorderStyle = FormBorderStyle.None;
    ShowInTaskbar   = false;
    TopMost         = true;
    MinimizeBox     = false;
    MaximizeBox     = false;
    StartPosition   = FormStartPosition.Manual;
    Text            = "Henks Hotkeys";
    BackColor       = Theme.WindowBackground;
    DoubleBuffered  = true;
    KeyPreview      = true;

    TryLoadIcon();
    BuildTabs();
    BuildCornerControls();

    Padding = new Padding( Dpi.S( 4 ), Dpi.S( 26 ), Dpi.S( 4 ), Dpi.S( 14 ) );
    m_tabs.Dock = DockStyle.Fill;
    Controls.Add( m_tabs );
    m_tabs.SendToBack();

    ComputeFullSize();
    Width  = m_fullWidth;
    Height = m_fullHeight;
    ApplySizeLimits();

    Theme.ApplyDarkFrame( Handle ); // realises the handle too

    m_favX = AppState.Ini.FavX;
    m_favY = AppState.Ini.FavY;

    m_activeWinTimer.Tick += ( _, _ ) => TrackActiveWindow();
    m_activeWinTimer.Start();

    foreach( Control c in m_helperButtons )
    {
      c.BringToFront();
    }
    m_clipIndicator.BringToFront();
    m_stripIndicator.BringToFront();
    m_toggleBtn.BringToFront();

    LayoutTopControls();
  }

  /// <summary>
  /// Attach the shared tray/right-click menu to the window surface and corner
  /// controls (UI.ahk OnRButtonUp showed the tray menu on right-click of the
  /// window and indicators).
  /// </summary>
  public void AttachContextMenu( ContextMenuStrip menu )
  {
    ContextMenuStrip               = menu;
    m_tabs.ContextMenuStrip        = menu;
    m_clipIndicator.ContextMenuStrip  = menu;
    m_stripIndicator.ContextMenuStrip = menu;
    m_toggleBtn.ContextMenuStrip      = menu;
    foreach( TabPanelControl panel in m_panels )
    {
      panel.ContextMenuStrip = menu;
    }
  }

  // ── No-activate, frameless-but-resizable window ───────────────────
  protected override bool ShowWithoutActivation => true;

  protected override CreateParams CreateParams
  {
    get
    {
      CreateParams cp = base.CreateParams;
      cp.Style   |= WS_CLIPCHILDREN;
      cp.ExStyle |= NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_TOPMOST;
      return cp;
    }
  }

  private void TryLoadIcon()
  {
    try
    {
      string ico = Path.Combine( AppState.BaseDir, "Images", "HenksHotkeys.ico" );
      if( File.Exists( ico ) )
      {
        Icon = new Icon( ico );
      }
    }
    catch { /* icon optional */ }
  }

  // ── Construction ─────────────────────────────────────────────────
  private void BuildTabs()
  {
    m_tabs.Font = Dpi.ScaledFont( "Segoe UI", 10f );

    foreach( TabModel model in AppState.Tabs )
    {
      var page = new System.Windows.Forms.TabPage( model.Name )
      {
        BackColor = Theme.WindowBackground
      };
      var panel = new TabPanelControl( model ) { Dock = DockStyle.Fill };
      page.Controls.Add( panel );
      m_panels.Add( panel );
      m_tabs.TabPages.Add( page );
    }

    int last = AppState.Ini.LastTab - 1;
    if( last >= 0 && last < m_tabs.TabPages.Count )
    {
      m_tabs.SelectedIndex = last;
    }

    m_tabs.SelectedIndexChanged += ( _, _ ) =>
    {
      AppState.Ini.SetLastTab( m_tabs.SelectedIndex + 1 );
    };
  }

  private void BuildCornerControls()
  {
    m_clipIndicator  = MakeIndicator( "○", "Clipboard send mode: OFF" );
    m_stripIndicator = MakeIndicator( "☺", "Strip emojis from comments: OFF" );

    m_toggleBtn = MakeHelperButton( "▲", "Shrink window", "Segoe UI Symbol", 11f, ToggleCollapsed );
    m_helperButtons.Remove( m_toggleBtn ); // toggle lives in the left cluster, always visible

    m_clipIndicator.Click  += ( _, _ ) => AppActions.ToggleClipboardSendMode();
    m_stripIndicator.Click += ( _, _ ) => AppActions.ToggleStripSendEmojis();
    Controls.Add( m_clipIndicator );
    Controls.Add( m_stripIndicator );
    // m_toggleBtn was already added to Controls by MakeHelperButton.

    MakeHelperButton( "🔄",  "Repaint / Refresh",          "Segoe UI Symbol", 10f, () => m_panels[m_tabs.SelectedIndex].Refresh() );
    MakeHelperButton( "⌫.", "Back 3, Replace with period", "Segoe UI Symbol", 10f, () => TextSender.SendInputKeys( "\b\b\b. " ) );
    MakeHelperButton( "⇚,", "Back 3, Insert Comma",        "Segoe UI Symbol", 14f, () => TextSender.SendInputKeys( "{Left}{Left}{Left}, " ) );
    MakeHelperButton( "↩",  "Enter / Newline",             "Segoe UI Symbol", 12f, () => TextSender.SendInputKeys( "{Enter}" ) );
    MakeHelperButton( "▲",  "Shrink window",               "Segoe UI Symbol", 12f, ToggleCollapsed );

    UpdateClipIndicator( AppState.UseClipSend );
    UpdateStripIndicator( AppState.StripSendEmojis );
  }

  private Label MakeIndicator( string glyph, string tip )
  {
    var lbl = new Label
    {
      Text      = glyph,
      AutoSize  = false,
      Size      = new Size( Dpi.S( 16 ), Dpi.S( 18 ) ),
      Font      = Dpi.ScaledFont( "Segoe UI Symbol", 10f ),
      TextAlign = ContentAlignment.MiddleCenter,
      ForeColor = Theme.IsDark ? Theme.DarkText : SystemColors.ControlText,
      BackColor = Color.Transparent,
      Cursor    = Cursors.Hand
    };
    m_tip.SetToolTip( lbl, tip );
    return lbl;
  }

  private Button MakeHelperButton( string text, string tip, string fontName, float fontSize, Action onClick )
  {
    var btn = new ThemedButton( "center" )
    {
      Text       = text,
      Font       = Dpi.ScaledFont( fontName, fontSize ),
      Size       = new Size( Dpi.S( 40 ), Dpi.S( 24 ) ),
      ClickAction= onClick,
      TabStop    = false
    };
    m_tip.SetToolTip( btn, tip );
    Controls.Add( btn );
    m_helperButtons.Add( btn );
    return btn;
  }

  private void ComputeFullSize()
  {
    int maxContentW = 0;
    foreach( TabModel m in AppState.Tabs )
    {
      maxContentW = Math.Max( maxContentW, m.ContentWidth );
    }

    // content + scrollbar + tab-control insets + form side padding (logical px)
    m_fullWidth = Dpi.S( maxContentW + 18 + 28 );

    int maxContentH = 0;
    foreach( TabModel m in AppState.Tabs )
    {
      maxContentH = Math.Max( maxContentH, m.ContentHeight );
    }
    int viewport = Math.Min( 330, Math.Max( 320, maxContentH ) );
    int defaultH = Dpi.S( viewport + 58 );

    int screenH  = Screen.PrimaryScreen!.Bounds.Height;
    int savedH   = AppState.Ini.WndHeight;
    m_fullHeight = savedH >= Dpi.S( 140 ) && savedH <= screenH ? savedH : defaultH;
  }

  private void ApplySizeLimits()
  {
    int screenH = Screen.PrimaryScreen!.Bounds.Height;
    MinimumSize = new Size( m_fullWidth, Dpi.S( 140 ) );
    MaximumSize = new Size( m_fullWidth, screenH );
  }

  // ── Show / hide ──────────────────────────────────────────────────
  public void ShowUi()
  {
    AppState.ActiveWindow = NativeMethods.GetForegroundWindow();

    RestoreSavedPosition();
    if( !Visible )
    {
      Show();
    }
    NativeMethods.SetWindowPos( Handle, new IntPtr( -1 ), 0, 0, 0, 0,
                                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE );

    if( AppState.Ini.IsCollapsed )
    {
      SetCollapsed( true, persist: false );
    }

    InstallWheelHook();
    AppState.Ini.SetWndOpen( true );
  }

  public void HideUi()
  {
    RemoveWheelHook();
    Hide();
    AppState.Ini.SetWndOpen( false );
  }

  public void ToggleUi()
  {
    if( !Visible )
    {
      ShowUi();
    }
    else
    {
      HideUi();
    }
  }

  private void RestoreSavedPosition()
  {
    int? x = AppState.Ini.WndX;
    int? y = AppState.Ini.WndY;
    if( x is int px && y is int py )
    {
      Location = new Point( px, py );
      return;
    }

    // No saved position: centre on the work area rather than landing at (0, 0),
    // which sits inside the top-of-screen snap zone.
    Rectangle area = Screen.PrimaryScreen!.WorkingArea;
    Location = new Point( area.Left + ( area.Width - Width ) / 2,
                          area.Top  + ( area.Height - Height ) / 2 );
  }

  // ── Collapse / expand ────────────────────────────────────────────
  public void ToggleCollapsed() => SetCollapsed( !m_collapsed );

  public void SetCollapsed( bool collapse, bool persist = true )
  {
    if( collapse == m_collapsed && persist )
    {
      // still allow the initial restore to apply layout
    }

    m_suppressResizePersist = true;
    try
    {
      if( collapse )
      {
        if( !m_collapsed )
        {
          m_fullHeight = Height;
        }
        m_collapsed = true;
        m_tabs.Visible = false;
        foreach( Control c in m_helperButtons ) c.Visible = false;
        m_toggleBtn.Text = "▼";
        m_tip.SetToolTip( m_toggleBtn, "Expand window" );

        MinimumSize = MaximumSize = new Size( Dpi.S( 72 ), Dpi.S( 26 ) );
        Size = new Size( Dpi.S( 72 ), Dpi.S( 26 ) );
      }
      else
      {
        m_collapsed = false;
        m_tabs.Visible = true;
        foreach( Control c in m_helperButtons ) c.Visible = true;
        m_toggleBtn.Text = "▲";
        m_tip.SetToolTip( m_toggleBtn, "Shrink window" );

        ApplySizeLimits();
        Size = new Size( m_fullWidth, m_fullHeight );
      }
      LayoutTopControls();
    }
    finally
    {
      m_suppressResizePersist = false;
    }

    if( persist )
    {
      AppState.Ini.SetCollapsed( collapse );
    }
  }

  // ── Layout of the top-strip controls ─────────────────────────────
  private void LayoutTopControls()
  {
    int yTop = Dpi.S( 1 );
    int gap1 = Dpi.S( 1 );

    // Left cluster: clip ○, toggle ▲/▼, strip ☺ — always visible.
    int clipX  = Dpi.S( 2 );
    int btnX   = clipX + m_clipIndicator.Width + gap1;
    int stripX = btnX + m_toggleBtn.Width + gap1;

    m_clipIndicator.Location  = new Point( clipX, Dpi.S( 4 ) );
    m_toggleBtn.Location      = new Point( btnX, yTop );
    m_stripIndicator.Location = new Point( stripX, Dpi.S( 4 ) );

    if( m_collapsed )
    {
      return;
    }

    // Right cluster: helper buttons, right-aligned.
    int gap = Dpi.S( 2 );
    int x   = ClientSize.Width - Dpi.S( 4 );
    foreach( Control c in m_helperButtons )
    {
      x -= c.Width;
      c.Location = new Point( x, yTop );
      x -= gap;
    }
  }

  protected override void OnResize( EventArgs e )
  {
    base.OnResize( e );
    LayoutTopControls();
  }

  // ── Favourite spot ───────────────────────────────────────────────
  public void SetFavouriteSpot()
  {
    m_favX = Left;
    m_favY = Top;
    AppState.Ini.SetFav( Left, Top );
  }

  public void MoveToFavouriteSpot()
  {
    m_favX = AppState.Ini.FavX;
    m_favY = AppState.Ini.FavY;
    if( m_favX is int fx && m_favY is int fy )
    {
      Location = new Point( fx, fy );
      AppState.Ini.SetWndPos( fx, fy );
    }
  }

  // ── Indicators ───────────────────────────────────────────────────
  public void UpdateClipIndicator( bool on )
  {
    m_clipIndicator.Text = on ? "●" : "○";
    m_tip.SetToolTip( m_clipIndicator, "Clipboard send mode: " + ( on ? "ON" : "OFF" ) );
  }

  public void UpdateStripIndicator( bool on )
  {
    m_stripIndicator.Text = on ? "☻" : "☺";
    m_tip.SetToolTip( m_stripIndicator, "Strip emojis from comments: " + ( on ? "ON" : "OFF" ) );
  }

  // ── Active-window tracking ───────────────────────────────────────
  private void TrackActiveWindow()
  {
    IntPtr h = NativeMethods.GetForegroundWindow();
    if( h == IntPtr.Zero || h == Handle )
    {
      return;
    }
    AppState.ActiveWindow = h;
  }

  // ── Border paint ─────────────────────────────────────────────────
  protected override void OnPaint( PaintEventArgs e )
  {
    base.OnPaint( e );
    int thickness = Dpi.S( Theme.BorderThickness );
    using var pen = new Pen( Theme.BorderColor, thickness );
    Rectangle r = ClientRectangle;
    int half = thickness / 2;
    e.Graphics.DrawRectangle( pen, r.Left + half, r.Top + half,
                              r.Width - thickness, r.Height - thickness );
  }

  // ── Window procedure: drag-move snapping + persistence ───────────
  protected override void WndProc( ref Message m )
  {
    switch( m.Msg )
    {
      case WM_MOVING:
        if( HandleMoving( m.LParam ) )
        {
          m.Result = new IntPtr( 1 );
          return;
        }
        break;

      case WM_EXITSIZEMOVE:
        PersistPositionAndSize();
        break;
    }

    base.WndProc( ref m );
  }

  // ── Manual move + vertical resize via mouse (UI.ahk) ─────────────
  // The window is frameless (no caption), so a press on the bare form surface
  // either starts an OS move (HTCAPTION) or a manual top/bottom edge resize.
  protected override void OnMouseDown( MouseEventArgs e )
  {
    base.OnMouseDown( e );
    if( e.Button != MouseButtons.Left )
    {
      return;
    }

    if( !m_collapsed )
    {
      string edge = ResizeEdgeAt( e.Location );
      if( edge.Length > 0 )
      {
        BeginResize( edge );
        return;
      }
    }

    // Bare-background press → start a window move via the OS move loop.
    // Record how far the cursor is from the window's top-left so the snap logic
    // can recover the cursor-implied position during WM_MOVING.
    NativeMethods.GetCursorPos( out NativeMethods.POINT cur );
    m_dragOffsetX = cur.X - Left;
    m_dragOffsetY = cur.Y - Top;

    NativeMethods.ReleaseCapture();
    NativeMethods.SendMessage( Handle, NativeMethods.WM_NCLBUTTONDOWN, new IntPtr( HTCAPTION ), IntPtr.Zero );
  }

  protected override void OnMouseMove( MouseEventArgs e )
  {
    base.OnMouseMove( e );

    if( m_resizing )
    {
      NativeMethods.GetCursorPos( out NativeMethods.POINT pt );
      int dy = pt.Y - m_resizeGrabY;
      int screenH = Screen.PrimaryScreen!.Bounds.Height;

      int minH = Dpi.S( 140 );
      int newTop, newH;
      if( m_resizeEdge == "bottom" )
      {
        newH   = Math.Clamp( m_resizeStartHeight + dy, minH, screenH );
        newTop = m_resizeStartTop;
      }
      else
      {
        int bottomFixed = m_resizeStartTop + m_resizeStartHeight;
        newH   = Math.Clamp( m_resizeStartHeight - dy, minH, screenH );
        newTop = bottomFixed - newH;
      }
      SetBounds( Left, newTop, Width, newH );
      return;
    }

    Cursor = !m_collapsed && ResizeEdgeAt( e.Location ).Length > 0 ? Cursors.SizeNS : Cursors.Default;
  }

  protected override void OnMouseUp( MouseEventArgs e )
  {
    base.OnMouseUp( e );
    if( m_resizing )
    {
      m_resizing = false;
      Capture    = false;
      Cursor     = Cursors.Default;
      PersistPositionAndSize();
    }
  }

  private string ResizeEdgeAt( Point clientPt )
  {
    int edge = Dpi.S( ResizeEdge );
    if( clientPt.Y <= edge )                     return "top";
    if( clientPt.Y >= ClientSize.Height - edge ) return "bottom";
    return "";
  }

  private void BeginResize( string edge )
  {
    NativeMethods.GetCursorPos( out NativeMethods.POINT pt );
    m_resizing          = true;
    m_resizeEdge        = edge;
    m_resizeGrabY       = pt.Y;
    m_resizeStartTop    = Top;
    m_resizeStartHeight = Height;
    Capture             = true;
  }

  // Faithful port of UI.ahk OnWindowMoving: snap to the favourite spot or the
  // top of the screen, but use the cursor-implied position plus a release
  // threshold so a snapped window can be dragged free again.
  private bool HandleMoving( IntPtr lParam )
  {
    const int snapThreshold    = 20;
    const int releaseThreshold = 30;

    NativeMethods.RECT r = Marshal.PtrToStructure<NativeMethods.RECT>( lParam );
    int w = r.Width;
    int h = r.Height;

    NativeMethods.GetCursorPos( out NativeMethods.POINT cur );
    int impliedLeft = cur.X - m_dragOffsetX;
    int impliedTop  = cur.Y - m_dragOffsetY;

    // ── Favourite-spot snap (highest priority) ──
    if( m_favX is int favX && m_favY is int favY )
    {
      if( m_snappedToFav )
      {
        if( Math.Abs( impliedLeft - favX ) >= releaseThreshold ||
            Math.Abs( impliedTop  - favY ) >= releaseThreshold )
        {
          m_snappedToFav = false;
          SetRect( ref r, impliedLeft, impliedTop, w, h );
        }
        else
        {
          SetRect( ref r, favX, favY, w, h );
        }
        Marshal.StructureToPtr( r, lParam, false );
        return true;
      }

      if( Math.Abs( r.Left - favX ) <= snapThreshold && Math.Abs( r.Top - favY ) <= snapThreshold )
      {
        m_snappedToFav = true;
        m_snappedToTop = false;
        SetRect( ref r, favX, favY, w, h );
        Marshal.StructureToPtr( r, lParam, false );
        return true;
      }
    }

    // ── Top-of-screen snap (y = 0) ──
    if( m_snappedToTop )
    {
      if( impliedTop >= releaseThreshold )
      {
        m_snappedToTop = false;
        r.Top = impliedTop; r.Bottom = impliedTop + h;
      }
      else
      {
        r.Top = 0; r.Bottom = h;
      }
      Marshal.StructureToPtr( r, lParam, false );
      return true;
    }

    if( r.Top <= snapThreshold )
    {
      m_snappedToTop = true;
      r.Top = 0; r.Bottom = h;
      Marshal.StructureToPtr( r, lParam, false );
      return true;
    }

    return false;
  }

  private static void SetRect( ref NativeMethods.RECT r, int left, int top, int w, int h )
  {
    r.Left = left; r.Top = top; r.Right = left + w; r.Bottom = top + h;
  }

  private void PersistPositionAndSize()
  {
    if( m_collapsed )
    {
      AppState.Ini.SetWndPos( Left, Top );
      return;
    }

    AppState.Ini.SetWndPos( Left, Top );
    if( !m_suppressResizePersist )
    {
      m_fullHeight = Height;
      AppState.Ini.SetWndHeight( Height );
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

  private IntPtr WheelHookProc( int nCode, IntPtr wParam, IntPtr lParam )
  {
    if( nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_MOUSEWHEEL && Visible && !m_collapsed )
    {
      var data = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>( lParam );
      if( NativeMethods.GetWindowRect( Handle, out NativeMethods.RECT rc ) &&
          data.pt.X >= rc.Left && data.pt.X < rc.Right &&
          data.pt.Y >= rc.Top  && data.pt.Y < rc.Bottom )
      {
        int delta = unchecked( (short)( ( data.mouseData >> 16 ) & 0xFFFF ) );
        if( delta != 0 && m_tabs.SelectedIndex >= 0 )
        {
          m_panels[m_tabs.SelectedIndex].ScrollByWheel( -( delta / 120 ) );
        }
        return new IntPtr( 1 ); // swallow
      }
    }
    return NativeMethods.CallNextHookEx( IntPtr.Zero, nCode, wParam, lParam );
  }

  // Escape hides the window (mirrors the AHK Escape handler).
  protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
  {
    if( keyData == Keys.Escape )
    {
      HideUi();
      return true;
    }
    return base.ProcessCmdKey( ref msg, keyData );
  }

  protected override void OnFormClosing( FormClosingEventArgs e )
  {
    RemoveWheelHook();
    m_activeWinTimer.Stop();
    base.OnFormClosing( e );
  }
}
