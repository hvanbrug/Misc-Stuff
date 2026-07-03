using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>Set the global emoji skin tone (#27): persist it, update the live value, and rebuild so
/// the Emojis tab re-tints. <paramref name="hex"/> "" = default/yellow, else "1f3fb".."1f3ff".</summary>
internal static class SkinToneCommands
{
  public static void Set( string hex )
  {
    if( AppState.SkinTone == hex )
    {
      return; // already the active tone
    }
    AppState.Settings.SetSkinTone( hex );
    AppState.SkinTone = hex;
    AppState.Window?.RetintEmojiInPlace(); // swap images on the existing buttons — no rebuild

  }
}
