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

  public MappingViewModel( string drive, string unc )
  {
    Drive = drive;
    Unc   = unc;
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
  public ObservableCollection<MappingViewModel> Mappings { get; }

  public ICommand ConnectCommand    { get; }
  public ICommand DisconnectCommand { get; }

  private string m_password = "";
  public string Password { get => m_password; set => SetField( ref m_password, value ); }

  public GroupViewModel( ShareGroup group, MainViewModel main )
  {
    Name     = group.Name;
    Subtitle = group.Subtitle;
    Username = group.Username;
    Mappings = new ObservableCollection<MappingViewModel>(
      group.Mappings.Select( m => new MappingViewModel( m.Drive, m.Unc ) ) );

    ConnectCommand    = new RelayCommand( () => _ = main.ConnectGroupAsync( this ),    () => !main.IsBusy );
    DisconnectCommand = new RelayCommand( () => _ = main.DisconnectGroupAsync( this ), () => !main.IsBusy );
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

  public MainViewModel()
  {
    Groups = new ObservableCollection<GroupViewModel>(
      ShareData.Groups.Select( g => new GroupViewModel( g, this ) ) );

    ConnectAllCommand    = new RelayCommand( () => _ = ConnectAllAsync(),    () => !IsBusy );
    DisconnectAllCommand = new RelayCommand( () => _ = DisconnectAllAsync(), () => !IsBusy );
    RefreshCommand       = new RelayCommand( () => _ = RefreshAsync(),       () => !IsBusy );
  }

  // Snapshot a group's data on the UI thread so the background worker never
  // touches the observable collections / view models.
  private static Job ToJob( GroupViewModel g )
    => new( g.Name, g.Username, g.Password, g.Mappings.Select( m => (m.Drive, m.Unc) ).ToList() );

  public Task ConnectGroupAsync( GroupViewModel g )
  {
    var jobs = new List<Job> { ToJob( g ) };
    return RunAsync( $"Connect — {g.Name}", () => ConnectJobs( jobs ) );
  }

  public Task DisconnectGroupAsync( GroupViewModel g )
  {
    var jobs = new List<Job> { ToJob( g ) };
    return RunAsync( $"Disconnect — {g.Name}", () => DisconnectJobs( jobs ) );
  }

  public Task ConnectAllAsync()
  {
    var jobs = Groups.Select( ToJob ).ToList();
    return RunAsync( "Connect — All", () => ConnectJobs( jobs ) );
  }

  public Task DisconnectAllAsync()
  {
    var jobs = Groups.Select( ToJob ).ToList();
    return RunAsync( "Disconnect — All", () => DisconnectJobs( jobs ) );
  }

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

  private static List<string> ConnectJobs( List<Job> jobs )
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
        log.Add( rc == 0 ? $"   {drive}  connected → {unc}" : $"   {drive}  {NetShare.Describe( rc )}" );
      }
    }
    return log;
  }

  private static List<string> DisconnectJobs( List<Job> jobs )
  {
    var log = new List<string>();
    foreach( Job job in jobs )
    {
      if( jobs.Count > 1 ) log.Add( $"[{job.Name}]" );
      foreach( (string drive, string unc) in job.Maps )
      {
        NetShare.StatusResult st = NetShare.GetStatus( drive, unc );
        if( st.Status == NetShare.Status.NotMapped )
        {
          log.Add( $"   {drive}  not connected" );
          continue;
        }
        int rc = NetShare.Disconnect( drive );
        log.Add( rc == 0 ? $"   {drive}  disconnected" : $"   {drive}  {NetShare.Describe( rc )}" );
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
    List<MappingViewModel> items = Groups.SelectMany( g => g.Mappings ).ToList();
    List<(string Drive, string Unc)> pairs = items.Select( m => (m.Drive, m.Unc) ).ToList();

    List<NetShare.StatusResult> statuses = await Task.Run(
      () => pairs.Select( p => NetShare.GetStatus( p.Drive, p.Unc ) ).ToList() );

    for( int i = 0; i < items.Count; i++ )
    {
      items[i].ApplyStatus( statuses[i] );
    }
    foreach( GroupViewModel g in Groups )
    {
      g.RefreshComputed();
    }
  }

  private void AppendLog( string line )
  {
    string stamped = DateTime.Now.ToString( "HH:mm:ss" ) + "  " + line;
    Log = Log.Length == 0 ? stamped : Log + Environment.NewLine + stamped;
  }
}
