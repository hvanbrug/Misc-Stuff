namespace HenksHotkeys.Core;

/// <summary>
/// Hook the (UI-layer) passphrase dialog plugs into, so <see cref="TabStore"/> can
/// ask for the master passphrase without depending on WPF. Arguments are
/// (creating: set a new passphrase vs unlock, retry: the previous attempt was
/// wrong). Returns the entered passphrase, or null if cancelled.
/// </summary>
internal static class PassphrasePrompt
{
  public static Func<bool, bool, string?>? Provider;

  public static string? Ask( bool creating, bool retry )
    => Provider?.Invoke( creating, retry );
}
