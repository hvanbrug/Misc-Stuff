using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HenksHotkeys.UI;

/// <summary>Small reusable controls shared by the button / heading dialogs: a numeric
/// up-down (spinner) wrapping a TextBox, and a mutually-exclusive Left/Center/Right
/// alignment picker (radio buttons styled as toggle buttons by <see cref="DialogChrome"/>).</summary>
internal static class DialogControls
{
  /// <summary>Wrap <paramref name="box"/> with ▲/▼ buttons that nudge the whole number it
  /// holds (clamped at <paramref name="min"/>). Returns the composite to place in the layout;
  /// the caller keeps <paramref name="box"/> to read the value.</summary>
  public static FrameworkElement Spin( Func<string, Brush> B, TextBox box, int min )
  {
    Button Nudge( string glyph, int delta )
    {
      var btn = new Button
      {
        Content         = glyph,
        Width           = 18,
        Height          = 13,
        MinWidth        = 0,
        Padding         = new Thickness( 0 ),
        FontSize        = 7,
        Cursor          = Cursors.Hand,
        Foreground      = B( "TextBody" ),
        Background      = B( "ControlBg" ),
        BorderBrush     = B( "ControlBorder" ),
        BorderThickness = new Thickness( 1 ),
      };
      btn.Click += ( _, _ ) =>
      {
        int v = int.TryParse( box.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n ) ? n : min;
        box.Text = Math.Max( min, v + delta ).ToString( CultureInfo.InvariantCulture );
      };
      return btn;
    }

    var stack = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness( 2, 0, 0, 0 ) };
    stack.Children.Add( Nudge( "▲", +1 ) );
    stack.Children.Add( Nudge( "▼", -1 ) );

    var row = new StackPanel { Orientation = Orientation.Horizontal };
    row.Children.Add( box );
    row.Children.Add( stack );
    return row;
  }

  /// <summary>A Left / Center / Right picker as three mutually-exclusive toggle-styled radio
  /// buttons (grouped by their shared parent). Returns the panel; <paramref name="selected"/>
  /// reads the chosen value ("left"/"center"/"right").</summary>
  public static FrameworkElement AlignPicker( string current, out Func<string> selected )
  {
    RadioButton Make( string label, string val ) => new()
    {
      Content   = label,
      IsChecked = string.Equals( current, val, StringComparison.OrdinalIgnoreCase ),
      Width     = 66,
      Height    = 28,
      Margin    = new Thickness( 0, 0, 6, 0 ),
    };

    RadioButton left   = Make( "Left",   "left" );
    RadioButton center = Make( "Center", "center" );
    RadioButton right  = Make( "Right",  "right" );
    if( left.IsChecked != true && center.IsChecked != true && right.IsChecked != true )
    {
      left.IsChecked = true; // fall back to left when the stored value is unrecognised
    }

    var panel = new StackPanel { Orientation = Orientation.Horizontal };
    panel.Children.Add( left );
    panel.Children.Add( center );
    panel.Children.Add( right );

    selected = () => center.IsChecked == true ? "center" : right.IsChecked == true ? "right" : "left";
    return panel;
  }
}
