using HenksHotkeys.Emoji;
using Xunit;

namespace HenksHotkeys.Tests;

public class TwemojiStemTests
{
  [Fact]
  public void SimpleEmoji_SurrogatePair()
  {
    Assert.Equal( "1f600", EmojiImageProvider.ToTwemojiStem( "😀" ) );
  }

  [Fact]
  public void StandaloneEmoji_StripsVariationSelector()
  {
    // ❤️ → "2764" (FE0F dropped for a standalone emoji)
    Assert.Equal( "2764", EmojiImageProvider.ToTwemojiStem( "❤️" ) );
  }

  [Fact]
  public void ZwjSequence_KeepsVariationSelector()
  {
    // ❤️‍🔥 = 2764 FE0F 200D 1F525; inside a ZWJ sequence FE0F is retained
    Assert.Equal( "2764-fe0f-200d-1f525", EmojiImageProvider.ToTwemojiStem( "❤️‍🔥" ) );
  }

  [Fact]
  public void FamilyZwjSequence()
  {
    Assert.Equal( "1f468-200d-1f469-200d-1f467", EmojiImageProvider.ToTwemojiStem( "👨‍👩‍👧" ) );
  }

  [Fact]
  public void EmptyString_EmptyStem()
  {
    Assert.Equal( "", EmojiImageProvider.ToTwemojiStem( "" ) );
  }
}
