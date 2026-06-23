using System.Drawing;
using HenksHotkeys.Native;
using Microsoft.Win32;

namespace HenksHotkeys.UI;

/// <summary>
/// Light/dark theming that follows the Windows app theme, decided once at
/// startup (Theme.ahk). In light mode the standard control look is kept; in dark
/// mode controls are painted from a small grey palette and the window frame is
/// darkened via DWM.
/// </summary>
internal static class Theme
{
  private static bool? s_isDark;

  public static bool IsDark
  {
    get
    {
      if( s_isDark is null )
      {
        s_isDark = ReadIsDark();
      }
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

  // ── Palette (greys; the AHK stores these as 0xRRGGBB) ────────────
  public static readonly Color DarkBackground = FromHex( 0x202020 );
  public static readonly Color DarkText       = FromHex( 0xDCDCDC );
  public static readonly Color ButtonFace     = FromHex( 0x3A3A3A );
  public static readonly Color ButtonPressed  = FromHex( 0x4A4A4A );
  public static readonly Color ButtonBorder   = FromHex( 0x555555 );
  public static readonly Color EmojiBackdrop  = FromHex( 0x404040 );

  public const int BorderThickness = 2;
  public static readonly Color DarkModeBorder  = FromHex( 0xAAAAAA );
  public static readonly Color LightModeBorder = FromHex( 0x555555 );

  public static Color BorderColor => IsDark ? DarkModeBorder : LightModeBorder;

  public static Color WindowBackground => IsDark ? DarkBackground : SystemColors.Control;
  public static Color TabStripBackground => IsDark ? DarkBackground : SystemColors.Control;

  private static Color FromHex( int rgb )
  {
    return Color.FromArgb( (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF );
  }

  // ── DWM dark frame for the (frameless) window ─────────────────────
  public static void ApplyDarkFrame( IntPtr hwnd )
  {
    if( !IsDark || hwnd == IntPtr.Zero )
    {
      return;
    }

    int enable = 1;
    try
    {
      NativeMethods.DwmSetWindowAttribute( hwnd,
                                           NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE,
                                           ref enable,
                                           sizeof( int ) );
    }
    catch
    {
      /* unsupported on older Windows */
    }

    var border = 0x202020; // COLORREF (grey, so byte order is symmetric)
    try
    {
      NativeMethods.DwmSetWindowAttribute( hwnd,
                                           NativeMethods.DWMWA_BORDER_COLOR,
                                           ref border,
                                           sizeof( int ) );
    }
    catch
    {
      /* Win11 only */
    }
  }
}
