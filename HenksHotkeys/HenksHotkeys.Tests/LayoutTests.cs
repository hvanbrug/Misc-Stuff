using HenksHotkeys.Core;
using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// Buttons start at (TabEdgeGap, TabEdgeGap); default button 35; cell pitch = 35 + ButtonGap.
public class LayoutTests
{
  private const int EG = Layout.TabEdgeGap; // the button inset from the tab boundary
  private const int CW = 35 + Layout.ButtonGap;

  private static ButtonDef B( string text ) => new() { Text = text };

  private static DataTabModel Build( int columns, params RowDef[] rows )
    => new( new TabEntry { Name = "T", Columns = columns, Rows = rows.ToList() } );

  [Fact]
  public void PerButtonGapBefore_ShiftsButtonRight()
  {
    DataTabModel t = Build( 10,
      new RowDef { Buttons = { B( "A" ), new ButtonDef { Text = "B", GapBefore = 1 } } } );

    Assert.Equal( 2, t.Symbols.Count );
    Assert.Equal( EG, t.Symbols[0].X );           // A at column 0
    Assert.Equal( EG + 2 * CW, t.Symbols[1].X );  // B pushed an extra cell right by the gap
  }

  [Fact]
  public void BlankButton_OccupiesACell_AsAnEditablePlaceholder()
  {
    DataTabModel t = Build( 10,
      new RowDef { Buttons = { B( "A" ), new ButtonDef { Blank = true }, B( "B" ) } } );

    Assert.Equal( 3, t.Symbols.Count );           // the blank is placed (hoverable / editable)
    Assert.Equal( EG, t.Symbols[0].X );
    Assert.True( t.Symbols[1].IsBlank );          // middle cell is the blank spacer
    Assert.Equal( EG + CW, t.Symbols[1].X );
    Assert.Equal( EG + 2 * CW, t.Symbols[2].X );  // B still sits in the third cell
  }

  [Fact]
  public void BlankRow_TakesVerticalSpace()
  {
    DataTabModel t = Build( 10,
      new RowDef { Buttons = { B( "A" ) } },
      new RowDef { Blank = true },
      new RowDef { Buttons = { B( "B" ) } } );

    Assert.Equal( 2, t.Symbols.Count );
    Assert.Equal( EG, t.Symbols[0].Y );
    Assert.Equal( EG + 2 * CW, t.Symbols[1].Y );  // B is two rows down (blank row took one)
  }

  [Fact]
  public void NormalizeRows_ClearsButtonsFromBlankRow()
  {
    var t = new TabEntry { Name = "T", Columns = 3,
      Rows = new() { new RowDef { Blank = true, Buttons = { B( "x" ), B( "y" ) } } } };

    Assert.True( VersionStamp.NormalizeRows( t ) );
    Assert.Empty( t.Rows![0].Buttons );
  }
}
