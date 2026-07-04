using PInvoke;

namespace HenksHotkeys.Core;

/// <summary>
/// The foreground-window OS integration behind an interface (#6): reading the current foreground
/// window (send targeting), and forcing a window to the foreground (dialogs / send activation).
/// The one implementation wraps the shared PInvoke lib; consumers depend on the interface.
/// </summary>
internal interface IForegroundWindow
{
  /// <summary>The window currently in the foreground (GetForegroundWindow).</summary>
  IntPtr Current();

  /// <summary>True if the handle is still a live window.</summary>
  bool IsWindow( IntPtr hwnd );

  /// <summary>Bring <paramref name="hwnd"/> to the foreground (a plain SetForegroundWindow — works
  /// when we already own the foreground).</summary>
  void Activate( IntPtr hwnd );

  /// <summary>Force <paramref name="hwnd"/> to the foreground even when we don't own it (our tool
  /// window is WS_EX_NOACTIVATE) by briefly attaching to the foreground thread's input queue.</summary>
  void ForceForeground( IntPtr hwnd );
}

internal sealed class ForegroundWindow : IForegroundWindow
{
  public IntPtr Current()            => Win32.GetForegroundWindow();
  public bool   IsWindow( IntPtr h )  => Win32.IsWindow( h );
  public void   Activate( IntPtr h )  => Win32.SetForegroundWindow( h );

  public void ForceForeground( IntPtr hwnd )
  {
    if( hwnd == IntPtr.Zero )
    {
      return;
    }
    IntPtr fg         = Win32.GetForegroundWindow();
    uint   fgThread   = fg == IntPtr.Zero ? 0 : Win32.GetWindowThreadProcessId( fg, out _ );
    uint   thisThread = Win32.GetCurrentThreadId();
    bool   attached   = fgThread != 0 && fgThread != thisThread && Win32.AttachThreadInput( thisThread, fgThread, true );
    try
    {
      Win32.BringWindowToTop( hwnd );
      Win32.SetForegroundWindow( hwnd );
    }
    finally
    {
      if( attached ) Win32.AttachThreadInput( thisThread, fgThread, false );
    }
  }
}
