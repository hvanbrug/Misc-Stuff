using System.Windows;
using System.Windows.Controls;

namespace NetworkShares;

public partial class MainWindow : Window
{
  private readonly MainViewModel m_vm = new();

  public MainWindow()
  {
    InitializeComponent();
    DataContext = m_vm;
    Loaded += async ( _, _ ) => await m_vm.RefreshAsync();
  }

  private void Log_TextChanged( object sender, TextChangedEventArgs e ) => LogBox.ScrollToEnd();
}
