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

    this.m_fontSize    := "s10"
    this.m_symBtnSizeY := 20

    this.RegisterButtons()
    this.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbol( 1, 1, 6, "Thanks 😊.",                       "", "Alt+Win+1" )
    super.RegisterSymbol( 1, 7, 6, "Thank you 😊.",                    "", "Alt+Win+2" )
    super.RegisterSymbol( 2, 1, 6, "Thank you very much 🤗.",          "", "Alt+Win+3" )
    super.RegisterSymbol( 2, 7, 6, "Thank you so much 🤗.",            "", "Alt+Win+4" )
    super.RegisterSymbol( 3, 1, 6, "Thanks, I appreciate it 😁.",      "", "Alt+Win+5" )
    super.RegisterSymbol( 3, 7, 6, "Thanks, I'm glad you like it 😁.", "", "Alt+Win+6" )
    super.RegisterSymbol( 4, 1, 6, "Thanks for the tip 🥰.",           "", "Alt+Win+7" )
    super.RegisterSymbol( 4, 7, 6, "`b, and thanks for the tip 🥰.",   "", "Alt+Win+8" )
    ;super.RegisterSymbol( 5, 1, 6, "", "", "" )
    ;super.RegisterSymbol( 5, 7, 6, "", "", "" )
  }
}



; Alt + Wnd + 1 => Thanks message
RegisterAction( "Alt+Win+1", "Thanks 😊", Thanks )
#!1::Thanks()
Thanks()
{
  DoSendText( 'Thanks 😊.' )
}

; Alt + Wnd + 2 => Thank you message
RegisterAction( "Alt+Win+2", "Thank you 😊", ThankYou )
#!2::ThankYou()
ThankYou()
{
  DoSendText( 'Thank you 😊.' )
}

; Alt + Wnd + 3 => Thank you very much message
RegisterAction( "Alt+Win+3", "Thank you very much 🤗", ThankYouVeryMuch )
#!3::ThankYouVeryMuch()
ThankYouVeryMuch()
{
  DoSendText( 'Thank you very much 🤗.' )
}

; Alt + Wnd + 4 => Thank you so much message
RegisterAction( "Alt+Win+4", "Thank you so much 🤗", ThankYouSoMuch )
#!4::ThankYouSoMuch()
ThankYouSoMuch()
{
  DoSendText( 'Thank you so much 🤗.' )
}

; Alt + Wnd + 5 => Thanks, I appreciate it message
RegisterAction( "Alt+Win+5", "Thanks, I appreciate it 😁", ThanksIAppreciateIt )
#!5::ThanksIAppreciateIt()
ThanksIAppreciateIt()
{
  DoSendText( 'Thanks. I appreciate it 😁.' )
}

; Alt + Wnd + 6 => Thanks, I'm glad you like it message
RegisterAction( "Alt+Win+6", "Thanks, I'm glad you like it 😁", ThanksImGladYouLikeIt )
#!6::ThanksImGladYouLikeIt()
ThanksImGladYouLikeIt()
{
  DoSendText( "Thanks. I'm glad you like it 😁." )
}

; Alt + Wnd + 7 => Thanks for the tip message
RegisterAction( "Alt+Win+7", "Thanks for the tip 🥰", ThanksForTheTip )
#!7::ThanksForTheTip()
ThanksForTheTip()
{
  DoSendText( "Thanks for the tip 🥰." )
}

; Alt + Wnd + 8 => And thanks for the tip message
RegisterAction( "Alt+Win+8", "And thanks for the tip 🥰", AndThanksForTheTip )
#!8::AndThanksForTheTip()
AndThanksForTheTip()
{
  DoSendText( "`b, and thanks for the tip 🥰." )
}
