using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

public class SendParserTests
{
  private const ushort VK_BACK   = 0x08;
  private const ushort VK_TAB    = 0x09;
  private const ushort VK_RETURN = 0x0D;
  private const ushort VK_LEFT   = 0x25;

  private static (ushort Vk, char Ch)[] Parse( string s )
    => TextSender.ParseSends( s ).Select( k => ( k.Vk, k.Ch ) ).ToArray();

  [Fact]
  public void PlainText_IsUnicodeChars()
  {
    Assert.Equal( new[] { ((ushort)0, 'a'), ((ushort)0, 'b'), ((ushort)0, 'c') }, Parse( "abc" ) );
  }

  [Fact]
  public void BraceEscapes_AreLiteralCharacters()
  {
    // password-style escapes: {!} {#} {@}
    Assert.Equal( new[] { ((ushort)0, 'H'), ((ushort)0, '!'), ((ushort)0, 'i' ) }, Parse( "H{!}i" ) );
    Assert.Equal( new[] { ((ushort)0, '#'), ((ushort)0, '@') }, Parse( "{#}{@}" ) );
  }

  [Fact]
  public void Newline_And_EnterToken_BothBecomeReturn()
  {
    Assert.Equal( new[] { ((ushort)VK_RETURN, '\0') }, Parse( "\n" ) );
    Assert.Equal( new[] { ((ushort)VK_RETURN, '\0') }, Parse( "{Enter}" ) );
  }

  [Fact]
  public void BackspaceControlChar_BecomesBackspaceKey()
  {
    Assert.Equal(
      new[] { ((ushort)VK_BACK, '\0'), ((ushort)VK_BACK, '\0'), ((ushort)VK_BACK, '\0'), ((ushort)0, '.'), ((ushort)0, ' ') },
      Parse( "\b\b\b. " ) );
  }

  [Fact]
  public void NamedKeys_AndTrailingText()
  {
    Assert.Equal(
      new[] { ((ushort)VK_LEFT, '\0'), ((ushort)VK_LEFT, '\0'), ((ushort)0, ',' ), ((ushort)0, ' ') },
      Parse( "{Left}{Left}, " ) );
  }

  [Fact]
  public void TabControlChar_AndToken()
  {
    Assert.Equal( new[] { ((ushort)VK_TAB, '\0') }, Parse( "\t" ) );
    Assert.Equal( new[] { ((ushort)VK_TAB, '\0') }, Parse( "{Tab}" ) );
  }

  [Fact]
  public void UnknownToken_IsDropped()
  {
    Assert.Equal( new[] { ((ushort)0, 'x'), ((ushort)0, 'y') }, Parse( "x{Bogus}y" ) );
  }

  [Fact]
  public void CarriageReturn_IsIgnored()
  {
    Assert.Equal( new[] { ((ushort)VK_RETURN, '\0') }, Parse( "\r\n" ) );
  }
}
