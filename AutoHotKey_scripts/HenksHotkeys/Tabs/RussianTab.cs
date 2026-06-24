using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

/// <summary>Russian (Cyrillic) alphabet tab (Russian.ahk).</summary>
internal sealed class RussianTab : TabModel
{
  public RussianTab() : base( "Russian" )
  {
    SetRowsOf( 17 );
    RegisterButtons();
    RecalcSizes();
  }

  private void RegisterButtons()
  {
    NextLine();

    RegisterSymbolX( 1, "А", "Uppercase A" );
    RegisterSymbolX( 1, "Б", "Uppercase Be" );
    RegisterSymbolX( 1, "В", "Uppercase Ve" );
    RegisterSymbolX( 1, "Г", "Uppercase Ge" );
    RegisterSymbolX( 1, "Д", "Uppercase De" );
    RegisterSymbolX( 1, "Е", "Uppercase Ye" );
    RegisterSymbolX( 1, "Ё", "Uppercase Yo" );
    RegisterSymbolX( 1, "Ж", "Uppercase Zhe" );
    RegisterSymbolX( 1, "З", "Uppercase Ze" );
    RegisterSymbolX( 1, "И", "Uppercase I" );
    RegisterSymbolX( 1, "Й", "Uppercase Short I" );
    RegisterSymbolX( 1, "К", "Uppercase Ka" );
    RegisterSymbolX( 1, "Л", "Uppercase El" );
    RegisterSymbolX( 1, "М", "Uppercase Em" );
    RegisterSymbolX( 1, "Н", "Uppercase En" );
    RegisterSymbolX( 1, "О", "Uppercase O" );
    RegisterSymbolX( 1, "П", "Uppercase Pe" );

    RegisterSymbolX( 1, "Р", "Uppercase Er" );
    RegisterSymbolX( 1, "С", "Uppercase Es" );
    RegisterSymbolX( 1, "Т", "Uppercase Te" );
    RegisterSymbolX( 1, "У", "Uppercase U" );
    RegisterSymbolX( 1, "Ф", "Uppercase Ef" );
    RegisterSymbolX( 1, "Х", "Uppercase Kha" );
    RegisterSymbolX( 1, "Ц", "Uppercase Tse" );
    RegisterSymbolX( 1, "Ч", "Uppercase Che" );
    RegisterSymbolX( 1, "Ш", "Uppercase Sha" );
    RegisterSymbolX( 1, "Щ", "Uppercase Shcha" );
    RegisterSymbolX( 1, "Ъ", "Uppercase Hard Sign" );
    RegisterSymbolX( 1, "Ы", "Uppercase Yeru" );
    RegisterSymbolX( 1, "Ь", "Uppercase Soft Sign" );
    RegisterSymbolX( 1, "Э", "Uppercase E" );
    RegisterSymbolX( 1, "Ю", "Uppercase Yu" );
    RegisterSymbolX( 1, "Я", "Uppercase Ya" );
    RegisterSpace();

    ShiftLineByThird();

    RegisterSymbolX( 1, "а", "Lowercase A" );
    RegisterSymbolX( 1, "б", "Lowercase Be" );
    RegisterSymbolX( 1, "в", "Lowercase Ve" );
    RegisterSymbolX( 1, "г", "Lowercase Ge" );
    RegisterSymbolX( 1, "д", "Lowercase De" );
    RegisterSymbolX( 1, "е", "Lowercase Ye" );
    RegisterSymbolX( 1, "ё", "Lowercase Yo" );
    RegisterSymbolX( 1, "ж", "Lowercase Zhe" );
    RegisterSymbolX( 1, "з", "Lowercase Ze" );
    RegisterSymbolX( 1, "и", "Lowercase I" );
    RegisterSymbolX( 1, "й", "Lowercase Short I" );
    RegisterSymbolX( 1, "к", "Lowercase Ka" );
    RegisterSymbolX( 1, "л", "Lowercase El" );
    RegisterSymbolX( 1, "м", "Lowercase Em" );
    RegisterSymbolX( 1, "н", "Lowercase En" );
    RegisterSymbolX( 1, "о", "Lowercase O" );
    RegisterSymbolX( 1, "п", "Lowercase Pe" );

    RegisterSymbolX( 1, "р", "Lowercase Er" );
    RegisterSymbolX( 1, "с", "Lowercase Es" );
    RegisterSymbolX( 1, "т", "Lowercase Te" );
    RegisterSymbolX( 1, "у", "Lowercase U" );
    RegisterSymbolX( 1, "ф", "Lowercase Ef" );
    RegisterSymbolX( 1, "х", "Lowercase Kha" );
    RegisterSymbolX( 1, "ц", "Lowercase Tse" );
    RegisterSymbolX( 1, "ч", "Lowercase Che" );
    RegisterSymbolX( 1, "ш", "Lowercase Sha" );
    RegisterSymbolX( 1, "щ", "Lowercase Shcha" );
    RegisterSymbolX( 1, "ъ", "Lowercase Hard Sign" );
    RegisterSymbolX( 1, "ы", "Lowercase Yeru" );
    RegisterSymbolX( 1, "ь", "Lowercase Soft Sign" );
    RegisterSymbolX( 1, "э", "Lowercase E" );
    RegisterSymbolX( 1, "ю", "Lowercase Yu" );
    RegisterSymbolX( 1, "я", "Lowercase Ya" );
  }
}
