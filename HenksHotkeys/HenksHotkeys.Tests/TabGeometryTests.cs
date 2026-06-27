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
  // Buttons start at (TabEdgeGap, TabEdgeGap); default button 35; cell pitch = 35 + ButtonGap.
  private const int Btn = 35;
  private const int EG  = Layout.TabEdgeGap; // the button inset from the tab boundary
  private const int CW  = Btn + Layout.ButtonGap; // ColWidth == RowHeight for a square button

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

    Assert.Equal( ( EG,          EG ),      ( t.Symbols[0].X, t.Symbols[0].Y ) );
    Assert.Equal( ( EG + CW,     EG ),      ( t.Symbols[1].X, t.Symbols[1].Y ) );
    Assert.Equal( ( EG + 2 * CW, EG ),      ( t.Symbols[2].X, t.Symbols[2].Y ) );
    Assert.Equal( ( EG,          EG + CW ), ( t.Symbols[3].X, t.Symbols[3].Y ) );

    Assert.Equal( Btn, t.Symbols[0].W );
    Assert.Equal( Btn, t.Symbols[0].H );

    // Content = button extent + a trailing TabEdgeGap on each side.
    Assert.Equal( EG + 2 * CW + Btn + EG, t.ContentWidth );
    Assert.Equal( EG + CW + Btn + EG,     t.ContentHeight );
  }

  [Fact]
  public void MultiCellWidth_ExpandsButtonWidth()
  {
    var t = new GeometryTab();
    t.Rows( 5 );
    t.Add( 2, "wide" );
    Assert.Equal( Btn * 2 + Layout.ButtonGap, t.Symbols[0].W );
  }

  [Fact]
  public void RegisterSpace_AdvancesSlot()
  {
    var t = new GeometryTab();
    t.Rows( 3 );
    t.Space( 2 );
    t.Add( 1, "a" );
    Assert.Equal( 3, t.Symbols[0].Slot );
    Assert.Equal( EG + 2 * CW, t.Symbols[0].X );
  }

  [Fact]
  public void ShiftLineByThird_OffsetsTheRowVertically()
  {
    var t = new GeometryTab();
    t.Rows( 20 );        // wide enough to avoid wrapping
    t.Add( 1, "a" );     // line 1
    t.Next();            // → line 2
    t.ShiftThird();      // lineShift += 1/3
    t.Add( 1, "b" );     // line 2, offset (1 + 1/3) rows

    Assert.Equal( EG, t.Symbols[0].Y );
    Assert.Equal( EG + (int)System.Math.Round( 4.0 / 3.0 * CW ), t.Symbols[1].Y );
  }

  [Fact]
  public void ColumnPrimary_LaysOutTopToBottom()
  {
    var t = new GeometryTab();
    t.Cols( 3 );         // column-primary: slot → Y, line → X
    t.Add( 1, "a" );
    t.Add( 1, "b" );
    t.Recalc();

    Assert.Equal( ( EG, EG ),      ( t.Symbols[0].X, t.Symbols[0].Y ) );
    Assert.Equal( ( EG, EG + CW ), ( t.Symbols[1].X, t.Symbols[1].Y ) );
  }
}
