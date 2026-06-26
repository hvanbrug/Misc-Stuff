using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using HenksHotkeys.Native;

namespace HenksHotkeys.Core;

/// <summary>
/// Lets the (non-elevated) app reposition windows owned by elevated processes
/// (e.g. Hyper-V's VMConnect), which UIPI otherwise forbids. The main app stays
/// at normal rights; on the first window it cannot move it launches a copy of
/// itself elevated, in "helper mode", which connects back over a named pipe and
/// performs the SetWindowPos. The helper stays resident (one UAC prompt for the
/// session) and exits when the main app does.
///
/// The main app is the pipe *server* (created at medium integrity, so the
/// elevated client can connect — the reverse is blocked by the mandatory label).
/// The pipe name is a fresh GUID passed on the command line, so it can't be
/// pre-squatted, and the helper is only launched after the server exists.
/// </summary>
internal static class ElevatedFit
{
  public const string HelperArg = "--elevated-helper";

  public static bool IsHelperArg( string[] args )
    => args.Length > 0 && args[0] == HelperArg;

  // ── Client side (the normal, non-elevated app) ───────────────────
  private static NamedPipeServerStream? s_pipe;
  private static StreamReader?          s_reader;
  private static StreamWriter?          s_writer;
  private static Process?               s_helper;

  /// <summary>Ask the elevated helper to fit a window to the given bounds.
  /// Launches the helper (UAC) the first time. Returns false if unavailable.</summary>
  public static bool Fill( IntPtr hwnd, int x, int y, int w, int h )
  {
    try
    {
      if( !EnsureHelper() )
      {
        return false;
      }
      s_writer!.WriteLine( $"FILL {hwnd.ToInt64()} {x} {y} {w} {h}" );
      string? resp = s_reader!.ReadLine();
      if( resp is null )
      {
        Reset(); // helper went away
      }
      return resp == "OK";
    }
    catch
    {
      Reset();
      return false;
    }
  }

  private static bool EnsureHelper()
  {
    if( s_pipe is { IsConnected: true } && s_helper is { HasExited: false } )
    {
      return true;
    }
    Reset();

    string? exe = Environment.ProcessPath;
    if( exe is null )
    {
      return false;
    }

    string pipeName = "HenksHotkeys.ElevatedFit." + Guid.NewGuid().ToString( "N" );
    s_pipe = new NamedPipeServerStream( pipeName, PipeDirection.InOut, 1 );

    try
    {
      s_helper = Process.Start( new ProcessStartInfo
      {
        FileName        = exe,
        UseShellExecute = true,
        Verb            = "runas", // elevate (UAC)
        Arguments       = $"{HelperArg} {pipeName} {Environment.ProcessId}",
      } );
    }
    catch
    {
      Reset(); // UAC declined or launch failed
      return false;
    }

    // Wait for the elevated helper to connect (after the user accepts UAC).
    if( !s_pipe.WaitForConnectionAsync().Wait( TimeSpan.FromSeconds( 30 ) ) || !s_pipe.IsConnected )
    {
      Reset();
      return false;
    }

    s_writer = new StreamWriter( s_pipe ) { AutoFlush = true };
    s_reader = new StreamReader( s_pipe );
    return true;
  }

  private static void Reset()
  {
    try { s_reader?.Dispose(); } catch { }
    try { s_writer?.Dispose(); } catch { }
    try { s_pipe?.Dispose();   } catch { }
    s_reader = null;
    s_writer = null;
    s_pipe   = null;
    // The helper exits on its own (broken pipe / parent gone).
  }

  /// <summary>Close the pipe so the helper exits. Call on app shutdown.</summary>
  public static void Shutdown() => Reset();

  // ── Helper side (the elevated copy, started with HelperArg) ───────
  public static void RunHelper( string[] args )
  {
    string pipeName  = args.Length > 1 ? args[1] : "";
    int    parentPid = args.Length > 2 && int.TryParse( args[2], out int p ) ? p : 0;

    WatchParent( parentPid );

    try
    {
      using var client = new NamedPipeClientStream( ".", pipeName, PipeDirection.InOut );
      client.Connect( 15000 );
      using var reader = new StreamReader( client );
      using var writer = new StreamWriter( client ) { AutoFlush = true };

      string? line;
      while( ( line = reader.ReadLine() ) != null )
      {
        writer.WriteLine( Handle( line ) );
      }
    }
    catch
    {
      // pipe gone / connect failed → fall through and exit
    }

    Environment.Exit( 0 );
  }

  private static void WatchParent( int pid )
  {
    if( pid == 0 )
    {
      Environment.Exit( 0 );
      return;
    }
    try
    {
      Process parent = Process.GetProcessById( pid );
      parent.EnableRaisingEvents = true;
      parent.Exited += ( _, _ ) => Environment.Exit( 0 );
    }
    catch
    {
      Environment.Exit( 0 ); // parent already gone
    }
  }

  private static string Handle( string line )
  {
    string[] t = line.Split( ' ' );
    if( t.Length == 6 && t[0] == "FILL"
        && long.TryParse( t[1], out long hl )
        && int.TryParse(  t[2], out int x )
        && int.TryParse(  t[3], out int y )
        && int.TryParse(  t[4], out int w )
        && int.TryParse(  t[5], out int h ) )
    {
      var hwnd = new IntPtr( hl );
      if( !NativeMethods.IsWindow( hwnd ) )
      {
        return "ERR";
      }
      NativeMethods.ShowWindow( hwnd, NativeMethods.SW_RESTORE );
      bool ok = NativeMethods.ApplyBounds( hwnd, x, y, w, h );
      return ok ? "OK" : "ERR";
    }
    return "ERR";
  }
}
