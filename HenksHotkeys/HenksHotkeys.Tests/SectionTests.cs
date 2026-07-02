using HenksHotkeys.Core;
using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// Section headers are dividers at a grid row index (a SectionDef): they label and
// space the rows beneath them, and are emitted as TabModel.SectionHeader elements.
public class SectionTests
{
  private const int EG = Layout.TabEdgeGap; // the button/header inset from the tab boundary

  private static ButtonDef B( string t, int row, int col ) => new() { Text = t, Row = row, Col = col };
  private static SectionDef Sec( string name, int row, double height = 0 )
    => new() { Name = name, Row = row, Height = height };
  private static SectionDef Head( string name, int row, int col, int span )
    => new() { Name = name, Row = row, Col = col, Span = span };

  private static DataTabModel Build( int cols, List<ButtonDef> buttons, List<SectionDef> sections )
    => new( new TabEntry { Name = "T", Columns = cols, Buttons = buttons, Sections = sections } );

  [Fact]
  public void SectionHeader_IsEmitted_AtTheOrigin()
  {
    DataTabModel t = Build( 5, new() { B( "$", 1, 0 ) }, new() { Sec( "Currency", 0 ) } );

    Assert.Single( t.Headers );
    Assert.Equal( "Currency", t.Headers[0].Name );
    Assert.Equal( EG, t.Headers[0].X );
    Assert.Equal( EG, t.Headers[0].Y );
  }

  [Fact]
  public void SectionHeader_ReservesItsHeight_PushingButtonsDown()
  {
    DataTabModel t = Build( 5, new() { B( "A", 1, 0 ) }, new() { Sec( "S", 0, 30 ) } );

    Assert.Equal( 30, t.Headers[0].Height );
    Assert.Equal( EG + 30, t.Symbols[0].Y ); // the button sits just below the 30px header
  }

  [Fact]
  public void SectionHeader_DefaultsToTheLayoutConstantHeight()
  {
    DataTabModel t = Build( 5, new() { B( "A", 1, 0 ) }, new() { Sec( "S", 0 ) } );

    Assert.Equal( Layout.SectionHeaderHeight, t.Headers[0].Height );
    Assert.Equal( EG + Layout.SectionHeaderHeight, t.Symbols[0].Y );
  }

  [Fact]
  public void HeaderWidth_SpansAllColumns()
  {
    DataTabModel t = Build( 4, new(), new() { Sec( "S", 0 ) } );
    Assert.Equal( 35 * 4 + Layout.ButtonGap * 3, t.Headers[0].Width );
  }

  [Fact]
  public void UnnamedSection_IsADividerWithNoLabel()
  {
    DataTabModel t = Build( 5,
      new() { B( "A", 0, 0 ), B( "B", 2, 0 ) },
      new() { Sec( "", 1 ) } );             // present-but-empty = a plain separator line

    Assert.Single( t.Headers );
    Assert.Equal( "", t.Headers[0].Name );
    Assert.Equal( 2, t.Symbols.Count );
  }

  [Fact]
  public void TabSig_ChangesWhenSectionLabelChanges()
  {
    var a = new TabEntry { Name = "T", Columns = 3, Sections = new() { Sec( "One", 0 ) } };
    var b = new TabEntry { Name = "T", Columns = 3, Sections = new() { Sec( "Two", 0 ) } };

    Assert.NotEqual( VersionStamp.TabSig( a ), VersionStamp.TabSig( b ) );
  }

  [Fact]
  public void Merge_PreservesSections()
  {
    // A tab whose winning layout has a section divider above a button.
    TabEntry Make( long mod ) => new()
    {
      Id = "t", Mod = mod, Name = "T", Columns = 3,
      Sections = new() { Sec( "Group", 0, 28 ) },
      Buttons  = new() { new ButtonDef { Id = "a", Mod = 1, Text = "A", Row = 1, Col = 0 } },
    };

    var local    = new TabFile { Tabs = { Make( 2 ) } };
    var incoming = new TabFile { Tabs = { Make( 1 ) } };

    TabFile m = VersionMerge.Merge( local, incoming );

    SectionDef header = m.Tabs[0].Sections![0];
    Assert.Equal( "Group", header.Name );
    Assert.Equal( 28, header.Height );
  }

  [Fact]
  public void SpanningHeading_IsButtonRowHeight_AndStartsAtItsColumn()
  {
    int cw = 35 + Layout.ButtonGap;
    DataTabModel t = Build( 8, new(), new() { Head( "H", 0, 2, 3 ) } );

    TabModel.SectionHeader h = t.Headers[0];
    Assert.Equal( EG + 2 * cw, h.X );                            // starts at column 2
    Assert.Equal( 35 * 3 + Layout.ButtonGap * 2, h.Width );      // spans 3 cells
    Assert.Equal( 35 + Layout.ButtonGap, h.Height );             // same height as a button row
  }

  [Fact]
  public void InsertBlankRow_ShiftsRowsAtAndBelowDown()
  {
    var tab = new TabEntry
    {
      Name = "T", Columns = 8,
      Buttons  = new() { B( "A", 0, 0 ), B( "B", 1, 0 ), B( "C", 2, 0 ) },
      Sections = new() { Sec( "H", 1 ) },
    };

    TabStore.InsertBlankRow( tab, 1 ); // open a blank row at index 1

    Assert.Equal( 0, tab.Buttons![0].Row );  // A unchanged (above the insert)
    Assert.Equal( 2, tab.Buttons![1].Row );  // B: 1 → 2
    Assert.Equal( 3, tab.Buttons![2].Row );  // C: 2 → 3
    Assert.Equal( 2, tab.Sections![0].Row ); // section at 1 → 2
  }

  [Fact]
  public void InsertHeadingRow_ShiftsDown_AndAddsHeadingAtTheRow()
  {
    var tab = new TabEntry { Name = "T", Columns = 8, Buttons = new() { B( "A", 1, 0 ) } };
    var heading = new SectionDef { Name = "Group", Col = 0, Span = 8 };

    TabStore.InsertHeadingRow( tab, 1, heading );

    Assert.Equal( 2, tab.Buttons![0].Row );      // A: 1 → 2 (its column is under the full-width heading)
    Assert.Single( tab.Sections! );
    Assert.Equal( 1, tab.Sections![0].Row );      // heading occupies the freed row
    Assert.Equal( "Group", tab.Sections![0].Name );
  }

  [Fact]
  public void InsertHeadingRow_ShiftsOnlyTheColumnsUnderTheHeading()
  {
    var tab = new TabEntry
    {
      Name = "T", Columns = 8,
      Buttons = new() { B( "under", 1, 2 ), B( "aside", 1, 6 ) },
    };
    var heading = new SectionDef { Name = "H", Col = 2, Span = 3 }; // covers columns 2–4

    TabStore.InsertHeadingRow( tab, 1, heading );

    Assert.Equal( 2, tab.Buttons!.First( b => b.Text == "under" ).Row ); // in the span → pushed down
    Assert.Equal( 1, tab.Buttons!.First( b => b.Text == "aside" ).Row ); // outside the span → unchanged
    Assert.Equal( 1, tab.Sections!.Single().Row );
  }

  [Fact]
  public void InsertHeadingRow_StaircasesToAWiderHeadingBelow()
  {
    // Insert a 2–4 heading at row 0, above a 1–5 heading at row 2. Cols 2–4 shift from row 0;
    // at the 1–5 heading the range widens to 1–5 and cols 1 & 5 shift from row 2 downward.
    var tab = new TabEntry
    {
      Name = "T", Columns = 8,
      Buttons = new()
      {
        B( "a1", 0, 1 ),  // col 1, above the wide heading → stays (outside 2–4)
        B( "b2", 1, 2 ),  // col 2 → in 2–4 → shifts
        B( "c1", 3, 1 ),  // col 1, below the wide heading → in widened 1–5 → shifts
        B( "c6", 3, 6 ),  // col 6 → outside 1–5 → stays
      },
      Sections = new() { new SectionDef { Name = "wide", Row = 2, Col = 1, Span = 5 } },
    };
    var heading = new SectionDef { Name = "H", Col = 2, Span = 3 };

    TabStore.InsertHeadingRow( tab, 0, heading );

    ButtonDef Get( string t ) => tab.Buttons!.First( b => b.Text == t );
    Assert.Equal( 0, Get( "a1" ).Row );  // col 1 above the wide heading: unaffected
    Assert.Equal( 2, Get( "b2" ).Row );  // col 2: 1 → 2
    Assert.Equal( 3, tab.Sections!.First( s => s.Name == "wide" ).Row ); // wide heading 2 → 3
    Assert.Equal( 4, Get( "c1" ).Row );  // col 1 below the wide heading: 3 → 4
    Assert.Equal( 3, Get( "c6" ).Row );  // col 6 outside the widened range: unchanged
    Assert.Equal( 0, tab.Sections!.First( s => s.Name == "H" ).Row ); // inserted heading at row 0
  }

  [Fact]
  public void Heading_CarriesItsTextAlignment()
  {
    DataTabModel t = Build( 6, new(),
      new() { new SectionDef { Name = "H", Row = 0, Col = 1, Span = 3, Align = "right" } } );

    Assert.Equal( "right", t.Headers[0].Align );
  }

  [Fact]
  public void TabSig_ChangesWhenHeadingAlignChanges()
  {
    var a = new TabEntry { Name = "T", Columns = 6, Sections = new() { new SectionDef { Name = "H", Row = 0, Align = "left" } } };
    var b = new TabEntry { Name = "T", Columns = 6, Sections = new() { new SectionDef { Name = "H", Row = 0, Align = "right" } } };

    Assert.NotEqual( VersionStamp.TabSig( a ), VersionStamp.TabSig( b ) );
  }

  [Fact]
  public void TabSig_ChangesWhenHeadingColumnOrSpanChanges()
  {
    var a = new TabEntry { Name = "T", Columns = 8, Sections = new() { Head( "H", 0, 1, 2 ) } };
    var b = new TabEntry { Name = "T", Columns = 8, Sections = new() { Head( "H", 0, 1, 3 ) } }; // span
    var c = new TabEntry { Name = "T", Columns = 8, Sections = new() { Head( "H", 0, 2, 2 ) } }; // col

    Assert.NotEqual( VersionStamp.TabSig( a ), VersionStamp.TabSig( b ) );
    Assert.NotEqual( VersionStamp.TabSig( a ), VersionStamp.TabSig( c ) );
  }
}
