using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using HenksHotkeys.UI;
using PInvoke;

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
    AppState.Settings.SetClipSendMode( AppState.UseClipSend );
    AppState.Window?.UpdateClipIndicator( AppState.UseClipSend );
  }

  public static void ToggleStripSendEmojis()
  {
    AppState.StripSendEmojis = !AppState.StripSendEmojis;
    AppState.Settings.SetStripCommentEmojis( AppState.StripSendEmojis );
    AppState.Window?.UpdateStripIndicator( AppState.StripSendEmojis );
  }

  // MoveWindowToWorkArea in Utilities.ahk: fit the active window to its monitor's
  // work area, compensating for the invisible DWM frame borders. If the window is
  // owned by an elevated process (UIPI blocks our SetWindowPos), hand the same
  // bounds to the elevated helper, which performs the move.
  public static void MoveWindowToWorkArea()
  {
    IWindowFit fit = AppState.WindowFit;
    IntPtr target = fit.Root( AppState.ActiveWindow );
    if( target == IntPtr.Zero || !fit.IsWindow( target ) )
    {
      return;
    }

    fit.Restore( target );
    Win32.RECT area = fit.WorkArea( target );

    if( !TryFillWorkArea( target, area, out (int X, int Y, int W, int H) rect, out _ ) )
    {
      ElevatedFit.Fill( target, rect.X, rect.Y, rect.W, rect.H );
    }
  }

  // Size/position the window so its visible (DWM) bounds fill the work area.
  // Outputs the computed target bounds (so a fallback can reuse them) and the
  // Win32 error from SetWindowPos (5 = ACCESS_DENIED → elevated window). Returns
  // false if the window clamped itself / couldn't be moved.
  private static bool TryFillWorkArea( IntPtr target, Win32.RECT area,
                                       out (int X, int Y, int W, int H) rect, out int setPosError )
  {
    IWindowFit fit = AppState.WindowFit;
    rect = default;
    setPosError = 0;
    if( !fit.GetWindowRect( target, out Win32.RECT win ) )
    {
      return false;
    }
    Win32.RECT frame = fit.ExtendedFrameBounds( target );

    int borderL = frame.Left  - win.Left;
    int borderT = frame.Top   - win.Top + 1;
    int borderR = win.Right   - frame.Right;
    int borderB = win.Bottom  - frame.Bottom;

    int x = area.Left   - borderL;
    int y = area.Top    - borderT;
    int w = area.Width  + borderL + borderR;
    int h = area.Height + borderT + borderB;
    rect = ( x, y, w, h );

    fit.ApplyBounds( target, x, y, w, h, out setPosError );

    if( !fit.GetWindowRect( target, out Win32.RECT after ) )
    {
      return false;
    }
    int visibleW = after.Width  - borderL - borderR;
    int visibleH = after.Height - borderT - borderB;
    return visibleW >= area.Width - 4 && visibleH >= area.Height - 4;
  }

  // SREFtoFullPrompt in PromptHelpers.ahk: replace " | " separators in the
  // selected text with ", " and put the result on the clipboard.
  public static async Task SrefToFullPrompt()
  {
    string haystack = await TextSender.GetSelectedTextThroughClipboard();
    string replaced = Regex.Replace( haystack, @"\s*\|\s*", ", " );
    if( replaced.Length > 0 )
    {
      try { Clipboard.SetText( replaced ); } catch { /* clipboard busy */ }
    }
  }

  // Diagnostic: report what the "fit to work area" tool sees for the active target
  // window, actually attempt the resize, and report whether the OS allowed it.
  // Run it with the problem window focused.
  public static void TestFunction()
  {
    IWindowFit fit = AppState.WindowFit;
    var sb = new StringBuilder();
    IntPtr raw  = AppState.ActiveWindow;
    IntPtr root = fit.Root( raw );
    IntPtr target = root != IntPtr.Zero ? root : raw;

    if( target == IntPtr.Zero || !fit.IsWindow( target ) )
    {
      MessageBox.Show( "No active target window captured.", "Fit diagnostics" );
      return;
    }

    string cls = fit.ClassName( target );
    uint   pid = fit.ProcessId( target );
    string proc = "?";
    try   { proc = Process.GetProcessById( (int)pid ).ProcessName; }
    catch { /* process gone */ }

    fit.GetWindowRect( target, out Win32.RECT win );
    Win32.RECT area = fit.WorkArea( target );

    sb.AppendLine( $"Target : {proc}.exe  (class {cls})" );
    sb.AppendLine( $"Root == foreground: {root == raw}" );
    sb.AppendLine( $"Before : {win.Width} x {win.Height}  at ({win.Left}, {win.Top})" );
    sb.AppendLine( $"WorkArea: {area.Width} x {area.Height}  at ({area.Left}, {area.Top})" );

    // Attempt the move directly and capture whether the OS permitted it.
    fit.Restore( target );
    bool filled = TryFillWorkArea( target, area, out (int X, int Y, int W, int H) rect, out int err );

    fit.GetWindowRect( target, out Win32.RECT after );
    sb.AppendLine( $"After  : {after.Width} x {after.Height}  at ({after.Left}, {after.Top})" );
    sb.AppendLine( $"Direct : filled={filled}  SetWindowPos error={err}" +
                   ( err == 5 ? "  (ACCESS DENIED — elevated window)" : "" ) );

    if( !filled )
    {
      bool viaHelper = ElevatedFit.Fill( target, rect.X, rect.Y, rect.W, rect.H );
      sb.AppendLine( $"Helper : {( viaHelper ? "filled via elevated helper" : "unavailable / declined" )}" );
    }

    MessageBox.Show( sb.ToString(), "Fit-to-work-area diagnostics" );
  }

  public static void ListHotkeys()
  {
    new HotkeyListWindow().Show();
  }
}
