using System.Drawing;
using System.Windows.Forms;

namespace HenksHotkeys.UI;

/// <summary>
/// A TabControl that paints its strip dark in dark mode (the standard control
/// has no dark style, so its labels and frame stay light otherwise — see
/// Theme.DarkenTabControl / PaintTabControl in Theme.ahk). In light mode it is a
/// plain TabControl.
/// </summary>
internal sealed class DarkTabControl : TabControl
{
  private readonly bool m_isDark;

  public DarkTabControl()
  {
    m_isDark = Theme.IsDark;
    if( m_isDark )
    {
      DrawMode = TabDrawMode.OwnerDrawFixed;
      SetStyle( ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true );
    }
  }

  protected override void OnDrawItem( DrawItemEventArgs e )
  {
    if( !m_isDark )
    {
      base.OnDrawItem( e );
      return;
    }

    Graphics  g    = e.Graphics;
    Rectangle rect = GetTabRect( e.Index );
    bool      sel  = e.Index == SelectedIndex;

    using( var bg = new SolidBrush( sel ? Theme.ButtonPressed : Theme.ButtonFace ) )
    {
      g.FillRectangle( bg, rect );
    }

    TextRenderer.DrawText( g, TabPages[e.Index].Text, Font, rect, Theme.DarkText,
                           TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                           | TextFormatFlags.SingleLine );
  }

  protected override void OnPaintBackground( PaintEventArgs pevent )
  {
    if( m_isDark )
    {
      pevent.Graphics.Clear( Theme.DarkBackground );
      return;
    }
    base.OnPaintBackground( pevent );
  }
}
