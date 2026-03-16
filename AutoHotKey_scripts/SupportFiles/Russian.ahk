; Russian Alphabet
; Each letter is registered as a symbol button on the "Russian" tab.
; Layout: 4 rows, going down then across.

RUSSIAN_TAB := 3
SetTabName( RUSSIAN_TAB, "Russian" )


; Column 1: А Б В Г
RegisterSymbol( 1, 1, RUSSIAN_TAB, "А", "A",   "А" )
RegisterSymbol( 2, 1, RUSSIAN_TAB, "Б", "Be",  "Б" )
RegisterSymbol( 3, 1, RUSSIAN_TAB, "В", "Ve",  "В" )
RegisterSymbol( 4, 1, RUSSIAN_TAB, "Г", "Ge",  "Г" )

; Column 2: Д Е Ё Ж
RegisterSymbol( 1, 2, RUSSIAN_TAB, "Д", "De",  "Д" )
RegisterSymbol( 2, 2, RUSSIAN_TAB, "Е", "Ye",  "Е" )
RegisterSymbol( 3, 2, RUSSIAN_TAB, "Ё", "Yo",  "Ё" )
RegisterSymbol( 4, 2, RUSSIAN_TAB, "Ж", "Zhe", "Ж" )

; Column 3: З И Й К
RegisterSymbol( 1, 3, RUSSIAN_TAB, "З", "Ze",      "З" )
RegisterSymbol( 2, 3, RUSSIAN_TAB, "И", "I",       "И" )
RegisterSymbol( 3, 3, RUSSIAN_TAB, "Й", "Short I", "Й" )
RegisterSymbol( 4, 3, RUSSIAN_TAB, "К", "Ka",      "К" )

; Column 4: Л М Н О
RegisterSymbol( 1, 4, RUSSIAN_TAB, "Л", "El",  "Л" )
RegisterSymbol( 2, 4, RUSSIAN_TAB, "М", "Em",  "М" )
RegisterSymbol( 3, 4, RUSSIAN_TAB, "Н", "En",  "Н" )
RegisterSymbol( 4, 4, RUSSIAN_TAB, "О", "O",   "О" )

; Column 5: П Р С Т
RegisterSymbol( 1, 5, RUSSIAN_TAB, "П", "Pe",  "П" )
RegisterSymbol( 2, 5, RUSSIAN_TAB, "Р", "Er",  "Р" )
RegisterSymbol( 3, 5, RUSSIAN_TAB, "С", "Es",  "С" )
RegisterSymbol( 4, 5, RUSSIAN_TAB, "Т", "Te",  "Т" )

; Column 6: У Ф Х Ц
RegisterSymbol( 1, 6, RUSSIAN_TAB, "У", "U",   "У" )
RegisterSymbol( 2, 6, RUSSIAN_TAB, "Ф", "Ef",  "Ф" )
RegisterSymbol( 3, 6, RUSSIAN_TAB, "Х", "Kha", "Х" )
RegisterSymbol( 4, 6, RUSSIAN_TAB, "Ц", "Tse", "Ц" )

; Column 7: Ч Ш Щ Ъ
RegisterSymbol( 1, 7, RUSSIAN_TAB, "Ч", "Che",       "Ч" )
RegisterSymbol( 2, 7, RUSSIAN_TAB, "Ш", "Sha",       "Ш" )
RegisterSymbol( 3, 7, RUSSIAN_TAB, "Щ", "Shcha",     "Щ" )
RegisterSymbol( 4, 7, RUSSIAN_TAB, "Ъ", "Hard Sign", "Ъ" )

; Column 8: Ы Ь Э Ю
RegisterSymbol( 1, 8, RUSSIAN_TAB, "Ы", "Yeru",      "Ы" )
RegisterSymbol( 2, 8, RUSSIAN_TAB, "Ь", "Soft Sign", "Ь" )
RegisterSymbol( 3, 8, RUSSIAN_TAB, "Э", "E",         "Э" )
RegisterSymbol( 4, 8, RUSSIAN_TAB, "Ю", "Yu",        "Ю" )

; Column 9: Я
RegisterSymbol( 1, 9, RUSSIAN_TAB, "Я", "Ya", "Я" )
