using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Interop;
using HenksHotkeys.Native;
using HenksHotkeys.UI;

namespace HenksHotkeys.Core;

/// <summary>
/// App-level commands invoked from tray menu, helper buttons, Tools/Prompts tab
/// buttons and global hotkeys. Ports the standalone functions from Utilities.ahk
/// and PromptHelpers.ahk (toggle modes, move-to-work-area, SREF conversion,
/// list hotkeys).
/// </summary>
internal static class AppActions
{
  public static void ToggleClipboardSendMode()
  {
    AppState.UseClipSend = !AppState.UseClipSend;
    AppState.Ini.SetClipSendMode( AppState.UseClipSend );
    AppState.Window?.UpdateClipIndicator( AppState.UseClipSend );
  }

  public static void ToggleStripSendEmojis()
  {
    AppState.StripSendEmojis = !AppState.StripSendEmojis;
    AppState.Ini.SetStripCommentEmojis( AppState.StripSendEmojis );
    AppState.Window?.UpdateStripIndicator( AppState.StripSendEmojis );
  }

  // MoveWindowToWorkArea in Utilities.ahk: fit the active window to its monitor's
  // work area, compensating for the invisible DWM frame borders.
  public static void MoveWindowToWorkArea()
  {
    IntPtr target = AppState.ActiveWindow;
    if( target == IntPtr.Zero || !NativeMethods.IsWindow( target ) )
    {
      return;
    }

    NativeMethods.ShowWindow( target, NativeMethods.SW_RESTORE );

    if( !NativeMethods.GetWindowRect( target, out NativeMethods.RECT win ) )
    {
      return;
    }
    NativeMethods.DwmGetWindowAttribute( target, NativeMethods.DWMWA_EXTENDED_FRAME_BOUNDS,
                                         out NativeMethods.RECT frame, 16 );

    int borderL = frame.Left   - win.Left;
    int borderT = frame.Top    - win.Top + 1;
    int borderR = win.Right    - frame.Right;
    int borderB = win.Bottom   - frame.Bottom;

    var area = Screen.FromHandle( target ).WorkingArea;
    int x = area.Left   - borderL;
    int y = area.Top    - borderT;
    int w = area.Width  + borderL + borderR;
    int h = area.Height + borderT + borderB;

    NativeMethods.SetWindowPos( target, IntPtr.Zero, x, y, w, h,
                                NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE );
  }

  // SREFtoFullPrompt in PromptHelpers.ahk: replace " | " separators in the
  // selected text with ", " and put the result on the clipboard.
  public static void SrefToFullPrompt()
  {
    string haystack = TextSender.GetSelectedTextThroughClipboard();
    string replaced = Regex.Replace( haystack, @"\s*\|\s*", ", " );
    if( replaced.Length > 0 )
    {
      try { Clipboard.SetText( replaced ); } catch { /* clipboard busy */ }
    }
  }

  public static void TestFunction()
  {
    var sb = new StringBuilder();
    var area = Screen.PrimaryScreen?.WorkingArea ?? default;
    sb.AppendLine( $"Work area: {area.Left}, {area.Top}, {area.Right}, {area.Bottom}" );

    if( AppState.Window is { } w )
    {
      IntPtr hwnd = new WindowInteropHelper( w ).Handle;
      if( hwnd != IntPtr.Zero )
      {
        NativeMethods.GetWindowRect( hwnd, out NativeMethods.RECT rc );
        sb.AppendLine( $"Window: {rc.Width} x {rc.Height}  at ({rc.Left}, {rc.Top})" );
      }
      sb.AppendLine( $"Size (DIP): {w.ActualWidth:0} x {w.ActualHeight:0}" );
    }

    MessageBox.Show( sb.ToString(), "Test Function" );
  }

  public static void ListHotkeys()
  {
    new HotkeyListWindow().Show();
  }
}
