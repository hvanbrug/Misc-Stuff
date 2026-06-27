using HenksHotkeys.Core;
using HenksHotkeys.UI;
using Xunit;

namespace HenksHotkeys.Tests;

// Section headers are header rows (a row with a "section" key): they label and
// space the rows beneath them, and are emitted as TabModel.SectionHeader elements.
public class SectionTests
{
  private const int EG = Layout.EdgeGap;

  private static ButtonDef B( string t ) => new() { Text = t };

  private static DataTabModel Build( int cols, params RowDef[] rows )
    => new( new TabEntry { Name = "T", Columns = cols, Rows = rows.ToList() } );

  [Fact]
  public void SectionHeader_IsEmitted_AtTheOrigin()
  {
    DataTabModel t = Build( 5,
      new RowDef { Section = "Currency" },
      new RowDef { Buttons = { B( "$" ) } } );

    Assert.Single( t.Headers );
    Assert.Equal( "Currency", t.Headers[0].Name );
    Assert.Equal( EG, t.Headers[0].X );
    Assert.Equal( EG, t.Headers[0].Y );
  }

  [Fact]
  public void SectionHeader_ReservesItsHeight_PushingButtonsDown()
  {
    DataTabModel t = Build( 5,
      new RowDef { Section = "S", HeaderHeight = 30 },
      new RowDef { Buttons = { B( "A" ) } } );

    Assert.Equal( 30, t.Headers[0].Height );
    Assert.Equal( EG + 30, t.Symbols[0].Y ); // the button sits just below the 30px header
  }

  [Fact]
  public void SectionHeader_DefaultsToTheLayoutConstantHeight()
  {
    DataTabModel t = Build( 5,
      new RowDef { Section = "S" },
      new RowDef { Buttons = { B( "A" ) } } );

    Assert.Equal( Layout.SectionHeaderHeight, t.Headers[0].Height );
    Assert.Equal( EG + Layout.SectionHeaderHeight, t.Symbols[0].Y );
  }

  [Fact]
  public void HeaderWidth_SpansAllColumns()
  {
    DataTabModel t = Build( 4, new RowDef { Section = "S" } );
    Assert.Equal( 35 * 4 + Layout.ButtonGap * 3, t.Headers[0].Width );
  }

  [Fact]
  public void UnnamedSection_IsADividerWithNoLabel()
  {
    DataTabModel t = Build( 5,
      new RowDef { Buttons = { B( "A" ) } },
      new RowDef { Section = "" },          // present-but-empty = a plain separator line
      new RowDef { Buttons = { B( "B" ) } } );

    Assert.Single( t.Headers );
    Assert.Equal( "", t.Headers[0].Name );
    Assert.Equal( 2, t.Symbols.Count );
  }

  [Fact]
  public void NormalizeRows_ClearsButtonsFromASectionRow()
  {
    var t = new TabEntry { Name = "T", Columns = 3,
      Rows = new() { new RowDef { Section = "S", Buttons = { B( "x" ) } } } };

    Assert.True( VersionStamp.NormalizeRows( t ) );
    Assert.Empty( t.Rows![0].Buttons );
  }

  [Fact]
  public void TabSig_ChangesWhenSectionLabelChanges()
  {
    var a = new TabEntry { Name = "T", Columns = 3, Rows = new() { new RowDef { Section = "One" } } };
    var b = new TabEntry { Name = "T", Columns = 3, Rows = new() { new RowDef { Section = "Two" } } };

    Assert.NotEqual( VersionStamp.TabSig( a ), VersionStamp.TabSig( b ) );
  }

  [Fact]
  public void Merge_PreservesSectionRows()
  {
    // A tab whose winning layout has a section header above a button row.
    TabEntry Make( long mod ) => new()
    {
      Id = "t", Mod = mod, Name = "T", Columns = 3,
      Rows = new()
      {
        new RowDef { Section = "Group", HeaderHeight = 28 },
        new RowDef { Buttons = { new ButtonDef { Id = "a", Mod = 1, Text = "A" } } },
      },
    };

    var local    = new TabFile { Tabs = { Make( 2 ) } };
    var incoming = new TabFile { Tabs = { Make( 1 ) } };

    TabFile m = VersionMerge.Merge( local, incoming );

    RowDef header = m.Tabs[0].Rows![0];
    Assert.Equal( "Group", header.Section );
    Assert.Equal( 28, header.HeaderHeight );
    Assert.True( header.IsSection );
  }
}
