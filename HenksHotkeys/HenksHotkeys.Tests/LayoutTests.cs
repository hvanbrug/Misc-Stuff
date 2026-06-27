using HenksHotkeys.Core;
using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// Defaults: SymOrg (15,35); button 35, gap 3 → ColWidth = RowHeight = 38.
public class LayoutTests
{
  private static ButtonDef B( string text ) => new() { Text = text };

  private static DataTabModel Build( int columns, params RowDef[] rows )
    => new( new TabEntry { Name = "T", Columns = columns, Rows = rows.ToList() } );

  [Fact]
  public void PerButtonGapBefore_ShiftsButtonRight()
  {
    DataTabModel t = Build( 10,
      new RowDef { Buttons = { B( "A" ), new ButtonDef { Text = "B", GapBefore = 1 } } } );

    Assert.Equal( 2, t.Symbols.Count );
    Assert.Equal( 15, t.Symbols[0].X );           // A at column 0
    Assert.Equal( 15 + 2 * 38, t.Symbols[1].X );  // B pushed an extra cell right by the gap
  }

  [Fact]
  public void BlankButton_LeavesAOneCellGap_AndDrawsNothing()
  {
    DataTabModel t = Build( 10,
      new RowDef { Buttons = { B( "A" ), new ButtonDef { Blank = true, Text = "ignored" }, B( "B" ) } } );

    Assert.Equal( 2, t.Symbols.Count );           // the blank cell is not drawn
    Assert.Equal( 15, t.Symbols[0].X );
    Assert.Equal( 15 + 2 * 38, t.Symbols[1].X );  // B skipped the blank cell
  }

  [Fact]
  public void BlankRow_TakesVerticalSpace()
  {
    DataTabModel t = Build( 10,
      new RowDef { Buttons = { B( "A" ) } },
      new RowDef { Blank = true },
      new RowDef { Buttons = { B( "B" ) } } );

    Assert.Equal( 2, t.Symbols.Count );
    Assert.Equal( 35, t.Symbols[0].Y );
    Assert.Equal( 35 + 2 * 38, t.Symbols[1].Y );  // B is two rows down (blank row took one)
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
