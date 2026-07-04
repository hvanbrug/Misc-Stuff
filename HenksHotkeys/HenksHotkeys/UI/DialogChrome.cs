using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using HenksHotkeys.Core;

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
  /// <summary>Theme a dialog: install the shared palette + rounded-button / switch
  /// styles into the window scope, set the background, and darken / round the frame.</summary>
  public static void Apply( Window win )
  {
    var rd = new ResourceDictionary();
    Palette.Install( rd, Theme.IsDark );      // same palette as the main window
    rd.MergedDictionaries.Add( Styles() );
    win.Resources  = rd;
    win.Background  = (Brush)rd["WindowBg"];
    win.SourceInitialized += ( _, _ ) =>
    {
      IntPtr h = new WindowInteropHelper( win ).Handle;
      Theme.ApplyDarkFrame( h );
      Theme.ApplyRoundedCorners( h );
    };

    // The app's main window is a non-activating tool window (WS_EX_NOACTIVATE), so when it spawns a
    // dialog the process usually isn't the foreground one and Windows quietly refuses to activate the
    // dialog — it appears on top but keystrokes go elsewhere. Force it to the foreground once shown.
    win.Loaded += ( _, _ ) => ForceForeground( win );
  }

  /// <summary>Bring a just-shown dialog to the foreground and give it the keyboard, working around
  /// the foreground lock by briefly attaching to the current foreground thread's input queue.</summary>
  public static void ForceForeground( Window win )
  {
    AppState.Foreground.ForceForeground( new WindowInteropHelper( win ).Handle ); // Win32 dance (#6)
    win.Activate();                                                               // WPF activation
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

  <Style TargetType='ToggleButton'>
    <Setter Property='FontSize'    Value='12'/>
    <Setter Property='Cursor'      Value='Hand'/>
    <Setter Property='Foreground'  Value='{DynamicResource TextBody}'/>
    <Setter Property='Background'  Value='{DynamicResource ControlBg}'/>
    <Setter Property='BorderBrush' Value='{DynamicResource ControlBorder}'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='ToggleButton'>
          <Border x:Name='bd' CornerRadius='5' BorderThickness='1'
                  Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                  SnapsToDevicePixels='True'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='bd' Property='Background' Value='{DynamicResource ControlHover}'/></Trigger>
            <Trigger Property='IsChecked' Value='True'>
              <Setter TargetName='bd' Property='Background'  Value='{DynamicResource SwitchOn}'/>
              <Setter TargetName='bd' Property='BorderBrush' Value='{DynamicResource SwitchOn}'/>
              <Setter Property='Foreground' Value='White'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='False'><Setter TargetName='bd' Property='Opacity' Value='0.45'/></Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style TargetType='RadioButton'>
    <Setter Property='FontSize'    Value='12'/>
    <Setter Property='Cursor'      Value='Hand'/>
    <Setter Property='Foreground'  Value='{DynamicResource TextBody}'/>
    <Setter Property='Background'  Value='{DynamicResource ControlBg}'/>
    <Setter Property='BorderBrush' Value='{DynamicResource ControlBorder}'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='RadioButton'>
          <Border x:Name='bd' CornerRadius='5' BorderThickness='1'
                  Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                  SnapsToDevicePixels='True'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='bd' Property='Background' Value='{DynamicResource ControlHover}'/></Trigger>
            <Trigger Property='IsChecked' Value='True'>
              <Setter TargetName='bd' Property='Background'  Value='{DynamicResource SwitchOn}'/>
              <Setter TargetName='bd' Property='BorderBrush' Value='{DynamicResource SwitchOn}'/>
              <Setter Property='Foreground' Value='White'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='False'><Setter TargetName='bd' Property='Opacity' Value='0.45'/></Trigger>
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
