using System.Windows.Forms;
using HenksHotkeys.Native;
using static HenksHotkeys.Native.NativeMethods;
using SysMarshal = System.Runtime.InteropServices.Marshal;

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

  // ── Public entry points ──────────────────────────────────────────

  /// <summary>Send text, honouring clipboard-send mode (DoSendText).</summary>
  public static void SendText( string msg )
  {
    if( AppState.UseClipSend )
    {
      SendViaClipboard( msg );
      return;
    }
    ActivateTarget();
    SendKeystrokes( msg );
  }

  /// <summary>Send a key sequence, always as input events (DoSendInput).</summary>
  public static void SendInputKeys( string msg )
  {
    ActivateTarget();
    SendKeystrokes( msg );
  }

  /// <summary>Copy the current selection and return it (GetSelectedTextThroughClipboard).</summary>
  public static string GetSelectedTextThroughClipboard()
  {
    string backup = SafeGetClipboardText();
    try
    {
      Clipboard.Clear();
      SendKeyVk( 0x43, NativeMethods.MOD_CONTROL ); // Ctrl+C
      Thread.Sleep( 150 );
      string txt = SafeGetClipboardText();
      return txt;
    }
    finally
    {
      if( backup.Length > 0 )
      {
        TrySetClipboardText( backup );
      }
    }
  }

  // ── Internals ────────────────────────────────────────────────────

  private static void ActivateTarget()
  {
    IntPtr target = AppState.ActiveWindow;
    if( target != IntPtr.Zero && IsWindow( target ) )
    {
      SetForegroundWindow( target );
      Thread.Sleep( 100 );
    }
  }

  private static void SendViaClipboard( string rawText )
  {
    string backup = SafeGetClipboardText();
    TrySetClipboardText( rawText );
    ActivateTarget();
    SendKeyVk( 0x56, NativeMethods.MOD_CONTROL ); // Ctrl+V
    Thread.Sleep( 150 );
    if( backup.Length > 0 )
    {
      TrySetClipboardText( backup );
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

  private static void TrySetClipboardText( string text )
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
        Thread.Sleep( 20 );
      }
    }
  }

  // Parse the AHK-flavoured string and emit the corresponding input events.
  private static void SendKeystrokes( string s )
  {
    var inputs = new List<INPUT>( s.Length * 2 );
    int i = 0;
    while( i < s.Length )
    {
      char c = s[i];

      if( c == '{' )
      {
        int close = s.IndexOf( '}', i + 1 );
        if( close > i )
        {
          string token = s.Substring( i + 1, close - i - 1 );
          AppendToken( inputs, token );
          i = close + 1;
          continue;
        }
      }

      switch( c )
      {
        case '\n': AppendVk( inputs, VK_RETURN ); break;
        case '\r':                                break; // ignore; \r\n collapses to one Enter
        case '\b': AppendVk( inputs, VK_BACK   ); break;
        case '\t': AppendVk( inputs, VK_TAB    ); break;
        default:   AppendUnicode( inputs, c    ); break;
      }
      i++;
    }

    if( inputs.Count > 0 )
    {
      INPUT[] arr = inputs.ToArray();
      SendInput( (uint)arr.Length, arr, SysMarshal.SizeOf<INPUT>() );
    }
  }

  private static void AppendToken( List<INPUT> inputs, string token )
  {
    // Single-character literal escapes ({!} {#} {@} {+} {^} {{} {}}).
    if( token.Length == 1 && "!#@+^{}".Contains( token[0] ) )
    {
      AppendUnicode( inputs, token[0] );
      return;
    }

    switch( token.ToLowerInvariant() )
    {
      case "enter":
      case "return":    AppendVk( inputs, VK_RETURN ); break;
      case "left":      AppendVk( inputs, VK_LEFT   ); break;
      case "right":     AppendVk( inputs, VK_RIGHT  ); break;
      case "up":        AppendVk( inputs, VK_UP     ); break;
      case "down":      AppendVk( inputs, VK_DOWN   ); break;
      case "backspace":
      case "bs":        AppendVk( inputs, VK_BACK   ); break;
      case "tab":       AppendVk( inputs, VK_TAB    ); break;
      case "space":     AppendUnicode( inputs, ' '  ); break;
      default:          /* unknown token: ignore */    break;
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

    if( ( modifierMask & NativeMethods.MOD_CONTROL ) != 0 ) inputs.Add( MakeKey( VK_CONTROL, 0, NativeMethods.KEYEVENTF_KEYDOWN ) );
    if( ( modifierMask & NativeMethods.MOD_SHIFT )   != 0 ) inputs.Add( MakeKey( VK_SHIFT,   0, NativeMethods.KEYEVENTF_KEYDOWN ) );
    if( ( modifierMask & NativeMethods.MOD_ALT )     != 0 ) inputs.Add( MakeKey( VK_MENU,    0, NativeMethods.KEYEVENTF_KEYDOWN ) );
    if( ( modifierMask & NativeMethods.MOD_WIN )     != 0 ) inputs.Add( MakeKey( VK_LWIN,    0, NativeMethods.KEYEVENTF_KEYDOWN ) );

    inputs.Add( MakeKey( vk, 0, 0 ) );
    inputs.Add( MakeKey( vk, 0, KEYEVENTF_KEYUP ) );

    if( ( modifierMask & NativeMethods.MOD_WIN )     != 0 ) inputs.Add( MakeKey( VK_LWIN,    0, NativeMethods.KEYEVENTF_KEYUP ) );
    if( ( modifierMask & NativeMethods.MOD_ALT )     != 0 ) inputs.Add( MakeKey( VK_MENU,    0, NativeMethods.KEYEVENTF_KEYUP ) );
    if( ( modifierMask & NativeMethods.MOD_SHIFT )   != 0 ) inputs.Add( MakeKey( VK_SHIFT,   0, NativeMethods.KEYEVENTF_KEYUP ) );
    if( ( modifierMask & NativeMethods.MOD_CONTROL ) != 0 ) inputs.Add( MakeKey( VK_CONTROL, 0, NativeMethods.KEYEVENTF_KEYUP ) );

    INPUT[] arr2 = inputs.ToArray();
    SendInput( (uint)arr2.Length, arr2, SysMarshal.SizeOf<INPUT>() );
  }

  private static INPUT MakeKey( ushort vk, ushort scan, uint flags )
  {
    return new INPUT
    {
      type = INPUT_KEYBOARD,
      u = new InputUnion
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
