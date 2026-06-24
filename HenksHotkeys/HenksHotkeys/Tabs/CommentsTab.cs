using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

/// <summary>Ready-made comments tab (CommentSupport.ahk). Emoji stripping applies here.</summary>
internal sealed class CommentsTab : TabModel
{
  public CommentsTab() : base( "Comments" )
  {
    FontSize          = 10f;
    SymBtnSizeX       = 320;
    SymBtnSizeY       = 24;
    EnableStripEmojis = true;

    SetRowsOf( 2 );
    RegisterButtons();
    RecalcSizes();
  }

  private void RegisterButtons()
  {
    RegisterSymbol( 1, 1, 0, 1, "Thanks 😊.",              null, "#!1", null, "left" );
    RegisterSymbol( 2, 1, 0, 1, "Thank you 😊.",           null, "#!2", null, "left" );
    RegisterSymbol( 3, 1, 0, 1, "Thanks a lot 😀.",        null, null,  null, "left" );
    RegisterSymbol( 4, 1, 0, 1, "Thank you kindly 😀.",    null, null,  null, "left" );
    RegisterSymbol( 5, 1, 0, 1, "Thank you very much 🤗.", null, "#!3", null, "left" );
    RegisterSymbol( 6, 1, 0, 1, "Thank you so much 🤗.",   null, "#!4", null, "left" );

    RegisterSymbol( 1, 2, 0, 1, "I appreciate it 😁.",        null, "#!5", null, "left" );
    RegisterSymbol( 2, 2, 0, 1, "I appreciate them 😁.",      null, null,  null, "left" );
    RegisterSymbol( 3, 2, 0, 1, "I really appreciate it 😁.", null, null,  null, "left" );
    RegisterSymbol( 4, 2, 0, 1, "I'm glad you like it 😁.",   null, "#!6", null, "left" );
    RegisterSymbol( 5, 2, 0, 1, "I'm glad you like them 😁.", null, null,  null, "left" );
    ForceNextSlot( 7, 1 );

    ShiftLineByThird();
    RegisterSymbolX( 1, "\b for your kind words and support 🙏.", null, null, null, "left" );
    RegisterSymbolX( 1, "I truly appreciate the time you took to share this 🙏.", null, null, null, "left" );
    RegisterSymbolX( 1, "\b for the thoughtful feedback, it means a lot to me 🙏.", null, null, null, "left" );
    RegisterSymbolX( 1, "\b for your support, it really motivates me to keep going 🙏.", null, null, null, "left" );
    RegisterSymbolX( 1, "\b, your encouragement genuinely made my day 😊.", null, null, null, "left" );
    RegisterSymbolX( 1, "I really appreciate you taking a moment to say that 🙏.", null, null, null, "left" );

    ShiftLineByThird();
    RegisterSymbolX( 1, "Thanks for the tip 🥰.",         null, "#!7", null, "left" );
    RegisterSymbolX( 1, "\b, and thanks for the tip 🥰.", null, "#!8", null, "left" );

    RegisterSymbolX( 1, "Thank you for the tip 🥰.",         null, null, null, "left" );
    RegisterSymbolX( 1, "\b, and thank you for the tip 🥰.", null, null, null, "left" );

    RegisterSymbolX( 1, "Thank you very much for the tip 😁.",         null, null, null, "left" );
    RegisterSymbolX( 1, "\b, and thank you very much for the tip 😁.", null, null, null, "left" );

    RegisterSymbolX( 1, "Thank you so much for the tip 😁.",         null, null, null, "left" );
    RegisterSymbolX( 1, "\b, and thank you so much for the tip 😁.", null, null, null, "left" );

    RegisterSymbolX( 1, "Thanks, I appreciate the tip 🥰.",         null, null, null, "left" );
    RegisterSymbolX( 1, "\b, and thanks, I appreciate the tip 🥰.", null, null, null, "left" );

    ShiftLineByThird();
    RegisterSymbolX( 1, "You're welcome 😊.",      null, "#!9", null, "left" );
    RegisterSymbolX( 1, "You're very welcome 🤗.", null, "#!0", null, "left" );
    RegisterSymbolX( 1, "You're most welcome 🤗.", null, null,  null, "left" );
    RegisterSymbolX( 1, "You're so welcome 🤗.",   null, null,  null, "left" );

    ShiftLineByThird();
    RegisterSymbolX( 1, "It was my pleasure 😊.",                null, null, null, "left" );
    RegisterSymbolX( 1, "It was truly my pleasure 😊.",          null, null, null, "left" );
    RegisterSymbolX( 1, "Anytime 🙂.",                           null, null, null, "left" );
    RegisterSymbolX( 1, "My pleasure 😊.",                       null, null, null, "left" );
    RegisterSymbolX( 1, "My genuine pleasure 😊.",               null, null, null, "left" );
    RegisterSymbolX( 1, "No problem at all 🙂.",                 null, null, null, "left" );
    RegisterSymbolX( 1, "I'm glad I could help 🙂.",             null, null, null, "left" );
    RegisterSymbolX( 1, "I was happy to help 🙂.",               null, null, null, "left" );
    RegisterSymbolX( 1, "I'm glad you found it helpful 😊.",     null, null, null, "left" );
    RegisterSymbolX( 1, "I'm glad it worked out for you 😊.",    null, null, null, "left" );
    RegisterSymbolX( 1, "It was no trouble at all 🙂.",          null, null, null, "left" );
    RegisterSymbolX( 1, "I'm thrilled it made a difference 😊.", null, null, null, "left" );
  }
}
