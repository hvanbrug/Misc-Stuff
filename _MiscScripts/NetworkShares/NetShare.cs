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
  private const int NERR_UseNotFound          = 2312;

  // USE_INFO_1.ui1_status values (lmuse.h) — the same states `net use` reports.
  private const uint USE_OK       = 0; // connected, session live
  private const uint USE_NETERR   = 4; // network error
  // Other values (1 paused, 2 session-lost, 3 disconnected, 5 connecting, 6 reconnecting)
  // all mean "mapped but not connected right now" — shown as Disconnected.

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

  [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
  private struct USE_INFO_1
  {
    public string ui1_local;
    public string ui1_remote;
    public string ui1_password;
    public uint   ui1_status;
    public uint   ui1_asg_type;
    public uint   ui1_refcount;
    public uint   ui1_usecount;
  }

  // NetUseGetInfo queries the workstation (LanmanWorkstation) redirector, which
  // reports the *live* session state (USE_OK vs disconnected) — finer than the
  // MPR view, which only tells us a letter is mapped. SMB only; WebDAV etc. fall
  // through to the WNetGetConnection result.
  [DllImport( "netapi32.dll", CharSet = CharSet.Unicode )]
  private static extern int NetUseGetInfo( string? uncServerName, string useName, uint level, out IntPtr bufPtr );

  [DllImport( "netapi32.dll" )]
  private static extern int NetApiBufferFree( IntPtr buffer );

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

  /// <summary>
  /// Remove the mapping on <paramref name="drive"/>. Returns a Win32 code (0 = success).
  /// When <paramref name="keepMapping"/> is true the live session is dropped but the
  /// persistent mapping is kept, so Windows restores it at the next sign-on / on access
  /// (the drive then shows as Disconnected). When false the mapping is removed entirely.
  /// </summary>
  public static int Disconnect( string drive, bool keepMapping = false )
  {
    uint flags = keepMapping ? 0u : CONNECT_UPDATE_PROFILE;
    return WNetCancelConnection2W( drive, flags, force: true );
  }

  /// <summary>Determine the current state of <paramref name="drive"/> relative to <paramref name="expectedUnc"/>.</summary>
  public static StatusResult GetStatus( string drive, string expectedUnc )
  {
    // 1) MPR view: is the letter mapped at all, and to what? (sees every provider,
    //    including WebDAV — unlike NetUseGetInfo, which is SMB only.)
    (int mprRc, string remote) = WNetGet( drive );

    if( mprRc == ERROR_NOT_CONNECTED )
    {
      return new StatusResult( Status.NotMapped, "", mprRc );
    }
    if( mprRc != NO_ERROR && mprRc != ERROR_CONNECTION_UNAVAIL )
    {
      return new StatusResult( Status.Error, remote, mprRc );
    }
    if( expectedUnc.Length > 0 && remote.Length > 0 && !SameTarget( remote, expectedUnc ) )
    {
      return new StatusResult( Status.OtherTarget, remote, mprRc );
    }

    // 2) Refine via the workstation redirector to tell *live* from *remembered*.
    if( TryGetUseStatus( drive, out uint useStatus, out string useRemote ) )
    {
      Status s = useStatus switch
      {
        USE_OK     => Status.Connected,
        USE_NETERR => Status.Unavailable,
        _          => Status.Unavailable, // paused / session-lost / disconnected / (re)connecting
      };
      return new StatusResult( s, remote.Length > 0 ? remote : useRemote, (int)useStatus );
    }

    // 3) Non-SMB provider (e.g. WebDAV): fall back to the MPR availability.
    return new StatusResult( mprRc == NO_ERROR ? Status.Connected : Status.Unavailable, remote, mprRc );
  }

  /// <summary>WNetGetConnection wrapper → (Win32 code, remote name or "").</summary>
  private static (int Code, string Remote) WNetGet( string drive )
  {
    var sb  = new StringBuilder( 260 );
    int len = sb.Capacity;
    int rc  = WNetGetConnectionW( drive, sb, ref len );
    if( rc == ERROR_MORE_DATA )
    {
      sb  = new StringBuilder( len );
      rc  = WNetGetConnectionW( drive, sb, ref len );
    }
    return (rc, rc is NO_ERROR or ERROR_CONNECTION_UNAVAIL ? sb.ToString() : "");
  }

  /// <summary>NetUseGetInfo wrapper. Returns false for free letters or non-SMB providers.</summary>
  private static bool TryGetUseStatus( string drive, out uint status, out string remote )
  {
    status = 0;
    remote = "";
    int rc = NetUseGetInfo( null, drive, 1, out IntPtr buf );
    if( rc != NO_ERROR )
    {
      return false; // ERROR_NOT_CONNECTED / NERR_UseNotFound / bad device type, etc.
    }
    try
    {
      var info = Marshal.PtrToStructure<USE_INFO_1>( buf );
      status = info.ui1_status;
      remote = info.ui1_remote ?? "";
      return true;
    }
    finally
    {
      NetApiBufferFree( buf );
    }
  }

  private static bool SameTarget( string a, string b )
  {
    return string.Equals( a.TrimEnd( '\\' ), b.TrimEnd( '\\' ), StringComparison.OrdinalIgnoreCase );
  }

  /// <summary>All currently-mapped network drives (A:–Z:) with their remote path and state.</summary>
  public static IReadOnlyList<(string Drive, string Remote, Status Status)> EnumerateMappedDrives()
  {
    var list = new List<(string, string, Status)>();
    for( char c = 'A'; c <= 'Z'; c++ )
    {
      string         drive = c + ":";
      StatusResult   st    = GetStatus( drive, "" ); // no expected target → never OtherTarget
      if( st.Status != Status.NotMapped )
      {
        list.Add( (drive, st.Remote, st.Status) );
      }
    }
    return list;
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
