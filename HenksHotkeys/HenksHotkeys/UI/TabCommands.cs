using System.Windows;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Right-click / drag actions on the tab strip: add a new data tab, edit an existing data tab's
/// settings, delete a tab, or reorder tabs. All mutate the live config via <see cref="TabStore"/>
/// and rebuild the UI. Built-in code tabs (Emojis / Tools) can be reordered or deleted but not
/// edited (their content isn't data).
/// </summary>
internal static class TabCommands
{
  /// <summary>Add a new data tab, inserted before <paramref name="atIndex"/> (append when past
  /// the end). Opens the tab editor first; nothing is added if it's cancelled.</summary>
  public static void Add( int atIndex )
  {
    var tab = new TabEntry { Name = "New tab", Columns = 6, Buttons = new List<ButtonDef>() };
    if( !TabEditDialog.Show( tab, "Add tab" ) )
    {
      return;
    }
    TabStore.AddTab( tab, atIndex );
    AppState.RequestReload?.Invoke();
  }

  /// <summary>Edit a data tab's settings. No-op for a built-in tab.</summary>
  public static void Edit( TabEntry tab )
  {
    if( !string.IsNullOrEmpty( tab.Builtin ) )
    {
      return;
    }
    if( TabEditDialog.Show( tab ) )
    {
      TabStore.SaveCurrent();
      AppState.RequestReload?.Invoke();
    }
  }

  public static void Delete( TabEntry tab )
  {
    string label = string.IsNullOrEmpty( tab.Name ) ? "this tab" : $"“{tab.Name}”";
    if( MessageBox.Show( $"Delete {label} and all its buttons?", "Delete tab",
                         MessageBoxButton.YesNo, MessageBoxImage.Warning ) != MessageBoxResult.Yes )
    {
      return;
    }
    if( TabStore.DeleteTab( tab ) )
    {
      AppState.RequestReload?.Invoke();
    }
    else
    {
      MessageBox.Show( "That's the only tab left — add another before deleting this one.",
                       "Delete tab", MessageBoxButton.OK, MessageBoxImage.Information );
    }
  }

  /// <summary>Move <paramref name="tab"/> so it lands before <paramref name="insertBeforeIndex"/>
  /// (an index into the current list). Used by tab-header drag-to-reorder.</summary>
  public static void Move( TabEntry tab, int insertBeforeIndex )
  {
    if( TabStore.MoveTab( tab, insertBeforeIndex ) )
    {
      AppState.RequestReload?.Invoke();
    }
  }
}
