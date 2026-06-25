using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
// Registry is read for the current theme; live changes are delivered to the
// main window via WM_SETTINGCHANGE ("ImmersiveColorSet"), which calls Refresh().

namespace NetworkShares;

/// <summary>
/// Drives a light / dark colour scheme by overwriting a set of named brush
/// resources at the application level. Controls reference these via
/// <c>DynamicResource</c>, so swapping the values re-themes the whole UI live.
/// The scheme follows the Windows "app" theme and updates when the user changes it.
/// </summary>
internal static class ThemeManager
{
  public static bool IsDark { get; private set; }

  /// <summary>Raised (on the UI thread) after the palette changes.</summary>
  public static event Action? ThemeChanged;

  // Soft, layered light scheme (the original look).
  private static readonly Dictionary<string, Color> Light = new()
  {
    ["WindowBg"]         = C( 0xF3, 0xF3, 0xF3 ),
    ["CardBg"]           = C( 0xFF, 0xFF, 0xFF ),
    ["CardBorder"]       = C( 0xE0, 0xE0, 0xE0 ),
    ["AccentBarBg"]      = C( 0xE8, 0xF0, 0xFE ),
    ["AccentBarBorder"]  = C( 0xC5, 0xD6, 0xF2 ),
    ["AccentText"]       = C( 0x5B, 0x6B, 0x8C ),
    ["TextPrimary"]      = C( 0x22, 0x22, 0x22 ),
    ["TextBody"]         = C( 0x44, 0x44, 0x44 ),
    ["TextSecondary"]    = C( 0x75, 0x75, 0x75 ),
    ["ControlBg"]        = C( 0xFF, 0xFF, 0xFF ),
    ["ControlBorder"]    = C( 0xC8, 0xC8, 0xC8 ),
    ["ControlHover"]     = C( 0xF0, 0xF0, 0xF0 ),
    ["ControlPressed"]   = C( 0xE4, 0xE4, 0xE4 ),
    ["InputBg"]          = C( 0xFF, 0xFF, 0xFF ),
    ["LogBg"]            = C( 0xFA, 0xFA, 0xFA ),
    ["ScrollThumb"]      = C( 0xC4, 0xC4, 0xC4 ),
    ["ScrollThumbHover"] = C( 0xA8, 0xA8, 0xA8 ),
  };

  // Graded dark scheme — layered greys (window < card < control) so depth still
  // reads, with a muted blue accent bar instead of pure black everywhere.
  private static readonly Dictionary<string, Color> Dark = new()
  {
    ["WindowBg"]         = C( 0x1E, 0x1E, 0x20 ),
    ["CardBg"]           = C( 0x2A, 0x2A, 0x2D ),
    ["CardBorder"]       = C( 0x3C, 0x3C, 0x40 ),
    ["AccentBarBg"]      = C( 0x26, 0x33, 0x49 ),
    ["AccentBarBorder"]  = C( 0x39, 0x4B, 0x68 ),
    ["AccentText"]       = C( 0x9D, 0xB4, 0xD8 ),
    ["TextPrimary"]      = C( 0xE8, 0xE8, 0xE8 ),
    ["TextBody"]         = C( 0xC4, 0xC4, 0xC4 ),
    ["TextSecondary"]    = C( 0x9A, 0x9A, 0x9E ),
    ["ControlBg"]        = C( 0x3A, 0x3A, 0x3E ),
    ["ControlBorder"]    = C( 0x55, 0x55, 0x5A ),
    ["ControlHover"]     = C( 0x46, 0x46, 0x4B ),
    ["ControlPressed"]   = C( 0x52, 0x52, 0x58 ),
    ["InputBg"]          = C( 0x33, 0x33, 0x37 ),
    ["LogBg"]            = C( 0x23, 0x23, 0x26 ),
    ["ScrollThumb"]      = C( 0x4A, 0x4A, 0x50 ),
    ["ScrollThumbHover"] = C( 0x60, 0x60, 0x68 ),
  };

  private static Color C( byte r, byte g, byte b ) => Color.FromRgb( r, g, b );

  /// <summary>Apply the current system theme. Call once at startup.</summary>
  public static void Initialize() => Apply( IsSystemDark() );

  /// <summary>Re-read the system theme and re-apply if it changed.</summary>
  public static void Refresh()
  {
    bool dark = IsSystemDark();
    if( dark != IsDark )
    {
      Apply( dark );
    }
  }

  public static void Apply( bool dark )
  {
    IsDark = dark;
    Dictionary<string, Color> map = dark ? Dark : Light;
    ResourceDictionary res = Application.Current.Resources;

    foreach( (string key, Color color) in map )
    {
      var brush = new SolidColorBrush( color );
      brush.Freeze();
      res[key] = brush;
    }

    ThemeChanged?.Invoke();
  }

  public static bool IsSystemDark()
  {
    try
    {
      using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize" );
      // AppsUseLightTheme: 0 = dark, 1 = light.
      if( key?.GetValue( "AppsUseLightTheme" ) is int light )
      {
        return light == 0;
      }
    }
    catch
    {
      // Registry unreadable — fall back to light.
    }
    return false;
  }
}
