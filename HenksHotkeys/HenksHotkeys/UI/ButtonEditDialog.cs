using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Modal editor for a single data-tab <see cref="ButtonDef"/>. On OK it writes the
/// values back into the supplied model object and returns true; the caller persists
/// (<see cref="TabStore.SaveCurrent"/>) and reloads. Styled like the NetworkShares tool
/// via <see cref="DialogChrome"/>: an accent header (with the Blank toggle), multiline
/// auto-growing Text / Description fields, and compact short fields.
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
    };
    DialogChrome.Apply( win );
    Brush B( string key ) => DialogChrome.Brush( win, key );

    var root = new StackPanel { Margin = new Thickness( 14 ), Width = 440 };

    root.Children.Add( Header( B, title ) );

    // ── Text + Description: multiline, wrapping, auto-growing (Enter = newline) ──
    // For a secret, reveal the current value on demand just to edit it (empty if locked).
    TextBox text = Multiline( B, root, "Text / value", b.IsSecret ? ( SecretSession.Reveal( b.Secret ) ?? "" ) : b.Text );
    TextBox desc = Multiline( B, root, "Description (tooltip / face)", b.Desc ?? "" );

    // ── Short fields share one row — they never hold much ──
    TextBox hotkey = SmallBox( B, b.Hotkey ?? "", 150 );
    TextBox width  = SmallBox( B, b.Width.ToString( CultureInfo.InvariantCulture ), 56 );
    var shortRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness( 0, 0, 0, 12 ) };
    shortRow.Children.Add( Labeled( B, "Hotkey (e.g. #!1)", hotkey, 0 ) );
    shortRow.Children.Add( Labeled( B, "Width (columns)", width, 14 ) );
    root.Children.Add( shortRow );

    // ── Checkboxes, grouped logically: display first, then security ──
    CheckBox showText  = Check( B, root, "Show text on the button face",        b.ShowText );
    CheckBox leftAlign = Check( B, root, "Left-align the text",                 b.Align == "left" );
    CheckBox tipText   = Check( B, root, "Include the text in the tooltip",     b.TipText );
    CheckBox sensitive = Check( B, root, "Sensitive — store the value encrypted", b.IsSecret );

    var hint = new TextBlock
    {
      Text         = "Sensitive values are encrypted on save and the face shows the description.",
      Foreground   = B( "TextSecondary" ),
      FontSize     = 11,
      TextWrapping = TextWrapping.Wrap,
      Margin       = new Thickness( 0, 4, 0, 10 ),
    };
    root.Children.Add( hint );

    var error = new TextBlock { Foreground = Brushes.IndianRed, TextWrapping = TextWrapping.Wrap, Margin = new Thickness( 0, 0, 0, 8 ) };
    root.Children.Add( error );

    var bar    = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    var ok     = new Button { Content = "OK",     Margin = new Thickness( 0, 0, 8, 0 ), IsDefault = true };
    var cancel = new Button { Content = "Cancel", IsCancel = true };
    bar.Children.Add( ok );
    bar.Children.Add( cancel );
    root.Children.Add( bar );

    ok.Click += ( _, _ ) =>
    {
      if( !int.TryParse( width.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int w ) || w < 1 )
      {
        error.Text = "Width must be a whole number ≥ 1."; return;
      }

      b.Width  = w;
      b.Align  = leftAlign.IsChecked == true ? "left" : "center";
      b.Desc   = string.IsNullOrEmpty( desc.Text )   ? null : desc.Text;
      b.Hotkey = string.IsNullOrEmpty( hotkey.Text ) ? null : hotkey.Text.Trim();
      b.ShowText = showText.IsChecked == true;
      b.TipText  = tipText.IsChecked  == true;

      string value = text.Text;
      if( sensitive.IsChecked == true )
      {
        // Put the plaintext into Secret (sealed by ProcessSecrets on save); never leave
        // it in Text, which is written to disk in the clear. Re-entering unlocks it.
        b.Secret = value;
        b.Locked = false;
        b.Text   = "";
      }
      else
      {
        b.Secret = null;
        b.Locked = false;
        b.Text   = value;
      }

      saved            = true;
      win.DialogResult = true;
    };

    win.Content = root;
    win.Loaded += ( _, _ ) => text.Focus();
    win.ShowDialog();
    return saved;
  }

  // ── Builders ─────────────────────────────────────────────────────
  private static Border Header( Func<string, Brush> B, string title )
  {
    var titles = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
    titles.Children.Add( new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeights.Bold, Foreground = B( "TextPrimary" ) } );
    titles.Children.Add( new TextBlock { Text = "Set the button’s text, behaviour and look.", FontSize = 11, Foreground = B( "AccentText" ) } );

    return new Border
    {
      Background      = B( "AccentBarBg" ),
      BorderBrush     = B( "AccentBarBorder" ),
      BorderThickness = new Thickness( 1 ),
      CornerRadius    = new CornerRadius( 6 ),
      Padding         = new Thickness( 12 ),
      Margin          = new Thickness( 0, 0, 0, 14 ),
      Child           = titles,
    };
  }

  private static TextBox Multiline( Func<string, Brush> B, Panel host, string label, string value )
  {
    host.Children.Add( Label( B, label ) );
    TextBox box = StyledBox( B, value );
    box.AcceptsReturn = true;                                   // Enter inserts a newline
    box.TextWrapping  = TextWrapping.Wrap;
    box.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
    box.MinLines = 2;                                           // grows / shrinks with content,
    box.MaxLines = 10;                                          // then scrolls past 10 lines
    box.Margin   = new Thickness( 0, 0, 0, 10 );
    host.Children.Add( box );
    return box;
  }

  private static StackPanel Labeled( Func<string, Brush> B, string label, FrameworkElement control, double leftMargin )
  {
    var sp = new StackPanel { Margin = new Thickness( leftMargin, 0, 0, 0 ) };
    sp.Children.Add( Label( B, label ) );
    sp.Children.Add( control );
    return sp;
  }

  private static TextBox SmallBox( Func<string, Brush> B, string value, double width )
  {
    TextBox box = StyledBox( B, value );
    box.Width               = width;
    box.HorizontalAlignment = HorizontalAlignment.Left;
    return box;
  }

  private static TextBox StyledBox( Func<string, Brush> B, string value ) => new()
  {
    Text            = value,
    Background      = B( "InputBg" ),
    Foreground      = B( "TextPrimary" ),
    BorderBrush     = B( "ControlBorder" ),
    BorderThickness = new Thickness( 1 ),
    CaretBrush      = B( "TextPrimary" ),
    Padding         = new Thickness( 6, 4, 6, 4 ),
    FontSize        = 12,
  };

  private static TextBlock Label( Func<string, Brush> B, string text ) => new()
  {
    Text       = text,
    Foreground = B( "TextSecondary" ),
    FontSize   = 11,
    Margin     = new Thickness( 0, 0, 0, 3 ),
  };

  private static CheckBox Check( Func<string, Brush> B, Panel host, string label, bool isChecked )
  {
    var cb = new CheckBox
    {
      Content    = label,
      IsChecked  = isChecked,
      Foreground = B( "TextBody" ),
      Margin     = new Thickness( 0, 0, 0, 6 ),
    };
    host.Children.Add( cb );
    return cb;
  }
}
