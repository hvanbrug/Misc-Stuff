using System.Text;

namespace HenksHotkeys.Core;

/// <summary>
/// Minimal INI persistence for the single [Window] section the app uses,
/// mirroring the IniFile.ahk helpers (WndOpen, Collapsed, ClipSendMode,
/// StripCommentEmojis, Height, X, Y, FavX, FavY, LastTab).
/// Values are read/written immediately so the file matches the AHK behaviour.
/// </summary>
internal sealed class IniFile
{
  private const string m_section = "Window";

  private readonly string                     m_path;
  private readonly Dictionary<string, string> m_values = new( StringComparer.OrdinalIgnoreCase );

  public IniFile( string path )
  {
    m_path = path;
    Load();
  }

  private void Load()
  {
    m_values.Clear();
    if( !File.Exists( m_path ) )
    {
      return;
    }

    bool inSection = false;
    foreach( string raw in File.ReadAllLines( m_path ) )
    {
      string line = raw.Trim();
      if( line.Length == 0 || line.StartsWith( ';' ) )
      {
        continue;
      }
      if( line.StartsWith( '[' ) && line.EndsWith( ']' ) )
      {
        inSection = string.Equals( line[1..^1].Trim(), m_section, StringComparison.OrdinalIgnoreCase );
        continue;
      }
      if( !inSection )
      {
        continue;
      }
      int eq = line.IndexOf( '=' );
      if( eq <= 0 )
      {
        continue;
      }
      string key = line[..eq].Trim();
      string val = line[(eq + 1)..].Trim();
      m_values[key] = val;
    }
  }

  private void Save()
  {
    var sb = new StringBuilder();
    sb.Append( '[' ).Append( m_section ).Append( ']' ).Append( "\r\n" );
    foreach( var kv in m_values )
    {
      sb.Append( kv.Key ).Append( '=' ).Append( kv.Value ).Append( "\r\n" );
    }
    File.WriteAllText( m_path, sb.ToString(), new UTF8Encoding( false ) );
  }

  public string ReadString( string key, string fallback )
  {
    return m_values.TryGetValue( key, out var v ) ? v : fallback;
  }

  public int ReadInt( string key, int fallback )
  {
    return m_values.TryGetValue( key, out var v ) && int.TryParse( v, out int n ) ? n : fallback;
  }

  public bool ReadBool( string key, bool fallback )
  {
    return ReadInt( key, fallback ? 1 : 0 ) == 1;
  }

  public void Write( string key, string value )
  {
    m_values[key] = value;
    Save();
  }

  public void Write( string key, int value )   => Write( key, value.ToString() );
  public void Write( string key, bool value )  => Write( key, value ? "1" : "0" );

  // ── Named accessors (parallel to INI_* in IniFile.ahk) ───────────
  public bool IsWndOpen           => ReadBool( "WndOpen", false );
  public bool IsCollapsed         => ReadBool( "Collapsed", false );
  public bool IsClipSendMode      => ReadBool( "ClipSendMode", false );
  public bool IsStripCommentEmojis=> ReadBool( "StripCommentEmojis", false );

  public void SetWndOpen( bool v )            => Write( "WndOpen", v );
  public void SetCollapsed( bool v )          => Write( "Collapsed", v );
  public void SetClipSendMode( bool v )       => Write( "ClipSendMode", v );
  public void SetStripCommentEmojis( bool v ) => Write( "StripCommentEmojis", v );

  public int  WndHeight              => ReadInt( "Height", 0 );
  public void SetWndHeight( int v )  => Write( "Height", v );

  public int? WndX => m_values.ContainsKey( "X" ) ? ReadInt( "X", 0 ) : null;
  public int? WndY => m_values.ContainsKey( "Y" ) ? ReadInt( "Y", 0 ) : null;
  public void SetWndPos( int x, int y ) { Write( "X", x ); Write( "Y", y ); }

  public int? FavX => m_values.ContainsKey( "FavX" ) ? ReadInt( "FavX", 0 ) : null;
  public int? FavY => m_values.ContainsKey( "FavY" ) ? ReadInt( "FavY", 0 ) : null;
  public void SetFav( int x, int y ) { Write( "FavX", x ); Write( "FavY", y ); }

  public int  LastTab             => ReadInt( "LastTab", 1 );
  public void SetLastTab( int v ) => Write( "LastTab", v );
}
