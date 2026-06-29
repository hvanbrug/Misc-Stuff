using System.Windows;
using System.Windows.Media;
using HenksHotkeys.Native;
using Microsoft.Win32;

namespace HenksHotkeys.UI;

/// <summary>
/// Light/dark theming that follows the Windows app theme, decided once at
/// startup (Theme.ahk). Dark visuals come from the WPF styles in App.xaml; this
/// class exposes the few brushes the code-built window needs, the dark-mode flag,
/// and the DWM dark-frame call.
/// </summary>
internal static class Theme
{
  private static bool? s_isDark;

  public static bool IsDark
  {
    get
    {
      s_isDark ??= ReadIsDark();
      return s_isDark.Value;
    }
  }

  private static bool ReadIsDark()
  {
    try
    {
      object? v = Registry.GetValue(
        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
        "AppsUseLightTheme", 1 );
      if( v is int light )
      {
        return light == 0;
      }
    }
    catch
    {
      // older Windows / missing value → treat as light
    }
    return false;
  }

  private static SolidColorBrush Frozen( byte r, byte g, byte b )
  {
    var brush = new SolidColorBrush( Color.FromRgb( r, g, b ) );
    brush.Freeze();
    return brush;
  }

  // Greys matching the NetworkShares dark scheme (layered, faint cool tint).
  public static readonly Brush DarkBackground  = Frozen( 0x1E, 0x1E, 0x20 );
  public static readonly Brush DarkText        = Frozen( 0xE8, 0xE8, 0xE8 );

  public const int BorderThickness = 1;

  /// <summary>Publish the palette for the current system theme into the app resources.
  /// Call once at startup before any window/style is built.</summary>
  public static void Apply() => Palette.Install( Application.Current.Resources, IsDark );

  // Convenience brushes for the code-built chrome, sourced from the shared palette.
  public static Brush WindowBackground => Palette.Brush( "WindowBg" );
  public static Brush BorderColor      => Palette.Brush( "CardBorder" );
  public static Brush BlankBgColor     => Brushes.Transparent;
  //public static Brush BlankBorderColor => Brushes.Transparent;
  public static Brush BlankBorderColor => Brushes.LightGray;
  public static Brush TextColor        => Palette.Brush( "TextPrimary" );

  // Tell DWM to render the window frame dark (Win10 2004+/Win11), so the thin
  // resize frame doesn't show up white.
  public static void ApplyDarkFrame( IntPtr hwnd )
  {
    if( !IsDark || hwnd == IntPtr.Zero )
    {
      return;
    }

    int enable = 1;
    try { NativeMethods.DwmSetWindowAttribute( hwnd, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref enable, sizeof( int ) ); }
    catch { /* unsupported on older Windows */ }

    int border = 0x201E1E; // COLORREF (0x00BBGGRR) for #1E1E20 — matches DarkBackground
    try { NativeMethods.DwmSetWindowAttribute( hwnd, NativeMethods.DWMWA_BORDER_COLOR, ref border, sizeof( int ) ); }
    catch { /* Win11 only */ }
  }

  /// <summary>Round the window corners (Win11). No-op on older Windows.</summary>
  public static void ApplyRoundedCorners( IntPtr hwnd )
  {
    if( hwnd == IntPtr.Zero )
    {
      return;
    }
    int pref = NativeMethods.DWMWCP_ROUND;
    try { NativeMethods.DwmSetWindowAttribute( hwnd, NativeMethods.DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, sizeof( int ) ); }
    catch { /* Win11 only */ }
  }
}
