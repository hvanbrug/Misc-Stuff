namespace HenksHotkeys.Core;

/// <summary>
/// Collects the global hotkey bindings declared while tabs are built (parallels
/// TabPage.m_globalHotkeyMap in UITabPage.ahk: first registration of a given
/// hotkey string wins, later duplicates are ignored). The
/// <see cref="GlobalHotkeyManager"/> consumes these once the UI is constructed.
/// </summary>
internal static class HotkeyRegistry
{
  public readonly record struct Binding( string Hotkey, Action Action );

  private static readonly List<Binding>   s_bindings = new();
  private static readonly HashSet<string> s_seen     = new( StringComparer.Ordinal );

  public static IReadOnlyList<Binding> Bindings => s_bindings;

  public static void Add( string hotkey, Action action )
  {
    if( string.IsNullOrEmpty( hotkey ) || s_seen.Contains( hotkey ) )
    {
      return;
    }
    s_seen.Add( hotkey );
    s_bindings.Add( new Binding( hotkey, action ) );
  }

  /// <summary>Drop all bindings so the tabs can be rebuilt (config reload).</summary>
  public static void Clear()
  {
    s_bindings.Clear();
    s_seen.Clear();
  }
}
