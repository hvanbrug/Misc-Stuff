using System.Windows;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Sending secret buttons. The value is decrypted only here, at the moment it is sent,
/// from the session key — it is never decrypted at load or kept in memory. A secret that
/// can't be decrypted (locked / orphaned) tells the user to re-enter it instead of
/// silently sending nothing.
/// </summary>
internal static class SecretCommands
{
  public static void Send( ButtonDef button )
  {
    string? plain = button.Locked ? null : SecretSession.Reveal( button.Secret );
    if( plain is null )
    {
      ShowLocked();
      return;
    }
    _ = TextSender.SendText( plain ); // consumed by the send pipeline, then dropped
  }

  private static void ShowLocked() => MessageBox.Show(
    "This secret couldn't be decrypted — it was sealed on another machine or with a different " +
    "passphrase, and its key is no longer in this file.\n\n" +
    "Right-click the button → Edit, and re-enter the value to fix it.",
    "Secret locked", MessageBoxButton.OK, MessageBoxImage.Warning );
}
