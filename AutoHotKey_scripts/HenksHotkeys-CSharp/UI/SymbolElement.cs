namespace HenksHotkeys.UI;

/// <summary>
/// One placed button definition on a tab, mirroring the element object built by
/// TabPage.RegisterSymbol in UITabPage.ahk.
/// </summary>
internal sealed class SymbolElement
{
  public int    Line;
  public int    Slot;
  public int    Width;
  public int    X;
  public int    Y;
  public int    W;
  public int    H;
  public string Char     = "";
  public string Desc     = "";
  public bool   ShowChar;
  public bool   TipChar;
  public string Hotkey   = "";
  public string Align    = "center";

  public Action ClickAction = static () => { };

  /// <summary>The control created for this element (set at layout time).</summary>
  public object? Ctrl;
}
