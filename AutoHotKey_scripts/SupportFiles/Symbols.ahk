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
    super.SetByCol( 11 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbolX( 1, "⇐", "Double Left Arrow"       )
    super.RegisterSymbolX( 1, "⟸", "Double Long Left Arrow"  )
    super.RegisterSymbolX( 1, "–", "En Dash"                 )
    super.RegisterSymbolX( 1, "•", "Bullet"                  )
    super.RegisterSymbolX( 1, "≈", "Almost Equal"            )
    super.RegisterSymbolX( 1, "Ω", "Omega"                   )
    super.NextLine()

    super.RegisterSymbolX( 1, "⇒", "Double Right Arrow"      )
    super.RegisterSymbolX( 1, "⟹", "Double Long Right Arrow" )
    super.RegisterSymbolX( 1, "—", "Em Dash"                 )
    super.RegisterSymbolX( 1, "©", "Copyright"               )
    super.RegisterSymbolX( 1, "±", "Plus-Minus"              )
    super.RegisterSymbolX( 1, "°", "Degree"                  )
    super.NextLine()

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

    super.RegisterSymbolX( 1, "ₐ", "Subscript a" )
    super.RegisterSymbolX( 1, "ₑ", "Subscript e" )
    super.RegisterSymbolX( 1, "ₒ", "Subscript o" )
    super.RegisterSymbolX( 1, "ₓ", "Subscript x" )

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
