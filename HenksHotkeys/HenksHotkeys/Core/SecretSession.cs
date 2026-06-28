namespace HenksHotkeys.Core;

/// <summary>
/// Reveals secret values on demand — at the moment they are sent — without keeping
/// anything sensitive around. Only the (non-secret) salt/iterations are held; the AES
/// key is re-derived from the locally-cached passphrase on every <see cref="Reveal"/>,
/// used once, and zeroed straight after. So neither the key nor the decrypted plaintext
/// is retained in memory between sends.
/// </summary>
internal static class SecretSession
{
  private static byte[]? s_salt;
  private static int     s_iterations;
  private static bool    s_available;

  /// <summary>Record the crypto parameters at load (these aren't secret) and whether a
  /// passphrase was validated, so sends can re-derive the key.</summary>
  public static void Configure( byte[] salt, int iterations, bool available )
  {
    s_salt       = salt;
    s_iterations = iterations;
    s_available  = available;
  }

  public static void Clear()
  {
    s_salt      = null;
    s_available = false;
  }

  /// <summary>True when a secret could be revealed (a passphrase was validated and the
  /// crypto parameters are known).</summary>
  public static bool Available => s_available && s_salt is not null;

  /// <summary>Decrypt a sealed value for immediate use, or null if unavailable / it can't
  /// be decrypted. The key is re-derived here and zeroed; the plaintext is the caller's
  /// to use and drop.</summary>
  public static string? Reveal( string? sealedValue )
  {
    if( !Available || string.IsNullOrEmpty( sealedValue ) || !Secrets.IsPassSealed( sealedValue ) )
    {
      return null;
    }

    string? pass = PassphraseStore.Load();
    if( pass is null )
    {
      return null;
    }

    byte[] key = Secrets.DeriveKey( pass, s_salt!, s_iterations );
    try     { return Secrets.Decrypt( key, sealedValue ); }
    catch   { return null; }
    finally { Array.Clear( key ); } // never keep the key
  }
}
