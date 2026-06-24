using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using HenksHotkeys.Native;

namespace HenksHotkeys.UI;

/// <summary>
/// A notification-area (tray) icon built on Shell_NotifyIcon and a hidden
/// HwndSource message window — a pure WPF + Win32 replacement for the WinForms
/// NotifyIcon. Right-click shows the supplied WPF <see cref="ContextMenu"/>;
/// double-click runs <paramref name="onDoubleClick"/>.
/// </summary>
internal sealed class TrayIcon : IDisposable
{
  private const int WM_APP          = 0x8000;
  private const int WM_TrayCallback = WM_APP + 1;
  private const int WM_LBUTTONDBLCLK = 0x0203;
  private const int WM_RBUTTONUP     = 0x0205;
  private const int WM_CONTEXTMENU   = 0x007B;

  private const int WS_POPUP        = unchecked( (int)0x80000000 );
  private const int WS_EX_TOOLWINDOW = 0x00000080;

  private readonly HwndSource  m_source;
  private readonly ContextMenu m_menu;
  private readonly Action      m_onDoubleClick;
  private readonly IntPtr      m_icon;
  private readonly bool        m_ownIcon;

  private NativeMethods.NOTIFYICONDATAW m_data;
  private bool m_disposed;

  public TrayIcon( ContextMenu menu, Action onDoubleClick, string tooltip )
  {
    m_menu          = menu;
    m_onDoubleClick = onDoubleClick;

    // Hidden top-level tool window to receive the tray callback messages.
    var prms = new HwndSourceParameters( "HenksHotkeysTray" )
    {
      Width               = 0,
      Height              = 0,
      WindowStyle         = WS_POPUP,
      ExtendedWindowStyle = WS_EX_TOOLWINDOW,
    };
    m_source = new HwndSource( prms );
    m_source.AddHook( WndProc );

    ( m_icon, m_ownIcon ) = LoadIcon();

    m_data = new NativeMethods.NOTIFYICONDATAW
    {
      cbSize           = Marshal.SizeOf<NativeMethods.NOTIFYICONDATAW>(),
      hWnd             = m_source.Handle,
      uID              = 1,
      uFlags           = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP,
      uCallbackMessage = WM_TrayCallback,
      hIcon            = m_icon,
      szTip            = tooltip,
    };
    NativeMethods.Shell_NotifyIconW( NativeMethods.NIM_ADD, ref m_data );
  }

  // The tray icon is the application's own exe icon (set via ApplicationIcon),
  // falling back to the shared system application icon.
  private static (IntPtr Handle, bool Owned) LoadIcon()
  {
    string? exe = Environment.ProcessPath;
    if( exe is not null )
    {
      var small = new IntPtr[1];
      if( NativeMethods.ExtractIconExW( exe, 0, null, small, 1 ) > 0 && small[0] != IntPtr.Zero )
      {
        return ( small[0], true );
      }
    }
    return ( NativeMethods.LoadIconW( IntPtr.Zero, NativeMethods.IDI_APPLICATION ), false );
  }

  private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
  {
    if( msg == WM_TrayCallback )
    {
      int mouse = (int)( lParam.ToInt64() & 0xFFFF );
      switch( mouse )
      {
        case WM_RBUTTONUP:
        case WM_CONTEXTMENU:
          ShowMenu();
          handled = true;
          break;

        case WM_LBUTTONDBLCLK:
          try { m_onDoubleClick(); } catch { /* ignore */ }
          handled = true;
          break;
      }
    }
    return IntPtr.Zero;
  }

  private void ShowMenu()
  {
    // Foreground the (hidden) message window so the WPF popup dismisses cleanly
    // when the user clicks elsewhere, then open the menu at the cursor.
    NativeMethods.SetForegroundWindow( m_source.Handle );
    m_menu.Placement = PlacementMode.MousePoint;
    m_menu.IsOpen    = true;
  }

  public void Dispose()
  {
    if( m_disposed )
    {
      return;
    }
    m_disposed = true;

    NativeMethods.Shell_NotifyIconW( NativeMethods.NIM_DELETE, ref m_data );
    if( m_ownIcon && m_icon != IntPtr.Zero )
    {
      NativeMethods.DestroyIcon( m_icon );
    }
    m_source.RemoveHook( WndProc );
    m_source.Dispose();
  }
}
