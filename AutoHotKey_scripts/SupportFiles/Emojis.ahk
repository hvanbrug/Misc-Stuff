; Face Emojis
; Each emoji is registered as a symbol button on the "Emojis" tab.
; Layout: 4 rows, going down then across.


class EmojisTabPage extends TabPage
{
  __New()
  {
    super.__New( "Emojis" )

    this.m_fontSize    := "s28"
    this.m_fontName    := "Segoe UI Emoji"
    this.m_symBtnSizeX := 50
    this.m_symBtnSizeY := 50

    this.RegisterButtons()
    this.RecalcSizes()
  }

  RegisterButtons()
  {
    ;RegisterEmoji(  1,  1, 1, EMOJIS_TAB, "😀", "GrinningX Face",                               ":grin:",    "1f600.png" )
    ;RegisterEmoji(  1,  2, 1, EMOJIS_TAB, "😁", "Beaming Face",                                ":beaming:", "1f601.png" )
    super.RegisterSymbol( 1,  1, 1, "😀", "Grinning Face",                               ":grin:"         )
    super.RegisterSymbol( 1,  2, 1, "😁", "Beaming Face",                                ":beaming:"      )
    super.RegisterSymbol( 1,  3, 1, "😂", "Face with Tears of Joy",                      ":joy:"          )
    super.RegisterSymbol( 1,  4, 1, "🤣", "Rolling on the Floor Laughing",               ":rofl:"         )
    super.RegisterSymbol( 1,  5, 1, "😃", "Smiling Face with Open Mouth",                ":smiley:"       )
    super.RegisterSymbol( 1,  6, 1, "😄", "Smiling Face with Open Mouth & Smiling Eyes", ":smile:"        )
    super.RegisterSymbol( 1,  7, 1, "😅", "Smiling Face with Sweat",                     ":sweat_smile:"  )
    super.RegisterSymbol( 1,  8, 1, "😆", "Smiling Face with Closed Eyes",               ":laughing:"     )
    super.RegisterSymbol( 1,  9, 1, "😉", "Winking Face",                                ":wink:"         )
    super.RegisterSymbol( 1, 10, 1, "😊", "Smiling Face with Smiling Eyes",              ":blush:"        )
    super.RegisterSymbol( 1, 11, 1, "😋", "Face Savoring Food",                          ":yum:"          )
    super.RegisterSymbol( 1, 12, 1, "😎", "Smiling Face with Sunglasses",                ":sunglasses:"   )

    super.RegisterSymbol( 2,  1, 1, "😍", "Heart Eyes",                     ":heart_eyes:"            )
    super.RegisterSymbol( 2,  2, 1, "😘", "Face Blowing a Kiss",            ":kissing_heart:"         )
    super.RegisterSymbol( 2,  3, 1, "😗", "Kissing Face",                   ":kissing:"               )
    super.RegisterSymbol( 2,  4, 1, "😙", "Kissing Face with Smiling Eyes", ":kissing_smiling_eyes:"  )
    super.RegisterSymbol( 2,  5, 1, "😚", "Kissing Face with Closed Eyes",  ":kissing_closed_eyes:"   )
    super.RegisterSymbol( 2,  6, 1, "🙂", "Slightly Smiling Face",          ":slightly_smiling:"      )
    super.RegisterSymbol( 2,  7, 1, "🤗", "Hugging Face",                   ":hugging:"               )
    super.RegisterSymbol( 2,  8, 1, "🤩", "Star-Struck",                    ":star_struck:"           )
    super.RegisterSymbol( 2,  9, 1, "🥰", "Smiling Face with Hearts",       ":smiling_hearts:"        )
    super.RegisterSymbol( 2, 10, 1, "😇", "Smiling Face with Halo",         ":innocent:"              )
    super.RegisterSymbol( 2, 11, 1, "🥲", "Smiling Face with Tear",         ":smiling_tear:"          )
    super.RegisterSymbol( 2, 12, 1, "😏", "Smirking Face",                  ":smirk:"                 )

    super.RegisterSymbol( 3,  1, 1, "😒", "Unamused Face",          ":unamused:"           )
    super.RegisterSymbol( 3,  2, 1, "😞", "Disappointed Face",      ":disappointed:"       )
    super.RegisterSymbol( 3,  3, 1, "😔", "Pensive Face",           ":pensive:"            )
    super.RegisterSymbol( 3,  4, 1, "😟", "Worried Face",           ":worried:"            )
    super.RegisterSymbol( 3,  5, 1, "😕", "Confused Face",          ":confused:"           )
    super.RegisterSymbol( 3,  6, 1, "🙁", "Slightly Frowning Face", ":slightly_frowning:"  )
    super.RegisterSymbol( 3,  7, 1, "☹️", "Frowning Face",          ":frowning:"           )
    super.RegisterSymbol( 3,  8, 1, "😣", "Persevering Face",       ":persevering:"        )
    super.RegisterSymbol( 3,  9, 1, "😖", "Confounded Face",        ":confounded:"         )
    super.RegisterSymbol( 3, 10, 1, "😫", "Tired Face",             ":tired:"              )
    super.RegisterSymbol( 3, 11, 1, "😩", "Weary Face",             ":weary:"              )
    super.RegisterSymbol( 3, 12, 1, "🥺", "Pleading Face",          ":pleading:"           )

    super.RegisterSymbol( 4,  1, 1, "😭", "Loudly Crying Face",         ":cry:"         )
    super.RegisterSymbol( 4,  2, 1, "😤", "Face with Steam From Nose",  ":face_steam:"  )
    super.RegisterSymbol( 4,  3, 1, "😠", "Angry Face",                 ":angry:"       )
    super.RegisterSymbol( 4,  4, 1, "😡", "Pouting Face",               ":pout:"        )
    super.RegisterSymbol( 4,  5, 1, "🤬", "Face with Symbols on Mouth", ":cursing:"     )
    super.RegisterSymbol( 4,  6, 1, "😳", "Flushed Face",               ":flushed:"     )
    super.RegisterSymbol( 4,  7, 1, "🥵", "Hot Face",                   ":hot:"         )
    super.RegisterSymbol( 4,  8, 1, "🥶", "Cold Face",                  ":cold:"        )
    super.RegisterSymbol( 4,  9, 1, "😱", "Face Screaming in Fear",     ":scream:"      )
    super.RegisterSymbol( 4, 10, 1, "😨", "Fearful Face",               ":fearful:"     )
    super.RegisterSymbol( 4, 11, 1, "😰", "Anxious Face with Sweat",    ":anxious:"     )
    super.RegisterSymbol( 4, 12, 1, "😥", "Sad but Relieved Face",      ":sad_relief:"  )

    super.RegisterSymbol( 5,  1, 1, "😓", "Downcast Face with Sweat",  ":downcast_sweat:"  )
    super.RegisterSymbol( 5,  2, 1, "🤔", "Thinking Face",             ":thinking:"        )
    super.RegisterSymbol( 5,  3, 1, "🤭", "Face with Hand Over Mouth", ":hand_over_mouth:" )
    super.RegisterSymbol( 5,  4, 1, "🤫", "Shushing Face",             ":shushing:"        )
    super.RegisterSymbol( 5,  5, 1, "🤥", "Lying Face",                ":lying:"           )
    super.RegisterSymbol( 5,  6, 1, "😶", "Face Without Mouth",        ":no_mouth:"        )
    super.RegisterSymbol( 5,  7, 1, "😐", "Neutral Face",              ":neutral:"         )
    super.RegisterSymbol( 5,  8, 1, "😬", "Grimacing Face",            ":grimacing:"       )
    super.RegisterSymbol( 5,  9, 1, "🙄", "Face with Rolling Eyes",    ":rolling_eyes:"    )
    super.RegisterSymbol( 5, 10, 1, "😯", "Hushed Face",               ":hushed:"          )
    super.RegisterSymbol( 5, 11, 1, "😴", "Sleeping Face",             ":sleeping:"        )
    super.RegisterSymbol( 5, 12, 1, "😪", "Sleepy Face",               ":sleepy:"          )
  }
}

