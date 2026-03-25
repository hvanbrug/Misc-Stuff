; Russian Alphabet
; Each letter is registered as a symbol button on the "Russian" tab.
; Layout: 4 rows, going down then across.


class RussianTabPage extends TabPage
{
  __New()
  {
    super.__New( "Russian" )

    this.RegisterButtons()
    this.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbol( 1, 1, 1, "А", "A",   "А" )
    super.RegisterSymbol( 2, 1, 1, "Б", "Be",  "Б" )
    super.RegisterSymbol( 3, 1, 1, "В", "Ve",  "В" )
    super.RegisterSymbol( 4, 1, 1, "Г", "Ge",  "Г" )

    super.RegisterSymbol( 1, 2, 1, "Д", "De",  "Д" )
    super.RegisterSymbol( 2, 2, 1, "Е", "Ye",  "Е" )
    super.RegisterSymbol( 3, 2, 1, "Ё", "Yo",  "Ё" )
    super.RegisterSymbol( 4, 2, 1, "Ж", "Zhe", "Ж" )

    super.RegisterSymbol( 1, 3, 1, "З", "Ze",      "З" )
    super.RegisterSymbol( 2, 3, 1, "И", "I",       "И" )
    super.RegisterSymbol( 3, 3, 1, "Й", "Short I", "Й" )
    super.RegisterSymbol( 4, 3, 1, "К", "Ka",      "К" )

    super.RegisterSymbol( 1, 4, 1, "Л", "El",  "Л" )
    super.RegisterSymbol( 2, 4, 1, "М", "Em",  "М" )
    super.RegisterSymbol( 3, 4, 1, "Н", "En",  "Н" )
    super.RegisterSymbol( 4, 4, 1, "О", "O",   "О" )

    super.RegisterSymbol( 1, 5, 1, "П", "Pe",  "П" )
    super.RegisterSymbol( 2, 5, 1, "Р", "Er",  "Р" )
    super.RegisterSymbol( 3, 5, 1, "С", "Es",  "С" )
    super.RegisterSymbol( 4, 5, 1, "Т", "Te",  "Т" )

    super.RegisterSymbol( 1, 6, 1, "У", "U",   "У" )
    super.RegisterSymbol( 2, 6, 1, "Ф", "Ef",  "Ф" )
    super.RegisterSymbol( 3, 6, 1, "Х", "Kha", "Х" )
    super.RegisterSymbol( 4, 6, 1, "Ц", "Tse", "Ц" )

    super.RegisterSymbol( 1, 7, 1, "Ч", "Che",       "Ч" )
    super.RegisterSymbol( 2, 7, 1, "Ш", "Sha",       "Ш" )
    super.RegisterSymbol( 3, 7, 1, "Щ", "Shcha",     "Щ" )
    super.RegisterSymbol( 4, 7, 1, "Ъ", "Hard Sign", "Ъ" )

    super.RegisterSymbol( 1, 8, 1, "Ы", "Yeru",      "Ы" )
    super.RegisterSymbol( 2, 8, 1, "Ь", "Soft Sign", "Ь" )
    super.RegisterSymbol( 3, 8, 1, "Э", "E",         "Э" )
    super.RegisterSymbol( 4, 8, 1, "Ю", "Yu",        "Ю" )

    super.RegisterSymbol( 1, 9, 1, "Я", "Ya", "Я" )
  }
}

