using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Emojis-tab favourites actions (#13): mark a catalog emoji as a favourite, unfavourite one, or
/// reorder them. Each updates <see cref="FavouritesStore"/> and rebuilds so the Emojis tab reflows
/// its Favourites section. The emoji catalog itself is never edited.
/// </summary>
internal static class FavouriteCommands
{
  public static void Add( string emoji )
  {
    if( FavouritesStore.Add( emoji ) )
    {
      AppState.Window?.ReconcileEmojiTab(); // in-place update, not a full rebuild
    }
  }

  public static void Remove( string ch )
  {
    if( FavouritesStore.Remove( ch ) )
    {
      AppState.Window?.ReconcileEmojiTab();
    }
  }

  public static void Reorder( string ch, int insertBeforeIndex )
  {
    if( FavouritesStore.Reorder( ch, insertBeforeIndex ) )
    {
      AppState.Window?.ReconcileEmojiTab();
    }
  }
}
