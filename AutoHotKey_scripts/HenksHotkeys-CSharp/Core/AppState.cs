using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HenksHotkeys.UI;

namespace HenksHotkeys.Core;

/// <summary>
/// App-wide mutable state and shared helpers, replacing the AHK globals from
/// UIConstants.ahk / Utilities.ahk (g_useClipSend, g_stripSendEmojis,
/// g_activeWindow, g_iniPath, g_uiTabs, g_hotkeyWnd).
/// </summary>
internal static class AppState
{
  public static bool   UseClipSend     { get; set; }
  public static bool   StripSendEmojis { get; set; }
  public static IntPtr ActiveWindow    { get; set; } = IntPtr.Zero;

  public static IniFile Ini { get; private set; } = null!;

  public static string BaseDir   => AppContext.BaseDirectory;
  public static string IniPath   => Path.Combine( BaseDir, "HenksHotkeys.ini" );
  public static string TwemojiDir => Path.Combine( BaseDir, "Images", "Twemoji" );

  public static HotkeyWindow Window { get; set; } = null!;

  public static List<TabModel> Tabs { get; } = new();

  /// <summary>(label, description) pairs for the ListHotkeys (^+a) view.</summary>
  public static List<(string Label, string Desc)> HotkeyHelp { get; } = new();

  public static void InitIni()
  {
    Ini = new IniFile( IniPath );
  }

  // ── Comments-tab emoji stripping (StripEmojis in Utilities.ahk) ──
  // Removes emoji code points then tidies the spaces left behind before any
  // trailing punctuation. The AHK pattern covers:
  //   U+1F000–U+1FFFF, U+2600–U+27BF, U+200D, U+FE0F, U+20E3.
  private static readonly Regex s_spaceBeforePunct = new( @"\s+([\.,;:!?])", RegexOptions.Compiled );
  private static readonly Regex s_multiSpace       = new( @"\s{2,}",          RegexOptions.Compiled );

  public static string StripEmojis( string text )
  {
    var sb = new StringBuilder( text.Length );
    foreach( Rune rune in text.EnumerateRunes() )
    {
      int v = rune.Value;
      bool isEmoji = ( v >= 0x1F000 && v <= 0x1FFFF ) ||
                     ( v >= 0x2600  && v <= 0x27BF  ) ||
                       v == 0x200D || v == 0xFE0F || v == 0x20E3;
      if( !isEmoji )
      {
        sb.Append( rune.ToString() );
      }
    }

    string result = sb.ToString();
    result = s_spaceBeforePunct.Replace( result, "$1" );
    result = s_multiSpace.Replace( result, " " );
    return result.Trim();
  }
}
