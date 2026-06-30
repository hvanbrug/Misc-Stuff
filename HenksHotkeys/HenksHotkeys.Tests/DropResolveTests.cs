using System.Windows;
using HenksHotkeys.Core;
using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// DataTabModel.ResolveDrop maps a pointer position to a grid action: place on an empty
// cell, insert before/after a button, or start a new row below everything.
public class DropResolveTests
{
  private static ButtonDef B( string t, int row, int col ) => new() { Text = t, Row = row, Col = col };

  // A 8-wide tab with A,B,C on row 0.
  private static DataTabModel Grid() => new( new TabEntry
  {
    Name = "T", Columns = 8,
    Buttons = new() { B( "A", 0, 0 ), B( "B", 0, 1 ), B( "C", 0, 2 ) },
  } );

  private static SymbolElement Sym( DataTabModel t, string text )
    => t.Symbols.First( s => ( s.Source as ButtonDef )?.Text == text );

  [Fact]
  public void EmptyCell_PlacesThere_NoShift()
  {
    DataTabModel t = Grid();
    DropSpot d = t.ResolveDrop( new Point( t.SymOrgX + 5 * t.ColWidth + 10, t.SymOrgY + 5 ), null );

    Assert.Equal( DropKind.PlaceEmpty, d.Kind );
    Assert.Equal( 0, d.Row );
    Assert.Equal( 5, d.Col );
  }

  [Fact]
  public void BelowEverything_IsANewRow()
  {
    DataTabModel t = Grid();
    DropSpot d = t.ResolveDrop( new Point( t.SymOrgX + 10, t.SymOrgY + t.RowHeight + 6 ), null );

    Assert.Equal( DropKind.NewRow, d.Kind );
    Assert.Equal( 1, d.Row );
  }

  [Fact]
  public void OverButtonLeftHalf_InsertsBefore()
  {
    DataTabModel t = Grid();
    SymbolElement b = Sym( t, "B" );
    DropSpot d = t.ResolveDrop( new Point( b.X + 2, t.SymOrgY + 5 ), null );

    Assert.Equal( DropKind.InsertBefore, d.Kind );
    Assert.Equal( 1, d.Col );           // lands at B's column (B shifts right)
  }

  [Fact]
  public void OverButtonRightHalf_InsertsAfter_BetweenItAndTheNext()
  {
    DataTabModel t = Grid();
    SymbolElement b = Sym( t, "B" );
    DropSpot d = t.ResolveDrop( new Point( b.X + b.W - 2, t.SymOrgY + 5 ), null );

    Assert.True( d.Kind is DropKind.InsertAfter or DropKind.InsertBefore ); // either framing of the same edge
    Assert.Equal( 2, d.Col );           // the slot between B and C
  }

  [Fact]
  public void DraggedButton_DoesNotCollideWithItself()
  {
    DataTabModel t = Grid();
    SymbolElement b = Sym( t, "B" );
    var bDef = (ButtonDef)b.Source!;

    DropSpot d = t.ResolveDrop( new Point( b.X + b.W / 2, t.SymOrgY + 5 ), bDef );

    Assert.Equal( DropKind.PlaceEmpty, d.Kind ); // its own cell reads as empty → place back
    Assert.Equal( 1, d.Col );
  }
}
