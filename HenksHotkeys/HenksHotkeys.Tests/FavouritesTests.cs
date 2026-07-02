using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

// Favourites reorder index math (#13). Uses the pure list helper so no favourites.json is touched.
public class FavouritesTests
{
  private static Favourite F( string ch ) => new() { Char = ch, Unicode = FavouritesStore.Encode( ch ) };
  private static List<Favourite> List( params string[] chars ) => chars.Select( F ).ToList();

  [Fact]
  public void Codec_RoundTripsMultiCodepointEmoji()
  {
    // Heart-on-fire is heart + variation selector + ZWJ + fire — all four codepoints must survive.
    const string emoji = "❤️‍🔥";
    string encoded = FavouritesStore.Encode( emoji );
    Assert.Equal( "U+2764 U+FE0F U+200D U+1F525", encoded );
    Assert.Equal( emoji, FavouritesStore.Decode( encoded ) );
  }

  [Fact]
  public void Decode_ToleratesBareHexAndSeparators()
  {
    Assert.Equal( "❤️", FavouritesStore.Decode( "2764, FE0F" ) ); // no "U+", comma-separated
  }

  [Fact]
  public void Migrate_UpgradesAGlyphOnlyEntry_ToCodepoints_PreservingOrder()
  {
    // The first favourites build wrote { char, desc }; migration fills in the codepoints in place.
    var list = new List<Favourite>
    {
      new() { Char = "💯" },        // legacy entry, no unicode
      new() { Char = "❤️‍🔥" },
    };

    Assert.True( FavouritesStore.Migrate( list ) );
    Assert.Equal( "U+1F4AF", list[0].Unicode );
    Assert.Equal( "U+2764 U+FE0F U+200D U+1F525", list[1].Unicode );
    Assert.Equal( new[] { "💯", "❤️‍🔥" }, list.Select( f => f.Emoji ) ); // order + emoji intact
    Assert.False( FavouritesStore.Migrate( list ) ); // idempotent
  }

  [Fact]
  public void ReorderInList_MovesForward_AccountingForTheRemoval()
  {
    var list = List( "😀", "😁", "😂" );
    Assert.True( FavouritesStore.ReorderInList( list, "😀", insertBeforeIndex: 2 ) );
    Assert.Equal( new[] { "😁", "😀", "😂" }, list.Select( f => f.Char ) );
  }

  [Fact]
  public void ReorderInList_MovesBackward_ToTheFront()
  {
    var list = List( "😀", "😁", "😂" );
    Assert.True( FavouritesStore.ReorderInList( list, "😂", 0 ) );
    Assert.Equal( new[] { "😂", "😀", "😁" }, list.Select( f => f.Char ) );
  }

  [Fact]
  public void ReorderInList_IsANoOp_WhenUnchangedOrMissing()
  {
    var list = List( "😀", "😁" );
    Assert.False( FavouritesStore.ReorderInList( list, "😀", 0 ) ); // already first
    Assert.False( FavouritesStore.ReorderInList( list, "😀", 1 ) ); // before itself → same slot
    Assert.False( FavouritesStore.ReorderInList( list, "🤔", 0 ) ); // not present
    Assert.Equal( new[] { "😀", "😁" }, list.Select( f => f.Char ) );
  }
}
