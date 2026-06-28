using System.Security.Cryptography;
using System.Text;

namespace HenksHotkeys.Core;

/// <summary>
/// Encrypts sensitive strings so they can be shared between machines. Secrets are
/// AES-GCM encrypted with a key derived (PBKDF2) from a master passphrase plus a
/// per-file salt stored in tabs.json — so the same passphrase decrypts the same
/// file on any machine ("penc:" prefix). The legacy per-machine DPAPI format
/// ("enc:") is still read so existing secrets can be migrated, and DPAPI is reused
/// to cache the passphrase locally.
/// </summary>
internal static class Secrets
{
  public const string PassPrefix  = "penc:"; // passphrase / AES-GCM — portable
  public const string DpapiPrefix = "enc:";  // legacy DPAPI — per machine

  public const int    DefaultIterations = 200_000;
  public const string Verifier          = "HenksHotkeys.secrets.v2";

  private static readonly byte[] DpapiEntropy = "HenksHotkeys.tabs.v1"u8.ToArray();

  public static bool IsPassSealed( string v )  => v.StartsWith( PassPrefix,  StringComparison.Ordinal );
  public static bool IsDpapiSealed( string v ) => v.StartsWith( DpapiPrefix, StringComparison.Ordinal );

  // ── Passphrase-derived AES-GCM (portable) ────────────────────────
  public static byte[] DeriveKey( string passphrase, byte[] salt, int iterations )
  {
    using var kdf = new Rfc2898DeriveBytes( passphrase, salt, iterations, HashAlgorithmName.SHA256 );
    return kdf.GetBytes( 32 );
  }

  public static string Encrypt( byte[] key, string plaintext )
  {
    byte[] nonce = RandomNumberGenerator.GetBytes( 12 );
    byte[] pt    = Encoding.UTF8.GetBytes( plaintext );
    byte[] ct    = new byte[pt.Length];
    byte[] tag   = new byte[16];
    using( var gcm = new AesGcm( key, 16 ) )
    {
      gcm.Encrypt( nonce, pt, ct, tag );
    }
    byte[] blob = new byte[12 + 16 + ct.Length];
    Buffer.BlockCopy( nonce, 0, blob, 0,  12 );
    Buffer.BlockCopy( tag,   0, blob, 12, 16 );
    Buffer.BlockCopy( ct,    0, blob, 28, ct.Length );
    return PassPrefix + Convert.ToBase64String( blob );
  }

  /// <summary>Decrypt a "penc:" value. Throws if the key is wrong or data corrupt.</summary>
  public static string Decrypt( byte[] key, string sealedValue )
  {
    byte[] blob  = Convert.FromBase64String( sealedValue[PassPrefix.Length..] );
    byte[] nonce = blob[..12];
    byte[] tag   = blob[12..28];
    byte[] ct    = blob[28..];
    byte[] pt    = new byte[ct.Length];
    using( var gcm = new AesGcm( key, 16 ) )
    {
      gcm.Decrypt( nonce, ct, tag, pt );
    }
    return Encoding.UTF8.GetString( pt );
  }

  /// <summary>True if <paramref name="key"/> correctly decrypts the stored verifier.</summary>
  public static bool VerifyKey( byte[] key, string verifier )
  {
    try   { return Decrypt( key, verifier ) == Verifier; }
    catch { return false; }
  }

  /// <summary>True if <paramref name="key"/> can decrypt the value, *without* materialising
  /// the plaintext as a string (the bytes are zeroed immediately). Used to flag locked
  /// secrets without keeping them in memory.</summary>
  public static bool CanDecrypt( byte[] key, string sealedValue )
  {
    byte[]? blob = null;
    byte[]? pt   = null;
    try
    {
      blob = Convert.FromBase64String( sealedValue[PassPrefix.Length..] );
      byte[] nonce = blob[..12];
      byte[] tag   = blob[12..28];
      byte[] ct    = blob[28..];
      pt = new byte[ct.Length];
      using var gcm = new AesGcm( key, 16 );
      gcm.Decrypt( nonce, ct, tag, pt );
      return true;
    }
    catch
    {
      return false;
    }
    finally
    {
      if( pt is not null ) Array.Clear( pt );
    }
  }

  // ── DPAPI (legacy-secret migration + local passphrase cache) ─────
  public static string DpapiUnseal( string sealedValue )
  {
    byte[] cipher = Convert.FromBase64String( sealedValue[DpapiPrefix.Length..] );
    byte[] data   = ProtectedData.Unprotect( cipher, DpapiEntropy, DataProtectionScope.CurrentUser );
    return Encoding.UTF8.GetString( data );
  }

  public static string DpapiSeal( string plaintext )
  {
    byte[] cipher = ProtectedData.Protect( Encoding.UTF8.GetBytes( plaintext ),
                                           DpapiEntropy, DataProtectionScope.CurrentUser );
    return DpapiPrefix + Convert.ToBase64String( cipher );
  }

  public static byte[] DpapiProtect( byte[] data )
    => ProtectedData.Protect( data, DpapiEntropy, DataProtectionScope.CurrentUser );

  public static byte[] DpapiUnprotect( byte[] data )
    => ProtectedData.Unprotect( data, DpapiEntropy, DataProtectionScope.CurrentUser );
}
