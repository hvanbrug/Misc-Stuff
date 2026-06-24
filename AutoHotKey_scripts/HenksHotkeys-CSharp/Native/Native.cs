using System.Runtime.InteropServices;

namespace HenksHotkeys.Native;

/// <summary>
/// P/Invoke surface used across the app. Mirrors the DllCall surface the
/// original AutoHotkey scripts relied on (input synthesis, foreground-window
/// tracking, global hotkeys, DWM dark frame, and low-level mouse hook).
/// </summary>
internal static class NativeMethods
{
  // ── Window styles ────────────────────────────────────────────────
  public const int GWL_STYLE   = -16;
  public const int GWL_EXSTYLE = -20;

  public const int WS_EX_TOOLWINDOW = 0x00000080;
  public const int WS_EX_TOPMOST    = 0x00000008;
  public const int WS_EX_NOACTIVATE = 0x08000000;

  // ── Window messages ──────────────────────────────────────────────
  public const int WM_HOTKEY        = 0x0312;
  public const int WM_MOUSEWHEEL    = 0x020A;
  public const int WM_NCLBUTTONDOWN = 0x00A1;
  public const int HTCAPTION        = 2;

  // ── SetWindowPos flags ───────────────────────────────────────────
  public const uint SWP_NOSIZE     = 0x0001;
  public const uint SWP_NOMOVE     = 0x0002;
  public const uint SWP_NOZORDER   = 0x0004;
  public const uint SWP_NOACTIVATE = 0x0010;

  // ── Global hotkey modifiers (RegisterHotKey) ─────────────────────
  public const uint MOD_ALT      = 0x0001;
  public const uint MOD_CONTROL  = 0x0002;
  public const uint MOD_SHIFT    = 0x0004;
  public const uint MOD_WIN      = 0x0008;
  public const uint MOD_NOREPEAT = 0x4000;

  // ── SendInput ────────────────────────────────────────────────────
  public const int  INPUT_KEYBOARD      = 1;
  public const uint KEYEVENTF_KEYDOWN   = 0x0000;
  public const uint KEYEVENTF_KEYUP     = 0x0002;
  public const uint KEYEVENTF_UNICODE   = 0x0004;

  // ── Low level mouse hook ─────────────────────────────────────────
  public const int WH_MOUSE_LL = 14;

  [StructLayout( LayoutKind.Sequential )]
  public struct POINT
  {
    public int X;
    public int Y;
  }

  [StructLayout( LayoutKind.Sequential )]
  public struct RECT
  {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
    public int Width  => Right - Left;
    public int Height => Bottom - Top;
  }

  [StructLayout( LayoutKind.Sequential )]
  public struct MSLLHOOKSTRUCT
  {
    public POINT pt;
    public uint  mouseData;
    public uint  flags;
    public uint  time;
    public IntPtr dwExtraInfo;
  }

  [StructLayout( LayoutKind.Sequential )]
  public struct INPUT
  {
    public uint     type;
    public InputUnion u;
  }

  [StructLayout( LayoutKind.Explicit )]
  public struct InputUnion
  {
    [FieldOffset( 0 )] public KEYBDINPUT ki;
    [FieldOffset( 0 )] public MOUSEINPUT mi;
    [FieldOffset( 0 )] public HARDWAREINPUT hi;
  }

  [StructLayout( LayoutKind.Sequential )]
  public struct KEYBDINPUT
  {
    public ushort wVk;
    public ushort wScan;
    public uint   dwFlags;
    public uint   time;
    public IntPtr dwExtraInfo;
  }

  [StructLayout( LayoutKind.Sequential )]
  public struct MOUSEINPUT
  {
    public int    dx;
    public int    dy;
    public uint   mouseData;
    public uint   dwFlags;
    public uint   time;
    public IntPtr dwExtraInfo;
  }

  [StructLayout( LayoutKind.Sequential )]
  public struct HARDWAREINPUT
  {
    public uint   uMsg;
    public ushort wParamL;
    public ushort wParamH;
  }

  public delegate IntPtr LowLevelMouseProc( int nCode, IntPtr wParam, IntPtr lParam );

  [DllImport( "user32.dll", SetLastError = true )]
  public static extern uint SendInput( uint nInputs, INPUT[] pInputs, int cbSize );

  [DllImport( "user32.dll" )]
  public static extern IntPtr GetForegroundWindow();

  [DllImport( "user32.dll" )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool SetForegroundWindow( IntPtr hWnd );

  [DllImport( "user32.dll" )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool IsWindow( IntPtr hWnd );

  [DllImport( "user32.dll" )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool IsIconic( IntPtr hWnd );

  [DllImport( "user32.dll" )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool ShowWindow( IntPtr hWnd, int nCmdShow );

  [DllImport( "user32.dll", SetLastError = true )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool RegisterHotKey( IntPtr hWnd, int id, uint fsModifiers, uint vk );

  [DllImport( "user32.dll", SetLastError = true )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool UnregisterHotKey( IntPtr hWnd, int id );

  [DllImport( "user32.dll" )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool GetWindowRect( IntPtr hWnd, out RECT lpRect );

  [DllImport( "user32.dll", SetLastError = true )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter,
                                          int X, int Y, int cx, int cy, uint uFlags );

  [DllImport( "user32.dll" )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool GetCursorPos( out POINT lpPoint );

  [DllImport( "user32.dll" )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool ReleaseCapture();

  [DllImport( "user32.dll" )]
  public static extern IntPtr SendMessage( IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam );

  [DllImport( "user32.dll", SetLastError = true )]
  public static extern IntPtr SetWindowsHookEx( int idHook, LowLevelMouseProc lpfn,
                                                IntPtr hMod, uint dwThreadId );

  [DllImport( "user32.dll", SetLastError = true )]
  [return: MarshalAs( UnmanagedType.Bool )]
  public static extern bool UnhookWindowsHookEx( IntPtr hhk );

  [DllImport( "user32.dll" )]
  public static extern IntPtr CallNextHookEx( IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam );

  [DllImport( "kernel32.dll" )]
  public static extern IntPtr GetModuleHandle( string? lpModuleName );

  [DllImport( "user32.dll" )]
  public static extern uint GetDpiForSystem();

  [DllImport( "user32.dll" )]
  public static extern uint GetDpiForWindow( IntPtr hwnd );

  [DllImport( "user32.dll", EntryPoint = "GetWindowLongPtrW" )]
  public static extern IntPtr GetWindowLongPtr( IntPtr hWnd, int nIndex );

  [DllImport( "user32.dll", EntryPoint = "SetWindowLongPtrW" )]
  public static extern IntPtr SetWindowLongPtr( IntPtr hWnd, int nIndex, IntPtr dwNewLong );

  [DllImport( "dwmapi.dll" )]
  public static extern int DwmSetWindowAttribute( IntPtr hwnd, int attr, ref int attrValue, int attrSize );

  [DllImport( "dwmapi.dll" )]
  public static extern int DwmGetWindowAttribute( IntPtr hwnd, int attr, out RECT attrValue, int attrSize );

  // DWMWA_EXTENDED_FRAME_BOUNDS
  public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

  public const int SW_RESTORE = 9;

  // DWMWA_USE_IMMERSIVE_DARK_MODE
  public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
  // DWMWA_BORDER_COLOR (Win11)
  public const int DWMWA_BORDER_COLOR = 34;
}
