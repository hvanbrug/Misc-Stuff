namespace NetworkShares;

/// <summary>One drive mapping: a drive letter and the UNC path it maps to.</summary>
internal sealed record ShareMapping( string Drive, string Unc );

/// <summary>A named group of mappings that share a username (from NetworkShares.cmd).</summary>
internal sealed record ShareGroup( string Name, string Subtitle, string Username, IReadOnlyList<ShareMapping> Mappings );

/// <summary>
/// The share groups, transcribed from _MiscScripts\NetworkShares.cmd. Edit here to
/// change hosts, drive letters or usernames.
/// </summary>
internal static class ShareData
{
  public static IReadOnlyList<ShareGroup> Groups { get; } = new[]
  {
    new ShareGroup( "LGS Shares", "user: LGS-Net\\VHE", "LGS-Net\\VHE", new[]
    {
      new ShareMapping( "H:", @"\\lgs-net.com\AKEL\AkelData" ),
      new ShareMapping( "M:", @"\\AKELDSKMSIBLD03\c$" ),
      new ShareMapping( "N:", @"\\AKELDSKMSIBLD03\d$" ),
      new ShareMapping( "O:", @"\\AKELDSKMSIBLD03\e$" ),
      new ShareMapping( "T:", @"\\AKELDSKMSIBLD01\c$" ),
      new ShareMapping( "U:", @"\\AKELDSKMSIBLD02\c$" ),
      new ShareMapping( "X:", @"\\AKELNASFIS01\Share" ),
    } ),

    new ShareGroup( "Local NAS", "192.168.1.30  ·  user: WORKGROUP\\hvanbrug", "WORKGROUP\\hvanbrug", new[]
    {
      new ShareMapping( "I:", @"\\192.168.1.30\NASData" ),
      new ShareMapping( "J:", @"\\192.168.1.30\Fred" ),
      new ShareMapping( "K:", @"\\192.168.1.30\Pebbles" ),
    } ),

    new ShareGroup( "Local PC", "GEO-WMXL1404988  ·  user: LGS-Net\\VHE", "LGS-Net\\VHE", new[]
    {
      new ShareMapping( "P:", @"\\GEO-WMXL1404988\c$" ),
      new ShareMapping( "Q:", @"\\GEO-WMXL1404988\d$" ),
      new ShareMapping( "R:", @"\\GEO-WMXL1404988\e$" ),
    } ),
  };
}
