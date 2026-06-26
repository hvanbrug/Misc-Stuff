using System.Windows;
using System.Windows.Controls;

namespace HenksHotkeys.UI;

/// <summary>
/// Modal prompt for the master secrets passphrase. When <paramref name="creating"/>
/// it asks the user to choose one (with a confirm field); otherwise it unlocks an
/// existing config. Returns the entered passphrase, or null if cancelled.
/// </summary>
internal static class PassphraseDialog
{
  public static string? Ask( bool creating, bool retry )
  {
    string? result = null;

    var win = new Window
    {
      Title                 = "Henk's Hotkeys — Secrets",
      SizeToContent         = SizeToContent.WidthAndHeight,
      ResizeMode            = ResizeMode.NoResize,
      WindowStartupLocation = WindowStartupLocation.CenterScreen,
      Topmost               = true,
      ShowInTaskbar         = false,
      Background            = Theme.WindowBackground,
    };
    win.SourceInitialized += ( _, _ ) =>
      Theme.ApplyDarkFrame( new System.Windows.Interop.WindowInteropHelper( win ).Handle );

    var panel = new StackPanel { Margin = new Thickness( 16 ), Width = 360 };

    string message = creating
      ? "Set a passphrase to encrypt your secrets.\nUse the same passphrase on your other machine to share them."
      : "Enter your secrets passphrase to unlock the saved entries.";

    panel.Children.Add( Label( message ) );

    if( retry )
    {
      panel.Children.Add( new TextBlock
      {
        Text       = "Incorrect passphrase — try again.",
        Foreground = System.Windows.Media.Brushes.IndianRed,
        Margin     = new Thickness( 0, 0, 0, 8 ),
      } );
    }

    var box = NewBox();
    panel.Children.Add( box );

    PasswordBox? confirm = null;
    if( creating )
    {
      panel.Children.Add( Label( "Confirm passphrase:" ) );
      confirm = NewBox();
      panel.Children.Add( confirm );
    }

    var error = new TextBlock { Foreground = System.Windows.Media.Brushes.IndianRed, Margin = new Thickness( 0, 0, 0, 4 ) };
    panel.Children.Add( error );

    var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    var ok      = new Button { Content = "OK",     Width = 80, Margin = new Thickness( 0, 0, 8, 0 ), IsDefault = true };
    var cancel  = new Button { Content = "Cancel", Width = 80, IsCancel = true };
    buttons.Children.Add( ok );
    buttons.Children.Add( cancel );
    panel.Children.Add( buttons );

    ok.Click += ( _, _ ) =>
    {
      if( box.Password.Length == 0 )
      {
        error.Text = "Please enter a passphrase.";
        return;
      }
      if( creating && box.Password != confirm!.Password )
      {
        error.Text = "The passphrases do not match.";
        return;
      }
      result = box.Password;
      win.DialogResult = true;
    };

    win.Content = panel;
    win.Loaded += ( _, _ ) => box.Focus();
    win.ShowDialog();
    return result;
  }

  private static TextBlock Label( string text ) => new()
  {
    Text         = text,
    Foreground   = Theme.TextColor,
    TextWrapping = TextWrapping.Wrap,
    Margin       = new Thickness( 0, 0, 0, 8 ),
  };

  private static PasswordBox NewBox() => new() { Margin = new Thickness( 0, 0, 0, 8 ), Padding = new Thickness( 3 ) };
}
