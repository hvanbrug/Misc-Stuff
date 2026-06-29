using HenksHotkeys.Core;
using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// The two orthogonal sizing flags: `proportional` (width fills the tab) and
// `square` (height tracks width).
public class ProportionalTests
{
  private const int EG  = Layout.TabEdgeGap;
  private const int BG  = Layout.ButtonGap;

  private static DataTabModel Make( bool proportional, bool square, int cols, int btnW, int btnH, int buttons )
  {
    var list = new List<ButtonDef>();
    for( int i = 0; i < buttons; i++ ) list.Add( new ButtonDef { Text = "b" + i, Row = 0, Col = i } );
    return new DataTabModel( new TabEntry
    {
      Name = "T", Columns = cols, ButtonWidth = btnW, ButtonHeight = btnH,
      Proportional = proportional, Square = square,
      Buttons = list,
    } );
  }

  // cell width that fills `cols` columns inside `contentWidth`.
  private static int Cell( int contentWidth, int cols )
    => ( contentWidth - 2 * EG - ( cols - 1 ) * BG ) / cols;

  [Fact]
  public void Fixed_KeepsButtonWidthAndHeight()
  {
    DataTabModel t = Make( proportional: false, square: false, cols: 3, btnW: 40, btnH: 30, buttons: 1 );
    Assert.Equal( 40, t.Symbols[0].W );
    Assert.Equal( 30, t.Symbols[0].H );
  }

  [Fact]
  public void FixedSquare_HeightFollowsWidth_IgnoringButtonHeight()
  {
    DataTabModel t = Make( proportional: false, square: true, cols: 3, btnW: 40, btnH: 99, buttons: 1 );
    Assert.Equal( 40, t.Symbols[0].W );
    Assert.Equal( 40, t.Symbols[0].H ); // square: buttonHeight ignored
  }

  [Fact]
  public void Proportional_DefersLayout_UntilApplied()
  {
    DataTabModel t = Make( proportional: true, square: false, cols: 3, btnW: 35, btnH: 35, buttons: 3 );
    Assert.Empty( t.Symbols );                          // not laid out yet
    Assert.Equal( 2 * EG + 3 * 35 + 2 * BG, t.MinContentWidth ); // natural minimum exposed
  }

  [Fact]
  public void Proportional_DividesWidthIntoEqualFillingCells()
  {
    DataTabModel t = Make( proportional: true, square: false, cols: 3, btnW: 35, btnH: 35, buttons: 3 );
    t.ApplyProportionalLayout( 300 );

    int cell = Cell( 300, 3 );
    Assert.Equal( cell, t.Symbols[0].W );
    Assert.Equal( 35,   t.Symbols[0].H );                 // height stays buttonHeight (not square)
    Assert.Equal( EG,                 t.Symbols[0].X );
    Assert.Equal( EG + cell + BG,     t.Symbols[1].X );
    Assert.Equal( EG + 2 * ( cell + BG ), t.Symbols[2].X );
  }

  [Fact]
  public void ProportionalSquare_GrowsToSquareCells()
  {
    DataTabModel t = Make( proportional: true, square: true, cols: 3, btnW: 35, btnH: 35, buttons: 3 );
    t.ApplyProportionalLayout( 300 );

    int cell = Cell( 300, 3 );
    Assert.Equal( cell, t.Symbols[0].W );
    Assert.Equal( cell, t.Symbols[0].H ); // square: height grew with the wider cell
  }

  [Fact]
  public void Proportional_NeverShrinksBelowNaturalButtonWidth()
  {
    DataTabModel t = Make( proportional: true, square: false, cols: 3, btnW: 100, btnH: 30, buttons: 3 );
    t.ApplyProportionalLayout( 120 ); // far narrower than 3*100; clamp to the natural width

    Assert.Equal( 100, t.Symbols[0].W );
  }

  [Fact]
  public void TabSig_ChangesWithTheFlags()
  {
    TabEntry Base() => new() { Name = "T", Columns = 3, Buttons = new() { new ButtonDef { Text = "a", Row = 0, Col = 0 } } };
    var plain = Base();
    var prop  = Base(); prop.Proportional = true;
    var sq    = Base(); sq.Square = true;

    Assert.NotEqual( VersionStamp.TabSig( plain ), VersionStamp.TabSig( prop ) );
    Assert.NotEqual( VersionStamp.TabSig( plain ), VersionStamp.TabSig( sq ) );
  }
}
