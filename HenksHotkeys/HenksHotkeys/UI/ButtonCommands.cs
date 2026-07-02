using System.Windows;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Right-click actions on a data-tab button: edit its properties, or delete it. Both
/// mutate the live model, persist via <see cref="TabStore"/>, and rebuild the UI.
/// </summary>
internal static class ButtonCommands
{
  public static void Edit( ButtonDef button )
  {
    if( ButtonEditDialog.Show( button ) )
    {
      TabStore.SaveCurrent();
      AppState.RequestReload?.Invoke();
    }
  }

  /// <summary>Add a new button at the grid cell the user right-clicked. An empty cell takes
  /// the button directly; clicking on a button inserts before/after it (shifting the row);
  /// below everything starts a new row.</summary>
  public static void AddHere( DataTabModel model, System.Windows.Point at )
  {
    var newButton = new ButtonDef();
    if( !ButtonEditDialog.Show( newButton, "Add button" ) )
    {
      return;
    }

    // Pass the new button so a sub-cell one targets the clicked cell (to join a free slot)
    // rather than insert-shifting the row.
    DropSpot spot = model.ResolveDrop( at, newButton );
    TabStore.AddButtonAt( model.Entry, spot.Row, spot.Col, newButton );
    AppState.RequestReload?.Invoke();
  }

  public static void Delete( ButtonDef button )
  {
    string label = button.Desc ?? button.Text;
    MessageBoxResult answer = MessageBox.Show(
      $"Delete this button{( string.IsNullOrEmpty( label ) ? "" : $" ({label})" )}?",
      "Delete button", MessageBoxButton.YesNo, MessageBoxImage.Question );

    if( answer == MessageBoxResult.Yes && TabStore.DeleteButton( button ) )
    {
      AppState.RequestReload?.Invoke();
    }
  }
}
