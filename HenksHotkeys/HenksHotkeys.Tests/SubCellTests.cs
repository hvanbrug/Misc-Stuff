using System.Windows;
using HenksHotkeys.Core;
using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// Sub-cell buttons: more than one button sharing a cell, split evenly across its width.
public class SubCellTests
{
  private const int EG  = Layout.TabEdgeGap;
  private const int Btn = 35;                 // default buttonWidth
  private const int Gap = Layout.ButtonGap;

  private static ButtonDef Sub( string t, int row, int col, int n, int i )
    => new() { Text = t, Row = row, Col = col, SubCells = n, SubCell = i };

  [Fact]
  public void ThreeSubCells_SplitTheCellWidth_LeftToRight()
  {
    var t = new DataTabModel( new TabEntry
    {
      Name = "T", Columns = 8,
      Buttons = new() { Sub( "A", 0, 0, 3, 0 ), Sub( "B", 0, 0, 3, 1 ), Sub( "C", 0, 0, 3, 2 ) },
    } );

    int subW = ( Btn - Gap * 2 ) / 3;          // cell width split three ways (with gaps)

    Assert.Equal( 3, t.Symbols.Count );
    Assert.All( t.Symbols, s => Assert.Equal( subW, s.W ) );      // each is a third of the cell
    Assert.All( t.Symbols, s => Assert.Equal( Btn, s.H ) );       // full cell height kept
    Assert.Equal( EG,                      t.Symbols[0].X );      // slot 0 at the cell's left
    Assert.Equal( EG + ( subW + Gap ),     t.Symbols[1].X );      // slot 1 one sub-width along
    Assert.Equal( EG + 2 * ( subW + Gap ), t.Symbols[2].X );      // slot 2
  }

  [Fact]
  public void ButtonSig_DistinguishesSubCellSlots()
  {
    var a = Sub( "X", 0, 0, 3, 0 );
    var b = Sub( "X", 0, 0, 3, 1 );            // same content + cell, different slot

    Assert.Equal( VersionStamp.ContentSig( a ), VersionStamp.ContentSig( b ) ); // same content
    Assert.NotEqual( VersionStamp.ButtonSig( a ), VersionStamp.ButtonSig( b ) ); // different position
  }

  [Fact]
  public void Merge_KeepsSubCellSiblings_InTheSameCell()
  {
    TabEntry Make() => new()
    {
      Id = "t", Mod = 1, Name = "T", Columns = 8,
      Buttons = new()
      {
        new ButtonDef { Id = "a", Mod = 1, Text = "A", Row = 0, Col = 0, SubCells = 3, SubCell = 0 },
        new ButtonDef { Id = "b", Mod = 1, Text = "B", Row = 0, Col = 0, SubCells = 3, SubCell = 1 },
        new ButtonDef { Id = "c", Mod = 1, Text = "C", Row = 0, Col = 0, SubCells = 3, SubCell = 2 },
      },
    };

    TabFile m = VersionMerge.Merge( new TabFile { Tabs = { Make() } }, new TabFile { Tabs = { Make() } } );

    List<ButtonDef> btns = m.Tabs[0].Buttons!;
    Assert.Equal( 3, btns.Count );
    Assert.All( btns, b => Assert.Equal( 0, b.Col ) );                        // all still in cell (0,0)
    Assert.Equal( new HashSet<int> { 0, 1, 2 }, btns.Select( b => b.SubCell ).ToHashSet() );
  }

  [Fact]
  public void Merge_SeparatesTwoWholeCellButtons_SharingACell()
  {
    // Two *different* buttons both claiming whole cell (0,0) is a real clash → one bumps aside.
    var local    = new TabFile { Tabs = { new TabEntry { Id = "t", Mod = 1, Name = "T", Columns = 8,
      Buttons = new() { new ButtonDef { Id = "a", Mod = 1, Text = "A", Row = 0, Col = 0 } } } } };
    var incoming = new TabFile { Tabs = { new TabEntry { Id = "t", Mod = 1, Name = "T", Columns = 8,
      Buttons = new() { new ButtonDef { Id = "b", Mod = 1, Text = "B", Row = 0, Col = 0 } } } } };

    TabFile m = VersionMerge.Merge( local, incoming );

    List<ButtonDef> btns = m.Tabs[0].Buttons!;
    Assert.Equal( 2, btns.Count );
    Assert.Equal( new HashSet<int> { 0, 1 }, btns.Select( b => b.Col ).ToHashSet() ); // no longer overlapping
  }

  [Fact]
  public void DraggingASubCellButton_TargetsTheWholeCell_NotAnInsert()
  {
    var t = new DataTabModel( new TabEntry
    {
      Name = "T", Columns = 8,
      Buttons = new() { Sub( "A", 0, 0, 3, 0 ) },   // a sub-cell button occupies slot 0 of cell (0,0)
    } );
    var moving = Sub( "B", 0, 5, 3, 1 );            // another sub-cell button being dragged in

    SymbolElement symA = t.Symbols[0];
    DropSpot d = t.ResolveDrop( new Point( symA.X + symA.W / 2.0, t.SymOrgY + 5 ), moving );

    Assert.Equal( DropKind.PlaceEmpty, d.Kind );    // targets the cell to join a free slot, no insert-shift
    Assert.Equal( 0, d.Row );
    Assert.Equal( 0, d.Col );
  }
}
