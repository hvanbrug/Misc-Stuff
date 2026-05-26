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
  }
}
