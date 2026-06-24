using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HenksHotkeys.Core;

namespace HenksHotkeys.Emoji;

/// <summary>
/// Loads Twemoji PNG images for emoji characters as WPF <see cref="ImageSource"/>s
/// (EmojiSupport.ahk debug path: Images\Twemoji\{stem}.png). The grey backdrop the
/// original composited in is now supplied by the emoji button style, so the
/// transparent PNG can be used directly. Returns null when no PNG exists, so the
/// caller falls back to the emoji-character glyph.
/// </summary>
internal static class EmojiImageProvider
{
  private static readonly Dictionary<string, (ImageSource? Img, string Stem)> s_cache = new( StringComparer.Ordinal );

  public readonly record struct Result( ImageSource? Image, string Stem );

  /// <summary>
  /// Twemoji filename stem for an emoji string (e.g. "😀" → "1f600",
  /// "👨‍👩‍👧" → "1f468-200d-1f469-200d-1f467"). FE0F is stripped for standalone
  /// emoji but kept inside ZWJ sequences, matching Twemoji's filenames.
  /// </summary>
  public static string ToTwemojiStem( string ch )
  {
    var parts  = new List<string>();
    bool hasZwj = ch.Contains( '‍' );

    int i = 0;
    while( i < ch.Length )
    {
      int cp = ch[i];
      if( char.IsHighSurrogate( ch[i] ) && i + 1 < ch.Length && char.IsLowSurrogate( ch[i + 1] ) )
      {
        cp = char.ConvertToUtf32( ch[i], ch[i + 1] );
        i++;
      }

      if( hasZwj || cp != 0xFE0F )
      {
        parts.Add( cp.ToString( "x" ) );
      }
      i++;
    }

    return parts.Count == 0 ? "" : string.Join( "-", parts );
  }

  /// <summary>
  /// Returns the emoji image (decoded to roughly <paramref name="pixelSize"/>) plus
  /// the resolved filename stem. <see cref="Result.Image"/> is null if no PNG exists.
  /// </summary>
  public static Result Get( string ch, int pixelSize )
  {
    string stem = ToTwemojiStem( ch );
    if( stem.Length == 0 )
    {
      return new Result( null, "" );
    }

    if( s_cache.TryGetValue( stem, out var hit ) )
    {
      return new Result( hit.Img, hit.Stem );
    }

    ImageSource? image = null;
    string path = Path.Combine( AppState.TwemojiDir, stem + ".png" );
    if( File.Exists( path ) )
    {
      try
      {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption       = BitmapCacheOption.OnLoad;
        bmp.CreateOptions     = BitmapCreateOptions.IgnoreColorProfile;
        bmp.DecodePixelWidth  = Math.Max( 1, pixelSize );
        bmp.UriSource         = new Uri( path, UriKind.Absolute );
        bmp.EndInit();
        bmp.Freeze();
        image = bmp;
      }
      catch
      {
        image = null;
      }
    }

    s_cache[stem] = ( image, stem );
    return new Result( image, stem );
  }
}
