using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// A simple read-only list of the registered hotkeys and their descriptions,
/// shown by the ^+a "list hotkeys" command (ListHotkeys in HenksHotkeys.ahk).
/// </summary>
internal sealed class HotkeyListWindow : Window
{
  public HotkeyListWindow()
  {
    Title         = "Henk's Hotkeys — registered hotkeys";
    Width         = 520;
    Height        = 600;
    WindowStartupLocation = WindowStartupLocation.CenterScreen;
    ResizeMode    = ResizeMode.CanResize;

    var box = new TextBox
    {
      IsReadOnly           = true,
      VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
      HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
      FontFamily           = new FontFamily( "Consolas" ),
      FontSize             = 13,
      Text                 = BuildText(),
      BorderThickness      = new Thickness( 0 ),
    };

    if( Theme.IsDark )
    {
      Background     = Theme.DarkBackground;
      box.Background = Theme.DarkBackground;
      box.Foreground = Theme.DarkText;
    }

    Content = box;
  }

  private static string BuildText()
  {
    var sb = new StringBuilder();
    foreach( (string label, string desc) in AppState.HotkeyHelp )
    {
      sb.Append( label.PadRight( 22 ) ).Append( "  " ).AppendLine( desc );
    }
    if( sb.Length == 0 )
    {
      sb.AppendLine( "(no hotkeys registered)" );
    }
    return sb.ToString();
  }
}
