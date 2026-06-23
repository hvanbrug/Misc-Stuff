using System.Drawing;
using HenksHotkeys.Native;

namespace HenksHotkeys.UI;

/// <summary>
/// DPI scaling helper. The layout maths (TabModel) are authored in logical
/// 96-dpi units, exactly like the AutoHotkey originals; the UI then scales those
/// pixel values — and the fonts — by the monitor DPI when building controls.
/// WinForms' own auto-scaling is turned off (AutoScaleMode.None) so this is the
/// single source of scaling, mirroring how the AHK GUI multiplied its layout by
/// the per-window DPI factor.
/// </summary>
internal static class Dpi
{
  private static float? s_scale;

  public static float Scale
  {
    get
    {
      s_scale ??= ComputeScale();
      return s_scale.Value;
    }
  }

  private static float ComputeScale()
  {
    try
    {
      uint dpi = NativeMethods.GetDpiForSystem();
      return dpi > 0 ? dpi / 96f : 1f;
    }
    catch
    {
      return 1f;
    }
  }

  /// <summary>Scale a logical (96-dpi) pixel value to physical pixels.</summary>
  public static int S( int logicalPixels ) => (int)Math.Round( logicalPixels * Scale );

  /// <summary>Scale a logical pixel value, keeping it a float.</summary>
  public static float SF( float logicalPixels ) => logicalPixels * Scale;

  /// <summary>
  /// Build a font whose size is expressed in physical pixels scaled by DPI, so
  /// rendering is deterministic regardless of WinForms point/DPI handling. The
  /// input is the AHK point size (e.g. "s14" → 14).
  /// </summary>
  public static Font ScaledFont( string name, float pointSize )
  {
    float px = pointSize * ( 96f / 72f ) * Scale;
    return new Font( name, px, GraphicsUnit.Pixel );
  }
}
