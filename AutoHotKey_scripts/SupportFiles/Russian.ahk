; Russian Alphabet
; Each letter is registered as a symbol button on the "Russian" tab.
; Layout: 4 rows, going down then across.


class RussianTabPage extends TabPage
{
  __New()
  {
    super.__New( "Russian" )
    super.SetByCol( 17 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.NextLine()

    super.RegisterSymbolX( 1, "А", "Uppercase A"       )
    super.RegisterSymbolX( 1, "Б", "Uppercase Be"      )
    super.RegisterSymbolX( 1, "В", "Uppercase Ve"      )
    super.RegisterSymbolX( 1, "Г", "Uppercase Ge"      )
    super.RegisterSymbolX( 1, "Д", "Uppercase De"      )
    super.RegisterSymbolX( 1, "Е", "Uppercase Ye"      )
    super.RegisterSymbolX( 1, "Ё", "Uppercase Yo"      )
    super.RegisterSymbolX( 1, "Ж", "Uppercase Zhe"     )
    super.RegisterSymbolX( 1, "З", "Uppercase Ze"      )
    super.RegisterSymbolX( 1, "И", "Uppercase I"       )
    super.RegisterSymbolX( 1, "Й", "Uppercase Short I" )
    super.RegisterSymbolX( 1, "К", "Uppercase Ka"      )
    super.RegisterSymbolX( 1, "Л", "Uppercase El"      )
    super.RegisterSymbolX( 1, "М", "Uppercase Em"      )
    super.RegisterSymbolX( 1, "Н", "Uppercase En"      )
    super.RegisterSymbolX( 1, "О", "Uppercase O"       )
    super.RegisterSymbolX( 1, "П", "Uppercase Pe"      )

    super.RegisterSymbolX( 1, "Р", "Uppercase Er"        )
    super.RegisterSymbolX( 1, "С", "Uppercase Es"        )
    super.RegisterSymbolX( 1, "Т", "Uppercase Te"        )
    super.RegisterSymbolX( 1, "У", "Uppercase U"         )
    super.RegisterSymbolX( 1, "Ф", "Uppercase Ef"        )
    super.RegisterSymbolX( 1, "Х", "Uppercase Kha"       )
    super.RegisterSymbolX( 1, "Ц", "Uppercase Tse"       )
    super.RegisterSymbolX( 1, "Ч", "Uppercase Che"       )
    super.RegisterSymbolX( 1, "Ш", "Uppercase Sha"       )
    super.RegisterSymbolX( 1, "Щ", "Uppercase Shcha"     )
    super.RegisterSymbolX( 1, "Ъ", "Uppercase Hard Sign" )
    super.RegisterSymbolX( 1, "Ы", "Uppercase Yeru"      )
    super.RegisterSymbolX( 1, "Ь", "Uppercase Soft Sign" )
    super.RegisterSymbolX( 1, "Э", "Uppercase E"         )
    super.RegisterSymbolX( 1, "Ю", "Uppercase Yu"        )
    super.RegisterSymbolX( 1, "Я", "Uppercase Ya"        )
    super.RegisterSpace()

    super.NextLine()

    super.RegisterSymbolX( 1, "а", "Lowercase A"       )
    super.RegisterSymbolX( 1, "б", "Lowercase Be"      )
    super.RegisterSymbolX( 1, "в", "Lowercase Ve"      )
    super.RegisterSymbolX( 1, "г", "Lowercase Ge"      )
    super.RegisterSymbolX( 1, "д", "Lowercase De"      )
    super.RegisterSymbolX( 1, "е", "Lowercase Ye"      )
    super.RegisterSymbolX( 1, "ё", "Lowercase Yo"      )
    super.RegisterSymbolX( 1, "ж", "Lowercase Zhe"     )
    super.RegisterSymbolX( 1, "з", "Lowercase Ze"      )
    super.RegisterSymbolX( 1, "и", "Lowercase I"       )
    super.RegisterSymbolX( 1, "й", "Lowercase Short I" )
    super.RegisterSymbolX( 1, "к", "Lowercase Ka"      )
    super.RegisterSymbolX( 1, "л", "Lowercase El"      )
    super.RegisterSymbolX( 1, "м", "Lowercase Em"      )
    super.RegisterSymbolX( 1, "н", "Lowercase En"      )
    super.RegisterSymbolX( 1, "о", "Lowercase O"       )
    super.RegisterSymbolX( 1, "п", "Lowercase Pe"      )

    super.RegisterSymbolX( 1, "р", "Lowercase Er"        )
    super.RegisterSymbolX( 1, "с", "Lowercase Es"        )
    super.RegisterSymbolX( 1, "т", "Lowercase Te"        )
    super.RegisterSymbolX( 1, "у", "Lowercase U"         )
    super.RegisterSymbolX( 1, "ф", "Lowercase Ef"        )
    super.RegisterSymbolX( 1, "х", "Lowercase Kha"       )
    super.RegisterSymbolX( 1, "ц", "Lowercase Tse"       )
    super.RegisterSymbolX( 1, "ч", "Lowercase Che"       )
    super.RegisterSymbolX( 1, "ш", "Lowercase Sha"       )
    super.RegisterSymbolX( 1, "щ", "Lowercase Shcha"     )
    super.RegisterSymbolX( 1, "ъ", "Lowercase Hard Sign" )
    super.RegisterSymbolX( 1, "ы", "Lowercase Yeru"      )
    super.RegisterSymbolX( 1, "ь", "Lowercase Soft Sign" )
    super.RegisterSymbolX( 1, "э", "Lowercase E"         )
    super.RegisterSymbolX( 1, "ю", "Lowercase Yu"        )
    super.RegisterSymbolX( 1, "я", "Lowercase Ya"        )
  }
}

