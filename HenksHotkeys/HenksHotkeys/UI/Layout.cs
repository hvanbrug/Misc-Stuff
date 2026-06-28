namespace HenksHotkeys.UI;

/// <summary>
/// Central layout constants (device-independent pixels). One place to tune the
/// spacing instead of the magic numbers that used to be scattered through the
/// tab geometry and the window chrome.
/// </summary>
internal static class Layout
{
  /// <summary>Gap between adjacent buttons, horizontally and vertically.</summary>
  public const int ButtonGap = 4;

  /// <summary>Gap between a container's edge and the controls inside it
  /// (window border, tab control, the button grid).</summary>
  public const int EdgeGap = 2;

  /// <summary>Gap between a tab's inner boundary and its buttons — independent of
  /// <see cref="EdgeGap"/> so the button grid can breathe inside the tab while the
  /// tab control itself still hugs the window with the regular edge gap.</summary>
  public const int TabEdgeGap = 8;

  /// <summary>Default height of a section-header label (device-independent px)
  /// when a header row doesn't set its own <c>headerHeight</c>.</summary>
  public const int SectionHeaderHeight = 24;

  /// <summary>Width reserved for the themed vertical scrollbar so the locked window
  /// width doesn't clip the rightmost buttons. Keep in sync with the ScrollBar style
  /// width in App.xaml.</summary>
  public const int ScrollBarWidth = 11;
}
