using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

/// <summary>Symbols tab — special characters, box drawing, super/subscripts (Symbols.ahk).</summary>
internal sealed class SymbolsTab : TabModel
{
  public SymbolsTab() : base( "Symbols" )
  {
    SetRowsOf( 17 );
    RegisterButtons();
    RecalcSizes();
  }

  private void RegisterButtons()
  {
    RegisterSymbolX( 1, "⇐", "Double Left Arrow" );
    RegisterSymbolX( 1, "⟸", "Double Long Left Arrow" );
    RegisterSymbolX( 1, "←", "Left Arrow" );
    RegisterSymbolX( 1, "↑", "Up Arrow" );
    RegisterSymbolX( 1, "↔", "Left-Right Arrow" );
    RegisterSymbolX( 1, "–", "En Dash" );
    RegisterSymbolX( 1, "≈", "Almost Equal" );
    RegisterSymbolX( 1, "≡", "Identical To" );
    RegisterSymbolX( 1, "≤", "Less Than or Equal To" );
    RegisterSymbolX( 1, "•", "Bullet" );
    RegisterSymbolX( 1, "Ω", "Omega" );
    NextLine();

    RegisterSymbolX( 1, "⇒", "Double Right Arrow" );
    RegisterSymbolX( 1, "⟹", "Double Long Right Arrow" );
    RegisterSymbolX( 1, "→", "Right Arrow" );
    RegisterSymbolX( 1, "↓", "Down Arrow" );
    RegisterSymbolX( 1, "↕", "Up-Down Arrow" );
    RegisterSymbolX( 1, "—", "Em Dash" );
    RegisterSymbolX( 1, "±", "Plus-Minus" );
    RegisterSymbolX( 1, "≠", "Not Equal" );
    RegisterSymbolX( 1, "≥", "Greater Than or Equal To" );
    RegisterSymbolX( 1, "°", "Degree" );
    RegisterSymbolX( 1, "©", "Copyright" );
    RegisterSymbolX( 1, "…", "Ellipsis" );
    NextLine();
    ShiftLineByThird();

    RegisterSymbolX( 1, "─", "Box Drawings Light Horizontal" );
    RegisterSymbolX( 1, "│", "Box Drawings Light Vertical" );
    RegisterSymbolX( 1, "┌", "Box Drawings Light Down and Right" );
    RegisterSymbolX( 1, "┐", "Box Drawings Light Down and Left" );
    RegisterSymbolX( 1, "└", "Box Drawings Light Up and Right" );
    RegisterSymbolX( 1, "┘", "Box Drawings Light Up and Left" );
    RegisterSymbolX( 1, "├", "Box Drawings Light Vertical and Right" );
    RegisterSymbolX( 1, "┤", "Box Drawings Light Vertical and Left" );
    RegisterSymbolX( 1, "┬", "Box Drawings Light Down and Horizontal" );
    RegisterSymbolX( 1, "┴", "Box Drawings Light Up and Horizontal" );
    RegisterSymbolX( 1, "┼", "Box Drawings Light Vertical and Horizontal" );
    NextLine();

    RegisterSymbolX( 1, "═", "Box Drawings Double Horizontal" );
    RegisterSymbolX( 1, "║", "Box Drawings Double Vertical" );
    RegisterSymbolX( 1, "╔", "Box Drawings Double Down and Right" );
    RegisterSymbolX( 1, "╗", "Box Drawings Double Down and Left" );
    RegisterSymbolX( 1, "╚", "Box Drawings Double Up and Right" );
    RegisterSymbolX( 1, "╝", "Box Drawings Double Up and Left" );
    RegisterSymbolX( 1, "╠", "Box Drawings Double Vertical and Right" );
    RegisterSymbolX( 1, "╣", "Box Drawings Double Vertical and Left" );
    RegisterSymbolX( 1, "╦", "Box Drawings Double Down and Horizontal" );
    RegisterSymbolX( 1, "╩", "Box Drawings Double Up and Horizontal" );
    RegisterSymbolX( 1, "╬", "Box Drawings Double Vertical and Horizontal" );
    NextLine();

    RegisterSpace( 2 );
    RegisterSymbolX( 1, "╒", "Box Drawings Down Single and Right Double" );
    RegisterSymbolX( 1, "╕", "Box Drawings Down Single and Left Double" );
    RegisterSymbolX( 1, "╘", "Box Drawings Up Single and Right Double" );
    RegisterSymbolX( 1, "╛", "Box Drawings Up Single and Left Double" );
    RegisterSymbolX( 1, "╞", "Box Drawings Vertical Single and Right Double" );
    RegisterSymbolX( 1, "╡", "Box Drawings Vertical Single and Left Double" );
    RegisterSymbolX( 1, "╤", "Box Drawings Down Single and Horizontal Double" );
    RegisterSymbolX( 1, "╧", "Box Drawings Up Single and Horizontal Double" );
    RegisterSymbolX( 1, "╪", "Box Drawings Vertical Single and Horizontal Double" );
    NextLine();

    RegisterSpace( 2 );
    RegisterSymbolX( 1, "╓", "Box Drawings Down Double and Right Single" );
    RegisterSymbolX( 1, "╖", "Box Drawings Down Double and Left Single" );
    RegisterSymbolX( 1, "╙", "Box Drawings Up Double and Right Single" );
    RegisterSymbolX( 1, "╜", "Box Drawings Up Double and Left Single" );
    RegisterSymbolX( 1, "╟", "Box Drawings Vertical Double and Right Single" );
    RegisterSymbolX( 1, "╢", "Box Drawings Vertical Double and Left Single" );
    RegisterSymbolX( 1, "╥", "Box Drawings Down Double and Horizontal Single" );
    RegisterSymbolX( 1, "╨", "Box Drawings Up Double and Horizontal Single" );
    RegisterSymbolX( 1, "╫", "Box Drawings Vertical Double and Horizontal Single" );
    NextLine();
    ShiftLineByThird();

    RegisterSymbolX( 1, "⁰", "Superscript 0" );
    RegisterSymbolX( 1, "¹", "Superscript 1" );
    RegisterSymbolX( 1, "²", "Superscript 2" );
    RegisterSymbolX( 1, "³", "Superscript 3" );
    RegisterSymbolX( 1, "⁴", "Superscript 4" );
    RegisterSymbolX( 1, "⁵", "Superscript 5" );
    RegisterSymbolX( 1, "⁶", "Superscript 6" );
    RegisterSymbolX( 1, "⁷", "Superscript 7" );
    RegisterSymbolX( 1, "⁸", "Superscript 8" );
    RegisterSymbolX( 1, "⁹", "Superscript 9" );
    RegisterSymbolX( 1, "ⁿ", "Superscript n" );
    NextLine();

    RegisterSymbolX( 1, "ₐ", "Subscript a" );
    RegisterSymbolX( 1, "ₑ", "Subscript e" );
    RegisterSymbolX( 1, "ₒ", "Subscript o" );
    RegisterSymbolX( 1, "ₓ", "Subscript x" );
    NextLine();
  }
}
