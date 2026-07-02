using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

// Multi-selection operations: collision-checked group move, and group delete (buttons +
// headings together).
public class SelectionTests
{
  private static ButtonDef B( string t, int row, int col, int width = 1 )
    => new() { Text = t, Row = row, Col = col, Width = width };

  [Fact]
  public void MoveSelection_ShiftsEveryItemByTheDelta()
  {
    var a = B( "A", 0, 0 );
    var b = B( "B", 0, 1 );
    var head = new SectionDef { Name = "H", Row = 0, Col = 2, Span = 2 };
    var tab = new TabEntry { Name = "T", Columns = 8, Buttons = new() { a, b }, Sections = new() { head } };

    bool ok = TabStore.MoveSelection( tab, new[] { a, b }, new[] { head }, dRow: 2, dCol: 1 );

    Assert.True( ok );
    Assert.Equal( ( 2, 1 ), ( a.Row, a.Col ) );
    Assert.Equal( ( 2, 2 ), ( b.Row, b.Col ) );
    Assert.Equal( ( 2, 3 ), ( head.Row, head.Col ) );
  }

  [Fact]
  public void MoveSelection_RefusedWhenItWouldCollideWithAnUnselectedButton()
  {
    var a = B( "A", 0, 0 );
    var wall = B( "wall", 1, 0 ); // not selected — sits where A would land
    var tab = new TabEntry { Name = "T", Columns = 8, Buttons = new() { a, wall } };

    bool ok = TabStore.MoveSelection( tab, new[] { a }, System.Array.Empty<SectionDef>(), dRow: 1, dCol: 0 );

    Assert.False( ok );
    Assert.Equal( ( 0, 0 ), ( a.Row, a.Col ) );      // nothing moved
    Assert.Equal( ( 1, 0 ), ( wall.Row, wall.Col ) );
  }

  [Fact]
  public void MoveSelection_AllowsSwappingPastAnItemThatIsAlsoSelected()
  {
    // Two adjacent buttons both selected can move as a block even though their paths overlap
    // the other's origin — only *unselected* cells block.
    var a = B( "A", 0, 0 );
    var b = B( "B", 0, 1 );
    var tab = new TabEntry { Name = "T", Columns = 8, Buttons = new() { a, b } };

    bool ok = TabStore.MoveSelection( tab, new[] { a, b }, System.Array.Empty<SectionDef>(), dRow: 0, dCol: 1 );

    Assert.True( ok );
    Assert.Equal( ( 0, 1 ), ( a.Row, a.Col ) );
    Assert.Equal( ( 0, 2 ), ( b.Row, b.Col ) );
  }

  [Fact]
  public void MoveSelection_RefusedWhenOutOfBounds()
  {
    var a = B( "A", 0, 0 );
    var tab = new TabEntry { Name = "T", Columns = 6, Buttons = new() { a } };

    Assert.False( TabStore.MoveSelection( tab, new[] { a }, System.Array.Empty<SectionDef>(), 0, -1 ) ); // off the left
    Assert.False( TabStore.MoveSelection( tab, new[] { a }, System.Array.Empty<SectionDef>(), -1, 0 ) ); // off the top
    Assert.Equal( ( 0, 0 ), ( a.Row, a.Col ) );
  }

  [Fact]
  public void MoveSelection_RefusedWhenAWideButtonWouldPassTheLastColumn()
  {
    var wide = B( "W", 0, 3, width: 3 ); // occupies cols 3,4,5 in a 6-wide tab
    var tab = new TabEntry { Name = "T", Columns = 6, Buttons = new() { wide } };

    Assert.False( TabStore.MoveSelection( tab, new[] { wide }, System.Array.Empty<SectionDef>(), 0, 1 ) ); // 6,7 out of range
    Assert.Equal( ( 0, 3 ), ( wide.Row, wide.Col ) );
  }

  [Fact]
  public void MoveSelectionToTab_RelocatesItems_KeepingRelativeLayout()
  {
    var a = B( "A", 0, 0 );
    var b = B( "B", 0, 2 );
    var head = new SectionDef { Name = "H", Row = 1, Col = 0, Span = 3 };
    var src = new TabEntry { Name = "S", Columns = 8, Buttons = new() { a, b }, Sections = new() { head } };
    var dst = new TabEntry { Name = "D", Columns = 8, Buttons = new() { B( "keep", 0, 0 ) } };

    // Anchor A onto (row 2, col 1) of the destination → delta (+2, +1).
    bool ok = TabStore.MoveSelectionToTab( src, dst, new[] { a, b }, new[] { head }, dRow: 2, dCol: 1 );

    Assert.True( ok );
    Assert.Empty( src.Buttons! );                    // moved out of the source
    Assert.Empty( src.Sections! );
    Assert.Equal( ( 2, 1 ), ( a.Row, a.Col ) );      // relative layout preserved in the destination
    Assert.Equal( ( 2, 3 ), ( b.Row, b.Col ) );
    Assert.Equal( ( 3, 1 ), ( head.Row, head.Col ) );
    Assert.Contains( dst.Buttons!, x => x.Text == "A" );
    Assert.Contains( dst.Buttons!, x => x.Text == "keep" ); // destination's own button untouched
  }

  [Fact]
  public void MoveSelectionToTab_RefusedOnCollisionOrOutOfBounds()
  {
    var a = B( "A", 0, 0 );
    var src = new TabEntry { Name = "S", Columns = 8, Buttons = new() { a } };
    var dst = new TabEntry { Name = "D", Columns = 8, Buttons = new() { B( "wall", 0, 0 ) } };

    Assert.False( TabStore.MoveSelectionToTab( src, dst, new[] { a }, System.Array.Empty<SectionDef>(), 0, 0 ) ); // onto "wall"
    Assert.Contains( a, src.Buttons! );              // nothing moved
    Assert.Single( dst.Buttons! );
  }

  [Fact]
  public void DeleteSelection_RemovesButtonsAndHeadings_LeavingTheRest()
  {
    var a = B( "A", 0, 0 );
    var b = B( "B", 0, 1 );
    var head = new SectionDef { Name = "H", Row = 1, Col = 0, Span = 3 };
    var tab = new TabEntry { Name = "T", Columns = 8, Buttons = new() { a, b }, Sections = new() { head } };

    bool ok = TabStore.DeleteSelection( tab, new[] { a }, new[] { head } );

    Assert.True( ok );
    Assert.Equal( new[] { b }, tab.Buttons! );   // only B remains
    Assert.Empty( tab.Sections! );               // heading gone
  }
}
