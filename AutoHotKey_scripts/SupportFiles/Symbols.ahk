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

    this.RegisterButtons()
    this.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbol( 1, 4, 1, "•", "Bullet"                  )
    super.RegisterSymbol( 1, 5, 1, "≈", "Almost Equal"            )
    super.RegisterSymbol( 1, 1, 1, "⇐", "Double Left Arrow"       )
    super.RegisterSymbol( 2, 1, 1, "⇒", "Double Right Arrow"      )
    super.RegisterSymbol( 1, 2, 1, "⟸", "Double Long Left Arrow"  )
    super.RegisterSymbol( 2, 2, 1, "⟹", "Double Long Right Arrow" )
    super.RegisterSymbol( 1, 3, 1, "–", "En Dash"                 )
    super.RegisterSymbol( 2, 3, 1, "—", "Em Dash"                 )

    super.RegisterSymbol( 1, 6, 1, "Ω", "Omega"      )
    super.RegisterSymbol( 2, 4, 1, "©", "Copyright"  )
    super.RegisterSymbol( 2, 5, 1, "±", "Plus-Minus" )
    super.RegisterSymbol( 2, 6, 1, "°", "Degree"     )

    super.RegisterSymbol( 3,  1, 1, "⁰", "Superscript 0" )
    super.RegisterSymbol( 3,  2, 1, "¹", "Superscript 1" )
    super.RegisterSymbol( 3,  3, 1, "²", "Superscript 2" )
    super.RegisterSymbol( 3,  4, 1, "³", "Superscript 3" )
    super.RegisterSymbol( 3,  5, 1, "⁴", "Superscript 4" )
    super.RegisterSymbol( 3,  6, 1, "⁵", "Superscript 5" )
    super.RegisterSymbol( 3,  7, 1, "⁶", "Superscript 6" )
    super.RegisterSymbol( 3,  8, 1, "⁷", "Superscript 7" )
    super.RegisterSymbol( 3,  9, 1, "⁸", "Superscript 8" )
    super.RegisterSymbol( 3, 10, 1, "⁹", "Superscript 9" )
    super.RegisterSymbol( 3, 11, 1, "ⁿ", "Superscript n" )

    super.RegisterSymbol( 4, 1, 1, "ₐ", "Subscript a" )
    super.RegisterSymbol( 4, 2, 1, "ₑ", "Subscript e" )
    super.RegisterSymbol( 4, 3, 1, "ₒ", "Subscript o" )
    super.RegisterSymbol( 4, 4, 1, "ₓ", "Subscript x" )

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
