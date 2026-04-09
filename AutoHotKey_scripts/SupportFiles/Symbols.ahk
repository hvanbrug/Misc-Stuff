; This script defines hotkeys for inserting special characters using
; combinations of Ctrl, Windows key, Shift, and other keys.

; ^ = Ctrl key
; + = Shift key
; # = Windows key
; ! = Alt key

; Ctrl + Wnd + Shift + . => Bullet character


class SymbolsTabPage extends TabPage
{
  __New()
  {
    super.__New( "Symbols" )
    super.SetRowsOf( 17 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbolX( 1, "⇐", "Double Left Arrow"       )
    super.RegisterSymbolX( 1, "⟸", "Double Long Left Arrow"  )
    super.RegisterSymbolX( 1, "←", "Left Arrow"              )
    super.RegisterSymbolX( 1, "↑", "Up Arrow"                )
    super.RegisterSymbolX( 1, "↔", "Left-Right Arrow"        )
    super.RegisterSymbolX( 1, "–", "En Dash"                 )
    super.RegisterSymbolX( 1, "≈", "Almost Equal"            )
    super.RegisterSymbolX( 1, "≡", "Identical To"            )
    super.RegisterSymbolX( 1, "≤", "Less Than or Equal To"   )
    super.RegisterSymbolX( 1, "•", "Bullet"                  )
    super.RegisterSymbolX( 1, "Ω", "Omega"                   )
    super.NextLine()

    super.RegisterSymbolX( 1, "⇒", "Double Right Arrow"      )
    super.RegisterSymbolX( 1, "⟹", "Double Long Right Arrow" )
    super.RegisterSymbolX( 1, "→", "Right Arrow"             )
    super.RegisterSymbolX( 1, "↓", "Down Arrow"              )
    super.RegisterSymbolX( 1, "↕", "Up-Down Arrow"           )
    super.RegisterSymbolX( 1, "—", "Em Dash"                 )
    super.RegisterSymbolX( 1, "±", "Plus-Minus"              )
    super.RegisterSymbolX( 1, "≠", "Not Equal"               )
    super.RegisterSymbolX( 1, "≥", "Greater Than or Equal To" )
    super.RegisterSymbolX( 1, "°", "Degree"                  )
    super.RegisterSymbolX( 1, "©", "Copyright"               )
    super.RegisterSymbolX( 1, "…", "Ellipsis"                )
    super.NextLine()
    super.ShiftLineByThird()

    super.RegisterSymbolX( 1, "─", "Box Drawings Light Horizontal" )
    super.RegisterSymbolX( 1, "│", "Box Drawings Light Vertical" )
    super.RegisterSymbolX( 1, "┌", "Box Drawings Light Down and Right" )
    super.RegisterSymbolX( 1, "┐", "Box Drawings Light Down and Left" )
    super.RegisterSymbolX( 1, "└", "Box Drawings Light Up and Right" )
    super.RegisterSymbolX( 1, "┘", "Box Drawings Light Up and Left" )
    super.RegisterSymbolX( 1, "├", "Box Drawings Light Vertical and Right" )
    super.RegisterSymbolX( 1, "┤", "Box Drawings Light Vertical and Left" )
    super.RegisterSymbolX( 1, "┬", "Box Drawings Light Down and Horizontal" )
    super.RegisterSymbolX( 1, "┴", "Box Drawings Light Up and Horizontal" )
    super.RegisterSymbolX( 1, "┼", "Box Drawings Light Vertical and Horizontal" )
    super.NextLine()

    super.RegisterSymbolX( 1, "═", "Box Drawings Double Horizontal" )
    super.RegisterSymbolX( 1, "║", "Box Drawings Double Vertical" )
    super.RegisterSymbolX( 1, "╔", "Box Drawings Double Down and Right" )
    super.RegisterSymbolX( 1, "╗", "Box Drawings Double Down and Left" )
    super.RegisterSymbolX( 1, "╚", "Box Drawings Double Up and Right" )
    super.RegisterSymbolX( 1, "╝", "Box Drawings Double Up and Left" )
    super.RegisterSymbolX( 1, "╠", "Box Drawings Double Vertical and Right" )
    super.RegisterSymbolX( 1, "╣", "Box Drawings Double Vertical and Left" )
    super.RegisterSymbolX( 1, "╦", "Box Drawings Double Down and Horizontal" )
    super.RegisterSymbolX( 1, "╩", "Box Drawings Double Up and Horizontal" )
    super.RegisterSymbolX( 1, "╬", "Box Drawings Double Vertical and Horizontal" )
    super.NextLine()

    super.RegisterSpace( 2 )
    super.RegisterSymbolX( 1, "╒", "Box Drawings Down Single and Right Double" )
    super.RegisterSymbolX( 1, "╕", "Box Drawings Down Single and Left Double" )
    super.RegisterSymbolX( 1, "╘", "Box Drawings Up Single and Right Double" )
    super.RegisterSymbolX( 1, "╛", "Box Drawings Up Single and Left Double" )
    super.RegisterSymbolX( 1, "╞", "Box Drawings Vertical Single and Right Double" )
    super.RegisterSymbolX( 1, "╡", "Box Drawings Vertical Single and Left Double" )
    super.RegisterSymbolX( 1, "╤", "Box Drawings Down Single and Horizontal Double" )
    super.RegisterSymbolX( 1, "╧", "Box Drawings Up Single and Horizontal Double" )
    super.RegisterSymbolX( 1, "╪", "Box Drawings Vertical Single and Horizontal Double" )
    super.NextLine()

    super.RegisterSpace( 2 )
    super.RegisterSymbolX( 1, "╓", "Box Drawings Down Double and Right Single" )
    super.RegisterSymbolX( 1, "╖", "Box Drawings Down Double and Left Single" )
    super.RegisterSymbolX( 1, "╙", "Box Drawings Up Double and Right Single" )
    super.RegisterSymbolX( 1, "╜", "Box Drawings Up Double and Left Single" )
    super.RegisterSymbolX( 1, "╟", "Box Drawings Vertical Double and Right Single" )
    super.RegisterSymbolX( 1, "╢", "Box Drawings Vertical Double and Left Single" )
    super.RegisterSymbolX( 1, "╥", "Box Drawings Down Double and Horizontal Single" )
    super.RegisterSymbolX( 1, "╨", "Box Drawings Up Double and Horizontal Single" )
    super.RegisterSymbolX( 1, "╫", "Box Drawings Vertical Double and Horizontal Single" )
    super.NextLine()
    super.ShiftLineByThird()

    super.RegisterSymbolX( 1, "⁰", "Superscript 0" )
    super.RegisterSymbolX( 1, "¹", "Superscript 1" )
    super.RegisterSymbolX( 1, "²", "Superscript 2" )
    super.RegisterSymbolX( 1, "³", "Superscript 3" )
    super.RegisterSymbolX( 1, "⁴", "Superscript 4" )
    super.RegisterSymbolX( 1, "⁵", "Superscript 5" )
    super.RegisterSymbolX( 1, "⁶", "Superscript 6" )
    super.RegisterSymbolX( 1, "⁷", "Superscript 7" )
    super.RegisterSymbolX( 1, "⁸", "Superscript 8" )
    super.RegisterSymbolX( 1, "⁹", "Superscript 9" )
    super.RegisterSymbolX( 1, "ⁿ", "Superscript n" )
    super.NextLine()

    super.RegisterSymbolX( 1, "ₐ", "Subscript a" )
    super.RegisterSymbolX( 1, "ₑ", "Subscript e" )
    super.RegisterSymbolX( 1, "ₒ", "Subscript o" )
    super.RegisterSymbolX( 1, "ₓ", "Subscript x" )
    super.NextLine()

    ; "àáâãäå"
    ; "èéêë"
    ; "ìíîï"
    ; "òóôõö"
    ; "ùúûü"
    ; "ÿ"
    ; "ĀāĂăĄą"
    ; "ĆćĈĉĊċČč"
    ; "ĎďĐđ"
    ; "ĒēĔĕĖėĘęĚě"
    ; "ĜĝĞğĠġĢģ"
    ; "ĤĥĦħ"
    ; "ĨĩĪīĬĭĮįİı"
    ; "ŃńŅņŇň"
  }
}
