using System.Drawing;
using System.Windows.Forms;
using HenksHotkeys.Core;
using HenksHotkeys.Emoji;

namespace HenksHotkeys.UI;

/// <summary>
/// Hosts one tab's scrollable button grid. Buttons are laid out absolutely on an
/// inner content panel that slides vertically; a docked scrollbar plus the mouse
/// wheel drive a smooth (lerped) scroll, mirroring the clip/content-panel +
/// animated-scroll design in UITabPage.ahk / UIScrolling.ahk.
/// </summary>
internal sealed class TabPanelControl : Panel
{
  private const double LerpFactor = 0.35;

  private readonly int        m_scrollBarWidth = Dpi.S( 18 );
  private readonly int        m_contentPx;       // content height in physical pixels
  private readonly TabModel   m_model;
  private readonly Panel      m_content;
  private readonly VScrollBar m_scroll;
  private readonly ToolTip    m_tip;
  private readonly System.Windows.Forms.Timer m_anim;

  private int m_scrollY;
  private int m_targetY;

  // Wheel acceleration state (DoWheel in UIScrolling.ahk).
  private long m_lastWheelMs;
  private int  m_accelPixels;

  public TabPanelControl( TabModel model )
  {
    m_model     = model;
    m_contentPx = Dpi.S( model.ContentHeight );
    BackColor   = Theme.WindowBackground;

    m_content = new Panel
    {
      Location  = new Point( 0, 0 ),
      BackColor = Theme.WindowBackground,
      Height    = m_contentPx,
    };
    Controls.Add( m_content );

    m_scroll = new VScrollBar
    {
      Dock     = DockStyle.Right,
      Width    = m_scrollBarWidth,
      Minimum  = 0,
      SmallChange = Dpi.S( 20 ),
    };
    m_scroll.Scroll += ( _, e ) => SetTarget( e.NewValue );
    Controls.Add( m_scroll );

    m_tip = new ToolTip
    {
      AutoPopDelay = 30000,
      InitialDelay = 300,
      ReshowDelay  = 100,
      ShowAlways   = true,
    };

    m_anim = new System.Windows.Forms.Timer { Interval = 16 };
    m_anim.Tick += OnAnimTick;

    BuildButtons();
  }

  private void BuildButtons()
  {
    int emojiPx = Dpi.S( (int)Math.Round( m_model.SymBtnSizeX * 0.8 ) );

    foreach( SymbolElement sym in m_model.Symbols )
    {
      var btn = new ThemedButton( sym.Align )
      {
        ClickAction              = sym.ClickAction,
        SendNewlineOnDoubleClick = true,
        Font                     = Dpi.ScaledFont( m_model.FontName, m_model.FontSize ),
        Size                     = new Size( Dpi.S( sym.W ), Dpi.S( sym.H ) ),
        Location                 = new Point( Dpi.S( sym.X ), Dpi.S( sym.Y + 5 - m_model.SymOrgY ) )
      };

      string buttonText = UiText.NormalizeDisplayText( sym.ShowChar ? sym.Char : sym.Desc );
      if( sym.Hotkey.Length > 0 ) buttonText = "• " + buttonText;
      if( sym.Align == "left" )   buttonText = " "  + buttonText;
      btn.Text = buttonText;

      string tip   = UiText.NormalizeDisplayText( sym.Desc );
      string stem  = "";

      if( m_model.UseEmojiImages )
      {
        EmojiImageProvider.Result res = EmojiImageProvider.Get( sym.Char, emojiPx );
        stem = res.Stem;
        if( res.Image is not null )
        {
          btn.EmojiImage = res.Image;
          btn.Image      = Theme.IsDark ? null : res.Image;
          btn.Text       = "";
        }
      }

      if( sym.Hotkey.Length > 0 )                     tip = Append( tip, HotkeyParser.Label( sym.Hotkey ) );
      if( sym.TipChar )                               tip = Append( tip, sym.Char );
      if( m_model.UseEmojiImages && stem.Length > 0 ) tip = Append( tip, "U+" + stem );

      if( tip.Length > 0 )
      {
        m_tip.SetToolTip( btn, tip );
      }

      sym.Ctrl = btn;
      m_content.Controls.Add( btn );
    }
  }

  private static string Append( string tip, string line )
  {
    return tip.Length == 0 ? line : tip + "\n" + line;
  }

  // ── Scrolling ────────────────────────────────────────────────────

  private int Viewport => Math.Max( 1, ClientSize.Height );

  private int MaxScrollY => Math.Max( 0, m_contentPx - Viewport );

  protected override void OnSizeChanged( EventArgs e )
  {
    base.OnSizeChanged( e );
    m_content.Width = Math.Max( 1, ClientSize.Width - ( m_scroll.Visible ? m_scroll.Width : 0 ) );
    m_content.Height = m_contentPx;
    ConfigureScrollbar();
    ClampAndApply();
  }

  private void ConfigureScrollbar()
  {
    int  max    = m_contentPx;
    bool needed = MaxScrollY > 0;
    m_scroll.Enabled     = needed;
    m_scroll.LargeChange = Viewport;
    m_scroll.Maximum     = needed ? Math.Max( 0, max ) : 0;
  }

  private void SetTarget( int value )
  {
    m_targetY = Clamp( value );
    if( !m_anim.Enabled )
    {
      m_anim.Start();
    }
  }

  public void ScrollByWheel( int notches )
  {
    int basePixels = Dpi.S( 4 ), maxPixels = Dpi.S( 40 ), accelStep = Dpi.S( 4 );
    const int windowMs = 150;

    long now = Environment.TickCount64;
    m_accelPixels = now - m_lastWheelMs <= windowMs
                      ? Math.Min( m_accelPixels + accelStep, maxPixels - basePixels )
                      : 0;
    m_lastWheelMs = now;

    int pixelsPerNotch = basePixels + m_accelPixels;
    SetTarget( m_targetY + (notches * pixelsPerNotch) );
  }

  protected override void OnMouseWheel( MouseEventArgs e )
  {
    ScrollByWheel( -e.Delta / 120 );
    base.OnMouseWheel( e );
  }

  private int Clamp( int v ) => Math.Max( 0, Math.Min( v, MaxScrollY ) );

  private void ClampAndApply()
  {
    m_targetY = Clamp( m_targetY );
    m_scrollY = Clamp( m_scrollY );
    ApplyScrollPosition();
  }

  private void OnAnimTick( object? sender, EventArgs e )
  {
    if( m_scrollY == m_targetY )
    {
      m_anim.Stop();
      return;
    }

    int diff = m_targetY - m_scrollY;
    m_scrollY = Math.Abs( diff ) <= 1
      ? m_targetY
      : m_scrollY + (int)Math.Round( diff * LerpFactor );

    ApplyScrollPosition();

    if( m_scrollY == m_targetY )
    {
      m_anim.Stop();
    }
  }

  private void ApplyScrollPosition()
  {
    m_content.Top = -m_scrollY;
    if( m_scroll.Enabled )
    {
      int v = Math.Max( m_scroll.Minimum, Math.Min( m_scrollY, m_scroll.Maximum ) );
      if( m_scroll.Value != v )
      {
        m_scroll.Value = v;
      }
    }
  }

  public void ResetScrollImmediate()
  {
    m_anim.Stop();
    m_scrollY = 0;
    m_targetY = 0;
    ApplyScrollPosition();
  }
}
