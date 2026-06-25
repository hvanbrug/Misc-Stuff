using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace NetworkShares;

public partial class MainWindow : Window
{
  private readonly MainViewModel m_vm = new();
  private IntPtr m_hwnd = IntPtr.Zero;

  public MainWindow()
  {
    InitializeComponent();
    DataContext = m_vm;
    Loaded   += async ( _, _ ) => await m_vm.RefreshAsync();
    Closed   += ( _, _ ) => ThemeManager.ThemeChanged -= ApplyCaptionTheme;
  }

  protected override void OnSourceInitialized( EventArgs e )
  {
    base.OnSourceInitialized( e );
    m_hwnd = new WindowInteropHelper( this ).Handle;
    HwndSource.FromHwnd( m_hwnd )?.AddHook( WndProc );
    ApplyCaptionTheme();
    ThemeManager.ThemeChanged += ApplyCaptionTheme;
  }

  private const int WM_SETTINGCHANGE = 0x001A;

  // Windows broadcasts WM_SETTINGCHANGE with "ImmersiveColorSet" when the user
  // flips the light/dark setting — re-evaluate and re-theme live.
  private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
  {
    if( msg == WM_SETTINGCHANGE && lParam != IntPtr.Zero &&
        Marshal.PtrToStringUni( lParam ) == "ImmersiveColorSet" )
    {
      ThemeManager.Refresh();
    }
    return IntPtr.Zero;
  }

  // Match the OS title bar (caption) to the chosen theme.
  private void ApplyCaptionTheme()
  {
    if( m_hwnd == IntPtr.Zero )
    {
      return;
    }
    int dark = ThemeManager.IsDark ? 1 : 0;
    DwmSetWindowAttribute( m_hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof( int ) );

    // DWM caches the caption; nudge a non-client frame change so it repaints
    // immediately when the theme flips on an already-visible window.
    SetWindowPos( m_hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE );
  }

  private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

  private const uint SWP_NOSIZE     = 0x0001;
  private const uint SWP_NOMOVE     = 0x0002;
  private const uint SWP_NOZORDER   = 0x0004;
  private const uint SWP_NOACTIVATE = 0x0010;
  private const uint SWP_FRAMECHANGED = 0x0020;

  [DllImport( "dwmapi.dll" )]
  private static extern int DwmSetWindowAttribute( IntPtr hwnd, int attr, ref int value, int size );

  [DllImport( "user32.dll" )]
  private static extern bool SetWindowPos( IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags );

  private void Log_TextChanged( object sender, TextChangedEventArgs e ) => LogBox.ScrollToEnd();
}
