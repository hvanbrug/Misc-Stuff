using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// Exposes TabModel's protected builder API so the layout maths can be tested.
internal sealed class GeometryTab : TabModel
{
  public GeometryTab() : base( "geo" ) { }

  public void Rows( int n )                 => SetRowsOf( n );
  public void Cols( int n )                 => SetColsOf( n );
  public void Add( int width, string ch )   => RegisterSymbolX( width, ch );
  public void Space( int n )                => RegisterSpace( n );
  public void Next( bool eol = false )      => NextLine( eol );
  public void ShiftThird( double n = 1 )    => ShiftLineByThird( n );
  public void Recalc()                      => RecalcSizes();
}

public class TabGeometryTests
{
  // Defaults: SymOrg (15,35), button 35x35, gap 3 → ColWidth/RowHeight = 38.
  [Fact]
  public void RowPrimary_LaysOutLeftToRight_ThenWraps()
  {
    var t = new GeometryTab();
    t.Rows( 3 );
    t.Add( 1, "a" );
    t.Add( 1, "b" );
    t.Add( 1, "c" );
    t.Add( 1, "d" ); // wraps to the next row
    t.Recalc();

    Assert.Equal( ( 15, 35 ), ( t.Symbols[0].X, t.Symbols[0].Y ) );
    Assert.Equal( ( 53, 35 ), ( t.Symbols[1].X, t.Symbols[1].Y ) );
    Assert.Equal( ( 91, 35 ), ( t.Symbols[2].X, t.Symbols[2].Y ) );
    Assert.Equal( ( 15, 73 ), ( t.Symbols[3].X, t.Symbols[3].Y ) );

    Assert.Equal( 35, t.Symbols[0].W );
    Assert.Equal( 35, t.Symbols[0].H );

    // ContentWidth = maxRight + 1; ContentHeight = (maxBottom - SymOrgY) + gap + 10.
    Assert.Equal( 127, t.ContentWidth );  // (91+35) + 1
    Assert.Equal( 86,  t.ContentHeight ); // (108-35) + 3 + 10
  }

  [Fact]
  public void MultiCellWidth_ExpandsButtonWidth()
  {
    var t = new GeometryTab();
    t.Rows( 5 );
    t.Add( 2, "wide" );
    Assert.Equal( 73, t.Symbols[0].W ); // 35*2 + 3*(2-1)
  }

  [Fact]
  public void RegisterSpace_AdvancesSlot()
  {
    var t = new GeometryTab();
    t.Rows( 3 );
    t.Space( 2 );
    t.Add( 1, "a" );
    Assert.Equal( 3, t.Symbols[0].Slot );
    Assert.Equal( 91, t.Symbols[0].X ); // 15 + (3-1)*38
  }

  [Fact]
  public void ShiftLineByThird_OffsetsTheRowVertically()
  {
    var t = new GeometryTab();
    t.Rows( 20 );        // wide enough to avoid wrapping
    t.Add( 1, "a" );     // line 1, Y = 35
    t.Next();            // → line 2
    t.ShiftThird();      // lineShift += 1/3
    t.Add( 1, "b" );     // line 2, Y = 35 + round((1 + 1/3) * 38) = 35 + 51

    Assert.Equal( 35, t.Symbols[0].Y );
    Assert.Equal( 86, t.Symbols[1].Y );
  }

  [Fact]
  public void ColumnPrimary_LaysOutTopToBottom()
  {
    var t = new GeometryTab();
    t.Cols( 3 );         // column-primary: slot → Y, line → X
    t.Add( 1, "a" );
    t.Add( 1, "b" );
    t.Recalc();

    Assert.Equal( ( 15, 35 ), ( t.Symbols[0].X, t.Symbols[0].Y ) );
    Assert.Equal( ( 15, 73 ), ( t.Symbols[1].X, t.Symbols[1].Y ) ); // same column, next row
  }
}
