using System.IO;
using System.Reflection;
using System.Security.Cryptography;
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
  /// Decrypt the secret buttons for use and (re)seal any that need it: plaintext the
  /// user typed, or legacy DPAPI values being migrated to the portable format.
  /// Obtains the passphrase from the local cache or by prompting. Returns true when
  /// the file changed and should be rewritten.
  /// </summary>
  internal static bool ProcessSecrets( TabFile file )
  {
    List<ButtonDef> secrets = file.Tabs
      .Where( t => t.Rows is not null )
      .SelectMany( t => t.Rows! )
      .SelectMany( r => r.Buttons )
      .Where( b => !string.IsNullOrEmpty( b.Secret ) )
      .ToList();

    if( secrets.Count == 0 )
    {
      return false;
    }

    // Salt lives in the file (so it travels with it). Created in memory now; only
    // persisted once we actually seal something, so cancelling leaves the file as-is.
    file.Crypto ??= new CryptoHeader();
    if( string.IsNullOrEmpty( file.Crypto.Salt ) )
    {
      file.Crypto.Salt = Convert.ToBase64String( RandomNumberGenerator.GetBytes( 16 ) );
    }
    byte[] salt       = Convert.FromBase64String( file.Crypto.Salt );
    int    iterations = file.Crypto.Iterations > 0 ? file.Crypto.Iterations : Secrets.DefaultIterations;

    bool dirty = false;
    byte[]? key = ObtainKey( file.Crypto, salt, iterations, ref dirty );
    if( key is null )
    {
      return dirty; // no passphrase available / cancelled → leave secrets locked
    }

    dirty |= ApplySecrets( secrets, key );
    return dirty;
  }

  /// <summary>Decrypt / (re)seal each secret button with the given key. Testable.</summary>
  internal static bool ApplySecrets( List<ButtonDef> secrets, byte[] key )
  {
    bool dirty = false;
    foreach( ButtonDef b in secrets )
    {
      string s = b.Secret!;
      if( Secrets.IsPassSealed( s ) )
      {
        try   { b.Plain = Secrets.Decrypt( key, s ); }
        catch { b.Plain = ""; } // wrong key / corrupt
      }
      else if( Secrets.IsDpapiSealed( s ) )
      {
        // Legacy per-machine secret → migrate to the portable format (only works on
        // the machine that originally sealed it; elsewhere it just stays locked).
        try
        {
          string plain = Secrets.DpapiUnseal( s );
          b.Plain  = plain;
          b.Secret = Secrets.Encrypt( key, plain );
          dirty    = true;
        }
        catch { b.Plain = ""; }
      }
      else // plaintext the user typed in
      {
        b.Plain  = s;
        b.Secret = Secrets.Encrypt( key, s );
        dirty    = true;
      }
    }
    return dirty;
  }

  // Resolve the AES key from the cached passphrase or by prompting (up to a few
  // attempts). Returns null if there is no passphrase available (cancelled / no UI).
  private static byte[]? ObtainKey( CryptoHeader crypto, byte[] salt, int iterations, ref bool dirty )
  {
    bool creating = string.IsNullOrEmpty( crypto.Verifier );

    // Try the locally-cached passphrase first.
    string? cached = PassphraseStore.Load();
    if( cached is not null )
    {
      byte[] k = Secrets.DeriveKey( cached, salt, iterations );
      if( creating || Secrets.VerifyKey( k, crypto.Verifier! ) )
      {
        if( creating ) { crypto.Verifier = Secrets.Encrypt( k, Secrets.Verifier ); dirty = true; }
        return k;
      }
      // cached passphrase no longer matches this file → fall through to prompting
    }

    for( int attempt = 0; attempt < 3; attempt++ )
    {
      string? pass = PassphrasePrompt.Ask( creating, attempt > 0 );
      if( string.IsNullOrEmpty( pass ) )
      {
        return null; // cancelled or no UI hooked up
      }
      byte[] k = Secrets.DeriveKey( pass, salt, iterations );
      if( creating || Secrets.VerifyKey( k, crypto.Verifier! ) )
      {
        PassphraseStore.Save( pass );
        if( creating ) { crypto.Verifier = Secrets.Encrypt( k, Secrets.Verifier ); dirty = true; }
        return k;
      }
      // wrong passphrase → loop (retry flag set)
    }
    return null;
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
