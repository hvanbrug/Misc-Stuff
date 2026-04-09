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

    super.SetRowsOf( 12 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbolX( 1, "😀", "Grinning Face`n:grin:"                                )
    super.RegisterSymbolX( 1, "😁", "Beaming Face`n:beaming:"                              )
    super.RegisterSymbolX( 1, "😂", "Face with Tears of Joy`n:joy:"                        )
    super.RegisterSymbolX( 1, "🤣", "Rolling on the Floor Laughing`n:rofl:"                )
    super.RegisterSymbolX( 1, "😃", "Smiling Face with Open Mouth`n:smiley:"               )
    super.RegisterSymbolX( 1, "😄", "Smiling Face with Open Mouth & Smiling Eyes`n:smile:" )
    super.RegisterSymbolX( 1, "😅", "Smiling Face with Sweat`n:sweat_smile:"               )
    super.RegisterSymbolX( 1, "😆", "Smiling Face with Closed Eyes`n:laughing:"            )
    super.RegisterSymbolX( 1, "😉", "Winking Face`n:wink:"                                 )
    super.RegisterSymbolX( 1, "😊", "Smiling Face with Smiling Eyes`n:blush:"              )
    super.RegisterSymbolX( 1, "😋", "Face Savoring Food`n:yum:"                            )
    super.RegisterSymbolX( 1, "😎", "Smiling Face with Sunglasses`n:sunglasses:"           )

    super.RegisterSymbolX( 1, "😍", "Heart Eyes`n:heart_eyes:"                               )
    super.RegisterSymbolX( 1, "😘", "Face Blowing a Kiss`n:kissing_heart:"                   )
    super.RegisterSymbolX( 1, "😗", "Kissing Face`n:kissing:"                                )
    super.RegisterSymbolX( 1, "😙", "Kissing Face with Smiling Eyes`n:kissing_smiling_eyes:" )
    super.RegisterSymbolX( 1, "😚", "Kissing Face with Closed Eyes`n:kissing_closed_eyes:"   )
    super.RegisterSymbolX( 1, "🙂", "Slightly Smiling Face`n:slightly_smiling:"              )
    super.RegisterSymbolX( 1, "🤗", "Hugging Face`n:hugging:"                                )
    super.RegisterSymbolX( 1, "🤩", "Star-Struck`n:star_struck:"                             )
    super.RegisterSymbolX( 1, "🥰", "Smiling Face with Hearts`n:smiling_hearts:"             )
    super.RegisterSymbolX( 1, "😇", "Smiling Face with Halo`n:innocent:"                     )
    super.RegisterSymbolX( 1, "🥲", "Smiling Face with Tear`n:smiling_tear:"                 )
    super.RegisterSymbolX( 1, "😏", "Smirking Face`n:smirk:"                                 )

    super.RegisterSymbolX( 1, "😒", "Unamused Face`n:unamused:"                   )
    super.RegisterSymbolX( 1, "😞", "Disappointed Face`n:disappointed:"           )
    super.RegisterSymbolX( 1, "😔", "Pensive Face`n:pensive:"                     )
    super.RegisterSymbolX( 1, "😟", "Worried Face`n:worried:"                     )
    super.RegisterSymbolX( 1, "😕", "Confused Face`n:confused:"                   )
    super.RegisterSymbolX( 1, "🙁", "Slightly Frowning Face`n:slightly_frowning:" )
    super.RegisterSymbolX( 1, "☹️", "Frowning Face`n:frowning:"                   )
    super.RegisterSymbolX( 1, "😣", "Persevering Face`n:persevering:"             )
    super.RegisterSymbolX( 1, "😖", "Confounded Face`n:confounded:"               )
    super.RegisterSymbolX( 1, "😫", "Tired Face`n:tired:"                         )
    super.RegisterSymbolX( 1, "😩", "Weary Face`n:weary:"                         )
    super.RegisterSymbolX( 1, "🥺", "Pleading Face`n:pleading:"                   )

    super.RegisterSymbolX( 1, "😭", "Loudly Crying Face`n:cry:"               )
    super.RegisterSymbolX( 1, "😤", "Face with Steam From Nose`n:face_steam:" )
    super.RegisterSymbolX( 1, "😠", "Angry Face`n:angry:"                     )
    super.RegisterSymbolX( 1, "😡", "Pouting Face`n:pout:"                    )
    super.RegisterSymbolX( 1, "🤬", "Face with Symbols on Mouth`n:cursing:"   )
    super.RegisterSymbolX( 1, "😳", "Flushed Face`n:flushed:"                 )
    super.RegisterSymbolX( 1, "🥵", "Hot Face`n:hot:"                         )
    super.RegisterSymbolX( 1, "🥶", "Cold Face`n:cold:"                       )
    super.RegisterSymbolX( 1, "😱", "Face Screaming in Fear`n:scream:"        )
    super.RegisterSymbolX( 1, "😨", "Fearful Face`n:fearful:"                 )
    super.RegisterSymbolX( 1, "😰", "Anxious Face with Sweat`n:anxious:"      )
    super.RegisterSymbolX( 1, "😥", "Sad but Relieved Face`n:sad_relief:"     )

    super.RegisterSymbolX( 1, "😓", "Downcast Face with Sweat`n:downcast_sweat:"   )
    super.RegisterSymbolX( 1, "🤔", "Thinking Face`n:thinking:"                    )
    super.RegisterSymbolX( 1, "🤭", "Face with Hand Over Mouth`n:hand_over_mouth:" )
    super.RegisterSymbolX( 1, "🤫", "Shushing Face`n:shushing:"                    )
    super.RegisterSymbolX( 1, "🤥", "Lying Face`n:lying:"                          )
    super.RegisterSymbolX( 1, "😶", "Face Without Mouth`n:no_mouth:"               )
    super.RegisterSymbolX( 1, "😐", "Neutral Face`n:neutral:"                      )
    super.RegisterSymbolX( 1, "😬", "Grimacing Face`n:grimacing:"                  )
    super.RegisterSymbolX( 1, "🙄", "Face with Rolling Eyes`n:rolling_eyes:"       )
    super.RegisterSymbolX( 1, "😯", "Hushed Face`n:hushed:"                        )
    super.RegisterSymbolX( 1, "😴", "Sleeping Face`n:sleeping:"                    )
    super.RegisterSymbolX( 1, "😪", "Sleepy Face`n:sleepy:"                        )
  }
}

