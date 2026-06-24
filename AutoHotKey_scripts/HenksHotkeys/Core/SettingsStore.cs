using System.IO;
using Newtonsoft.Json;

namespace HenksHotkeys.Core;

/// <summary>
/// Loads and saves <see cref="Settings"/> as JSON (Newtonsoft.Json) under
/// %LocalAppData%\HenksHotkeys\. Each setter writes the file immediately, keeping
/// the same "persist on change" behaviour the old INI store had. Exposes the same
/// accessor surface the rest of the app already used.
/// </summary>
internal sealed class SettingsStore
{
  private readonly string   m_path;
  private readonly Settings m_data;

  public SettingsStore()
  {
    string dir = Path.Combine(
      Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
      "HenksHotkeys" );
    m_path = Path.Combine( dir, "settings.json" );
    m_data = Load( m_path );
  }

  private static Settings Load( string path )
  {
    try
    {
      if( File.Exists( path ) )
      {
        return JsonConvert.DeserializeObject<Settings>( File.ReadAllText( path ) ) ?? new Settings();
      }
    }
    catch
    {
      // corrupt / unreadable → start fresh
    }
    return new Settings();
  }

  private void Save()
  {
    try
    {
      Directory.CreateDirectory( Path.GetDirectoryName( m_path )! );
      File.WriteAllText( m_path, JsonConvert.SerializeObject( m_data, Formatting.Indented ) );
    }
    catch
    {
      // never let a failed write crash the app
    }
  }

  // ── Accessors (parallel to the former IniFile / INI_* helpers) ───
  public bool IsWndOpen            => m_data.WndOpen;
  public bool IsCollapsed          => m_data.Collapsed;
  public bool IsClipSendMode       => m_data.ClipSendMode;
  public bool IsStripCommentEmojis => m_data.StripCommentEmojis;

  public void SetWndOpen( bool v )            { m_data.WndOpen = v;            Save(); }
  public void SetCollapsed( bool v )          { m_data.Collapsed = v;          Save(); }
  public void SetClipSendMode( bool v )       { m_data.ClipSendMode = v;       Save(); }
  public void SetStripCommentEmojis( bool v ) { m_data.StripCommentEmojis = v; Save(); }

  public int  WndHeight              => m_data.Height ?? 0;
  public void SetWndHeight( int v )  { m_data.Height = v; Save(); }

  public int? WndX => m_data.X;
  public int? WndY => m_data.Y;
  public void SetWndPos( int x, int y ) { m_data.X = x; m_data.Y = y; Save(); }

  public int? FavX => m_data.FavX;
  public int? FavY => m_data.FavY;
  public void SetFav( int x, int y ) { m_data.FavX = x; m_data.FavY = y; Save(); }

  public int  LastTab             => m_data.LastTab;
  public void SetLastTab( int v ) { m_data.LastTab = v; Save(); }
}
