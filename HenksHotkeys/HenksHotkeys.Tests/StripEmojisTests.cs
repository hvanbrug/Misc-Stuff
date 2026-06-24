using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

public class StripEmojisTests
{
  [Fact]
  public void TrailingEmojiBeforePunctuation_RemovedAndSpaceTidied()
  {
    Assert.Equal( "Thanks.", AppState.StripEmojis( "Thanks 😊." ) );
    Assert.Equal( "Thank you so much.", AppState.StripEmojis( "Thank you so much 🤗." ) );
  }

  [Fact]
  public void EmojiInMiddle_CollapsesDoubleSpace()
  {
    Assert.Equal( "a b", AppState.StripEmojis( "a 😀 b" ) );
  }

  [Fact]
  public void HeartWithVariationSelector_FullyRemoved()
  {
    // ❤️ = U+2764 (in 2600–27BF) + U+FE0F
    Assert.Equal( "", AppState.StripEmojis( "❤️" ) );
  }

  [Fact]
  public void PlainText_Unchanged()
  {
    Assert.Equal( "no emoji here", AppState.StripEmojis( "no emoji here" ) );
  }

  [Fact]
  public void ZwjSequence_Removed()
  {
    // family emoji (ZWJ sequence of code points all > U+1F000 plus ZWJ)
    Assert.Equal( "family", AppState.StripEmojis( "family 👨‍👩‍👧" ) );
  }
}
