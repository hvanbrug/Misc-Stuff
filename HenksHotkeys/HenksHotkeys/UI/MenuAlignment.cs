using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace HenksHotkeys.UI;

/// <summary>
/// Forces right-handed placement for context menus and tooltips. WPF places them to the
/// LEFT of the cursor when Windows' "left-handed" menu setting
/// (<see cref="SystemParameters.MenuDropAlignment"/>) is on; this clears WPF's cached
/// value for our process only (it does not change the system setting). WPF re-reads the
/// value on a system settings broadcast, so we re-apply it whenever that happens.
/// </summary>
internal static class MenuAlignment
{
  private static readonly FieldInfo? s_field =
    typeof( SystemParameters ).GetField( "_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static );

  public static void ForceRightHanded()
  {
    Apply();
    SystemEvents.UserPreferenceChanged += ( _, _ ) => Apply();
  }

  private static void Apply()
  {
    if( SystemParameters.MenuDropAlignment )
    {
      s_field?.SetValue( null, false );
    }
  }
}
