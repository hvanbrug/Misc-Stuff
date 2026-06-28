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

  /// <summary>Add a new button where the user right-clicked the open tab area. The new
  /// button is placed next to the nearest existing button (after it when the click is to
  /// its right or below, before it otherwise); an empty tab just gets a first row.</summary>
  public static void AddHere( DataTabModel model, System.Windows.Point at )
  {
    var newButton = new ButtonDef();
    if( !ButtonEditDialog.Show( newButton, "Add button" ) )
    {
      return;
    }

    SymbolElement? nearest = null;
    double best = double.MaxValue;
    foreach( SymbolElement s in model.Symbols )
    {
      if( s.Source is null )
      {
        continue;
      }
      double dx = at.X - ( s.X + s.W * 0.5 );
      double dy = at.Y - ( s.Y + s.H * 0.5 );
      double d2 = dx * dx + dy * dy;
      if( d2 < best )
      {
        best    = d2;
        nearest = s;
      }
    }

    if( nearest?.Source is ButtonDef anchor )
    {
      bool after = at.Y > nearest.Y + nearest.H || at.X >= nearest.X + nearest.W * 0.5;
      TabStore.InsertButton( anchor, newButton, after );
    }
    else
    {
      TabStore.AddButton( model.Entry, newButton );
    }
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
