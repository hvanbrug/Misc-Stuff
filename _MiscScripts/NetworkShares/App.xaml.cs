using System.Windows;

namespace NetworkShares;

public partial class App : Application
{
  protected override void OnStartup( StartupEventArgs e )
  {
    // Populate the themed brush resources before the main window is created.
    ThemeManager.Initialize();
    base.OnStartup( e );
  }
}
