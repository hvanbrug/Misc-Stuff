using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using HenksHotkeys.Core;
using HenksHotkeys.UI;

namespace HenksHotkeys.Emoji;

/// <summary>
/// Loads Twemoji PNG images for emoji characters and composites them onto a grey
/// backdrop, reproducing EmojiSupport.ahk (debug path: Images\Twemoji\{stem}.png).
/// Returns null when no matching PNG exists, so the caller falls back to the
/// emoji-character text label.
/// </summary>
internal static class EmojiImageProvider
{
  private static readonly Dictionary<string, (Bitmap? Bmp, string Stem)> s_cache = new( StringComparer.Ordinal );

  public readonly record struct Result( Bitmap? Image, string Stem );

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
  /// Returns a composited bitmap of the given pixel size for the emoji, plus the
  /// resolved filename stem. <see cref="Result.Image"/> is null if no PNG exists.
  /// </summary>
  public static Result Get( string ch, int pixelSize )
  {
    string stem = ToTwemojiStem( ch );
    if( stem.Length == 0 )
    {
      return new Result( null, "" );
    }

    string cacheKey = stem + "@" + pixelSize;
    if( s_cache.TryGetValue( cacheKey, out var hit ) )
    {
      return new Result( hit.Bmp, hit.Stem );
    }

    Bitmap? composed = null;
    string  path     = Path.Combine( AppState.TwemojiDir, stem + ".png" );
    if( File.Exists( path ) )
    {
      try
      {
        using var src = (Bitmap)Image.FromFile( path );
        composed = new Bitmap( pixelSize, pixelSize );
        using Graphics g = Graphics.FromImage( composed );
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.Clear( Theme.EmojiBackdrop );
        g.DrawImage( src, new Rectangle( 0, 0, pixelSize, pixelSize ) );
      }
      catch
      {
        composed = null;
      }
    }

    s_cache[cacheKey] = ( composed, stem );
    return new Result( composed, stem );
  }
}
