using HenksHotkeys.Native;

namespace HenksHotkeys.Core;

/// <summary>
/// Parses AutoHotkey-style hotkey strings ("^+a", "!+1", "#!1", "^+#F9",
/// "^!+6", "^!v") into RegisterHotKey modifiers and a virtual-key code, and
/// produces the human-readable label used in tooltips (HotkeyLabel in
/// Utilities.ahk: ^→Ctrl-, +→Shift-, #→Win-, !→Alt-).
/// </summary>
internal static class HotkeyParser
{
  public readonly record struct Parsed( uint Modifiers, uint VirtualKey );

  public static Parsed? Parse( string hotkey )
  {
    if( string.IsNullOrEmpty( hotkey ) )
    {
      return null;
    }

    uint mods = 0;
    int  i    = 0;
    while( i < hotkey.Length )
    {
      char c = hotkey[i];
      if( c == '^' ) { mods |= NativeMethods.MOD_CONTROL; ++i; continue; }
      if( c == '+' ) { mods |= NativeMethods.MOD_SHIFT;   ++i; continue; }
      if( c == '#' ) { mods |= NativeMethods.MOD_WIN;     ++i; continue; }
      if( c == '!' ) { mods |= NativeMethods.MOD_ALT;     ++i; continue; }
      break;
    }

    string key = hotkey[i..];
    if( key.Length == 0 )
    {
      return null;
    }

    uint vk;
    if( (key[0] == 'F' || key[0] == 'f') && key.Length > 1 &&
        int.TryParse( key.AsSpan( 1 ), out int fnum ) && fnum >= 1 && fnum <= 24 )
    {
      vk = (uint)( 0x70 + ( fnum - 1 ) ); // VK_F1 = 0x70
    }
    else if( key.Length == 1 )
    {
      char k = char.ToUpperInvariant( key[0] );
      vk = k; // letters and digits map directly to their VK code
    }
    else
    {
      return null; // unsupported key name
    }

    return new Parsed( mods, vk );
  }

  /// <summary>Split an AHK hotkey into its modifier flags and the trailing key, so the
  /// editor can show modifiers as toggle buttons and the key in its own box.</summary>
  public static void Split( string? hotkey, out bool ctrl, out bool alt, out bool win, out bool shift, out string key )
  {
    ctrl = alt = win = shift = false;
    key  = "";
    if( string.IsNullOrEmpty( hotkey ) )
    {
      return;
    }
    int i = 0;
    for( ; i < hotkey.Length; i++ )
    {
      switch( hotkey[i] )
      {
        case '^': ctrl  = true; continue;
        case '!': alt   = true; continue;
        case '#': win   = true; continue;
        case '+': shift = true; continue;
      }
      break;
    }
    key = hotkey[i..].Trim();
  }

  /// <summary>Build an AHK hotkey from modifier flags + a key (in the fixed order
  /// Ctrl/Alt/Win/Shift). Empty key → empty string (no hotkey).</summary>
  public static string Compose( bool ctrl, bool alt, bool win, bool shift, string? key )
  {
    key = key?.Trim() ?? "";
    if( key.Length == 0 )
    {
      return "";
    }
    var sb = new System.Text.StringBuilder();
    if( ctrl )  sb.Append( '^' );
    if( alt )   sb.Append( '!' );
    if( win )   sb.Append( '#' );
    if( shift ) sb.Append( '+' );
    return sb.Append( key ).ToString();
  }

  public static string Label( string hotkey )
  {
    string s = hotkey.ToUpperInvariant();
    s = s.Replace( "^", "Ctrl-" )
         .Replace( "+", "Shift-" )
         .Replace( "#", "Win-" )
         .Replace( "!", "Alt-" );
    return s;
  }
}
