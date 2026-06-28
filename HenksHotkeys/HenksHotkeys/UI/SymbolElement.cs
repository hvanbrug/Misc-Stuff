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

  /// <summary>The data-tab button this element was built from, if any. Null for the
  /// built-in code tabs (Emojis / Tools), which aren't JSON-editable. Gives the
  /// right-click menu a handle on the underlying model to edit / delete.</summary>
  public Core.ButtonDef? Source;

  /// <summary>True when this element is a blank spacer cell: it occupies its cell but
  /// draws nothing (a faint border on hover) and sends nothing — still right-click
  /// editable so it can be turned into a real button.</summary>
  public bool IsBlank;
}
