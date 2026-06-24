using System.Text;

namespace HenksHotkeys.UI;

/// <summary>
/// Display-text helpers ported from TabPage.NormalizeDisplayText (UITabPage.ahk).
/// Replaces control characters with visible glyphs so button labels / tooltips
/// never contain raw newlines or tabs.
/// </summary>
internal static class UiText
{
  public static string NormalizeDisplayText( string text )
  {
    var sb = new StringBuilder( text.Length );
    foreach( char c in text )
    {
      sb.Append( c switch
      {
        '\n'     => '↵',
        '\r'     => '␍',
        '\t'     => '⇥',
        '\b'     => '⌫',
        '\a'     => '␇',
        '\f'     => '␌',
        '\v'     => '␋',
        '\x1B'   => '⎋',
        '\x7F'   => '⌦',
        _        => c
      } );
    }
    return sb.ToString();
  }
}
