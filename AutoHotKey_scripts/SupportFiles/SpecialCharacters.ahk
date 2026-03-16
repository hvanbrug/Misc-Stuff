; This script defines hotkeys for inserting special characters using
; combinations of Ctrl, Windows key, Shift, and other keys.

; ^ = Ctrl key
; + = Shift key
; # = Windows key
; ! = Alt key

; Ctrl + Wnd + Shift + . => Bullet character
SPECIAL_TAB := 1
SetTabName( SPECIAL_TAB, "Special" )


RegisterSymbol( 1, 4, SPECIAL_TAB, "•", "Bullet",                  "" )
RegisterSymbol( 1, 5, SPECIAL_TAB, "≈", "Almost Equal",            "" )
RegisterSymbol( 1, 1, SPECIAL_TAB, "⇐", "Double Left Arrow",       "" )
RegisterSymbol( 2, 1, SPECIAL_TAB, "⇒", "Double Right Arrow",      "" )
RegisterSymbol( 1, 2, SPECIAL_TAB, "⟸", "Double Long Left Arrow",  "" )
RegisterSymbol( 2, 2, SPECIAL_TAB, "⟹", "Double Long Right Arrow", "" )
RegisterSymbol( 1, 3, SPECIAL_TAB, "–", "En Dash",                 "" )
RegisterSymbol( 2, 3, SPECIAL_TAB, "—", "Em Dash",                 "" )

RegisterSymbol( 1, 6, SPECIAL_TAB, "Ω", "Omega",      "" )
RegisterSymbol( 2, 4, SPECIAL_TAB, "©", "Copyright",  "" )
RegisterSymbol( 2, 5, SPECIAL_TAB, "±", "Plus-Minus", "" )
RegisterSymbol( 2, 6, SPECIAL_TAB, "°", "Degree",     "" )

; Row 3: Superscripts
RegisterSymbol( 3, 1,  SPECIAL_TAB, "⁰", "Superscript 0", "" )
RegisterSymbol( 3, 2,  SPECIAL_TAB, "¹", "Superscript 1", "" )
RegisterSymbol( 3, 3,  SPECIAL_TAB, "²", "Superscript 2", "" )
RegisterSymbol( 3, 4,  SPECIAL_TAB, "³", "Superscript 3", "" )
RegisterSymbol( 3, 5,  SPECIAL_TAB, "⁴", "Superscript 4", "" )
RegisterSymbol( 3, 6,  SPECIAL_TAB, "⁵", "Superscript 5", "" )
RegisterSymbol( 3, 7,  SPECIAL_TAB, "⁶", "Superscript 6", "" )
RegisterSymbol( 3, 8,  SPECIAL_TAB, "⁷", "Superscript 7", "" )
RegisterSymbol( 3, 9,  SPECIAL_TAB, "⁸", "Superscript 8", "" )
RegisterSymbol( 3, 10, SPECIAL_TAB, "⁹", "Superscript 9", "" )
RegisterSymbol( 3, 11, SPECIAL_TAB, "ⁿ", "Superscript n", "" )

; Row 4: Subscripts
RegisterSymbol( 4, 1, SPECIAL_TAB, "ₐ", "Subscript a", "" )
RegisterSymbol( 4, 2, SPECIAL_TAB, "ₑ", "Subscript e", "" )
RegisterSymbol( 4, 3, SPECIAL_TAB, "ₒ", "Subscript o", "" )
RegisterSymbol( 4, 4, SPECIAL_TAB, "ₓ", "Subscript x", "" )

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