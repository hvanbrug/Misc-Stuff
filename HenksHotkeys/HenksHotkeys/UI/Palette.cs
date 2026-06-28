using System.Windows;
using System.Windows.Media;

namespace HenksHotkeys.UI;

/// <summary>
/// The single light/dark colour table for the whole app, matching the layered
/// NetworkShares look (window &lt; card &lt; control greys, a muted accent, a faint
/// scrollbar). <see cref="Install"/> publishes the current mode's brushes into the
/// application resources under these names so the App.xaml styles resolve them via
/// <c>DynamicResource</c>; code pulls a brush with <see cref="Brush"/>.
/// </summary>
internal static class Palette
{
  private static Color C( byte r, byte g, byte b ) => Color.FromRgb( r, g, b );

  private static readonly Dictionary<string, Color> Light = new()
  {
    ["WindowBg"]         = C( 0xF3, 0xF3, 0xF3 ),
    ["CardBg"]           = C( 0xFF, 0xFF, 0xFF ),
    ["CardBorder"]       = C( 0xD8, 0xD8, 0xD8 ),
    ["AccentBarBg"]      = C( 0xE8, 0xF0, 0xFE ),
    ["AccentBarBorder"]  = C( 0xC5, 0xD6, 0xF2 ),
    ["AccentText"]       = C( 0x42, 0x55, 0x80 ),
    ["TextPrimary"]      = C( 0x22, 0x22, 0x22 ),
    ["TextBody"]         = C( 0x44, 0x44, 0x44 ),
    ["TextSecondary"]    = C( 0x75, 0x75, 0x75 ),
    ["ControlBg"]        = C( 0xFF, 0xFF, 0xFF ),
    ["ControlBorder"]    = C( 0xC8, 0xC8, 0xC8 ),
    ["ControlHover"]     = C( 0xEC, 0xF1, 0xFB ),
    ["ControlPressed"]   = C( 0xDC, 0xE6, 0xF7 ),
    ["InputBg"]          = C( 0xFF, 0xFF, 0xFF ),
    ["EmojiBg"]          = C( 0xF6, 0xF6, 0xF6 ),
    ["SwitchOn"]         = C( 0x2F, 0x6F, 0xD6 ),
    ["ScrollThumb"]      = C( 0xC4, 0xC4, 0xC4 ),
    ["ScrollThumbHover"] = C( 0xA8, 0xA8, 0xA8 ),
  };

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
    ["EmojiBg"]          = C( 0x44, 0x44, 0x4A ),
    ["SwitchOn"]         = C( 0x4C, 0x8B, 0xF5 ),
    ["ScrollThumb"]      = C( 0x4A, 0x4A, 0x50 ),
    ["ScrollThumbHover"] = C( 0x60, 0x60, 0x68 ),
  };

  private static Dictionary<string, Color> s_current = Dark;

  /// <summary>Publish the chosen mode's brushes into the app resources (by name).</summary>
  public static void Install( ResourceDictionary res, bool dark )
  {
    s_current = dark ? Dark : Light;
    foreach( (string key, Color color) in s_current )
    {
      var brush = new SolidColorBrush( color );
      brush.Freeze();
      res[key] = brush;
    }
  }

  /// <summary>A frozen brush for the current mode (for code-built controls).</summary>
  public static Brush Brush( string key )
  {
    if( Application.Current?.Resources[key] is Brush b )
    {
      return b;
    }
    var fallback = new SolidColorBrush( s_current.TryGetValue( key, out Color c ) ? c : Colors.Magenta );
    fallback.Freeze();
    return fallback;
  }

  public static Color Colour( string key ) => s_current.TryGetValue( key, out Color c ) ? c : Colors.Magenta;
}
