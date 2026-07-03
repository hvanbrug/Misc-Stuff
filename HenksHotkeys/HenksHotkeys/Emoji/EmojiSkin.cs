using System.Text;

namespace HenksHotkeys.Emoji;

/// <summary>
/// Emoji skin-tone application (#27). One global tone is chosen; a toneable emoji is re-tinted by
/// inserting the tone modifier after its base character — but only when Twemoji actually ships an
/// image for the toned form, so families/couples and non-toneable emoji safely stay yellow.
/// </summary>
internal static class EmojiSkin
{
  private const char VS16 = '️'; // variation selector-16 (the tone modifier supersedes it)

  /// <summary>The selectable tones: modifier codepoint ("" = default/yellow) and a label.</summary>
  public static readonly (string Hex, string Label)[] Tones =
  {
    ( "",       "Default"      ),
    ( "1f3fb",  "Light"        ),
    ( "1f3fc",  "Medium-Light" ),
    ( "1f3fd",  "Medium"       ),
    ( "1f3fe",  "Medium-Dark"  ),
    ( "1f3ff",  "Dark"         ),
  };

  /// <summary>The raised-hand swatch for a tone, used by the picker (yellow for "").</summary>
  public static string Swatch( string hex ) => hex.Length == 0 ? "✋" : ApplyRaw( "✋", hex );

  /// <summary>Re-tint <paramref name="emoji"/> to <paramref name="toneHex"/> if a toned image
  /// exists; otherwise return it unchanged. <paramref name="toneHex"/> "" = default/yellow.</summary>
  public static string Apply( string emoji, string toneHex )
  {
    if( toneHex.Length == 0 || emoji.Length == 0 )
    {
      return emoji;
    }
    string toned = ApplyRaw( emoji, toneHex );
    return EmojiImageProvider.HasImageFor( toned ) ? toned : emoji;
  }

  // Insert the tone modifier after the base (first) codepoint, dropping a VS16 that immediately
  // follows it (the modifier fully-qualifies the sequence, so VS16 is not needed there).
  private static string ApplyRaw( string emoji, string toneHex )
  {
    int    baseN = char.IsHighSurrogate( emoji[0] ) ? 2 : 1; // UTF-16 length of the base rune
    string rest  = emoji[baseN..];
    if( rest.Length > 0 && rest[0] == VS16 )
    {
      rest = rest[1..];
    }
    string tone = char.ConvertFromUtf32( Convert.ToInt32( toneHex, 16 ) );
    return new StringBuilder( emoji.Length + 2 ).Append( emoji, 0, baseN ).Append( tone ).Append( rest ).ToString();
  }
}
