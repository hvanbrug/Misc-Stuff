using System.Windows;
using static PInvoke.Win32;

namespace HenksHotkeys.Core;

/// <summary>
/// Synthesises keyboard input to the previously-active window, reproducing the
/// AutoHotkey send helpers (DoSendText, DoSendInput, DoSendViaClipboard) from
/// Utilities.ahk. Interprets the small subset of AHK Send syntax the data uses:
/// brace escapes ({!} {#} {@} {+} {^} {{} {}}), named keys ({Enter} {Left}
/// {Right} {Backspace} {Tab}), and the control chars produced by `n `t `b.
/// </summary>
internal static class TextSender
{
  private const ushort VK_BACK   = 0x08;
  private const ushort VK_TAB    = 0x09;
  private const ushort VK_RETURN = 0x0D;
  private const ushort VK_LEFT   = 0x25;
  private const ushort VK_UP     = 0x26;
  private const ushort VK_RIGHT  = 0x27;
  private const ushort VK_DOWN   = 0x28;

  // Serialises sends so the now-responsive UI can't fire two overlapping sends
  // (which would interleave keystrokes). The synchronous version got this for
  // free by blocking the UI thread; the async version makes it explicit.
  private static readonly SemaphoreSlim s_sendGate = new( 1, 1 );

  // ── Public entry points ──────────────────────────────────────────

  /// <summary>Send text, honouring clipboard-send mode (DoSendText). Runs on the
  /// UI thread but awaits its waits, so the window stays responsive mid-send.</summary>
  public static async Task SendText( string msg )
  {
    await s_sendGate.WaitAsync();
    try
    {
      if( AppState.UseClipSend )
      {
        await SendViaClipboard( msg );
      }
      else
      {
        await ActivateTarget();
        SendKeystrokes( msg );
      }
    }
    catch { /* a failed send shouldn't crash the app (calls are fire-and-forget) */ }
    finally { s_sendGate.Release(); }
  }

  /// <summary>Send a key sequence, always as input events (DoSendInput).</summary>
  public static async Task SendInputKeys( string msg )
  {
    await s_sendGate.WaitAsync();
    try
    {
      await ActivateTarget();
      SendKeystrokes( msg );
    }
    catch { /* ignore */ }
    finally { s_sendGate.Release(); }
  }

  /// <summary>Copy the current selection and return it (GetSelectedTextThroughClipboard).</summary>
  public static async Task<string> GetSelectedTextThroughClipboard()
  {
    await s_sendGate.WaitAsync();
    string backup = SafeGetClipboardText();
    try
    {
      Clipboard.Clear();
      SendKeyVk( 0x43, MOD_CONTROL ); // Ctrl+C
      await Task.Delay( 150 );
      return SafeGetClipboardText();
    }
    finally
    {
      if( backup.Length > 0 )
      {
        await TrySetClipboardText( backup );
      }
      s_sendGate.Release();
    }
  }

  // ── Internals ────────────────────────────────────────────────────

  private static async Task ActivateTarget()
  {
    IntPtr target = AppState.ActiveWindow;
    if( target != IntPtr.Zero && AppState.Foreground.IsWindow( target ) )
    {
      AppState.Foreground.Activate( target );
      await Task.Delay( 100 );
    }
  }

  private static async Task SendViaClipboard( string rawText )
  {
    string backup = SafeGetClipboardText();
    await TrySetClipboardText( rawText );
    await ActivateTarget();
    SendKeyVk( 0x56, MOD_CONTROL ); // Ctrl+V
    await Task.Delay( 150 );
    if( backup.Length > 0 )
    {
      await TrySetClipboardText( backup );
    }
  }

  private static string SafeGetClipboardText()
  {
    try
    {
      return Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
    }
    catch
    {
      return string.Empty;
    }
  }

  private static async Task TrySetClipboardText( string text )
  {
    for( int attempt = 0; attempt < 3; attempt++ )
    {
      try
      {
        Clipboard.SetText( text );
        return;
      }
      catch
      {
        await Task.Delay( 20 );
      }
    }
  }

  /// <summary>
  /// One resolved keystroke from an AHK-flavoured send string: a virtual key when
  /// <see cref="Vk"/> is non-zero, otherwise the Unicode character <see cref="Ch"/>.
  /// </summary>
  internal readonly record struct SendKey( ushort Vk, char Ch );

  // Parse the AHK-flavoured string and emit the corresponding input events.
  private static void SendKeystrokes( string s )
  {
    IReadOnlyList<SendKey> keys = ParseSends( s );
    if( keys.Count == 0 )
    {
      return;
    }

    var inputs = new List<INPUT>( keys.Count * 2 );
    foreach( SendKey k in keys )
    {
      if( k.Vk != 0 )
      {
        AppendVk( inputs, k.Vk );
      }
      else
      {
        AppendUnicode( inputs, k.Ch );
      }
    }

    SendInput( inputs.ToArray() );
  }

  // Pure: turn an AHK-flavoured send string into a sequence of keystrokes.
  // Interprets brace escapes ({!} {#} {@} {+} {^} {{} {}}), named keys
  // ({Enter} {Left} {Right} {Up} {Down} {Backspace}/{BS} {Tab} {Space}) and the
  // control chars from `n / `t / `b. Unknown {tokens} are dropped, lone {/} stay literal.
  internal static IReadOnlyList<SendKey> ParseSends( string s )
  {
    var keys = new List<SendKey>( s.Length );
    int i = 0;
    while( i < s.Length )
    {
      char c = s[i];

      if( c == '{' )
      {
        int close = s.IndexOf( '}', i + 1 );
        if( close > i )
        {
          AppendToken( keys, s.Substring( i + 1, close - i - 1 ) );
          i = close + 1;
          continue;
        }
      }

      switch( c )
      {
        case '\n': keys.Add( new SendKey( VK_RETURN, '\0' ) ); break;
        case '\r':                                              break; // ignore; \r\n collapses to one Enter
        case '\b': keys.Add( new SendKey( VK_BACK,   '\0' ) ); break;
        case '\t': keys.Add( new SendKey( VK_TAB,    '\0' ) ); break;
        default:   keys.Add( new SendKey( 0, c )            ); break;
      }
      i++;
    }
    return keys;
  }

  private static void AppendToken( List<SendKey> keys, string token )
  {
    // Single-character literal escapes ({!} {#} {@} {+} {^} {{} {}}).
    if( token.Length == 1 && "!#@+^{}".Contains( token[0] ) )
    {
      keys.Add( new SendKey( 0, token[0] ) );
      return;
    }

    switch( token.ToLowerInvariant() )
    {
      case "enter":
      case "return":    keys.Add( new SendKey( VK_RETURN, '\0' ) ); break;
      case "left":      keys.Add( new SendKey( VK_LEFT,   '\0' ) ); break;
      case "right":     keys.Add( new SendKey( VK_RIGHT,  '\0' ) ); break;
      case "up":        keys.Add( new SendKey( VK_UP,     '\0' ) ); break;
      case "down":      keys.Add( new SendKey( VK_DOWN,   '\0' ) ); break;
      case "backspace":
      case "bs":        keys.Add( new SendKey( VK_BACK,   '\0' ) ); break;
      case "tab":       keys.Add( new SendKey( VK_TAB,    '\0' ) ); break;
      case "space":     keys.Add( new SendKey( 0, ' ' )          ); break;
      default:          /* unknown token: ignore */                break;
    }
  }

  private static void AppendUnicode( List<INPUT> inputs, char ch )
  {
    inputs.Add( MakeKey( 0, ch, KEYEVENTF_UNICODE ) );
    inputs.Add( MakeKey( 0, ch, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP ) );
  }

  private static INPUT MakeKey( ushort vk, char scanChar, uint flags )
  {
    return MakeKey( vk, (ushort)scanChar, flags );
  }

  private static void AppendVk( List<INPUT> inputs, ushort vk )
  {
    inputs.Add( MakeKey( vk, 0, 0 ) );
    inputs.Add( MakeKey( vk, 0, KEYEVENTF_KEYUP ) );
  }

  // Press and release a virtual key with optional modifier keys held down.
  private static void SendKeyVk( ushort vk, uint modifierMask )
  {
    var inputs = new List<INPUT>();
    const ushort VK_CONTROL = 0x11, VK_SHIFT = 0x10, VK_MENU = 0x12, VK_LWIN = 0x5B;

    if( ( modifierMask & MOD_CONTROL ) != 0 ) inputs.Add( MakeKey( VK_CONTROL, 0, KEYEVENTF_KEYDN ) );
    if( ( modifierMask & MOD_SHIFT )   != 0 ) inputs.Add( MakeKey( VK_SHIFT,   0, KEYEVENTF_KEYDN ) );
    if( ( modifierMask & MOD_ALT )     != 0 ) inputs.Add( MakeKey( VK_MENU,    0, KEYEVENTF_KEYDN ) );
    if( ( modifierMask & MOD_WIN )     != 0 ) inputs.Add( MakeKey( VK_LWIN,    0, KEYEVENTF_KEYDN ) );

    inputs.Add( MakeKey( vk, 0, 0 ) );
    inputs.Add( MakeKey( vk, 0, KEYEVENTF_KEYUP ) );

    if( ( modifierMask & MOD_WIN )     != 0 ) inputs.Add( MakeKey( VK_LWIN,    0, KEYEVENTF_KEYUP ) );
    if( ( modifierMask & MOD_ALT )     != 0 ) inputs.Add( MakeKey( VK_MENU,    0, KEYEVENTF_KEYUP ) );
    if( ( modifierMask & MOD_SHIFT )   != 0 ) inputs.Add( MakeKey( VK_SHIFT,   0, KEYEVENTF_KEYUP ) );
    if( ( modifierMask & MOD_CONTROL ) != 0 ) inputs.Add( MakeKey( VK_CONTROL, 0, KEYEVENTF_KEYUP ) );

    SendInput( inputs.ToArray() );
  }

  private static INPUT MakeKey( ushort vk, ushort scan, uint flags )
  {
    return new INPUT
    {
      type = INPUT_KEYBOARD,
      U = new InputUnion
      {
        ki = new KEYBDINPUT
        {
          wVk         = vk,
          wScan       = scan,
          dwFlags     = flags,
          time        = 0,
          dwExtraInfo = IntPtr.Zero
        }
      }
    };
  }
}
