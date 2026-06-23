using System.Windows.Forms;
using HenksHotkeys.Native;

namespace HenksHotkeys.Core;

/// <summary>
/// Registers all global hotkeys on a message-only window and dispatches WM_HOTKEY
/// to the bound actions. Replaces AutoHotkey's built-in Hotkey() registrations
/// (the per-symbol bindings collected in <see cref="HotkeyRegistry"/> plus the
/// app-level ones such as ^+x toggle and ^+a list).
/// </summary>
internal sealed class GlobalHotkeyManager : IDisposable
{
  private sealed class MessageWindow : NativeWindow
  {
    private readonly GlobalHotkeyManager m_owner;

    public MessageWindow( GlobalHotkeyManager owner )
    {
      m_owner = owner;
      var cp = new CreateParams { Caption = "HenksHotkeysMsgWnd" };
      CreateHandle( cp ); // message-only-ish; never shown
    }

    protected override void WndProc( ref Message m )
    {
      if( m.Msg == NativeMethods.WM_HOTKEY )
      {
        m_owner.Dispatch( (int)m.WParam );
      }
      base.WndProc( ref m );
    }
  }

  private readonly MessageWindow           m_window;
  private readonly Dictionary<int, Action> m_actions = new();
  private int m_nextId = 1;

  public GlobalHotkeyManager()
  {
    m_window = new MessageWindow( this );
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
    if( !NativeMethods.RegisterHotKey( m_window.Handle, id,
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
      NativeMethods.UnregisterHotKey( m_window.Handle, id );
    }
    m_actions.Clear();
    m_window.DestroyHandle();
  }
}
