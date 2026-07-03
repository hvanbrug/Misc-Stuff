using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HenksHotkeys.Emoji;

/// <summary>
/// Loads Twemoji PNG images for emoji characters from the assembly's embedded
/// resources (EmbeddedResource items named "twemoji.{stem}.png" in the csproj),
/// so there are no loose files at runtime. The grey backdrop is supplied by the
/// emoji button style, so the transparent PNG is used directly. Returns null when
/// no image exists, so the caller falls back to the emoji-character glyph.
/// </summary>
internal static class EmojiImageProvider
{
  private static readonly Assembly s_asm = Assembly.GetExecutingAssembly();
  private static readonly Dictionary<string, (ImageSource? Img, string Stem)> s_cache = new( StringComparer.Ordinal );
  private static HashSet<string>? s_stems; // available Twemoji stems (from the embedded resource names)

  public readonly record struct Result( ImageSource? Image, string Stem );

  /// <summary>The set of Twemoji stems we actually ship an image for (parsed once from the
  /// embedded resource names "twemoji.{stem}.png").</summary>
  private static HashSet<string> Stems()
  {
    if( s_stems is null )
    {
      s_stems = new HashSet<string>( StringComparer.Ordinal );
      foreach( string name in s_asm.GetManifestResourceNames() )
      {
        if( name.StartsWith( "twemoji.", StringComparison.Ordinal ) && name.EndsWith( ".png", StringComparison.Ordinal ) )
        {
          s_stems.Add( name[8..^4] ); // strip "twemoji." and ".png"
        }
      }
    }
    return s_stems;
  }

  /// <summary>True when a Twemoji image exists for <paramref name="stem"/> — used to decide whether
  /// a skin-toned variant is real before applying it.</summary>
  public static bool HasImage( string stem ) => stem.Length > 0 && Stems().Contains( stem );

  /// <summary>True when the emoji string <paramref name="ch"/> has a Twemoji image.</summary>
  public static bool HasImageFor( string ch ) => HasImage( ToTwemojiStem( ch ) );

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
  /// the resolved filename stem. <see cref="Result.Image"/> is null if no image exists.
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

    ImageSource? image = Decode( stem, pixelSize );
    s_cache[stem] = ( image, stem );
    return new Result( image, stem );
  }

  private static ImageSource? Decode( string stem, int pixelSize )
  {
    // GetManifestResourceStream returns null (no exception) when the emoji has no
    // embedded image, so the caller falls back to the glyph.
    using Stream? s = s_asm.GetManifestResourceStream( "twemoji." + stem + ".png" );
    if( s is null )
    {
      return null;
    }

    try
    {
      var bmp = new BitmapImage();
      bmp.BeginInit();
      bmp.CacheOption      = BitmapCacheOption.OnLoad;   // read fully now; stream can be disposed after
      bmp.CreateOptions    = BitmapCreateOptions.IgnoreColorProfile;
      bmp.DecodePixelWidth = Math.Max( 1, pixelSize );
      bmp.StreamSource     = s;
      bmp.EndInit();
      bmp.Freeze();
      return bmp;
    }
    catch
    {
      return null;
    }
  }
}
