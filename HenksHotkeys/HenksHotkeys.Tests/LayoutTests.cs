using HenksHotkeys.Core;
using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// Coordinate layout: buttons start at (TabEdgeGap, TabEdgeGap); default button 35;
// cell pitch = 35 + ButtonGap. Each button carries its own (row, col); empty cells and
// rows are simply skipped indices, never materialised.
public class LayoutTests
{
  private const int EG = Layout.TabEdgeGap; // the button inset from the tab boundary
  private const int CW = 35 + Layout.ButtonGap;

  private static ButtonDef B( string text, int row, int col ) => new() { Text = text, Row = row, Col = col };

  private static DataTabModel Build( int columns, params ButtonDef[] buttons )
    => new( new TabEntry { Name = "T", Columns = columns, Buttons = buttons.ToList() } );

  [Fact]
  public void ColumnIndex_PositionsButtonHorizontally()
  {
    DataTabModel t = Build( 10, B( "A", 0, 0 ), B( "B", 0, 2 ) );

    Assert.Equal( 2, t.Symbols.Count );
    Assert.Equal( EG, t.Symbols[0].X );           // A at column 0
    Assert.Equal( EG + 2 * CW, t.Symbols[1].X );  // B at column 2
  }

  [Fact]
  public void EmptyCell_IsASkippedColumn_NotMaterialised()
  {
    DataTabModel t = Build( 10, B( "A", 0, 0 ), B( "B", 0, 2 ) );

    Assert.Equal( 2, t.Symbols.Count );           // the empty middle cell is not a control
    Assert.Equal( EG, t.Symbols[0].X );
    Assert.Equal( EG + 2 * CW, t.Symbols[1].X );  // B still sits in the third cell
  }

  [Fact]
  public void EmptyRow_TakesVerticalSpace_AsASkippedRowIndex()
  {
    DataTabModel t = Build( 10, B( "A", 0, 0 ), B( "B", 2, 0 ) );

    Assert.Equal( 2, t.Symbols.Count );
    Assert.Equal( EG, t.Symbols[0].Y );
    Assert.Equal( EG + 2 * CW, t.Symbols[1].Y );  // B is two rows down (row 1 left empty)
  }
}
