using System.Security.Cryptography;
using System.Text;

namespace HenksHotkeys.Core;

/// <summary>
/// Seals / unseals sensitive strings with Windows DPAPI (per-user). A sealed value
/// is "enc:" + base64 and can only be decrypted by the same Windows account on the
/// same machine, so secrets in tabs.json are not readable as plaintext on disk.
/// </summary>
internal static class Secrets
{
  private const string Prefix = "enc:";
  private static readonly byte[] Entropy = "HenksHotkeys.tabs.v1"u8.ToArray();

  public static bool IsSealed( string value ) => value.StartsWith( Prefix, StringComparison.Ordinal );

  public static string Seal( string plaintext )
  {
    byte[] cipher = ProtectedData.Protect( Encoding.UTF8.GetBytes( plaintext ),
                                           Entropy,
                                           DataProtectionScope.CurrentUser );
    return Prefix + Convert.ToBase64String( cipher );
  }

  public static string Unseal( string sealedValue )
  {
    byte[] cipher = Convert.FromBase64String( sealedValue[Prefix.Length..] );
    byte[] data   = ProtectedData.Unprotect( cipher, Entropy, DataProtectionScope.CurrentUser );
    return Encoding.UTF8.GetString( data );
  }
}
