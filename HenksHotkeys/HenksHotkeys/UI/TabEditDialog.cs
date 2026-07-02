using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Modal editor for a data tab's own settings (<see cref="TabEntry"/>) — name, column count,
/// button font size / family, button size, and the layout toggles. On OK it writes the values
/// back into the supplied entry and returns true; the caller persists and reloads. Used for both
/// "Add tab…" (a fresh entry) and "Edit tab…" (an existing data tab). Styled via
/// <see cref="DialogChrome"/> to match the button / heading editors.
/// </summary>
internal static class TabEditDialog
{
  public static bool Show( TabEntry t, string title = "Edit tab" )
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

    var root = new StackPanel { Margin = new Thickness( 14 ), Width = 380 };
    root.Children.Add( Header( B, title ) );

    // ── Name ──
    root.Children.Add( Label( B, "Tab name" ) );
    TextBox name = StyledBox( B, t.Name ?? "" );
    name.Margin = new Thickness( 0, 0, 0, 12 );
    root.Children.Add( name );

    // ── Columns + font size ──
    TextBox columns  = SmallBox( B, t.Columns.ToString( CultureInfo.InvariantCulture ), 52 );
    TextBox fontSize = SmallBox( B, t.FontSize.ToString( CultureInfo.InvariantCulture ), 52 );
    var row1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness( 0, 0, 0, 12 ) };
    row1.Children.Add( Labeled( B, "Columns", DialogControls.Spin( B, columns, 1 ), 0 ) );
    row1.Children.Add( Labeled( B, "Font size (pt)", DialogControls.Spin( B, fontSize, 6 ), 16 ) );
    root.Children.Add( row1 );

    // ── Font family ──
    root.Children.Add( Label( B, "Font family" ) );
    TextBox fontName = StyledBox( B, t.FontName );
    fontName.Margin = new Thickness( 0, 0, 0, 12 );
    root.Children.Add( fontName );

    // ── Button width + height ──
    TextBox btnW = SmallBox( B, t.ButtonWidth.ToString( CultureInfo.InvariantCulture ), 52 );
    TextBox btnH = SmallBox( B, t.ButtonHeight.ToString( CultureInfo.InvariantCulture ), 52 );
    var row2 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness( 0, 0, 0, 12 ) };
    row2.Children.Add( Labeled( B, "Button width (px)",  DialogControls.Spin( B, btnW, 1 ), 0 ) );
    row2.Children.Add( Labeled( B, "Button height (px)", DialogControls.Spin( B, btnH, 1 ), 16 ) );
    root.Children.Add( row2 );

    // ── Layout toggles ──
    CheckBox proportional = Check( B, root, "Proportional — buttons fill the tab width",  t.Proportional );
    CheckBox square       = Check( B, root, "Square — button height tracks its width",    t.Square );
    CheckBox emojiImages  = Check( B, root, "Render emoji as images",                     t.EmojiImages );
    CheckBox stripEmojis  = Check( B, root, "Strip emoji from sent text",                 t.StripEmojis );

    var error = new TextBlock { Foreground = Brushes.IndianRed, TextWrapping = TextWrapping.Wrap, Margin = new Thickness( 0, 8, 0, 8 ) };
    root.Children.Add( error );

    var bar    = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    var ok     = new Button { Content = "OK",     Margin = new Thickness( 0, 0, 8, 0 ), IsDefault = true };
    var cancel = new Button { Content = "Cancel", IsCancel = true };
    bar.Children.Add( ok );
    bar.Children.Add( cancel );
    root.Children.Add( bar );

    ok.Click += ( _, _ ) =>
    {
      if( name.Text.Trim().Length == 0 )
      {
        error.Text = "The tab needs a name."; return;
      }
      if( !int.TryParse( columns.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int cols ) || cols < 1 )
      {
        error.Text = "Columns must be a whole number ≥ 1."; return;
      }
      if( !double.TryParse( fontSize.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double fs ) || fs < 1 )
      {
        error.Text = "Font size must be a number ≥ 1."; return;
      }
      if( !int.TryParse( btnW.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int bw ) || bw < 1 ||
          !int.TryParse( btnH.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int bh ) || bh < 1 )
      {
        error.Text = "Button width and height must be whole numbers ≥ 1."; return;
      }

      t.Name         = name.Text.Trim();
      t.Columns      = cols;
      t.FontSize     = fs;
      t.FontName     = fontName.Text.Trim().Length == 0 ? "Segoe UI" : fontName.Text.Trim();
      t.ButtonWidth  = bw;
      t.ButtonHeight = bh;
      t.Proportional = proportional.IsChecked == true;
      t.Square       = square.IsChecked       == true;
      t.EmojiImages  = emojiImages.IsChecked  == true;
      t.StripEmojis  = stripEmojis.IsChecked  == true;

      saved            = true;
      win.DialogResult = true;
    };

    win.Content = root;
    win.Loaded += ( _, _ ) => name.Focus();
    win.ShowDialog();
    return saved;
  }

  // ── Builders (mirroring ButtonEditDialog for a consistent look) ───
  private static Border Header( Func<string, Brush> B, string title )
  {
    var titles = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
    titles.Children.Add( new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeights.Bold, Foreground = B( "TextPrimary" ) } );
    titles.Children.Add( new TextBlock { Text = "Set the tab’s name, columns and button look.", FontSize = 11, Foreground = B( "AccentText" ) } );

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
