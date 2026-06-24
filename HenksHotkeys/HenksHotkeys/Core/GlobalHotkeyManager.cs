using System.Windows.Interop;
using HenksHotkeys.Native;

namespace HenksHotkeys.Core;

/// <summary>
/// Registers all global hotkeys on a message-only window (HwndSource) and
/// dispatches WM_HOTKEY to the bound actions. Replaces AutoHotkey's built-in
/// Hotkey() registrations (the per-symbol bindings collected in
/// <see cref="HotkeyRegistry"/> plus the app-level ones such as ^+x and ^+a).
/// </summary>
internal sealed class GlobalHotkeyManager : IDisposable
{
  private static readonly IntPtr HWND_MESSAGE = new( -3 );

  private readonly HwndSource              m_source;
  private readonly Dictionary<int, Action> m_actions = new();
  private int m_nextId = 1;

  public GlobalHotkeyManager()
  {
    var prms = new HwndSourceParameters( "HenksHotkeysHotkeys" )
    {
      ParentWindow = HWND_MESSAGE, // message-only window
    };
    m_source = new HwndSource( prms );
    m_source.AddHook( WndProc );
  }

  /// <summary>Register a hotkey by AHK-style string. Returns false on parse/registration failure.</summary>
  public bool Register( string hotkey, Action action )
  {
    HotkeyParser.Parsed? parsed = HotkeyParser.Parse( hotkey );
    if( parsed is null )
    {
      return false;
    }

    int id = m_nextId++;
    if( !NativeMethods.RegisterHotKey( m_source.Handle, id,
                                       parsed.Value.Modifiers | NativeMethods.MOD_NOREPEAT,
                                       parsed.Value.VirtualKey ) )
    {
      return false;
    }

    m_actions[id] = action;
    return true;
  }

  /// <summary>Register every binding collected while the tabs were built.</summary>
  public void RegisterCollected()
  {
    foreach( HotkeyRegistry.Binding b in HotkeyRegistry.Bindings )
    {
      Register( b.Hotkey, b.Action );
    }
  }

  private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
  {
    if( msg == NativeMethods.WM_HOTKEY )
    {
      Dispatch( (int)wParam );
      handled = true;
    }
    return IntPtr.Zero;
  }

  private void Dispatch( int id )
  {
    if( m_actions.TryGetValue( id, out Action? action ) )
    {
      try { action(); } catch { /* never let a hotkey action crash the loop */ }
    }
  }

  public void Dispose()
  {
    foreach( int id in m_actions.Keys )
    {
      NativeMethods.UnregisterHotKey( m_source.Handle, id );
    }
    m_actions.Clear();
    m_source.RemoveHook( WndProc );
    m_source.Dispose();
  }
}
