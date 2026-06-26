using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

public class SecretsTests
{
  // Fake fixtures only — never put a real credential in a test.
  private const string FakeSecret = "test-secret-{!}value-123";

  [Fact]
  public void SealThenUnseal_RoundTrips()
  {
    string sealed_ = Secrets.Seal( FakeSecret );

    Assert.True( Secrets.IsSealed( sealed_ ) );
    Assert.DoesNotContain( FakeSecret, sealed_ );     // not stored in the clear
    Assert.Equal( FakeSecret, Secrets.Unseal( sealed_ ) );
  }

  [Fact]
  public void ProcessSecrets_SealsPlaintext_AndDecryptsForUse()
  {
    var file = new TabFile
    {
      Tabs =
      {
        new TabEntry
        {
          Name    = "Sensitive",
          Columns = 1,
          Rows    = new()
          {
            new RowDef { Buttons = { new ButtonDef { Secret = FakeSecret, Desc = "pswd" } } },
          },
        },
      },
    };

    ButtonDef btn = file.Tabs[0].Rows![0].Buttons[0];

    // First pass: plaintext gets sealed (dirty) and decrypted into Plain.
    Assert.True( TabStore.ProcessSecrets( file ) );
    Assert.True( Secrets.IsSealed( btn.Secret! ) );
    Assert.Equal( FakeSecret, btn.Plain );

    // Second pass: already sealed, so nothing to persist, still decrypts.
    Assert.False( TabStore.ProcessSecrets( file ) );
    Assert.Equal( FakeSecret, btn.Plain );
  }
}
