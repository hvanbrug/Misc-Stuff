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

  /// <summary>Width reserved for the vertical scrollbar so the locked window
  /// width doesn't clip the rightmost buttons when it appears.</summary>
  public const int ScrollBarWidth = 14;
}
