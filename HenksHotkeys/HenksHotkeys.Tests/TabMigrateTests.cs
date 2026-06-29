using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

// Upgrading a legacy (rows-based) tabs.json to the coordinate format.
public class TabMigrateTests
{
  private static ButtonDef B( string t ) => new() { Text = t };

  private static TabFile One( TabEntry t ) => new() { Tabs = { t } };

  private static TabEntry Migrate( params RowDef[] rows )
  {
    var f = One( new TabEntry { Name = "T", Columns = 5, Rows = rows.ToList() } );
    Assert.True( TabMigrate.Migrate( f ) );
    return f.Tabs[0];
  }

  [Fact]
  public void Migrate_AssignsRowAndColumnIndices()
  {
    TabEntry t = Migrate(
      new RowDef { Buttons = { B( "A" ), B( "B" ) } },
      new RowDef { Buttons = { B( "C" ) } } );

    Assert.Null( t.Rows );                       // legacy layout consumed
    Assert.Equal( 3, t.Buttons!.Count );
    Assert.Collection( t.Buttons!,
      b => Assert.Equal( ( 0, 0 ), ( b.Row, b.Col ) ),
      b => Assert.Equal( ( 0, 1 ), ( b.Row, b.Col ) ),
      b => Assert.Equal( ( 1, 0 ), ( b.Row, b.Col ) ) );
  }

  [Fact]
  public void Migrate_DropsBlankCells_LeavingAColumnGap()
  {
    TabEntry t = Migrate(
      new RowDef { Buttons = { B( "A" ), new ButtonDef { Blank = true }, B( "B" ) } } );

    Assert.Equal( 2, t.Buttons!.Count );         // the blank cell is not stored
    Assert.Equal( 0, t.Buttons![0].Col );
    Assert.Equal( 2, t.Buttons![1].Col );        // B skips the empty middle column
  }

  [Fact]
  public void Migrate_DropsBlankRows_LeavingARowGap()
  {
    TabEntry t = Migrate(
      new RowDef { Buttons = { B( "A" ) } },
      new RowDef { Blank = true },
      new RowDef { Buttons = { B( "B" ) } } );

    Assert.Equal( 0, t.Buttons![0].Row );
    Assert.Equal( 2, t.Buttons![1].Row );        // B is two rows down (row 1 left empty)
  }

  [Fact]
  public void Migrate_TurnsSectionRowsIntoSectionDefs()
  {
    TabEntry t = Migrate(
      new RowDef { Section = "Group", HeaderHeight = 28 },
      new RowDef { Buttons = { B( "A" ) } } );

    Assert.Single( t.Sections! );
    Assert.Equal( "Group", t.Sections![0].Name );
    Assert.Equal( 0, t.Sections![0].Row );
    Assert.Equal( 28, t.Sections![0].Height );
    Assert.Equal( 1, t.Buttons![0].Row );        // button sits below the section row
  }

  [Fact]
  public void Migrate_WrapsAnOverWideRow_ToColumns()
  {
    var btns = Enumerable.Range( 0, 7 ).Select( i => B( "x" + i ) ).ToList();
    TabEntry t = Migrate( new RowDef { Buttons = btns } );

    Assert.Equal( 7, t.Buttons!.Count );                         // nothing lost
    Assert.All( t.Buttons!, b => Assert.True( b.Col < 5 ) );     // no column exceeds the tab width
    Assert.Contains( t.Buttons!, b => b.Row == 1 );              // overflowed onto a second row
  }

  [Fact]
  public void Migrate_IsIdempotent_OnAlreadyCoordinateTabs()
  {
    var f = One( new TabEntry { Name = "T", Columns = 5,
      Buttons = new() { new ButtonDef { Text = "A", Row = 0, Col = 0 } } } );

    Assert.False( TabMigrate.Migrate( f ) );      // no legacy rows → nothing to do
  }
}
