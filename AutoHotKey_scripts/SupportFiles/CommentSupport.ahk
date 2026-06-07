; CommentSupport.ahk
; A collection of helpful, ready made comments for use in various contexts.
; These are meant to be used as quick responses to common situations, such
; as thanking someone for a compliment or a tip, or responding to feedback
; on a creation.


class CommentsTabPage extends TabPage
{
  __New()
  {
    super.__New( "Comments" )

    super.m_fontSize    := "s10"
    super.m_symBtnSizeX := 320
    super.m_symBtnSizeY := 24

    super.SetRowsOf( 2 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbol( 1, 1, 0, 1, "Thanks 😊.",              unset, "#!1", unset, "left" )
    super.RegisterSymbol( 2, 1, 0, 1, "Thank you 😊.",           unset, "#!2", unset, "left" )
    super.RegisterSymbol( 3, 1, 0, 1, "Thanks a lot 😀.",        unset, unset, unset, "left" )
    super.RegisterSymbol( 4, 1, 0, 1, "Thank you kindly 😀.",    unset, unset, unset, "left" )
    super.RegisterSymbol( 5, 1, 0, 1, "Thank you very much 🤗.", unset, "#!3", unset, "left" )
    super.RegisterSymbol( 6, 1, 0, 1, "Thank you so much 🤗.",   unset, "#!4", unset, "left" )

    super.RegisterSymbol( 1, 2, 0, 1, "I appreciate it 😁.",        unset, "#!5", unset, "left" )
    super.RegisterSymbol( 2, 2, 0, 1, "I appreciate them 😁.",      unset, unset, unset, "left" )
    super.RegisterSymbol( 3, 2, 0, 1, "I really appreciate it 😁.", unset, unset, unset, "left" )
    super.RegisterSymbol( 4, 2, 0, 1, "I'm glad you like it 😁.",   unset, "#!6", unset, "left" )
    super.RegisterSymbol( 5, 2, 0, 1, "I'm glad you like them 😁.", unset, unset, unset, "left" )
    super.ForceNextSlot( 7, 1 )

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, "`b for your kind words and support 🙏.", unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "I truly appreciate the time you took to share this 🙏.", unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "`b for the thoughtful feedback, it means a lot to me 🙏.", unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "`b for your support, it really motivates me to keep going 🙏.", unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "`b, your encouragement genuinely made my day 😊.", unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "I really appreciate you taking a moment to say that 🙏.", unset, unset, unset, "left" )

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, "Thanks for the tip 🥰.",         unset, "#!7", unset, "left" )
    super.RegisterSymbolX( 1, "`b, and thanks for the tip 🥰.", unset, "#!8", unset, "left" )

    super.RegisterSymbolX( 1, "Thank you for the tip 🥰.",         unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "`b, and thank you for the tip 🥰.", unset, unset, unset, "left" )

    super.RegisterSymbolX( 1, "Thank you very much for the tip 😁.",         unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "`b, and thank you very much for the tip 😁.", unset, unset, unset, "left" )

    super.RegisterSymbolX( 1, "Thank you so much for the tip 😁.",         unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "`b, and thank you so much for the tip 😁.", unset, unset, unset, "left" )

    super.RegisterSymbolX( 1, "Thanks, I appreciate the tip 🥰.",         unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "`b, and thanks, I appreciate the tip 🥰.", unset, unset, unset, "left" )

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, "You're welcome 😊.",      unset, "#!9", unset, "left" )
    super.RegisterSymbolX( 1, "You're very welcome 🤗.", unset, "#!0", unset, "left" )
    super.RegisterSymbolX( 1, "You're most welcome 🤗.", unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "You're so welcome 🤗.",   unset, unset, unset, "left" )

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, "It was my pleasure 😊.",                unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "It was truly my pleasure 😊.",          unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "Anytime 🙂.",                           unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "My pleasure 😊.",                       unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "My genuine pleasure 😊.",               unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "No problem at all 🙂.",                 unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "I'm glad I could help 🙂.",             unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "I was happy to help 🙂.",               unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "I'm glad you found it helpful 😊.",     unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "I'm glad it worked out for you 😊.",    unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "It was no trouble at all 🙂.",          unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "I'm thrilled it made a difference 😊.", unset, unset, unset, "left" )
  }
}
