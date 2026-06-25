using System.IO;
using System.Reflection;
using HenksHotkeys.Tabs;
using HenksHotkeys.UI;
using Newtonsoft.Json;

namespace HenksHotkeys.Core;

/// <summary>
/// Loads the tab/button content from %LocalAppData%\HenksHotkeys\tabs.json,
/// writing the embedded default there on first run. Data entries become
/// <see cref="DataTabModel"/>s; entries that name a built-in (Emojis / Tools)
/// become the corresponding code tab. Falls back to the embedded default if the
/// user's file is missing or unreadable.
/// </summary>
internal static class TabStore
{
  private const string ResourceName = "tabs.default.json";

  public static string Path => System.IO.Path.Combine(
    Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
    "HenksHotkeys", "tabs.json" );

  public static List<TabModel> Load()
  {
    string json = ReadOrSeed();
    TabFile? file = TryParse( json ) ?? TryParse( ReadDefault() );

    var tabs = new List<TabModel>();
    if( file is null )
    {
      return tabs;
    }

    foreach( TabEntry entry in file.Tabs )
    {
      TabModel? tab = Build( entry );
      if( tab is not null )
      {
        tabs.Add( tab );
      }
    }
    return tabs;
  }

  private static TabModel? Build( TabEntry entry )
  {
    if( !string.IsNullOrEmpty( entry.Builtin ) )
    {
      return entry.Builtin switch
      {
        "Emojis" => new EmojisTab(),
        "Tools"  => new ToolsTab(),
        _        => null, // unknown built-in name → skip
      };
    }
    return new DataTabModel( entry );
  }

  // Read the user's file, creating it from the embedded default the first time.
  private static string ReadOrSeed()
  {
    try
    {
      if( File.Exists( Path ) )
      {
        return File.ReadAllText( Path );
      }

      string seed = ReadDefault();
      Directory.CreateDirectory( System.IO.Path.GetDirectoryName( Path )! );
      File.WriteAllText( Path, seed );
      return seed;
    }
    catch
    {
      return ReadDefault();
    }
  }

  private static string ReadDefault()
  {
    using Stream? s = Assembly.GetExecutingAssembly().GetManifestResourceStream( ResourceName );
    if( s is null )
    {
      return "{\"tabs\":[]}";
    }
    using var reader = new StreamReader( s );
    return reader.ReadToEnd();
  }

  private static TabFile? TryParse( string json )
  {
    try   { return JsonConvert.DeserializeObject<TabFile>( json ); }
    catch { return null; }
  }
}
