using HenksHotkeys.Core;
using Xunit;

namespace HenksHotkeys.Tests;

// Tab-level operations (#11 / #15): the pure reorder index math, and that a reorder and a
// tab deletion survive the cross-machine merge.
public class TabOpsTests
{
  private static TabEntry T( string name ) => new() { Name = name, Columns = 1 };
  private static string[] Names( TabFile f ) => f.Tabs.Select( t => t.Name! ).ToArray();

  [Fact]
  public void ReorderTabs_MovesForward_AccountingForTheRemoval()
  {
    var a = T( "A" ); var b = T( "B" ); var c = T( "C" );
    var tabs = new List<TabEntry> { a, b, c };

    Assert.True( TabStore.ReorderTabs( tabs, a, insertBeforeIndex: 2 ) ); // drop A before C
    Assert.Equal( new[] { "B", "A", "C" }, tabs.Select( t => t.Name ) );
  }

  [Fact]
  public void ReorderTabs_MovesBackward_ToTheFront()
  {
    var a = T( "A" ); var b = T( "B" ); var c = T( "C" );
    var tabs = new List<TabEntry> { a, b, c };

    Assert.True( TabStore.ReorderTabs( tabs, c, insertBeforeIndex: 0 ) );
    Assert.Equal( new[] { "C", "A", "B" }, tabs.Select( t => t.Name ) );
  }

  [Fact]
  public void ReorderTabs_IsANoOp_WhenThePositionIsUnchanged()
  {
    var a = T( "A" ); var b = T( "B" );
    var tabs = new List<TabEntry> { a, b };

    Assert.False( TabStore.ReorderTabs( tabs, a, 0 ) ); // already first
    Assert.False( TabStore.ReorderTabs( tabs, a, 1 ) ); // before itself → same slot
    Assert.Equal( new[] { "A", "B" }, tabs.Select( t => t.Name ) );
  }

  [Fact]
  public void Merge_KeepsTheLocalTabOrder_AfterAReorder()
  {
    var a = T( "A" ); var b = T( "B" );
    var stamped = new TabFile { Tabs = { a, b } };
    VersionStamp.Stamp( stamped, null ); // assign ids / clocks

    var local    = new TabFile { Tabs = { b, a } }; // reordered locally (same entry instances)
    var incoming = new TabFile
    {
      Tabs =
      {
        new TabEntry { Id = a.Id, Mod = a.Mod, Name = "A", Columns = 1 },
        new TabEntry { Id = b.Id, Mod = b.Mod, Name = "B", Columns = 1 },
      },
    };

    TabFile merged = VersionMerge.Merge( local, incoming );

    Assert.Equal( new[] { "B", "A" }, Names( merged ) ); // local order wins
  }

  [Fact]
  public void Stamp_TombstonesADeletedTab()
  {
    var a = T( "A" ); var b = T( "B" );
    var file = new TabFile { Tabs = { a, b } };
    VersionStamp.Stamp( file, null );

    var shadow = new TabFile
    {
      Tabs =
      {
        new TabEntry { Id = a.Id, Mod = a.Mod, Name = "A", Columns = 1 },
        new TabEntry { Id = b.Id, Mod = b.Mod, Name = "B", Columns = 1 },
      },
    };

    string deletedId = b.Id!;
    file.Tabs.Remove( b );
    VersionStamp.Stamp( file, shadow );

    Assert.NotNull( file.Deleted );
    Assert.Contains( file.Deleted!, d => d.Id == deletedId );
  }
}
