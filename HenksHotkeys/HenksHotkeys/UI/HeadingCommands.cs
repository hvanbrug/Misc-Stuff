using System.Windows;
using HenksHotkeys.Core;

namespace HenksHotkeys.UI;

/// <summary>
/// Right-click actions for headings on a data tab: insert a new heading where the user
/// clicked (starting at that column, spanning to the row's end by default), or edit / delete
/// an existing one. All mutate the live model, persist via <see cref="TabStore"/>, and rebuild.
/// </summary>
internal static class HeadingCommands
{
  public static void AddHere( DataTabModel model, System.Windows.Point at )
  {
    int columns = Math.Max( 1, model.Entry.Columns );
    int row     = model.RowAt( at.Y );
    int col     = Math.Min( model.ColAt( at.X ), columns - 1 );

    var heading = new SectionDef { Row = row, Col = col, Span = Math.Max( 1, columns - col ) };
    if( !HeadingEditDialog.Show( heading, columns, "Insert heading" ) )
    {
      return;
    }
    TabStore.AddHeading( model.Entry, heading );
    AppState.RequestReload?.Invoke();
  }

  public static void Edit( DataTabModel model, SectionDef heading )
  {
    if( HeadingEditDialog.Show( heading, Math.Max( 1, model.Entry.Columns ) ) )
    {
      TabStore.SaveCurrent();
      AppState.RequestReload?.Invoke();
    }
  }

  public static void Delete( SectionDef heading )
  {
    string label = string.IsNullOrEmpty( heading.Name ) ? "divider" : $"“{heading.Name}”";
    if( MessageBox.Show( $"Delete heading {label}?", "Delete heading",
                         MessageBoxButton.YesNo, MessageBoxImage.Question ) == MessageBoxResult.Yes
        && TabStore.DeleteHeading( heading ) )
    {
      AppState.RequestReload?.Invoke();
    }
  }
}
