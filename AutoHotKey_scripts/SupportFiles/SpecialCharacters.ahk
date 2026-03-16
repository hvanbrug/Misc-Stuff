; This script defines hotkeys for inserting special characters using
; combinations of Ctrl, Windows key, Shift, and other keys.

; ^ = Ctrl key
; + = Shift key
; # = Windows key
; ! = Alt key

; Ctrl + Wnd + Shift + . => Bullet character
RegisterSymbol( "•", "Bullet", "Ctrl+Win+Shift+Period", BulletCharacter, 1, 4 )
^#+.::BulletCharacter()
BulletCharacter()
{
  DoSendText( '•' )
}

; Ctrl + Wnd + ` => Almost Equal character
RegisterSymbol( "≈", "Almost Equal", "Ctrl+Win+Backtick", AlmostEqualCharacter, 1, 5 )
^#`::AlmostEqualCharacter()
AlmostEqualCharacter()
{
  DoSendText( '≈' )
}

; Ctrl + Wnd + , => Double Left Arrow character
RegisterSymbol( "⇐", "Double Left Arrow", "Ctrl+Win+Comma", DoubleArrowLeftCharacter, 1, 1 )
^#,::DoubleArrowLeftCharacter()
DoubleArrowLeftCharacter()
{
  DoSendText( '⇐' )
}

; Ctrl + Wnd + . => Double Right Arrow character
RegisterSymbol( "⇒", "Double Right Arrow", "Ctrl+Win+Period", DoubleArrowRightCharacter, 2, 1 )
^#.::DoubleArrowRightCharacter()
DoubleArrowRightCharacter()
{
  DoSendText( '⇒' )
}

; Ctrl + Shift + , => Double Long Left Arrow character
RegisterSymbol( "⟸", "Double Long Left Arrow", "Ctrl+Shift+Comma", DoubleLongArrowLeftCharacter, 1, 2 )
^+,::DoubleLongArrowLeftCharacter()
DoubleLongArrowLeftCharacter()
{
  DoSendText( '⟸' )
}

; Ctrl + Shift + . => Double Long Right Arrow character
RegisterSymbol( "⟹", "Double Long Right Arrow", "Ctrl+Shift+Period", DoubleLongArrowRightCharacter, 2, 2 )
^+.::DoubleLongArrowRightCharacter()
DoubleLongArrowRightCharacter()
{
  DoSendText( '⟹' )
}

; En Dash character
RegisterSymbol( "–", "En Dash", "TBD", EnDashCharacter, 1, 3 )
;^#TBD::EnDashCharacter()
EnDashCharacter()
{
  DoSendText( '–' )
}

; Em Dash character
RegisterSymbol( "—", "Em Dash", "TBD", EmDashCharacter, 2, 3 )
;^#TBD::EmDashCharacter()
EmDashCharacter()
{
  DoSendText( '—' )
}

; Omega character (uppercase)
RegisterSymbol( "Ω", "Omega", "TBD", OmegaCharacter, 1, 6 )
;^#TBD::OmegaCharacter()
OmegaCharacter()
{
  DoSendText( 'Ω' )
}

; 2, 4, "©"
RegisterSymbol( "©", "Copyright", "TBD", CopyrightCharacter, 2, 4 )
;^#TBD::CopyrightCharacter()
CopyrightCharacter()
{
  DoSendText( '©' )
}

; 2, 5, "±"
RegisterSymbol( "±", "Plus-Minus", "TBD", PlusMinusCharacter, 2, 5 )
;^#TBD::PlusMinusCharacter()
PlusMinusCharacter()
{
  DoSendText( '±' )
}

; 2, 6, "°"
RegisterSymbol( "°", "Degree", "TBD", DegreeCharacter, 2, 6 )
;^#TBD::DegreeCharacter()
DegreeCharacter()
{
  DoSendText( '°' )
}

; Row 3: Superscripts
RegisterSymbol( "⁰", "Superscript 0", "TBD", Superscript0, 3, 1 )
Superscript0()
{
  DoSendText( '⁰' )
}

RegisterSymbol( "²", "Superscript 2", "TBD", Superscript2, 3, 2 )
Superscript2()
{
  DoSendText( '²' )
}

RegisterSymbol( "³", "Superscript 3", "TBD", Superscript3, 3, 3 )
Superscript3()
{
  DoSendText( '³' )
}

RegisterSymbol( "⁴", "Superscript 4", "TBD", Superscript4, 3, 4 )
Superscript4()
{
  DoSendText( '⁴' )
}

RegisterSymbol( "⁵", "Superscript 5", "TBD", Superscript5, 3, 5 )
Superscript5()
{
  DoSendText( '⁵' )
}

RegisterSymbol( "⁶", "Superscript 6", "TBD", Superscript6, 3, 6 )
Superscript6()
{
  DoSendText( '⁶' )
}

RegisterSymbol( "⁷", "Superscript 7", "TBD", Superscript7, 3, 7 )
Superscript7()
{
  DoSendText( '⁷' )
}

RegisterSymbol( "⁸", "Superscript 8", "TBD", Superscript8, 3, 8 )
Superscript8()
{
  DoSendText( '⁸' )
}

RegisterSymbol( "⁹", "Superscript 9", "TBD", Superscript9, 3, 9 )
Superscript9()
{
  DoSendText( '⁹' )
}

RegisterSymbol( "ⁿ", "Superscript n", "TBD", SuperscriptN, 3, 10 )
SuperscriptN()
{
  DoSendText( 'ⁿ' )
}

; Row 4: Subscripts
RegisterSymbol( "ₐ", "Subscript a", "TBD", SubscriptA, 4, 1 )
SubscriptA()
{
  DoSendText( 'ₐ' )
}

RegisterSymbol( "ₑ", "Subscript e", "TBD", SubscriptE, 4, 2 )
SubscriptE()
{
  DoSendText( 'ₑ' )
}

RegisterSymbol( "ₒ", "Subscript o", "TBD", SubscriptO, 4, 3 )
SubscriptO()
{
  DoSendText( 'ₒ' )
}

RegisterSymbol( "ₓ", "Subscript x", "TBD", SubscriptX, 4, 4 )
SubscriptX()
{
  DoSendText( 'ₓ' )
}

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