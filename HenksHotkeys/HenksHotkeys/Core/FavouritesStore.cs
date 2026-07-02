using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace HenksHotkeys.Core;

/// <summary>One favourited emoji. <see cref="Unicode"/> is the authoritative value — the full
/// codepoint sequence as text (e.g. "U+2764 U+FE0F U+200D U+1F525"), including any variation /
/// ZWJ / skin-tone codes — so the file is explicit and hard to mangle. <see cref="Char"/> is kept
/// only as a visual aid; the emoji actually sent/shown is decoded from <see cref="Unicode"/>.</summary>
internal sealed class Favourite
{
  [JsonProperty( "unicode" )] public string Unicode { get; set; } = "";
  [JsonProperty( "char" )]    public string Char    { get; set; } = "";

  /// <summary>The emoji string this favourite resolves to: decoded from <see cref="Unicode"/>,
  /// falling back to <see cref="Char"/> for a legacy/hand-written entry with no codepoints.</summary>
  [JsonIgnore]
  public string Emoji => FavouritesStore.Decode( Unicode ) is { Length: > 0 } d ? d : Char;
}

/// <summary>
/// The user's Emojis-tab favourites (issue #13) — a small ordered list persisted to
/// <c>%LocalAppData%\HenksHotkeys\favourites.json</c>, independent of the emoji catalog (which the
/// user can't edit). Each entry stores the emoji's codepoints (authoritative) plus the glyph as a
/// visual aid. The Emojis tab renders these at the top under a "Favourites" heading; the right-click
/// menu marks / unmarks them and a drag reorders them. With no file the tab starts with no
/// favourites; the file is created on the first mark. A file from the first favourites build
/// (glyph + description) is migrated to codepoints on load.
/// </summary>
internal static class FavouritesStore
{
  private static string Path =>
    System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                            "HenksHotkeys", "favourites.json" );

  private static List<Favourite>? s_cache;

  /// <summary>Drop the cached list so the next <see cref="Load"/> re-reads the file — called on
  /// Reload Configuration so a hand-edited favourites.json takes effect.</summary>
  public static void Invalidate() => s_cache = null;

  public static IReadOnlyList<Favourite> Load()
  {
    if( s_cache is not null )
    {
      return s_cache;
    }
    try
    {
      if( File.Exists( Path ) )
      {
        List<Favourite> list = JsonConvert.DeserializeObject<List<Favourite>>( File.ReadAllText( Path ) ) ?? new();
        bool migrated = Migrate( list );  // upgrade an old glyph/desc file to codepoints in place
        s_cache = list;
        if( migrated ) Save();            // rewrite it cleanly (order preserved, desc dropped)
      }
      else
      {
        s_cache = new();                  // no file → start with no favourites (created on first Add)
      }
    }
    catch
    {
      s_cache = new();
    }
    return s_cache;
  }

  /// <summary>Upgrade a legacy entry (glyph-only, from the first favourites build, possibly with a
  /// now-unused "desc") to carry its codepoints. Order is untouched. Returns true if it changed
  /// anything. The redundant "desc" simply isn't a model field, so it drops out on the next save.</summary>
  internal static bool Migrate( List<Favourite> list )
  {
    bool changed = false;
    foreach( Favourite f in list )
    {
      if( string.IsNullOrWhiteSpace( f.Unicode ) && !string.IsNullOrEmpty( f.Char ) )
      {
        f.Unicode = Encode( f.Char );
        changed   = true;
      }
    }
    return changed;
  }

  /// <summary>Append an emoji (given as its glyph) to the favourites, storing its codepoints.
  /// No-op if already present. Returns true if it was added (so the caller reloads).</summary>
  public static bool Add( string emoji )
  {
    List<Favourite> list = Mutable();
    if( string.IsNullOrEmpty( emoji ) || list.Any( f => f.Emoji == emoji ) )
    {
      return false;
    }
    list.Add( Make( emoji ) );
    return Save();
  }

  public static bool Remove( string emoji )
  {
    List<Favourite> list = Mutable();
    return list.RemoveAll( f => f.Emoji == emoji ) > 0 && Save();
  }

  /// <summary>Move the favourite <paramref name="emoji"/> so it lands before
  /// <paramref name="insertBeforeIndex"/> (an index into the current list). Returns false on no-op.</summary>
  public static bool Reorder( string emoji, int insertBeforeIndex )
    => ReorderInList( Mutable(), emoji, insertBeforeIndex ) && Save();

  /// <summary>Pure list-move: reposition the favourite for <paramref name="emoji"/> so it lands
  /// before <paramref name="insertBeforeIndex"/> (an index into the current list). Returns false on
  /// no-op. Factored out so the index math is unit-testable without touching the favourites file.</summary>
  internal static bool ReorderInList( List<Favourite> list, string emoji, int insertBeforeIndex )
  {
    int from = list.FindIndex( f => f.Emoji == emoji );
    if( from < 0 )
    {
      return false;
    }
    int to = insertBeforeIndex > from ? insertBeforeIndex - 1 : insertBeforeIndex;
    to = Math.Clamp( to, 0, list.Count - 1 );
    if( to == from )
    {
      return false;
    }
    Favourite f2 = list[from];
    list.RemoveAt( from );
    list.Insert( to, f2 );
    return true;
  }

  // ── Codepoint text <-> emoji string ──────────────────────────────
  /// <summary>The full codepoint sequence of an emoji as space-separated "U+XXXX" tokens.</summary>
  public static string Encode( string emoji )
  {
    if( string.IsNullOrEmpty( emoji ) )
    {
      return "";
    }
    var parts = new List<string>();
    foreach( Rune r in emoji.EnumerateRunes() )
    {
      parts.Add( "U+" + r.Value.ToString( "X4", CultureInfo.InvariantCulture ) );
    }
    return string.Join( " ", parts );
  }

  /// <summary>Rebuild an emoji string from a codepoint list. Tolerant: accepts "U+"/"0x"/bare hex,
  /// separated by spaces / commas / semicolons; ignores anything unparseable.</summary>
  public static string Decode( string unicode )
  {
    if( string.IsNullOrWhiteSpace( unicode ) )
    {
      return "";
    }
    var sb = new StringBuilder();
    foreach( string tok in unicode.Split( new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries ) )
    {
      string h = tok.Trim();
      if( h.StartsWith( "U+", StringComparison.OrdinalIgnoreCase ) || h.StartsWith( "0x", StringComparison.OrdinalIgnoreCase ) )
      {
        h = h[2..];
      }
      if( int.TryParse( h, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cp ) && Rune.IsValid( cp ) )
      {
        sb.Append( new Rune( cp ).ToString() );
      }
    }
    return sb.ToString();
  }

  private static Favourite Make( string emoji ) => new() { Char = emoji, Unicode = Encode( emoji ) };

  private static List<Favourite> Mutable()
  {
    Load();
    return s_cache!;
  }


  private static bool Save()
  {
    try
    {
      Directory.CreateDirectory( System.IO.Path.GetDirectoryName( Path )! );
      File.WriteAllText( Path, JsonConvert.SerializeObject( s_cache, Formatting.Indented ) );
    }
    catch
    {
      // A failed write just means the change isn't persisted; the in-memory list still updated.
    }
    return true;
  }
}
