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
    super.RegisterSymbolX( 1, "Thanks 😊.",                       unset, "#!1", unset, "left" )
    super.RegisterSymbolX( 1, "Thank you 😊.",                    unset, "#!2", unset, "left" )

    super.RegisterSymbolX( 1, "Thank you kindly 😀.",             unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "Thanks a lot 😀.",                 unset, unset, unset, "left" )

    super.RegisterSymbolX( 1, "Thank you very much 🤗.",          unset, "#!3", unset, "left" )
    super.RegisterSymbolX( 1, "Thank you so much 🤗.",            unset, "#!4", unset, "left" )

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, "Thanks. I appreciate it 😁.",      unset, "#!5", unset, "left" )
    super.RegisterSymbolX( 1, "Thanks. I'm glad you like it 😁.", unset, "#!6", unset, "left" )

    super.RegisterSymbolX( 1, "Thank you. I appreciate it 😁.",      unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "Thank you. I'm glad you like it 😁.", unset, unset, unset, "left" )

    super.RegisterSymbolX( 1, "Thank you very much. I appreciate it 😁.",      unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "Thank you very much. I'm glad you like it 😁.", unset, unset, unset, "left" )

    super.RegisterSymbolX( 1, "Thank you so much. I appreciate it 😁.",      unset, unset, unset, "left" )
    super.RegisterSymbolX( 1, "Thank you so much. I'm glad you like it 😁.", unset, unset, unset, "left" )

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, "Thanks for the tip 🥰.",           unset, "#!7", unset, "left" )
    super.RegisterSymbolX( 1, "`b, and thanks for the tip 🥰.",   unset, "#!8", unset, "left" )

    super.RegisterSymbolX( 1, "Thank you for the tip 🥰.",           unset, unset, unset, "left" )
    super.RegisterSpace()
    super.RegisterSymbolX( 1, "Thank you very much for the tip 😁.", unset, unset, unset, "left" )
    super.RegisterSpace()
    super.RegisterSymbolX( 1, "Thank you so much for the tip 😁.",   unset, unset, unset, "left" )
    super.RegisterSpace()
    super.RegisterSymbolX( 1, "Thanks, I appreciate the tip 🥰.",    unset, unset, unset, "left" )
    super.RegisterSpace()

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, "You're welcome 😊.",      unset, "#!9", unset, "left" )
    super.RegisterSymbolX( 1, "You're very welcome 🤗.", unset, "#!0", unset, "left" )
    super.RegisterSymbolX( 1, "You're most welcome 🤗.", unset, unset, unset, "left" )
  }
}




; Alt + Wnd + 1 => Thanks message
;RegisterAction( "Alt+Win+1", "Thanks 😊", Thanks )
;#!1::Thanks()
;Thanks()
;{
;  DoSendText( 'Thanks 😊.' )
;}

; Alt + Wnd + 2 => Thank you message
;RegisterAction( "Alt+Win+2", "Thank you 😊", ThankYou )
;#!2::ThankYou()
;ThankYou()
;{
;  DoSendText( 'Thank you 😊.' )
;}

; Alt + Wnd + 3 => Thank you very much message
;RegisterAction( "Alt+Win+3", "Thank you very much 🤗", ThankYouVeryMuch )
;#!3::ThankYouVeryMuch()
;ThankYouVeryMuch()
;{
;  DoSendText( 'Thank you very much 🤗.' )
;}

; Alt + Wnd + 4 => Thank you so much message
;RegisterAction( "Alt+Win+4", "Thank you so much 🤗", ThankYouSoMuch )
;#!4::ThankYouSoMuch()
;ThankYouSoMuch()
;{
;  DoSendText( 'Thank you so much 🤗.' )
;}

; Alt + Wnd + 5 => Thanks, I appreciate it message
;RegisterAction( "Alt+Win+5", "Thanks, I appreciate it 😁", ThanksIAppreciateIt )
;#!5::ThanksIAppreciateIt()
;ThanksIAppreciateIt()
;{
;  DoSendText( 'Thanks. I appreciate it 😁.' )
;}

; Alt + Wnd + 6 => Thanks, I'm glad you like it message
;RegisterAction( "Alt+Win+6", "Thanks, I'm glad you like it 😁", ThanksImGladYouLikeIt )
;#!6::ThanksImGladYouLikeIt()
;ThanksImGladYouLikeIt()
;{
;  DoSendText( "Thanks. I'm glad you like it 😁." )
;}

; Alt + Wnd + 7 => Thanks for the tip message
;RegisterAction( "Alt+Win+7", "Thanks for the tip 🥰", ThanksForTheTip )
;#!7::ThanksForTheTip()
;ThanksForTheTip()
;{
;  DoSendText( "Thanks for the tip 🥰." )
;}

; Alt + Wnd + 8 => And thanks for the tip message
;RegisterAction( "Alt+Win+8", "And thanks for the tip 🥰", AndThanksForTheTip )
;#!8::AndThanksForTheTip()
;AndThanksForTheTip()
;{
;  DoSendText( "`b, and thanks for the tip 🥰." )
;}
