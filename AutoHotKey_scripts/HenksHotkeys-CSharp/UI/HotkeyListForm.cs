using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// A simple read-only list of the registered hotkeys and their descriptions,
/// shown by the ^+a "list hotkeys" command (ListHotkeys in HenksHotkeys.ahk).
/// </summary>
internal sealed class HotkeyListForm : Form
{
  public HotkeyListForm()
  {
    Text          = "Henk's Hotkeys — registered hotkeys";
    StartPosition = FormStartPosition.CenterScreen;
    Size          = new Size( 520, 600 );
    MinimizeBox   = false;
    MaximizeBox   = false;

    var box = new TextBox
    {
      Multiline  = true,
      ReadOnly   = true,
      ScrollBars = ScrollBars.Vertical,
      Dock       = DockStyle.Fill,
      Font       = new Font( "Consolas", 9f ),
      WordWrap   = false,
      Text       = BuildText()
    };

    if( Theme.IsDark )
    {
      BackColor     = Theme.DarkBackground;
      box.BackColor = Theme.DarkBackground;
      box.ForeColor = Theme.DarkText;
      box.BorderStyle = BorderStyle.None;
    }

    Controls.Add( box );
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
