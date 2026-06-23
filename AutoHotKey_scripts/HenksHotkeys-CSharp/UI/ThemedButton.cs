using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// A push button that renders dark (owner-drawn face + light text/image) when
/// the app is in dark mode, and as a standard themed button in light mode —
/// matching Theme.MakeOwnerDrawn / DrawOwnerButton from Theme.ahk. A double
/// click sends the click text and then an Enter, reproducing the BN_DBLCLK
/// behaviour wired up in UI.ahk (OnButtonDoubleClick → SymbolClick).
/// </summary>
internal sealed class ThemedButton : Button
{
  private const int FaceInset = 2;

  private readonly bool   m_isDark;
  private readonly string m_align;
  private bool            m_pressed;

  [Browsable( false )]
  [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
  public Action ClickAction { get; set; } = static () => { };

  [Browsable( false )]
  [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
  public Bitmap? EmojiImage { get; set; }

  /// <summary>When true, a double-click appends an Enter after the text (symbol
  /// buttons), reproducing the BN_DBLCLK behaviour from UI.ahk. Off for helper
  /// and indicator buttons.</summary>
  [Browsable( false )]
  [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
  public bool SendNewlineOnDoubleClick { get; set; }

  private long m_lastClickMs;

  public ThemedButton( string align )
  {
    m_align  = align;
    m_isDark = Theme.IsDark;

    // NB: do NOT enable ControlStyles.StandardClick here — ButtonBase already
    // raises Click on mouse-up, so adding StandardClick fires Click twice per
    // physical click. Double-click is detected by timing in OnClick instead.

    if( m_isDark )
    {
      SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true );
      FlatStyle = FlatStyle.Flat;
      FlatAppearance.BorderSize = 0;
      BackColor = Theme.DarkBackground;
      ForeColor = Theme.DarkText;
    }
    else
    {
      FlatStyle = FlatStyle.System;
      // Only the left-aligned text buttons (comments, tools, …) need ellipsis;
      // centered single-glyph buttons should always show their glyph in full.
      AutoEllipsis = align == "left";
      TextAlign = align == "left" ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleCenter;
    }
  }

  protected override void OnMouseDown( MouseEventArgs e )
  {
    m_pressed = true;
    if( m_isDark ) Invalidate();
    base.OnMouseDown( e );
  }

  protected override void OnMouseUp( MouseEventArgs e )
  {
    m_pressed = false;
    if( m_isDark ) Invalidate();
    base.OnMouseUp( e );
  }

  protected override void OnClick( EventArgs e )
  {
    base.OnClick( e );

    long now    = Environment.TickCount64;
    bool isPair = now - m_lastClickMs <= SystemInformation.DoubleClickTime;
    m_lastClickMs = now;

    // On the second click of a double-click, send a newline instead of the text
    // again (the first click already sent it) — matching UI.ahk's BN_DBLCLK path.
    if( SendNewlineOnDoubleClick && isPair )
    {
      m_lastClickMs = 0; // reset so a triple-click doesn't chain
      try { TextSender.SendInputKeys( "{Enter}" ); } catch { /* ignore */ }
      return;
    }

    try { ClickAction(); } catch { /* never let a send failure kill the UI */ }
  }

  protected override void OnPaint( PaintEventArgs e )
  {
    if( !m_isDark )
    {
      base.OnPaint( e );
      return;
    }

    Graphics g  = e.Graphics;
    Rectangle r = ClientRectangle;

    // Gap (the cell margin) in the panel colour, then the inset button face.
    using( var bg = new SolidBrush( Theme.DarkBackground ) )
    {
      g.FillRectangle( bg, r );
    }

    Rectangle face = new( r.Left + FaceInset, r.Top + FaceInset,
                          r.Width - FaceInset * 2, r.Height - FaceInset * 2 );

    using( var faceBrush = new SolidBrush( m_pressed ? Theme.ButtonPressed : Theme.ButtonFace ) )
    {
      g.FillRectangle( faceBrush, face );
    }
    using( var pen = new Pen( Theme.ButtonBorder ) )
    {
      g.DrawRectangle( pen, face.Left, face.Top, face.Width - 1, face.Height - 1 );
    }

    if( EmojiImage is not null )
    {
      int ix = face.Left + ( face.Width  - EmojiImage.Width )  / 2;
      int iy = face.Top  + ( face.Height - EmojiImage.Height ) / 2;
      g.DrawImage( EmojiImage, ix, iy, EmojiImage.Width, EmojiImage.Height );
      return;
    }

    if( !string.IsNullOrEmpty( Text ) )
    {
      TextFormatFlags flags = TextFormatFlags.VerticalCenter
                            | TextFormatFlags.SingleLine
                            | ( m_align == "left"
                                ? TextFormatFlags.Left | TextFormatFlags.EndEllipsis
                                : TextFormatFlags.HorizontalCenter );
      TextRenderer.DrawText( g, Text, Font, face, Theme.DarkText, flags );
    }
  }
}
