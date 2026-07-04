namespace HenksHotkeys.Core;

/// <summary>
/// Registers global (system-wide) hotkeys and dispatches them to actions. The one implementation
/// (<see cref="GlobalHotkeyManager"/>) wraps the Win32 RegisterHotKey plumbing on a message-only
/// window — this interface keeps the rest of the app off that detail (#6, isolate the Win32 interop).
/// </summary>
internal interface IGlobalHotkeys : IDisposable
{
  /// <summary>Register a hotkey by AHK-style string ("^+a"). False on parse/registration failure.</summary>
  bool Register( string hotkey, Action action );

  /// <summary>Register every binding collected while the tabs were built.</summary>
  void RegisterCollected();
}
