using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;

namespace HenksHotkeys.UI;

/// <summary>
/// Shared visual treatment for the app's modal dialogs, matching the layered
/// light/dark look of the NetworkShares tool (window &lt; card &lt; control greys, a
/// muted accent header bar, rounded buttons). The palette is exposed as window
/// resources under the NetworkShares names so dialog code can pull a brush by key,
/// and an implicit rounded-<see cref="Button"/> style is applied window-wide.
/// </summary>
internal static class DialogChrome
{
  private static Color C( byte r, byte g, byte b ) => Color.FromRgb( r, g, b );

  private static readonly Dictionary<string, Color> Light = new()
  {
    ["WindowBg"]        = C( 0xF3, 0xF3, 0xF3 ),
    ["CardBg"]          = C( 0xFF, 0xFF, 0xFF ),
    ["CardBorder"]      = C( 0xE0, 0xE0, 0xE0 ),
    ["AccentBarBg"]     = C( 0xE8, 0xF0, 0xFE ),
    ["AccentBarBorder"] = C( 0xC5, 0xD6, 0xF2 ),
    ["AccentText"]      = C( 0x5B, 0x6B, 0x8C ),
    ["TextPrimary"]     = C( 0x22, 0x22, 0x22 ),
    ["TextBody"]        = C( 0x44, 0x44, 0x44 ),
    ["TextSecondary"]   = C( 0x75, 0x75, 0x75 ),
    ["ControlBg"]       = C( 0xFF, 0xFF, 0xFF ),
    ["ControlBorder"]   = C( 0xC8, 0xC8, 0xC8 ),
    ["ControlHover"]    = C( 0xF0, 0xF0, 0xF0 ),
    ["ControlPressed"]  = C( 0xE4, 0xE4, 0xE4 ),
    ["InputBg"]         = C( 0xFF, 0xFF, 0xFF ),
    ["SwitchOn"]        = C( 0x2F, 0x6F, 0xD6 ),
  };

  private static readonly Dictionary<string, Color> Dark = new()
  {
    ["WindowBg"]        = C( 0x1E, 0x1E, 0x20 ),
    ["CardBg"]          = C( 0x2A, 0x2A, 0x2D ),
    ["CardBorder"]      = C( 0x3C, 0x3C, 0x40 ),
    ["AccentBarBg"]     = C( 0x26, 0x33, 0x49 ),
    ["AccentBarBorder"] = C( 0x39, 0x4B, 0x68 ),
    ["AccentText"]      = C( 0x9D, 0xB4, 0xD8 ),
    ["TextPrimary"]     = C( 0xE8, 0xE8, 0xE8 ),
    ["TextBody"]        = C( 0xC4, 0xC4, 0xC4 ),
    ["TextSecondary"]   = C( 0x9A, 0x9A, 0x9E ),
    ["ControlBg"]       = C( 0x3A, 0x3A, 0x3E ),
    ["ControlBorder"]   = C( 0x55, 0x55, 0x5A ),
    ["ControlHover"]    = C( 0x46, 0x46, 0x4B ),
    ["ControlPressed"]  = C( 0x52, 0x52, 0x58 ),
    ["InputBg"]         = C( 0x33, 0x33, 0x37 ),
    ["SwitchOn"]        = C( 0x4C, 0x8B, 0xF5 ),
  };

  /// <summary>Theme a dialog: install the palette + rounded-button style, set the
  /// window background, and darken the title bar when in dark mode.</summary>
  public static void Apply( Window win )
  {
    var rd = new ResourceDictionary();
    foreach( (string key, Color color) in ( Theme.IsDark ? Dark : Light ) )
    {
      var brush = new SolidColorBrush( color );
      brush.Freeze();
      rd[key] = brush;
    }
    rd.MergedDictionaries.Add( Styles() );
    win.Resources  = rd;
    win.Background  = (Brush)rd["WindowBg"];
    win.SourceInitialized += ( _, _ ) =>
      Theme.ApplyDarkFrame( new WindowInteropHelper( win ).Handle );
  }

  public static Brush Brush( Window win, string key ) => (Brush)win.FindResource( key );

  // Implicit styles for the dialog: a NetworkShares rounded Button, and a toggle-switch
  // CheckBox (pill track + sliding knob, accent colour when on).
  private static ResourceDictionary Styles()
  {
    const string xaml = @"
<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Style TargetType='Button'>
    <Setter Property='Padding'     Value='12,5'/>
    <Setter Property='MinWidth'    Value='84'/>
    <Setter Property='FontSize'    Value='12'/>
    <Setter Property='Cursor'      Value='Hand'/>
    <Setter Property='Foreground'  Value='{DynamicResource TextPrimary}'/>
    <Setter Property='Background'  Value='{DynamicResource ControlBg}'/>
    <Setter Property='BorderBrush' Value='{DynamicResource ControlBorder}'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='Button'>
          <Border x:Name='bd' CornerRadius='5' BorderThickness='1'
                  Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                  Padding='{TemplateBinding Padding}' SnapsToDevicePixels='True'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='bd' Property='Background' Value='{DynamicResource ControlHover}'/></Trigger>
            <Trigger Property='IsPressed'   Value='True'><Setter TargetName='bd' Property='Background' Value='{DynamicResource ControlPressed}'/></Trigger>
            <Trigger Property='IsEnabled'   Value='False'><Setter TargetName='bd' Property='Opacity' Value='0.45'/></Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style TargetType='CheckBox'>
    <Setter Property='Foreground' Value='{DynamicResource TextBody}'/>
    <Setter Property='Cursor'     Value='Hand'/>
    <Setter Property='FontSize'   Value='12'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='CheckBox'>
          <StackPanel Orientation='Horizontal' Background='Transparent'>
            <Border x:Name='track' Width='36' Height='20' CornerRadius='10' VerticalAlignment='Center'
                    Background='{DynamicResource ControlBg}' BorderBrush='{DynamicResource ControlBorder}' BorderThickness='1'>
              <Border x:Name='knob' Width='14' Height='14' CornerRadius='7' Margin='2'
                      HorizontalAlignment='Left' Background='{DynamicResource TextSecondary}'/>
            </Border>
            <ContentPresenter Margin='9,0,0,0' VerticalAlignment='Center' RecognizesAccessKey='True'/>
          </StackPanel>
          <ControlTemplate.Triggers>
            <Trigger Property='IsChecked' Value='True'>
              <Setter TargetName='track' Property='Background'          Value='{DynamicResource SwitchOn}'/>
              <Setter TargetName='track' Property='BorderBrush'         Value='{DynamicResource SwitchOn}'/>
              <Setter TargetName='knob'  Property='Background'          Value='White'/>
              <Setter TargetName='knob'  Property='HorizontalAlignment' Value='Right'/>
            </Trigger>
            <Trigger Property='IsMouseOver' Value='True'>
              <Setter TargetName='track' Property='BorderBrush' Value='{DynamicResource ControlHover}'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='False'>
              <Setter Property='Opacity' Value='0.45'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
</ResourceDictionary>";
    return (ResourceDictionary)XamlReader.Parse( xaml );
  }
}
