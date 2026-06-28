using System.Text;
using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

public class SecretsTests
{
  // Fake fixtures only — never put a real credential in a test.
  private const string FakeSecret = "test-secret-{!}value-123";
  private const string FakePass   = "correct horse battery staple";

  private static byte[] TestKey( string pass = FakePass )
  {
    byte[] salt = Encoding.UTF8.GetBytes( "0123456789abcdef" );
    return Secrets.DeriveKey( pass, salt, 50_000 );
  }

  [Fact]
  public void EncryptThenDecrypt_RoundTrips_WithSameKey()
  {
    byte[] key = TestKey();
    string sealed_ = Secrets.Encrypt( key, FakeSecret );

    Assert.True( Secrets.IsPassSealed( sealed_ ) );
    Assert.DoesNotContain( FakeSecret, sealed_ );      // not stored in the clear
    Assert.Equal( FakeSecret, Secrets.Decrypt( key, sealed_ ) );
  }

  [Fact]
  public void Decrypt_WithWrongPassphrase_Fails()
  {
    string sealed_ = Secrets.Encrypt( TestKey( "right" ), FakeSecret );
    Assert.ThrowsAny<System.Exception>( () => Secrets.Decrypt( TestKey( "wrong" ), sealed_ ) );
  }

  [Fact]
  public void ApplySecrets_SealsPlaintext_ThenDecryptsOnDemand()
  {
    byte[] key = TestKey();
    var btn = new ButtonDef { Secret = FakeSecret, Desc = "pswd" };
    var list = new List<ButtonDef> { btn };

    // First pass: plaintext is sealed (dirty). The value is never kept in memory — it's
    // decryptable on demand from the ciphertext.
    Assert.True( TabStore.ApplySecrets( list, key ) );
    Assert.True( Secrets.IsPassSealed( btn.Secret! ) );
    Assert.False( btn.Locked );
    Assert.Equal( FakeSecret, Secrets.Decrypt( key, btn.Secret! ) );

    // Second pass: already sealed → nothing to persist, still unlocked.
    Assert.False( TabStore.ApplySecrets( list, key ) );
    Assert.False( btn.Locked );
  }

  [Fact]
  public void ApplySecrets_MigratesLegacyDpapiSecret_ToPortableFormat()
  {
    byte[] key = TestKey();
    var btn = new ButtonDef { Secret = Secrets.DpapiSeal( FakeSecret ), Desc = "pswd" };
    var list = new List<ButtonDef> { btn };

    Assert.True( Secrets.IsDpapiSealed( btn.Secret! ) );

    Assert.True( TabStore.ApplySecrets( list, key ) );
    Assert.True( Secrets.IsPassSealed( btn.Secret! ) );          // now portable
    Assert.False( btn.Locked );
    Assert.Equal( FakeSecret, Secrets.Decrypt( key, btn.Secret! ) );
  }

  [Fact]
  public void ApplySecrets_FlagsLocked_WhenSealedWithADifferentKey()
  {
    // Sealed with one key, processed with another (the orphaned-secret case).
    var btn = new ButtonDef { Secret = Secrets.Encrypt( TestKey( "other-pass" ), FakeSecret ), Desc = "pswd" };
    var list = new List<ButtonDef> { btn };

    Assert.False( TabStore.ApplySecrets( list, TestKey() ) ); // already sealed → no rewrite
    Assert.True( btn.Locked );                                // but can't decrypt → locked
  }
}
