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

  public static string Path
  {
    get => System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                                   "HenksHotkeys", "tabs.json" );
  }

  private static readonly JsonSerializerSettings JsonSettings = new()
  {
    Formatting           = Formatting.Indented,
    NullValueHandling    = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore,
  };

  /// <summary>False when the user's tabs.json failed to parse and the embedded
  /// default was used instead (so a reload can warn about a bad edit).</summary>
  public static bool LastParseOk { get; private set; } = true;

  public static List<TabModel> Load()
  {
    string json = ReadOrSeed();
    TabFile? file = TryParse( json );
    LastParseOk = file is not null;
    file ??= TryParse( ReadDefault() );

    var tabs = new List<TabModel>();
    if( file is null )
    {
      return tabs;
    }

    // Encrypt any plaintext secrets in place and persist; decrypt the rest for use.
    if( ProcessSecrets( file ) )
    {
      TrySave( file );
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

  /// <summary>
  /// For every secret button: decrypt sealed values into <see cref="ButtonDef.Plain"/>,
  /// or seal a freshly-typed plaintext value (returning true so the file is rewritten).
  /// </summary>
  internal static bool ProcessSecrets( TabFile file )
  {
    bool dirty = false;
    foreach( TabEntry tab in file.Tabs )
    {
      if( tab.Rows is null )
      {
        continue;
      }
      foreach( RowDef row in tab.Rows )
      {
        foreach( ButtonDef b in row.Buttons )
        {
          if( string.IsNullOrEmpty( b.Secret ) )
          {
            continue;
          }
          if( Secrets.IsSealed( b.Secret ) )
          {
            try
            {
              b.Plain = Secrets.Unseal( b.Secret );
            }
            catch
            {
              // sealed by a different user / machine
              b.Plain = "";
            }
          }
          else
          {
            b.Plain = b.Secret; // use the typed plaintext now…
            try
            {
              b.Secret = Secrets.Seal( b.Plain ); dirty = true; // …and seal it on disk
            }
            catch
            {
              // DPAPI unavailable → leave as-is
            }
          }
        }
      }
    }
    return dirty;
  }

  private static void TrySave( TabFile file )
  {
    try
    {
      Directory.CreateDirectory( System.IO.Path.GetDirectoryName( Path )! );
      File.WriteAllText( Path, JsonConvert.SerializeObject( file, JsonSettings ) );
    }
    catch
    {
      // a failed rewrite must never crash startup
    }
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
