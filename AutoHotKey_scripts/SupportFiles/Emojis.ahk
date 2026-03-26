; Face Emojis
; Each emoji is registered as a symbol button on the "Emojis" tab.
; Layout: 4 rows, going down then across.


class EmojisTabPage extends TabPage
{
  __New()
  {
    super.__New( "Emojis" )

    super.m_fontSize    := "s28"
    super.m_fontName    := "Segoe UI Emoji"
    super.m_symBtnSizeX := 50
    super.m_symBtnSizeY := 50

    super.SetByCol( 12 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbolX( 1, "😀", "Grinning Face",                               ":grin:"         )
    super.RegisterSymbolX( 1, "😁", "Beaming Face",                                ":beaming:"      )
    super.RegisterSymbolX( 1, "😂", "Face with Tears of Joy",                      ":joy:"          )
    super.RegisterSymbolX( 1, "🤣", "Rolling on the Floor Laughing",               ":rofl:"         )
    super.RegisterSymbolX( 1, "😃", "Smiling Face with Open Mouth",                ":smiley:"       )
    super.RegisterSymbolX( 1, "😄", "Smiling Face with Open Mouth & Smiling Eyes", ":smile:"        )
    super.RegisterSymbolX( 1, "😅", "Smiling Face with Sweat",                     ":sweat_smile:"  )
    super.RegisterSymbolX( 1, "😆", "Smiling Face with Closed Eyes",               ":laughing:"     )
    super.RegisterSymbolX( 1, "😉", "Winking Face",                                ":wink:"         )
    super.RegisterSymbolX( 1, "😊", "Smiling Face with Smiling Eyes",              ":blush:"        )
    super.RegisterSymbolX( 1, "😋", "Face Savoring Food",                          ":yum:"          )
    super.RegisterSymbolX( 1, "😎", "Smiling Face with Sunglasses",                ":sunglasses:"   )

    super.RegisterSymbolX( 1, "😍", "Heart Eyes",                     ":heart_eyes:"            )
    super.RegisterSymbolX( 1, "😘", "Face Blowing a Kiss",            ":kissing_heart:"         )
    super.RegisterSymbolX( 1, "😗", "Kissing Face",                   ":kissing:"               )
    super.RegisterSymbolX( 1, "😙", "Kissing Face with Smiling Eyes", ":kissing_smiling_eyes:"  )
    super.RegisterSymbolX( 1, "😚", "Kissing Face with Closed Eyes",  ":kissing_closed_eyes:"   )
    super.RegisterSymbolX( 1, "🙂", "Slightly Smiling Face",          ":slightly_smiling:"      )
    super.RegisterSymbolX( 1, "🤗", "Hugging Face",                   ":hugging:"               )
    super.RegisterSymbolX( 1, "🤩", "Star-Struck",                    ":star_struck:"           )
    super.RegisterSymbolX( 1, "🥰", "Smiling Face with Hearts",       ":smiling_hearts:"        )
    super.RegisterSymbolX( 1, "😇", "Smiling Face with Halo",         ":innocent:"              )
    super.RegisterSymbolX( 1, "🥲", "Smiling Face with Tear",         ":smiling_tear:"          )
    super.RegisterSymbolX( 1, "😏", "Smirking Face",                  ":smirk:"                 )

    super.RegisterSymbolX( 1, "😒", "Unamused Face",          ":unamused:"           )
    super.RegisterSymbolX( 1, "😞", "Disappointed Face",      ":disappointed:"       )
    super.RegisterSymbolX( 1, "😔", "Pensive Face",           ":pensive:"            )
    super.RegisterSymbolX( 1, "😟", "Worried Face",           ":worried:"            )
    super.RegisterSymbolX( 1, "😕", "Confused Face",          ":confused:"           )
    super.RegisterSymbolX( 1, "🙁", "Slightly Frowning Face", ":slightly_frowning:"  )
    super.RegisterSymbolX( 1, "☹️", "Frowning Face",          ":frowning:"           )
    super.RegisterSymbolX( 1, "😣", "Persevering Face",       ":persevering:"        )
    super.RegisterSymbolX( 1, "😖", "Confounded Face",        ":confounded:"         )
    super.RegisterSymbolX( 1, "😫", "Tired Face",             ":tired:"              )
    super.RegisterSymbolX( 1, "😩", "Weary Face",             ":weary:"              )
    super.RegisterSymbolX( 1, "🥺", "Pleading Face",          ":pleading:"           )

    super.RegisterSymbolX( 1, "😭", "Loudly Crying Face",         ":cry:"         )
    super.RegisterSymbolX( 1, "😤", "Face with Steam From Nose",  ":face_steam:"  )
    super.RegisterSymbolX( 1, "😠", "Angry Face",                 ":angry:"       )
    super.RegisterSymbolX( 1, "😡", "Pouting Face",               ":pout:"        )
    super.RegisterSymbolX( 1, "🤬", "Face with Symbols on Mouth", ":cursing:"     )
    super.RegisterSymbolX( 1, "😳", "Flushed Face",               ":flushed:"     )
    super.RegisterSymbolX( 1, "🥵", "Hot Face",                   ":hot:"         )
    super.RegisterSymbolX( 1, "🥶", "Cold Face",                  ":cold:"        )
    super.RegisterSymbolX( 1, "😱", "Face Screaming in Fear",     ":scream:"      )
    super.RegisterSymbolX( 1, "😨", "Fearful Face",               ":fearful:"     )
    super.RegisterSymbolX( 1, "😰", "Anxious Face with Sweat",    ":anxious:"     )
    super.RegisterSymbolX( 1, "😥", "Sad but Relieved Face",      ":sad_relief:"  )

    super.RegisterSymbolX( 1, "😓", "Downcast Face with Sweat",  ":downcast_sweat:"  )
    super.RegisterSymbolX( 1, "🤔", "Thinking Face",             ":thinking:"        )
    super.RegisterSymbolX( 1, "🤭", "Face with Hand Over Mouth", ":hand_over_mouth:" )
    super.RegisterSymbolX( 1, "🤫", "Shushing Face",             ":shushing:"        )
    super.RegisterSymbolX( 1, "🤥", "Lying Face",                ":lying:"           )
    super.RegisterSymbolX( 1, "😶", "Face Without Mouth",        ":no_mouth:"        )
    super.RegisterSymbolX( 1, "😐", "Neutral Face",              ":neutral:"         )
    super.RegisterSymbolX( 1, "😬", "Grimacing Face",            ":grimacing:"       )
    super.RegisterSymbolX( 1, "🙄", "Face with Rolling Eyes",    ":rolling_eyes:"    )
    super.RegisterSymbolX( 1, "😯", "Hushed Face",               ":hushed:"          )
    super.RegisterSymbolX( 1, "😴", "Sleeping Face",             ":sleeping:"        )
    super.RegisterSymbolX( 1, "😪", "Sleepy Face",               ":sleepy:"          )
  }
}

