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

  public Action ClickAction = static () => {};

  /// <summary>The control created for this element (set at layout time).</summary>
  public object? Ctrl;

  /// <summary>The data-tab button this element was built from, if any. Null for the
  /// built-in code tabs (Emojis / Tools), which aren't JSON-editable. Gives the
  /// right-click menu a handle on the underlying model to edit / delete.</summary>
  public Core.ButtonDef? Source;

  /// <summary>True for an emoji sitting in the Emojis tab's user "Favourites" section (#13):
  /// it gets the Unfavourite menu and can be dragged to reorder. Main-catalog emoji are false
  /// (they get the Mark-as-favourite menu instead).</summary>
  public bool IsFavourite;
}
