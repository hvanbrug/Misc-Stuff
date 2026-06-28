using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Modal editor for a single data-tab <see cref="ButtonDef"/> — every button-specific
/// property. On OK it writes the values back into the supplied model object and returns
/// true; the caller then persists (<see cref="TabStore.SaveCurrent"/>) and reloads.
/// </summary>
internal static class ButtonEditDialog
{
  public static bool Show( ButtonDef b, string title = "Edit button" )
  {
    bool saved = false;

    var win = new Window
    {
      Title                 = "Henk's Hotkeys — " + title,
      SizeToContent         = SizeToContent.WidthAndHeight,
      ResizeMode            = ResizeMode.NoResize,
      WindowStartupLocation = WindowStartupLocation.CenterScreen,
      Topmost               = true,
      ShowInTaskbar         = false,
      Background            = Theme.WindowBackground,
    };
    win.SourceInitialized += ( _, _ ) =>
      Theme.ApplyDarkFrame( new System.Windows.Interop.WindowInteropHelper( win ).Handle );

    var panel = new StackPanel { Margin = new Thickness( 16 ), Width = 380 };

    // A secret button keeps its sent value encrypted and shows its description on the
    // face; the Text box doubles as the plaintext value when Sensitive is ticked.
    TextBox text    = Field( panel, "Text / value:", b.IsSecret ? ( b.Plain ?? "" ) : b.Text );
    TextBox desc    = Field( panel, "Description (tooltip / face):", b.Desc ?? "" );
    TextBox hotkey  = Field( panel, "Hotkey (e.g. #!1):", b.Hotkey ?? "" );
    TextBox width   = Field( panel, "Width (cells):", b.Width.ToString( CultureInfo.InvariantCulture ) );
    TextBox gap     = Field( panel, "Gap before (cells):", b.GapBefore.ToString( CultureInfo.InvariantCulture ) );

    CheckBox leftAlign = Check( panel, "Left-align text", b.Align == "left" );
    CheckBox showText  = Check( panel, "Show text on the button face", b.ShowText );
    CheckBox tipText   = Check( panel, "Include the text in the tooltip", b.TipText );
    CheckBox blank     = Check( panel, "Blank spacer (draws nothing)", b.Blank );
    CheckBox sensitive = Check( panel, "Sensitive — store the value encrypted", b.IsSecret );

    var hint = new TextBlock
    {
      Foreground   = Theme.TextColor,
      Opacity      = 0.7,
      TextWrapping = TextWrapping.Wrap,
      Margin       = new Thickness( 0, 0, 0, 8 ),
      Text         = "When sensitive, the value is encrypted on save and the face shows the description.",
    };
    panel.Children.Add( hint );

    // A blank cell ignores its content, so grey those fields out while it's ticked.
    void SyncBlank()
    {
      bool bl = blank.IsChecked == true;
      text.IsEnabled = desc.IsEnabled = hotkey.IsEnabled = !bl;
      showText.IsEnabled = tipText.IsEnabled = sensitive.IsEnabled = !bl;
    }
    blank.Checked   += ( _, _ ) => SyncBlank();
    blank.Unchecked += ( _, _ ) => SyncBlank();
    SyncBlank();

    var error = new TextBlock { Foreground = Brushes.IndianRed, Margin = new Thickness( 0, 0, 0, 4 ) };
    panel.Children.Add( error );

    var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    var ok      = new Button { Content = "OK",     Width = 80, Margin = new Thickness( 0, 0, 8, 0 ), IsDefault = true };
    var cancel  = new Button { Content = "Cancel", Width = 80, IsCancel = true };
    buttons.Children.Add( ok );
    buttons.Children.Add( cancel );
    panel.Children.Add( buttons );

    ok.Click += ( _, _ ) =>
    {
      if( !int.TryParse( width.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int w ) || w < 1 )
      {
        error.Text = "Width must be a whole number ≥ 1."; return;
      }
      if( !double.TryParse( gap.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double g ) || g < 0 )
      {
        error.Text = "Gap before must be a number ≥ 0."; return;
      }

      b.Width     = w;
      b.GapBefore = g;
      b.Align     = leftAlign.IsChecked == true ? "left" : "center";

      if( blank.IsChecked == true )
      {
        // A blank cell carries no content — clear it all out so nothing stale lingers.
        b.Blank    = true;
        b.Text     = "";
        b.Desc     = null;
        b.Hotkey   = null;
        b.Secret   = null;
        b.Plain    = null;
        b.ShowText = true;
        b.TipText  = false;
      }
      else
      {
        b.Blank    = false;
        b.Desc     = string.IsNullOrEmpty( desc.Text )   ? null : desc.Text;
        b.Hotkey   = string.IsNullOrEmpty( hotkey.Text ) ? null : hotkey.Text.Trim();
        b.ShowText = showText.IsChecked == true;
        b.TipText  = tipText.IsChecked  == true;

        string value = text.Text;
        if( sensitive.IsChecked == true )
        {
          // Route the plaintext into Secret (sealed on save); never leave it in Text,
          // which is written to disk in the clear.
          b.Secret = value;
          b.Plain  = value;
          b.Text   = "";
        }
        else
        {
          b.Secret = null;
          b.Plain  = null;
          b.Text   = value;
        }
      }

      saved            = true;
      win.DialogResult = true;
    };

    win.Content = panel;
    win.Loaded += ( _, _ ) => text.Focus();
    win.ShowDialog();
    return saved;
  }

  private static TextBox Field( Panel host, string label, string value )
  {
    host.Children.Add( new TextBlock { Text = label, Foreground = Theme.TextColor, Margin = new Thickness( 0, 0, 0, 2 ) } );
    var box = new TextBox { Text = value, Padding = new Thickness( 3 ), Margin = new Thickness( 0, 0, 0, 8 ) };
    if( Theme.IsDark )
    {
      box.Background      = s_inputBg;
      box.Foreground      = Theme.TextColor;
      box.CaretBrush      = Theme.TextColor;
      box.BorderBrush     = Theme.BorderColor;
    }
    host.Children.Add( box );
    return box;
  }

  private static CheckBox Check( Panel host, string label, bool isChecked )
  {
    var cb = new CheckBox
    {
      Content    = label,
      IsChecked  = isChecked,
      Foreground = Theme.TextColor,
      Margin     = new Thickness( 0, 0, 0, 8 ),
    };
    host.Children.Add( cb );
    return cb;
  }

  private static readonly Brush s_inputBg = MakeFrozen( 0x2D, 0x2D, 0x30 );

  private static Brush MakeFrozen( byte r, byte g, byte b )
  {
    var brush = new SolidColorBrush( Color.FromRgb( r, g, b ) );
    brush.Freeze();
    return brush;
  }
}
