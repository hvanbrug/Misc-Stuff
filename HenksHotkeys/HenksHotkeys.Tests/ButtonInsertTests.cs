using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

// Inserting/placing a button shifts a row to make room — and only that row (no wrap-around
// cascade into the rows below).
public class ButtonInsertTests
{
  private static ButtonDef B( string t, int row, int col ) => new() { Text = t, Row = row, Col = col };

  private static TabEntry SixWide()
  {
    var buttons = new List<ButtonDef>();
    for( int i = 1; i <= 12; i++ ) buttons.Add( B( i.ToString(), ( i - 1 ) / 6, ( i - 1 ) % 6 ) );
    return new TabEntry { Name = "T", Columns = 6, Buttons = buttons };
  }

  [Fact]
  public void AddButtonAt_IntoAFullRow_ExtendsThatRow_WithoutWrapping()
  {
    TabEntry tab = SixWide(); // row0 = 1..6, row1 = 7..12

    TabStore.AddButtonAt( tab, 0, 4, new ButtonDef { Text = "77" } ); // right-clicked button "5"

    (int, int) Pos( string t ) { ButtonDef b = tab.Buttons!.First( x => x.Text == t ); return ( b.Row, b.Col ); }

    Assert.Equal( ( 0, 4 ), Pos( "77" ) ); // inserted at the clicked cell
    Assert.Equal( ( 0, 5 ), Pos( "5" ) );  // 5 pushed right
    Assert.Equal( ( 0, 6 ), Pos( "6" ) );  // 6 pushed right — row extends to a 7th column, no wrap
    Assert.Equal( ( 1, 0 ), Pos( "7" ) );  // the next row is untouched
    Assert.Equal( ( 1, 5 ), Pos( "12" ) );
  }

  [Fact]
  public void AddButtonAt_StopsAtTheFirstGap_InTheRow()
  {
    // row0: A@0, B@1, (gap@2), C@3 — inserting at 0 shifts only A,B into the gap.
    var tab = new TabEntry { Name = "T", Columns = 6,
      Buttons = new() { B( "A", 0, 0 ), B( "B", 0, 1 ), B( "C", 0, 3 ) } };

    TabStore.AddButtonAt( tab, 0, 0, new ButtonDef { Text = "X" } );

    (int, int) Pos( string t ) { ButtonDef b = tab.Buttons!.First( x => x.Text == t ); return ( b.Row, b.Col ); }
    Assert.Equal( ( 0, 0 ), Pos( "X" ) );
    Assert.Equal( ( 0, 1 ), Pos( "A" ) );
    Assert.Equal( ( 0, 2 ), Pos( "B" ) ); // B fills the former gap
    Assert.Equal( ( 0, 3 ), Pos( "C" ) ); // C untouched (was past the gap)
  }
}
