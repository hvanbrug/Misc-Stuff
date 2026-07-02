using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Modal editor for a heading (<see cref="SectionDef"/>): its label, start column and span
/// (in cell-width intervals). On OK it writes the values back into the supplied object and
/// returns true; the caller persists and reloads. Styled like the button dialog via
/// <see cref="DialogChrome"/>.
/// </summary>
internal static class HeadingEditDialog
{
  public static bool Show( SectionDef s, int columns, string title = "Edit heading" )
  {
    bool saved = false;
    int  cols  = Math.Max( 1, columns );

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

    var root = new StackPanel { Margin = new Thickness( 14 ), Width = 360 };
    root.Children.Add( new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeights.Bold,
                                       Foreground = B( "TextPrimary" ), Margin = new Thickness( 0, 0, 0, 12 ) } );

    root.Children.Add( Label( B, "Heading text (blank = a plain divider line)" ) );
    TextBox name = Box( B, s.Name );
    name.Margin = new Thickness( 0, 0, 0, 12 );
    root.Children.Add( name );

    TextBox startCol = Small( B, Math.Max( 0, s.Col ).ToString( CultureInfo.InvariantCulture ) );
    int     defSpan  = s.Span > 0 ? s.Span : Math.Max( 1, cols - Math.Max( 0, s.Col ) );
    TextBox span     = Small( B, defSpan.ToString( CultureInfo.InvariantCulture ) );
    var shortRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness( 0, 0, 0, 12 ) };
    shortRow.Children.Add( Labeled( B, "Start column", DialogControls.Spin( B, startCol, 0 ), 0 ) );
    shortRow.Children.Add( Labeled( B, "Span (cols)",  DialogControls.Spin( B, span, 1 ), 16 ) );
    root.Children.Add( shortRow );

    root.Children.Add( Label( B, "Text alignment" ) );
    FrameworkElement alignPicker = DialogControls.AlignPicker( s.Align, out Func<string> getAlign );
    alignPicker.Margin = new Thickness( 0, 0, 0, 12 );
    root.Children.Add( alignPicker );

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
      if( !int.TryParse( startCol.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int c ) || c < 0 || c >= cols )
      {
        error.Text = $"Start column must be between 0 and {cols - 1}."; return;
      }
      if( !int.TryParse( span.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int sp ) || sp < 1 || c + sp > cols )
      {
        error.Text = $"Span must be between 1 and {cols - c} at this start column."; return;
      }

      s.Name  = name.Text ?? "";
      s.Col   = c;
      s.Span  = sp;
      s.Align = getAlign();
      saved            = true;
      win.DialogResult = true;
    };

    win.Content = root;
    win.Loaded += ( _, _ ) => name.Focus();
    win.ShowDialog();
    return saved;
  }

  // ── Builders (mirroring ButtonEditDialog's styling) ──────────────
  private static TextBox Box( Func<string, Brush> B, string value ) => new()
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

  private static TextBox Small( Func<string, Brush> B, string value )
  {
    TextBox box = Box( B, value );
    box.Width               = 64;
    box.HorizontalAlignment = HorizontalAlignment.Left;
    return box;
  }

  private static StackPanel Labeled( Func<string, Brush> B, string label, FrameworkElement control, double leftMargin )
  {
    var sp = new StackPanel { Margin = new Thickness( leftMargin, 0, 0, 0 ) };
    sp.Children.Add( Label( B, label ) );
    sp.Children.Add( control );
    return sp;
  }

  private static TextBlock Label( Func<string, Brush> B, string text ) => new()
  {
    Text       = text,
    Foreground = B( "TextSecondary" ),
    FontSize   = 11,
    Margin     = new Thickness( 0, 0, 0, 3 ),
  };
}
