using System.Runtime.InteropServices;
using System.Text;
using PInvoke;

namespace HenksHotkeys.Core;

/// <summary>
/// Window snap/fit OS integration behind an interface (#6): resolving the root window, reading
/// window / DWM-frame / work-area rectangles, and applying exact bounds. The single implementation
/// wraps the shared PInvoke lib; the fit <em>policy</em> (border compensation, retry, elevation
/// fallback) stays in <see cref="AppActions"/>. Also used by the elevated helper (<see cref="ElevatedFit"/>).
/// </summary>
internal interface IWindowFit
{
  /// <summary>Top-level root of <paramref name="hwnd"/> (GetAncestor GA_ROOT).</summary>
  IntPtr Root( IntPtr hwnd );

  /// <summary>True if the handle is still a live window.</summary>
  bool IsWindow( IntPtr hwnd );

  /// <summary>Restore a (possibly maximized/minimized) window so it can be freely sized.</summary>
  void Restore( IntPtr hwnd );

  /// <summary>The full window rectangle including the invisible resize border (GetWindowRect).</summary>
  bool GetWindowRect( IntPtr hwnd, out Win32.RECT rect );

  /// <summary>The visible (DWM extended-frame) bounds, excluding the invisible resize border.</summary>
  Win32.RECT ExtendedFrameBounds( IntPtr hwnd );

  /// <summary>Work area (physical px) of the monitor <paramref name="hwnd"/> is on.</summary>
  Win32.RECT WorkArea( IntPtr hwnd );

  /// <summary>Move/size a window to exact bounds, bracketed by ENTER/EXIT size-move so a hosted RDP
  /// client (Hyper-V enhanced session) renegotiates its inner desktop resolution the way it does for
  /// an interactive edge-drag. Returns the SetWindowPos result; <paramref name="setPosError"/> is the
  /// Win32 error on failure (5 = ACCESS_DENIED → elevated window), captured before the trailing
  /// SendMessage can overwrite it.</summary>
  bool ApplyBounds( IntPtr hwnd, int x, int y, int w, int h, out int setPosError );

  /// <summary>Window class name (diagnostics).</summary>
  string ClassName( IntPtr hwnd );

  /// <summary>Owning process id (diagnostics).</summary>
  uint ProcessId( IntPtr hwnd );
}

internal sealed class WindowFit : IWindowFit
{
  public IntPtr Root( IntPtr hwnd )      => Win32.GetAncestor( hwnd, Win32.GA_ROOT );
  public bool   IsWindow( IntPtr hwnd )   => Win32.IsWindow( hwnd );
  public void   Restore( IntPtr hwnd )    => Win32.ShowWindow( hwnd, Win32.SW_RESTORE );

  public bool GetWindowRect( IntPtr hwnd, out Win32.RECT rect ) => Win32.GetWindowRect( hwnd, out rect );

  public Win32.RECT ExtendedFrameBounds( IntPtr hwnd )
  {
    Win32.DwmGetWindowAttribute( hwnd, Win32.DWMWA_EXTENDED_FRAME_BOUNDS,
                                 out Win32.RECT frame, Marshal.SizeOf<Win32.RECT>() );
    return frame;
  }

  public Win32.RECT WorkArea( IntPtr hwnd ) => Win32.GetWorkArea( hwnd );

  public bool ApplyBounds( IntPtr hwnd, int x, int y, int w, int h, out int setPosError )
  {
    Win32.SendMessage( hwnd, Win32.WM_ENTERSIZEMOVE, IntPtr.Zero, IntPtr.Zero );
    bool ok = Win32.SetWindowPos( hwnd, IntPtr.Zero, x, y, w, h, Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE );
    setPosError = ok ? 0 : Marshal.GetLastWin32Error(); // capture before SendMessage clobbers last error
    Win32.SendMessage( hwnd, Win32.WM_EXITSIZEMOVE, IntPtr.Zero, IntPtr.Zero );
    return ok;
  }

  public string ClassName( IntPtr hwnd )
  {
    var sb = new StringBuilder( 256 );
    Win32.GetClassName( hwnd, sb, sb.Capacity );
    return sb.ToString();
  }

  public uint ProcessId( IntPtr hwnd )
  {
    Win32.GetWindowThreadProcessId( hwnd, out uint pid );
    return pid;
  }
}
