using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace NetworkShares;

/// <summary>
/// Thin wrapper over the WNet (mpr.dll) APIs for mapping, unmapping and querying
/// network drives. Using the API directly (rather than spawning <c>net use</c>)
/// keeps the password off any command line and returns numeric error codes that
/// can be turned into short, readable messages.
/// </summary>
internal static class NetShare
{
  public enum Status
  {
    NotMapped,    // drive letter is free
    Connected,    // mapped to the expected target
    OtherTarget,  // mapped, but to a different UNC
    Unavailable,  // remembered but not currently connected
    Error,        // query failed
  }

  public readonly record struct StatusResult( Status Status, string Remote, int Code );

  // ── Win32 ────────────────────────────────────────────────────────
  private const uint RESOURCETYPE_DISK      = 0x00000001;
  private const uint CONNECT_UPDATE_PROFILE = 0x00000001; // make the mapping persistent

  private const int NO_ERROR                  = 0;
  private const int ERROR_MORE_DATA           = 234;
  private const int ERROR_NOT_CONNECTED       = 2250;
  private const int ERROR_CONNECTION_UNAVAIL  = 1201;

  [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
  private struct NETRESOURCE
  {
    public uint    dwScope;
    public uint    dwType;
    public uint    dwDisplayType;
    public uint    dwUsage;
    public string? lpLocalName;
    public string? lpRemoteName;
    public string? lpComment;
    public string? lpProvider;
  }

  [DllImport( "mpr.dll", CharSet = CharSet.Unicode )]
  private static extern int WNetAddConnection2W( ref NETRESOURCE netResource, string? password, string? username, uint flags );

  [DllImport( "mpr.dll", CharSet = CharSet.Unicode )]
  private static extern int WNetCancelConnection2W( string name, uint flags, [MarshalAs( UnmanagedType.Bool )] bool force );

  [DllImport( "mpr.dll", CharSet = CharSet.Unicode )]
  private static extern int WNetGetConnectionW( string localName, StringBuilder remoteName, ref int length );

  // ── Operations ───────────────────────────────────────────────────

  /// <summary>Map <paramref name="drive"/> (e.g. "H:") to <paramref name="unc"/>. Returns a Win32 code (0 = success).</summary>
  public static int Connect( string drive, string unc, string? username, string? password )
  {
    var nr = new NETRESOURCE
    {
      dwType       = RESOURCETYPE_DISK,
      lpLocalName  = drive,
      lpRemoteName = unc,
    };
    // Blank credentials → null so Windows uses the current / stored (Credential
    // Manager) credentials rather than attempting an empty logon.
    string? user = string.IsNullOrEmpty( username ) ? null : username;
    string? pass = string.IsNullOrEmpty( password ) ? null : password;
    return WNetAddConnection2W( ref nr, pass, user, CONNECT_UPDATE_PROFILE );
  }

  /// <summary>Remove the mapping on <paramref name="drive"/>. Returns a Win32 code (0 = success).</summary>
  public static int Disconnect( string drive )
  {
    return WNetCancelConnection2W( drive, CONNECT_UPDATE_PROFILE, force: true );
  }

  /// <summary>Determine the current state of <paramref name="drive"/> relative to <paramref name="expectedUnc"/>.</summary>
  public static StatusResult GetStatus( string drive, string expectedUnc )
  {
    var sb  = new StringBuilder( 260 );
    int len = sb.Capacity;
    int rc  = WNetGetConnectionW( drive, sb, ref len );

    if( rc == ERROR_MORE_DATA )
    {
      sb = new StringBuilder( len );
      rc = WNetGetConnectionW( drive, sb, ref len );
    }

    return rc switch
    {
      NO_ERROR                 => new StatusResult( SameTarget( sb.ToString(), expectedUnc ) ? Status.Connected : Status.OtherTarget, sb.ToString(), rc ),
      ERROR_NOT_CONNECTED      => new StatusResult( Status.NotMapped,   "", rc ),
      ERROR_CONNECTION_UNAVAIL => new StatusResult( Status.Unavailable, "", rc ),
      _                        => new StatusResult( Status.Error,       "", rc ),
    };
  }

  private static bool SameTarget( string a, string b )
  {
    return string.Equals( a.TrimEnd( '\\' ), b.TrimEnd( '\\' ), StringComparison.OrdinalIgnoreCase );
  }

  // ── Friendly, password-free error text ───────────────────────────
  public static string Describe( int code )
  {
    string friendly = code switch
    {
      0    => "OK",
      5    => "Access denied.",
      53   => "Network path not found (is the host reachable?).",
      67   => "The share name was not found on the server.",
      85   => "That drive letter is already in use.",
      86   => "The password is incorrect.",
      1219 => "Already connected to that server with a different account — disconnect the existing connection first.",
      1326 => "Logon failed — wrong username or password.",
      1330 => "The password has expired.",
      1331 => "The account is disabled.",
      2202 => "No username/password was supplied and none is stored.",
      2250 => "The drive is not currently connected.",
      _    => new Win32Exception( code ).Message,
    };
    return $"{friendly} (error {code})";
  }
}
