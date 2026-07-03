using System.IO;
using Newtonsoft.Json;

namespace HenksHotkeys.Core;

/// <summary>
/// Which Emojis-tab section headers are collapsed (#26) — a small persisted set of section names,
/// in <c>%LocalAppData%\HenksHotkeys\emoji-collapsed.json</c>. A collapsed section's buttons are
/// simply not built, so everything below flows up. Independent of the emoji catalog.
/// </summary>
internal static class EmojiSectionStore
{
  private static string Path =>
    System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                            "HenksHotkeys", "emoji-collapsed.json" );

  private static HashSet<string>? s_cache;

  /// <summary>Drop the cache so the next read re-loads the file (called on Reload Configuration).</summary>
  public static void Invalidate() => s_cache = null;

  private static HashSet<string> Load()
  {
    if( s_cache is not null )
    {
      return s_cache;
    }
    try
    {
      s_cache = File.Exists( Path )
        ? new HashSet<string>( JsonConvert.DeserializeObject<List<string>>( File.ReadAllText( Path ) ) ?? new(), StringComparer.Ordinal )
        : new HashSet<string>( StringComparer.Ordinal );
    }
    catch
    {
      s_cache = new HashSet<string>( StringComparer.Ordinal );
    }
    return s_cache;
  }

  public static bool IsCollapsed( string name ) => Load().Contains( name );

  /// <summary>Flip a section's collapsed state and persist. Returns the new state (true = collapsed).</summary>
  public static bool Toggle( string name )
  {
    HashSet<string> set = Load();
    bool collapsed = !set.Remove( name ) && set.Add( name );
    Save();
    return collapsed;
  }

  private static void Save()
  {
    try
    {
      Directory.CreateDirectory( System.IO.Path.GetDirectoryName( Path )! );
      File.WriteAllText( Path, JsonConvert.SerializeObject( s_cache, Formatting.Indented ) );
    }
    catch
    {
      // A failed write just means the collapse state isn't persisted; the in-memory set still updated.
    }
  }
}
