using System.IO;
using System.Text;

namespace HenksHotkeys.Core;

/// <summary>
/// Caches the master secrets passphrase on this machine, DPAPI-protected, so it is
/// entered only once per machine. The cache is per-machine and never shared.
/// </summary>
internal static class PassphraseStore
{
  private static string Path => System.IO.Path.Combine(
    Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
    "HenksHotkeys", "passphrase.bin" );

  public static string? Load()
  {
    try
    {
      if( File.Exists( Path ) )
      {
        return Encoding.UTF8.GetString( Secrets.DpapiUnprotect( File.ReadAllBytes( Path ) ) );
      }
    }
    catch
    {
      // unreadable / different user → treat as not cached
    }
    return null;
  }

  public static void Save( string passphrase )
  {
    try
    {
      Directory.CreateDirectory( System.IO.Path.GetDirectoryName( Path )! );
      File.WriteAllBytes( Path, Secrets.DpapiProtect( Encoding.UTF8.GetBytes( passphrase ) ) );
    }
    catch
    {
      // a failed cache write just means we prompt again next time
    }
  }
}
