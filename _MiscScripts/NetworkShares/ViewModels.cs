using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace NetworkShares;

internal abstract class ObservableObject : INotifyPropertyChanged
{
  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged( [CallerMemberName] string? name = null )
    => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );

  protected bool SetField<T>( ref T field, T value, [CallerMemberName] string? name = null )
  {
    if( EqualityComparer<T>.Default.Equals( field, value ) )
    {
      return false;
    }
    field = value;
    OnPropertyChanged( name );
    return true;
  }
}

internal static class Palette
{
  public static readonly Brush Green  = Frozen( 0x2E, 0x7D, 0x32 );
  public static readonly Brush Orange = Frozen( 0xEF, 0x6C, 0x00 );
  public static readonly Brush Red    = Frozen( 0xC6, 0x28, 0x28 );
  public static readonly Brush Gray   = Frozen( 0x75, 0x75, 0x75 );

  private static Brush Frozen( byte r, byte g, byte b )
  {
    var brush = new SolidColorBrush( Color.FromRgb( r, g, b ) );
    brush.Freeze();
    return brush;
  }
}

internal sealed class MappingViewModel : ObservableObject
{
  public string Drive { get; }
  public string Unc   { get; }

  public ICommand ConnectCommand    { get; }
  public ICommand ReconnectCommand  { get; }
  public ICommand DisconnectCommand { get; }
  public ICommand UnmapCommand      { get; }

  /// <summary>False for discovered "other" mappings — we have no configured target/credentials to map them fresh.</summary>
  public bool CanConnect { get; }

  /// <summary>Current state, used to enable only the actions that make sense.</summary>
  public NetShare.Status State { get; private set; } = NetShare.Status.NotMapped;

  private bool IsMapped => State is NetShare.Status.Connected or NetShare.Status.Unavailable or NetShare.Status.OtherTarget;

  public MappingViewModel( string drive, string unc, GroupViewModel group, MainViewModel main )
  {
    Drive      = drive;
    Unc        = unc;
    CanConnect = group.IsConnectable;

    ConnectCommand    = new RelayCommand( () => _ = main.ConnectMappingAsync( group, this ),    () => !main.IsBusy && CanConnect && State == NetShare.Status.NotMapped );
    ReconnectCommand  = new RelayCommand( () => _ = main.ReconnectMappingAsync( group, this ),  () => !main.IsBusy && State == NetShare.Status.Unavailable );
    DisconnectCommand = new RelayCommand( () => _ = main.DisconnectMappingAsync( group, this ), () => !main.IsBusy && State == NetShare.Status.Connected );
    UnmapCommand      = new RelayCommand( () => _ = main.UnmapMappingAsync( group, this ),      () => !main.IsBusy && IsMapped );
  }

  private string m_statusText = "—";
  public string StatusText { get => m_statusText; private set => SetField( ref m_statusText, value ); }

  private Brush m_statusBrush = Palette.Gray;
  public Brush StatusBrush { get => m_statusBrush; private set => SetField( ref m_statusBrush, value ); }

  public bool IsConnected { get; private set; }

  public void ApplyStatus( NetShare.StatusResult r )
  {
    (string text, Brush brush, bool connected) = r.Status switch
    {
      NetShare.Status.Connected   => ( "Connected",                Palette.Green,  true ),
      NetShare.Status.NotMapped   => ( "Not mapped",               Palette.Gray,   false ),
      NetShare.Status.Unavailable => ( "Disconnected",             Palette.Orange, false ),
      NetShare.Status.OtherTarget => ( $"Other: {r.Remote}",       Palette.Orange, false ),
      _                           => ( NetShare.Describe( r.Code ), Palette.Red,    false ),
    };
    State       = r.Status;
    IsConnected = connected;
    StatusText  = text;
    StatusBrush = brush;
  }
}

internal sealed class GroupViewModel : ObservableObject
{
  public string Name     { get; }
  public string Subtitle { get; }
  public string Username { get; }
  public ObservableCollection<MappingViewModel> Mappings { get; } = new();

  /// <summary>True for the predefined groups; false for the discovered "Other Mappings" group
  /// (which has no Connect button or password field — only Disconnect).</summary>
  public bool IsConnectable { get; }

  public ICommand ConnectCommand    { get; }
  public ICommand ReconnectCommand  { get; }
  public ICommand DisconnectCommand { get; }
  public ICommand UnmapCommand      { get; }

  private string m_password = "";
  public string Password { get => m_password; set => SetField( ref m_password, value ); }

  public GroupViewModel( string name, string subtitle, string username, bool isConnectable, MainViewModel main )
  {
    Name          = name;
    Subtitle      = subtitle;
    Username      = username;
    IsConnectable = isConnectable;

    ConnectCommand    = new RelayCommand( () => _ = main.ConnectGroupAsync( this ),    () => !main.IsBusy && IsConnectable );
    ReconnectCommand  = new RelayCommand( () => _ = main.ReconnectGroupAsync( this ),  () => !main.IsBusy );
    DisconnectCommand = new RelayCommand( () => _ = main.DisconnectGroupAsync( this ), () => !main.IsBusy );
    UnmapCommand      = new RelayCommand( () => _ = main.UnmapGroupAsync( this ),      () => !main.IsBusy );
  }

  public static GroupViewModel FromShareGroup( ShareGroup g, MainViewModel main )
  {
    var vm = new GroupViewModel( g.Name, g.Subtitle, g.Username, isConnectable: true, main );
    foreach( ShareMapping m in g.Mappings )
    {
      vm.Mappings.Add( new MappingViewModel( m.Drive, m.Unc, vm, main ) );
    }
    return vm;
  }

  private string m_overallText = "";
  public string OverallText { get => m_overallText; private set => SetField( ref m_overallText, value ); }

  private Brush m_overallBrush = Palette.Gray;
  public Brush OverallBrush { get => m_overallBrush; private set => SetField( ref m_overallBrush, value ); }

  public void RefreshComputed()
  {
    int total = Mappings.Count;
    int conn  = Mappings.Count( m => m.IsConnected );
    OverallText  = $"{conn}/{total} connected";
    OverallBrush = conn == total ? Palette.Green : conn == 0 ? Palette.Gray : Palette.Orange;
  }
}

internal sealed class MainViewModel : ObservableObject
{
  private sealed record Job( string Name, string User, string Pass, List<(string Drive, string Unc)> Maps );

  public ObservableCollection<GroupViewModel> Groups { get; }

  public ICommand ConnectAllCommand    { get; }
  public ICommand DisconnectAllCommand { get; }
  public ICommand RefreshCommand       { get; }

  private bool m_isBusy;
  public bool IsBusy
  {
    get => m_isBusy;
    private set { if( SetField( ref m_isBusy, value ) ) CommandManager.InvalidateRequerySuggested(); }
  }

  private string m_log = "";
  public string Log { get => m_log; private set => SetField( ref m_log, value ); }

  private readonly List<GroupViewModel> m_predefined;
  private readonly GroupViewModel        m_otherGroup;

  public MainViewModel()
  {
    m_predefined = ShareData.Groups.Select( g => GroupViewModel.FromShareGroup( g, this ) ).ToList();
    m_otherGroup = new GroupViewModel( "Other Mappings", "drives mapped outside the groups above", "", isConnectable: false, this );

    Groups = new ObservableCollection<GroupViewModel>( m_predefined );

    ConnectAllCommand    = new RelayCommand( () => _ = ConnectAllAsync(),    () => !IsBusy );
    DisconnectAllCommand = new RelayCommand( () => _ = DisconnectAllAsync(), () => !IsBusy );
    RefreshCommand       = new RelayCommand( () => _ = RefreshAsync(),       () => !IsBusy );
  }

  // Snapshot a group's data on the UI thread so the background worker never
  // touches the observable collections / view models. The password is passed
  // explicitly so "reconnect" can force saved-credential use (blank password).
  private static Job ToJob( GroupViewModel g, string pass )
    => new( g.Name, g.Username, pass, g.Mappings.Select( m => (m.Drive, m.Unc) ).ToList() );

  private static Job OneJob( GroupViewModel g, MappingViewModel m, string pass )
    => new( g.Name, g.Username, pass, new List<(string, string)> { (m.Drive, m.Unc) } );

  // ── Per-entry ────────────────────────────────────────────────────
  public Task ConnectMappingAsync( GroupViewModel g, MappingViewModel m )
    => RunAsync( $"Connect — {m.Drive}", () => ConnectJobs( new List<Job> { OneJob( g, m, g.Password ) }, "connected" ) );

  public Task ReconnectMappingAsync( GroupViewModel g, MappingViewModel m )
    => RunAsync( $"Reconnect — {m.Drive}", () => ConnectJobs( new List<Job> { OneJob( g, m, "" ) }, "reconnected" ) );

  public Task DisconnectMappingAsync( GroupViewModel g, MappingViewModel m )
    => RunAsync( $"Disconnect — {m.Drive}", () => DisconnectJobs( new List<Job> { OneJob( g, m, "" ) }, keepMapping: true ) );

  public Task UnmapMappingAsync( GroupViewModel g, MappingViewModel m )
    => RunAsync( $"Unmap — {m.Drive}", () => DisconnectJobs( new List<Job> { OneJob( g, m, "" ) }, keepMapping: false ) );

  // ── Per-group ────────────────────────────────────────────────────
  public Task ConnectGroupAsync( GroupViewModel g )
    => RunAsync( $"Connect — {g.Name}", () => ConnectJobs( new List<Job> { ToJob( g, g.Password ) }, "connected" ) );

  public Task ReconnectGroupAsync( GroupViewModel g )
    => RunAsync( $"Reconnect — {g.Name}", () => ConnectJobs( new List<Job> { ToJob( g, "" ) }, "reconnected" ) );

  public Task DisconnectGroupAsync( GroupViewModel g )
    => RunAsync( $"Disconnect — {g.Name}", () => DisconnectJobs( new List<Job> { ToJob( g, "" ) }, keepMapping: true ) );

  public Task UnmapGroupAsync( GroupViewModel g )
    => RunAsync( $"Unmap — {g.Name}", () => DisconnectJobs( new List<Job> { ToJob( g, "" ) }, keepMapping: false ) );

  // ── "All" — managed (predefined) groups only; discovered "other"
  //    mappings are left alone and handled via their own buttons. ────
  public Task ConnectAllAsync()
    => RunAsync( "Connect — All", () => ConnectJobs( m_predefined.Select( g => ToJob( g, g.Password ) ).ToList(), "connected" ) );

  public Task DisconnectAllAsync()
    => RunAsync( "Disconnect — All", () => DisconnectJobs( m_predefined.Select( g => ToJob( g, "" ) ).ToList(), keepMapping: true ) );

  public async Task RefreshAsync()
  {
    if( IsBusy )
    {
      return;
    }
    IsBusy = true;
    try     { await RefreshStatusesAsync(); }
    finally { IsBusy = false; }
  }

  private static List<string> ConnectJobs( List<Job> jobs, string verb )
  {
    var log = new List<string>();
    foreach( Job job in jobs )
    {
      if( jobs.Count > 1 ) log.Add( $"[{job.Name}]" );
      foreach( (string drive, string unc) in job.Maps )
      {
        NetShare.StatusResult st = NetShare.GetStatus( drive, unc );
        if( st.Status == NetShare.Status.Connected )
        {
          log.Add( $"   {drive}  already connected" );
          continue;
        }
        if( st.Status == NetShare.Status.OtherTarget )
        {
          log.Add( $"   {drive}  in use by {st.Remote} — skipped" );
          continue;
        }
        int rc = NetShare.Connect( drive, unc, job.User, job.Pass );
        log.Add( rc == 0 ? $"   {drive}  {verb} → {unc}" : $"   {drive}  {NetShare.Describe( rc )}" );
      }
    }
    return log;
  }

  // keepMapping: drop the live session but keep the persistent mapping (Disconnect);
  // otherwise remove the mapping entirely (Unmap).
  private static List<string> DisconnectJobs( List<Job> jobs, bool keepMapping )
  {
    string verb = keepMapping ? "disconnected" : "unmapped";
    var log = new List<string>();
    foreach( Job job in jobs )
    {
      if( jobs.Count > 1 ) log.Add( $"[{job.Name}]" );
      foreach( (string drive, string unc) in job.Maps )
      {
        NetShare.StatusResult st = NetShare.GetStatus( drive, unc );
        if( st.Status == NetShare.Status.NotMapped )
        {
          log.Add( $"   {drive}  not mapped" );
          continue;
        }
        int rc = NetShare.Disconnect( drive, keepMapping );
        log.Add( rc == 0 ? $"   {drive}  {verb}" : $"   {drive}  {NetShare.Describe( rc )}" );
      }
    }
    return log;
  }

  private async Task RunAsync( string title, Func<List<string>> work )
  {
    if( IsBusy )
    {
      return;
    }
    IsBusy = true;
    AppendLog( "── " + title + " ──" );
    try
    {
      List<string> lines = await Task.Run( work );
      foreach( string line in lines )
      {
        AppendLog( line );
      }
      await RefreshStatusesAsync();
    }
    catch( Exception ex )
    {
      AppendLog( "   unexpected error: " + ex.Message );
    }
    finally
    {
      IsBusy = false;
    }
  }

  private async Task RefreshStatusesAsync()
  {
    List<MappingViewModel> items = m_predefined.SelectMany( g => g.Mappings ).ToList();
    List<(string Drive, string Unc)> pairs = items.Select( m => (m.Drive, m.Unc) ).ToList();

    // Statuses for the predefined mappings + a snapshot of every mapped drive.
    (List<NetShare.StatusResult> statuses, IReadOnlyList<(string Drive, string Remote, NetShare.Status Status)> mapped) =
      await Task.Run( () => (
        pairs.Select( p => NetShare.GetStatus( p.Drive, p.Unc ) ).ToList(),
        NetShare.EnumerateMappedDrives() ) );

    for( int i = 0; i < items.Count; i++ )
    {
      items[i].ApplyStatus( statuses[i] );
    }

    // Anything mapped that isn't one of our known drive letters is an "other" mapping.
    var known = new HashSet<string>( items.Select( m => m.Drive ), StringComparer.OrdinalIgnoreCase );
    var others = mapped.Where( e => !known.Contains( e.Drive ) ).ToList();
    UpdateOtherGroup( others );

    foreach( GroupViewModel g in m_predefined )
    {
      g.RefreshComputed();
    }
    m_otherGroup.RefreshComputed();
  }

  private void UpdateOtherGroup( List<(string Drive, string Remote, NetShare.Status Status)> others )
  {
    m_otherGroup.Mappings.Clear();
    foreach( (string drive, string remote, NetShare.Status status) in others )
    {
      var m = new MappingViewModel( drive, remote, m_otherGroup, this );
      m.ApplyStatus( new NetShare.StatusResult( status, remote, 0 ) );
      m_otherGroup.Mappings.Add( m );
    }

    bool present = Groups.Contains( m_otherGroup );
    if( others.Count > 0 && !present )
    {
      Groups.Add( m_otherGroup );
    }
    else if( others.Count == 0 && present )
    {
      Groups.Remove( m_otherGroup );
    }
  }

  private void AppendLog( string line )
  {
    string stamped = DateTime.Now.ToString( "HH:mm:ss" ) + "  " + line;
    Log = Log.Length == 0 ? stamped : Log + Environment.NewLine + stamped;
  }
}
