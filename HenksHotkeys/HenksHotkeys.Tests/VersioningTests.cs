using HenksHotkeys.Core;
using Newtonsoft.Json;
using Xunit;

namespace HenksHotkeys.Tests;

public class VersioningTests
{
  private static ButtonDef Btn( string? id, long mod, string text, string? desc = null )
    => new() { Id = id, Mod = mod, Text = text, Desc = desc };

  private static TabEntry Tab( string? id, long mod, string name, params ButtonDef[] btns )
    => new() { Id = id, Mod = mod, Name = name, Columns = 3, Rows = new() { new RowDef { Buttons = btns.ToList() } } };

  private static TabFile File( params TabEntry[] tabs ) => new() { Tabs = tabs.ToList() };

  private static HashSet<string> ButtonIds( TabFile f, string tabId )
    => f.Tabs.Where( t => t.Id == tabId )
            .SelectMany( t => VersionStamp.Buttons( t ) )
            .Select( b => b.Id! ).ToHashSet();

  private static ButtonDef? Find( TabFile f, string buttonId )
    => f.Tabs.SelectMany( VersionStamp.Buttons ).FirstOrDefault( b => b.Id == buttonId );

  // ── Stamp ────────────────────────────────────────────────────────
  [Fact]
  public void Stamp_AssignsIdsAndClocks_ToNewElements()
  {
    var f = File( new TabEntry { Name = "T", Columns = 3,
      Rows = new() { new RowDef { Buttons = { new ButtonDef { Text = "A" } } } } } );

    Assert.True( VersionStamp.Stamp( f, null ) );

    TabEntry t = f.Tabs[0];
    Assert.False( string.IsNullOrEmpty( t.Id ) );
    Assert.True( t.Mod > 0 );
    ButtonDef b = t.Rows![0].Buttons[0];
    Assert.False( string.IsNullOrEmpty( b.Id ) );
    Assert.True( b.Mod > 0 );
  }

  [Fact]
  public void Stamp_TombstonesRemovedButtons()
  {
    TabFile shadow = File( Tab( "t", 1, "T", Btn( "a", 1, "A" ), Btn( "b", 1, "B" ) ) );
    TabFile cur    = File( Tab( "t", 1, "T", Btn( "a", 1, "A" ) ) ); // b removed

    Assert.True( VersionStamp.Stamp( cur, shadow ) );
    Assert.NotNull( cur.Deleted );
    Assert.Contains( cur.Deleted!, d => d.Id == "b" );
  }

  // ── Merge ────────────────────────────────────────────────────────
  [Fact]
  public void Merge_UnionsButtonsAddedOnEachSide()
  {
    TabFile local    = File( Tab( "t", 1, "T", Btn( "a", 1, "A" ), Btn( "b", 2, "B" ) ) );
    TabFile incoming = File( Tab( "t", 1, "T", Btn( "a", 1, "A" ), Btn( "c", 2, "C" ) ) );

    TabFile m = VersionMerge.Merge( local, incoming );

    Assert.Equal( new HashSet<string> { "a", "b", "c" }, ButtonIds( m, "t" ) );
  }

  [Fact]
  public void Merge_NewerEditWins()
  {
    TabFile local    = File( Tab( "t", 1, "T", Btn( "a", 5, "old" ) ) );
    TabFile incoming = File( Tab( "t", 1, "T", Btn( "a", 9, "new" ) ) );

    Assert.Equal( "new", Find( VersionMerge.Merge( local, incoming ), "a" )!.Text );
    Assert.Equal( "new", Find( VersionMerge.Merge( incoming, local ), "a" )!.Text ); // order-independent
  }

  [Fact]
  public void Merge_DeletePropagates()
  {
    TabFile local    = File( Tab( "t", 1, "T", Btn( "a", 1, "A" ), Btn( "b", 1, "B" ) ) );
    TabFile incoming = File( Tab( "t", 1, "T", Btn( "a", 1, "A" ) ) );
    incoming.Deleted = new() { new Tombstone { Id = "b", Mod = 5 } };

    Assert.DoesNotContain( "b", ButtonIds( VersionMerge.Merge( local, incoming ), "t" ) );
  }

  [Fact]
  public void Merge_NewerEditBeatsOlderDelete()
  {
    TabFile local    = File( Tab( "t", 1, "T", Btn( "a", 1, "A" ) ) );
    local.Deleted    = new() { new Tombstone { Id = "b", Mod = 5 } };
    TabFile incoming = File( Tab( "t", 1, "T", Btn( "a", 1, "A" ), Btn( "b", 9, "revived" ) ) );

    Assert.Contains( "b", ButtonIds( VersionMerge.Merge( local, incoming ), "t" ) );
  }

  [Fact]
  public void Merge_AddsNewTabFromIncoming()
  {
    TabFile local    = File( Tab( "t1", 1, "One", Btn( "a", 1, "A" ) ) );
    TabFile incoming = File( Tab( "t1", 1, "One", Btn( "a", 1, "A" ) ),
                             Tab( "t2", 1, "Two", Btn( "z", 1, "Z" ) ) );

    TabFile m = VersionMerge.Merge( local, incoming );
    Assert.Equal( new[] { "t1", "t2" }, m.Tabs.Select( t => t.Id ) );
  }

  [Fact]
  public void Merge_SurvivesJsonRoundTrip_LikeFileShare()
  {
    // Machine A: starts from a stamped base, adds button "b".
    var a = File( Tab( null, 0, "T", Btn( null, 0, "A" ) ) );
    VersionStamp.Stamp( a, null );
    string baseJson = JsonConvert.SerializeObject( a );

    // Both machines start from that shared base (deserialized copies).
    TabFile local    = JsonConvert.DeserializeObject<TabFile>( baseJson )!;
    TabFile incoming = JsonConvert.DeserializeObject<TabFile>( baseJson )!;

    string tabId = local.Tabs[0].Id!;
    local.Tabs[0].Rows![0].Buttons.Add( Btn( "loc", VersionStamp.Now() + 10, "FromLaptop" ) );
    incoming.Tabs[0].Rows![0].Buttons.Add( Btn( "vm", VersionStamp.Now() + 20, "FromVM" ) );

    // Simulate writing/reading files, then merge incoming into local.
    local    = JsonConvert.DeserializeObject<TabFile>( JsonConvert.SerializeObject( local ) )!;
    incoming = JsonConvert.DeserializeObject<TabFile>( JsonConvert.SerializeObject( incoming ) )!;
    TabFile merged = VersionMerge.Merge( local, incoming );

    HashSet<string> ids = ButtonIds( merged, tabId );
    Assert.Contains( "loc", ids );   // laptop's addition kept
    Assert.Contains( "vm",  ids );   // vm's addition kept (not clobbered)
  }

  [Fact]
  public void Reconcile_ThenMerge_DoesNotDuplicateIndependentlyStampedTabs()
  {
    // Same "Symbols" tab independently stamped with different ids on each machine.
    TabFile vm  = File( Tab( "Vt", 1, "Symbols", Btn( "Vb", 1, "A" ), Btn( "Vd", 1, "VMonly" ) ) );
    TabFile lap = File( Tab( "Lt", 1, "Symbols", Btn( "Lb", 1, "A" ), Btn( "Lc", 1, "LapOnly" ) ) );

    VersionMerge.ReconcileIds( vm, lap );
    TabFile m = VersionMerge.Merge( vm, lap );

    Assert.Single( m.Tabs );                                  // one Symbols tab, not two
    HashSet<string> texts = m.Tabs[0].Rows!.SelectMany( r => r.Buttons ).Select( b => b.Text ).ToHashSet();
    Assert.Equal( new HashSet<string> { "A", "VMonly", "LapOnly" }, texts ); // "A" unified, both extras kept
  }

  [Fact]
  public void CollapseDuplicateTabs_MergesSameNameTabs_KeepingDistinctButtons()
  {
    TabFile f = File(
      Tab( "t1", 2, "Symbols", Btn( "a", 1, "A" ), Btn( "b", 1, "B" ) ),
      Tab( "t2", 1, "Symbols", Btn( "c", 1, "A" ), Btn( "d", 1, "VMonly" ) ) ); // c duplicates A

    int removed = VersionMerge.CollapseDuplicateTabs( f );

    Assert.Equal( 1, removed );
    Assert.Single( f.Tabs );
    HashSet<string> texts = f.Tabs[0].Rows!.SelectMany( r => r.Buttons ).Select( b => b.Text ).ToHashSet();
    Assert.Equal( new HashSet<string> { "A", "B", "VMonly" }, texts ); // duplicate "A" dropped
  }

  [Fact]
  public void NormalizeRows_SplitsAnOverWideRow_ToColumnWidth()
  {
    var btns = Enumerable.Range( 0, 7 ).Select( i => Btn( "b" + i, 1, "x" + i ) ).ToArray();
    var t = new TabEntry { Name = "T", Columns = 3, Rows = new() { new RowDef { Buttons = btns.ToList() } } };

    Assert.True( VersionStamp.NormalizeRows( t ) );
    Assert.All( t.Rows!, r => Assert.True( r.Buttons.Count <= 3 ) );  // no row exceeds columns
    Assert.Equal( 7, t.Rows!.Sum( r => r.Buttons.Count ) );          // nothing lost
    Assert.False( VersionStamp.NormalizeRows( t ) );                 // idempotent
  }

  [Fact]
  public void Merge_IsCommutative_OnButtonSet()
  {
    TabFile local    = File( Tab( "t", 2, "T", Btn( "a", 3, "A2" ), Btn( "b", 1, "B" ) ) );
    TabFile incoming = File( Tab( "t", 1, "T", Btn( "a", 1, "A1" ), Btn( "c", 1, "C" ) ) );

    Assert.Equal( ButtonIds( VersionMerge.Merge( local, incoming ), "t" ),
                  ButtonIds( VersionMerge.Merge( incoming, local ), "t" ) );
  }
}
