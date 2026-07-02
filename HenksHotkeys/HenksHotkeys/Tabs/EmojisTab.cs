using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

// Emoji tab (Emojis.ahk) — organized by Unicode category, 12 columns,
// row-primary, with a half-row gap between categories. Buttons render Twemoji
// PNG images (see EmojiImageProvider) and fall back to the emoji glyph.
// NOTE: the button definitions below are generated 1:1 from SupportFiles\Emojis.ahk.
internal sealed class EmojisTab : TabModel
{
  public EmojisTab() : base( "Emojis" )
  {
    FontSize       = 28f;
    FontName       = "Segoe UI Emoji";
    SymBtnSizeX    = 50;
    SymBtnSizeY    = 50;
    UseEmojiImages = true;
    SetRowsOf( 12 );
    RegisterButtons();
    BackfillFavouriteNames(); // favourites carry only codepoints; borrow names from the catalog
    RecalcSizes();
  }

  private bool m_placedCategory; // whether anything has been placed yet (drives the pre-heading gap)

  private void RegisterButtons()
  {
    m_placedCategory = RegisterFavourites(); // own "Favourites" heading + buttons, or nothing

    Category( "Smileys & Emotion", RegisterSmileys );
    Category( "Hearts & Love",     RegisterHeartsAndEmotion );
    Category( "Gestures",          RegisterGestures );
    Category( "People & Body",     RegisterPeople );
    Category( "Animals",           RegisterAnimals );
    Category( "Plants & Sky",      RegisterPlantsAndNature );
    Category( "Food & Drink",      RegisterFoodAndDrink );
    Category( "Travel & Places",   RegisterTravelAndPlaces );
    Category( "Activities",        RegisterActivities );
    Category( "Objects",           RegisterObjects );
    Category( "Symbols",           RegisterSymbols );
  }

  // A labelled emoji category: a little separation from the one above (none for the very first),
  // a heading, then the category's buttons flowing below it.
  private void Category( string name, Action register )
  {
    NextLine( true );                        // finish the previous category's partial row
    if( m_placedCategory ) ShiftLineByHalf(); // gap above the heading (not for the first one)
    RegisterSectionHeader( name );
    register();
    m_placedCategory = true;
  }

  /// <summary>Emoji in this tab's Favourites section, in display order (top of the tab).</summary>
  public IReadOnlyList<SymbolElement> Favourites => Symbols.Where( s => s.IsFavourite ).ToList();

  /// <summary>Columns the favourites (and the whole tab) flow across.</summary>
  public int FavouriteColumns => MaxSlots;

  // ─── Favourites (user-configurable, #13) ─────────────────────────
  // Rendered from FavouritesStore under a "Favourites" heading. Because the tab is a plain
  // left-to-right flow, re-registering the list in a new order reflows every row automatically —
  // exactly the "insert-and-cascade" the drag reorder wants. Returns true if any were placed.
  private bool RegisterFavourites()
  {
    IReadOnlyList<Core.Favourite> favs = Core.FavouritesStore.Load();
    if( favs.Count == 0 )
    {
      return false;
    }

    RegisterSectionHeader( "Favourites" ); // heading spanning the row, buttons drop below it

    foreach( Core.Favourite f in favs )
    {
      RegisterSymbolX( 1, f.Emoji ); // the emoji is decoded from its codepoints (authoritative)
      Symbols[^1].IsFavourite = true; // tag the button just placed
    }
    return true;
  }

  // Favourites are stored as bare codepoints (no description), so recover each one's friendly
  // tooltip name from the matching catalog emoji — which is registered later in the same build.
  // Called once after all categories are placed.
  private void BackfillFavouriteNames()
  {
    var names = new Dictionary<string, string>();
    foreach( SymbolElement s in Symbols )
    {
      if( !s.IsFavourite && !names.ContainsKey( s.Char ) ) names[s.Char] = s.Desc;
    }
    foreach( SymbolElement s in Symbols )
    {
      if( s.IsFavourite && names.TryGetValue( s.Char, out string? d ) ) s.Desc = d;
    }
  }

  // ─── Smileys & Emotion — Faces ─────────────────────────────────
  private void RegisterSmileys()
  {
    RegisterSymbolX( 1, "😀", "Grinning Face\n:grinning:" );
    RegisterSymbolX( 1, "😃", "Grinning Face with Big Eyes\n:smiley:" );
    RegisterSymbolX( 1, "😄", "Grinning Face with Smiling Eyes\n:smile:" );
    RegisterSymbolX( 1, "😁", "Beaming Face with Smiling Eyes\n:grin:" );
    RegisterSymbolX( 1, "😆", "Grinning Squinting Face\n:laughing:" );
    RegisterSymbolX( 1, "😅", "Grinning Face with Sweat\n:sweat_smile:" );
    RegisterSymbolX( 1, "🤣", "Rolling on the Floor Laughing\n:rofl:" );
    RegisterSymbolX( 1, "😂", "Face with Tears of Joy\n:joy:" );
    RegisterSymbolX( 1, "🙂", "Slightly Smiling Face\n:slightly_smiling_face:" );
    RegisterSymbolX( 1, "🙃", "Upside-Down Face\n:upside_down_face:" );
    RegisterSymbolX( 1, "🫠", "Melting Face\n:melting_face:" );
    RegisterSymbolX( 1, "😉", "Winking Face\n:wink:" );

    RegisterSymbolX( 1, "😊", "Smiling Face with Smiling Eyes\n:blush:" );
    RegisterSymbolX( 1, "😇", "Smiling Face with Halo\n:innocent:" );
    RegisterSymbolX( 1, "🥰", "Smiling Face with Hearts\n:smiling_face_with_hearts:" );
    RegisterSymbolX( 1, "😍", "Smiling Face with Heart-Eyes\n:heart_eyes:" );
    RegisterSymbolX( 1, "🤩", "Star-Struck\n:star_struck:" );
    RegisterSymbolX( 1, "😘", "Face Blowing a Kiss\n:kissing_heart:" );
    RegisterSymbolX( 1, "😗", "Kissing Face\n:kissing:" );
    RegisterSymbolX( 1, "☺️", "Smiling Face\n:relaxed:" );
    RegisterSymbolX( 1, "😚", "Kissing Face with Closed Eyes\n:kissing_closed_eyes:" );
    RegisterSymbolX( 1, "😙", "Kissing Face with Smiling Eyes\n:kissing_smiling_eyes:" );
    RegisterSymbolX( 1, "🥲", "Smiling Face with Tear\n:smiling_face_with_tear:" );
    RegisterSymbolX( 1, "😋", "Face Savoring Food\n:yum:" );

    RegisterSymbolX( 1, "😛", "Face with Tongue\n:stuck_out_tongue:" );
    RegisterSymbolX( 1, "😜", "Winking Face with Tongue\n:stuck_out_tongue_winking_eye:" );
    RegisterSymbolX( 1, "🤪", "Zany Face\n:zany_face:" );
    RegisterSymbolX( 1, "😝", "Squinting Face with Tongue\n:stuck_out_tongue_closed_eyes:" );
    RegisterSymbolX( 1, "🤑", "Money-Mouth Face\n:money_mouth_face:" );
    RegisterSymbolX( 1, "🤗", "Hugging Face\n:hugging_face:" );
    RegisterSymbolX( 1, "🤭", "Face with Hand Over Mouth\n:hand_over_mouth:" );
    RegisterSymbolX( 1, "🫢", "Face with Open Eyes and Hand Over Mouth\n:face_with_open_eyes_and_hand_over_mouth:" );
    RegisterSymbolX( 1, "🫣", "Face with Peeking Eye\n:face_with_peeking_eye:" );
    RegisterSymbolX( 1, "🤫", "Shushing Face\n:shushing_face:" );
    RegisterSymbolX( 1, "🤔", "Thinking Face\n:thinking:" );
    RegisterSymbolX( 1, "🫡", "Saluting Face\n:saluting_face:" );

    RegisterSymbolX( 1, "🤐", "Zipper-Mouth Face\n:zipper_mouth_face:" );
    RegisterSymbolX( 1, "🤨", "Face with Raised Eyebrow\n:raised_eyebrow:" );
    RegisterSymbolX( 1, "😐", "Neutral Face\n:neutral_face:" );
    RegisterSymbolX( 1, "😑", "Expressionless Face\n:expressionless:" );
    RegisterSymbolX( 1, "😶", "Face Without Mouth\n:no_mouth:" );
    RegisterSymbolX( 1, "🫥", "Dotted Line Face\n:dotted_line_face:" );
    RegisterSymbolX( 1, "😶‍🌫️", "Face in Clouds\n:face_in_clouds:" );
    RegisterSymbolX( 1, "😏", "Smirking Face\n:smirk:" );
    RegisterSymbolX( 1, "😒", "Unamused Face\n:unamused:" );
    RegisterSymbolX( 1, "🙄", "Face with Rolling Eyes\n:roll_eyes:" );
    RegisterSymbolX( 1, "😬", "Grimacing Face\n:grimacing:" );
    RegisterSymbolX( 1, "😮‍💨", "Face Exhaling\n:face_exhaling:" );

    RegisterSymbolX( 1, "🤥", "Lying Face\n:lying_face:" );
    RegisterSymbolX( 1, "🫨", "Shaking Face\n:shaking_face:" );
    RegisterSymbolX( 1, "🙂‍↔️", "Head Shaking Horizontally\n:head_shaking_horizontally:" );
    RegisterSymbolX( 1, "🙂‍↕️", "Head Shaking Vertically\n:head_shaking_vertically:" );
    RegisterSymbolX( 1, "😌", "Relieved Face\n:relieved:" );
    RegisterSymbolX( 1, "😔", "Pensive Face\n:pensive:" );
    RegisterSymbolX( 1, "😪", "Sleepy Face\n:sleepy:" );
    RegisterSymbolX( 1, "🤤", "Drooling Face\n:drooling_face:" );
    RegisterSymbolX( 1, "😴", "Sleeping Face\n:sleeping:" );
    RegisterSymbolX( 1, "🫩", "Face with Bags Under Eyes\n:face_with_bags_under_eyes:" );
    RegisterSymbolX( 1, "😷", "Face with Medical Mask\n:mask:" );
    RegisterSymbolX( 1, "🤒", "Face with Thermometer\n:face_with_thermometer:" );

    RegisterSymbolX( 1, "🤕", "Face with Head-Bandage\n:head_bandage:" );
    RegisterSymbolX( 1, "🤢", "Nauseated Face\n:nauseated_face:" );
    RegisterSymbolX( 1, "🤮", "Face Vomiting\n:vomiting_face:" );
    RegisterSymbolX( 1, "🤧", "Sneezing Face\n:sneezing_face:" );
    RegisterSymbolX( 1, "🥵", "Hot Face\n:hot_face:" );
    RegisterSymbolX( 1, "🥶", "Cold Face\n:cold_face:" );
    RegisterSymbolX( 1, "🥴", "Woozy Face\n:woozy_face:" );
    RegisterSymbolX( 1, "😵", "Face with Crossed-Out Eyes\n:dizzy_face:" );
    RegisterSymbolX( 1, "😵‍💫", "Face with Spiral Eyes\n:face_with_spiral_eyes:" );
    RegisterSymbolX( 1, "🤯", "Exploding Head\n:exploding_head:" );
    RegisterSymbolX( 1, "🤠", "Cowboy Hat Face\n:cowboy_hat_face:" );
    RegisterSymbolX( 1, "🥳", "Partying Face\n:partying_face:" );

    RegisterSymbolX( 1, "🥸", "Disguised Face\n:disguised_face:" );
    RegisterSymbolX( 1, "😎", "Smiling Face with Sunglasses\n:sunglasses:" );
    RegisterSymbolX( 1, "🤓", "Nerd Face\n:nerd_face:" );
    RegisterSymbolX( 1, "🧐", "Face with Monocle\n:monocle_face:" );
    RegisterSymbolX( 1, "😕", "Confused Face\n:confused:" );
    RegisterSymbolX( 1, "🫤", "Face with Diagonal Mouth\n:diagonal_mouth_face:" );
    RegisterSymbolX( 1, "😟", "Worried Face\n:worried:" );
    RegisterSymbolX( 1, "🙁", "Slightly Frowning Face\n:slightly_frowning_face:" );
    RegisterSymbolX( 1, "☹️", "Frowning Face\n:frowning_face:" );
    RegisterSymbolX( 1, "😮", "Face with Open Mouth\n:open_mouth:" );
    RegisterSymbolX( 1, "😯", "Hushed Face\n:hushed:" );
    RegisterSymbolX( 1, "😲", "Astonished Face\n:astonished:" );

    RegisterSymbolX( 1, "😳", "Flushed Face\n:flushed:" );
    RegisterSymbolX( 1, "🥺", "Pleading Face\n:pleading_face:" );
    RegisterSymbolX( 1, "🥹", "Face Holding Back Tears\n:face_holding_back_tears:" );
    RegisterSymbolX( 1, "😦", "Frowning Face with Open Mouth\n:frowning:" );
    RegisterSymbolX( 1, "😧", "Anguished Face\n:anguished:" );
    RegisterSymbolX( 1, "😨", "Fearful Face\n:fearful:" );
    RegisterSymbolX( 1, "😰", "Anxious Face with Sweat\n:cold_sweat:" );
    RegisterSymbolX( 1, "😥", "Sad but Relieved Face\n:disappointed_relieved:" );
    RegisterSymbolX( 1, "😢", "Crying Face\n:cry:" );
    RegisterSymbolX( 1, "😭", "Loudly Crying Face\n:sob:" );
    RegisterSymbolX( 1, "😱", "Face Screaming in Fear\n:scream:" );
    RegisterSymbolX( 1, "😖", "Confounded Face\n:confounded:" );

    RegisterSymbolX( 1, "😣", "Persevering Face\n:persevere:" );
    RegisterSymbolX( 1, "😞", "Disappointed Face\n:disappointed:" );
    RegisterSymbolX( 1, "😓", "Downcast Face with Sweat\n:sweat:" );
    RegisterSymbolX( 1, "😩", "Weary Face\n:weary:" );
    RegisterSymbolX( 1, "😫", "Tired Face\n:tired_face:" );
    RegisterSymbolX( 1, "🥱", "Yawning Face\n:yawning_face:" );
    RegisterSymbolX( 1, "😤", "Face with Steam From Nose\n:triumph:" );
    RegisterSymbolX( 1, "😡", "Pouting Face\n:pout:" );
    RegisterSymbolX( 1, "😠", "Angry Face\n:angry:" );
    RegisterSymbolX( 1, "🤬", "Face with Symbols on Mouth\n:cursing_face:" );
    RegisterSymbolX( 1, "😈", "Smiling Face with Horns\n:smiling_imp:" );
    RegisterSymbolX( 1, "👿", "Angry Face with Horns\n:imp:" );

    RegisterSymbolX( 1, "💀", "Skull\n:skull:" );
    RegisterSymbolX( 1, "☠️", "Skull and Crossbones\n:skull_and_crossbones:" );
    RegisterSymbolX( 1, "💩", "Pile of Poo\n:poop:" );
    RegisterSymbolX( 1, "🤡", "Clown Face\n:clown_face:" );
    RegisterSymbolX( 1, "👹", "Ogre\n:japanese_ogre:" );
    RegisterSymbolX( 1, "👺", "Goblin\n:japanese_goblin:" );
    RegisterSymbolX( 1, "👻", "Ghost\n:ghost:" );
    RegisterSymbolX( 1, "👽", "Alien\n:alien:" );
    RegisterSymbolX( 1, "👾", "Alien Monster\n:space_invader:" );
    RegisterSymbolX( 1, "🤖", "Robot\n:robot:" );
    RegisterSymbolX( 1, "😺", "Grinning Cat\n:smiley_cat:" );
    RegisterSymbolX( 1, "😸", "Grinning Cat with Smiling Eyes\n:smile_cat:" );

    RegisterSymbolX( 1, "😹", "Cat with Tears of Joy\n:joy_cat:" );
    RegisterSymbolX( 1, "😻", "Smiling Cat with Heart-Eyes\n:heart_eyes_cat:" );
    RegisterSymbolX( 1, "😼", "Cat with Wry Smile\n:smirk_cat:" );
    RegisterSymbolX( 1, "😽", "Kissing Cat\n:kissing_cat:" );
    RegisterSymbolX( 1, "🙀", "Weary Cat\n:scream_cat:" );
    RegisterSymbolX( 1, "😿", "Crying Cat\n:crying_cat_face:" );
    RegisterSymbolX( 1, "😾", "Pouting Cat\n:pouting_cat:" );
    RegisterSymbolX( 1, "🙈", "See-No-Evil Monkey\n:see_no_evil:" );
    RegisterSymbolX( 1, "🙉", "Hear-No-Evil Monkey\n:hear_no_evil:" );
    RegisterSymbolX( 1, "🙊", "Speak-No-Evil Monkey\n:speak_no_evil:" );
    RegisterSymbolX( 1, "💋", "Kiss Mark\n:kiss:" );
    RegisterSymbolX( 1, "💌", "Love Letter\n:love_letter:" );

    RegisterSymbolX( 1, "💘", "Heart with Arrow\n:heartpulse:" );
    RegisterSymbolX( 1, "💝", "Heart with Ribbon\n:gift_heart:" );
    RegisterSymbolX( 1, "💖", "Sparkling Heart\n:sparkling_heart:" );
    RegisterSymbolX( 1, "💗", "Growing Heart\n:heart_decoration:" );
    RegisterSymbolX( 1, "💓", "Beating Heart\n:heartbeat:" );
    RegisterSymbolX( 1, "💞", "Revolving Hearts\n:revolving_hearts:" );
    RegisterSymbolX( 1, "💕", "Two Hearts\n:two_hearts:" );
    RegisterSymbolX( 1, "💟", "Heart Decoration\n:heart_decoration_symbol:" );
    RegisterSymbolX( 1, "❣️", "Heart Exclamation\n:heart_exclamation:" );
    RegisterSymbolX( 1, "💔", "Broken Heart\n:broken_heart:" );
    RegisterSymbolX( 1, "❤️‍🔥", "Heart on Fire\n:heart_on_fire:" );
    RegisterSymbolX( 1, "❤️‍🩹", "Mending Heart\n:mending_heart:" );

    RegisterSymbolX( 1, "❤️", "Red Heart\n:red_heart:" );
    RegisterSymbolX( 1, "🩷", "Pink Heart\n:pink_heart:" );
    RegisterSymbolX( 1, "🧡", "Orange Heart\n:orange_heart:" );
    RegisterSymbolX( 1, "💛", "Yellow Heart\n:yellow_heart:" );
    RegisterSymbolX( 1, "💚", "Green Heart\n:green_heart:" );
    RegisterSymbolX( 1, "💙", "Blue Heart\n:blue_heart:" );
    RegisterSymbolX( 1, "🩵", "Light Blue Heart\n:light_blue_heart:" );
    RegisterSymbolX( 1, "💜", "Purple Heart\n:purple_heart:" );
    RegisterSymbolX( 1, "🤎", "Brown Heart\n:brown_heart:" );
    RegisterSymbolX( 1, "🖤", "Black Heart\n:black_heart:" );
    RegisterSymbolX( 1, "🩶", "Grey Heart\n:grey_heart:" );
    RegisterSymbolX( 1, "🤍", "White Heart\n:white_heart:" );

    RegisterSymbolX( 1, "💯", "Hundred Points\n:100:" );
    RegisterSymbolX( 1, "💢", "Anger Symbol\n:anger:" );
    RegisterSymbolX( 1, "💥", "Collision\n:boom:" );
    RegisterSymbolX( 1, "💫", "Dizzy\n:dizzy:" );
    RegisterSymbolX( 1, "💦", "Sweat Droplets\n:sweat_drops:" );
    RegisterSymbolX( 1, "💨", "Dashing Away\n:dash:" );
    RegisterSymbolX( 1, "🕳️", "Hole\n:hole:" );
    RegisterSymbolX( 1, "💬", "Speech Balloon\n:speech_balloon:" );
    RegisterSymbolX( 1, "👁️‍🗨️", "Eye in Speech Bubble\n:eye_in_speech_bubble:" );
    RegisterSymbolX( 1, "🗨️", "Left Speech Bubble\n:left_speech_bubble:" );
    RegisterSymbolX( 1, "🗯️", "Right Anger Bubble\n:right_anger_bubble:" );
    RegisterSymbolX( 1, "💭", "Thought Balloon\n:thought_balloon:" );

    RegisterSymbolX( 1, "💤", "ZZZ\n:zzz:" );





//    super.RegisterSymbolX( 1, "😀", "Grinning Face`n:grin:"                                )
//    super.RegisterSymbolX( 1, "😁", "Beaming Face`n:beaming:"                              )
//    super.RegisterSymbolX( 1, "😂", "Face with Tears of Joy`n:joy:"                        )
//    super.RegisterSymbolX( 1, "🤣", "Rolling on the Floor Laughing`n:rofl:"                )
//    super.RegisterSymbolX( 1, "😃", "Smiling Face with Open Mouth`n:smiley:"               )
//    super.RegisterSymbolX( 1, "😄", "Smiling Face with Smiling Eyes`n:smile:"              )
//    super.RegisterSymbolX( 1, "😅", "Smiling Face with Sweat`n:sweat_smile:"               )
//    super.RegisterSymbolX( 1, "😆", "Smiling Face with Closed Eyes`n:laughing:"            )
//    super.RegisterSymbolX( 1, "😉", "Winking Face`n:wink:"                                 )
//    super.RegisterSymbolX( 1, "😊", "Smiling Face with Smiling Eyes`n:blush:"              )
//    super.RegisterSymbolX( 1, "😋", "Face Savoring Food`n:yum:"                            )
//    super.RegisterSymbolX( 1, "😎", "Smiling Face with Sunglasses`n:sunglasses:"           )

//    super.RegisterSymbolX( 1, "😍", "Heart Eyes`n:heart_eyes:"                               )
//    super.RegisterSymbolX( 1, "😘", "Face Blowing a Kiss`n:kissing_heart:"                   )
//    super.RegisterSymbolX( 1, "😗", "Kissing Face`n:kissing:"                                )
//    super.RegisterSymbolX( 1, "😙", "Kissing Face with Smiling Eyes`n:kissing_smiling_eyes:" )
//    super.RegisterSymbolX( 1, "😚", "Kissing Face with Closed Eyes`n:kissing_closed_eyes:"   )
//    super.RegisterSymbolX( 1, "🙂", "Slightly Smiling Face`n:slightly_smiling:"              )
//    super.RegisterSymbolX( 1, "🤗", "Hugging Face`n:hugging:"                                )
//    super.RegisterSymbolX( 1, "🤩", "Star-Struck`n:star_struck:"                             )
//    super.RegisterSymbolX( 1, "🥰", "Smiling Face with Hearts`n:smiling_hearts:"             )
//    super.RegisterSymbolX( 1, "😇", "Smiling Face with Halo`n:innocent:"                     )
//    super.RegisterSymbolX( 1, "🥲", "Smiling Face with Tear`n:smiling_tear:"                 )
//    super.RegisterSymbolX( 1, "🤭", "Face with Hand over Mouth`n:hand_over_mouth:"           )

//    super.RegisterSymbolX( 1, "🤫", "Shushing Face`n:shushing:"                    )
//    super.RegisterSymbolX( 1, "🤔", "Thinking Face`n:thinking:"                    )
//    super.RegisterSymbolX( 1, "🤐", "Zipper-Mouth Face`n:zipper_mouth:"            )
//    super.RegisterSymbolX( 1, "🤨", "Face with Raised Eyebrow`n:raised_eyebrow:"   )
//    super.RegisterSymbolX( 1, "😐", "Neutral Face`n:neutral_face:"                 )
//    super.RegisterSymbolX( 1, "😑", "Expressionless Face`n:expressionless:"        )
//    super.RegisterSymbolX( 1, "😶", "Face without Mouth`n:no_mouth:"               )
//    super.RegisterSymbolX( 1, "😏", "Smirking Face`n:smirk:"                       )
//    super.RegisterSymbolX( 1, "😒", "Unamused Face`n:unamused:"                    )
//    super.RegisterSymbolX( 1, "🙄", "Face with Rolling Eyes`n:rolling_eyes:"       )
//    super.RegisterSymbolX( 1, "😬", "Grimacing Face`n:grimacing:"                  )
//    super.RegisterSymbolX( 1, "🤥", "Lying Face`n:lying:"                          )

//    super.RegisterSymbolX( 1, "😌", "Relieved Face`n:relieved:"                )
//    super.RegisterSymbolX( 1, "😔", "Pensive Face`n:pensive:"                  )
//    super.RegisterSymbolX( 1, "😪", "Sleepy Face`n:sleepy:"                    )
//    super.RegisterSymbolX( 1, "🤤", "Drooling Face`n:drooling:"                )
//    super.RegisterSymbolX( 1, "😴", "Sleeping Face`n:sleeping:"                )
//    super.RegisterSymbolX( 1, "🥱", "Yawning Face`n:yawning:"                  )
//    super.RegisterSymbolX( 1, "😷", "Face with Medical Mask`n:mask:"           )
//    super.RegisterSymbolX( 1, "🤒", "Face with Thermometer`n:thermometer:"     )
//    super.RegisterSymbolX( 1, "🤕", "Face with Head Bandage`n:head_bandage:"   )
//    super.RegisterSymbolX( 1, "🤢", "Nauseated Face`n:nauseated:"              )
//    super.RegisterSymbolX( 1, "🤮", "Face Vomiting`n:vomiting:"                )
//    super.RegisterSymbolX( 1, "🤧", "Sneezing Face`n:sneezing:"                )

//    super.RegisterSymbolX( 1, "🥵", "Hot Face`n:hot:"                          )
//    super.RegisterSymbolX( 1, "🥶", "Cold Face`n:cold:"                        )
//    super.RegisterSymbolX( 1, "🥴", "Woozy Face`n:woozy:"                      )
//    super.RegisterSymbolX( 1, "😵", "Dizzy Face`n:dizzy_face:"                 )
//    super.RegisterSymbolX( 1, "🤯", "Exploding Head`n:exploding_head:"         )
//    super.RegisterSymbolX( 1, "🤠", "Cowboy Hat Face`n:cowboy:"                )
//    super.RegisterSymbolX( 1, "🥳", "Partying Face`n:partying:"                )
//    super.RegisterSymbolX( 1, "🥸", "Disguised Face`n:disguised:"              )
//    super.RegisterSymbolX( 1, "🤓", "Nerd Face`n:nerd:"                        )
//    super.RegisterSymbolX( 1, "🧐", "Face with Monocle`n:monocle:"             )
//    super.RegisterSymbolX( 1, "😎", "Smiling with Sunglasses`n:sunglasses2:"   )
//    super.RegisterSymbolX( 1, "🥹", "Face Holding Back Tears`n:holding_tears:" )

//    super.RegisterSymbolX( 1, "😕", "Confused Face`n:confused:"                    )
//    super.RegisterSymbolX( 1, "😟", "Worried Face`n:worried:"                      )
//    super.RegisterSymbolX( 1, "🙁", "Slightly Frowning Face`n:slightly_frowning:"  )
//    super.RegisterSymbolX( 1, "☹️", "Frowning Face`n:frowning:"                    )
//    super.RegisterSymbolX( 1, "😮", "Face with Open Mouth`n:open_mouth:"           )
//    super.RegisterSymbolX( 1, "😯", "Hushed Face`n:hushed:"                        )
//    super.RegisterSymbolX( 1, "😲", "Astonished Face`n:astonished:"               )
//    super.RegisterSymbolX( 1, "😳", "Flushed Face`n:flushed:"                      )
//    super.RegisterSymbolX( 1, "🥺", "Pleading Face`n:pleading:"                    )
//    super.RegisterSymbolX( 1, "😦", "Frowning Open Mouth`n:frowning_open:"         )
//    super.RegisterSymbolX( 1, "😧", "Anguished Face`n:anguished:"                  )
//    super.RegisterSymbolX( 1, "😨", "Fearful Face`n:fearful:"                      )

//    super.RegisterSymbolX( 1, "😰", "Anxious Face with Sweat`n:cold_sweat:"    )
//    super.RegisterSymbolX( 1, "😥", "Sad but Relieved`n:disappointed_relieved:" )
//    super.RegisterSymbolX( 1, "😢", "Crying Face`n:cry:"                        )
//    super.RegisterSymbolX( 1, "😭", "Loudly Crying Face`n:sob:"                 )
//    super.RegisterSymbolX( 1, "😱", "Screaming in Fear`n:scream:"               )
//    super.RegisterSymbolX( 1, "😖", "Confounded Face`n:confounded:"             )
//    super.RegisterSymbolX( 1, "😣", "Persevering Face`n:persevering:"           )
//    super.RegisterSymbolX( 1, "😞", "Disappointed Face`n:disappointed:"         )
//    super.RegisterSymbolX( 1, "😓", "Downcast with Sweat`n:sweat:"              )
//    super.RegisterSymbolX( 1, "😩", "Weary Face`n:weary:"                       )
//    super.RegisterSymbolX( 1, "😫", "Tired Face`n:tired_face:"                  )
//    super.RegisterSymbolX( 1, "😤", "Face with Steam from Nose`n:triumph:"      )

//    super.RegisterSymbolX( 1, "😡", "Pouting Face`n:rage:"                              )
//    super.RegisterSymbolX( 1, "😠", "Angry Face`n:angry:"                               )
//    super.RegisterSymbolX( 1, "🤬", "Face with Symbols on Mouth`n:symbols_on_mouth:"    )
//    super.RegisterSymbolX( 1, "😈", "Smiling Face with Horns`n:smiling_imp:"            )
//    super.RegisterSymbolX( 1, "👿", "Angry Face with Horns`n:imp:"                      )
//    super.RegisterSymbolX( 1, "💀", "Skull`n:skull:"                                     )
//    super.RegisterSymbolX( 1, "☠️", "Skull and Crossbones`n:skull_crossbones:"          )
//    super.RegisterSymbolX( 1, "💩", "Pile of Poo`n:poop:"                               )
//    super.RegisterSymbolX( 1, "🤡", "Clown Face`n:clown:"                               )
//    super.RegisterSymbolX( 1, "👹", "Ogre`n:japanese_ogre:"                             )
//    super.RegisterSymbolX( 1, "👺", "Goblin`n:japanese_goblin:"                         )
//    super.RegisterSymbolX( 1, "👻", "Ghost`n:ghost:"                                     )

//    super.RegisterSymbolX( 1, "👽", "Alien`n:alien:"               )
//    super.RegisterSymbolX( 1, "👾", "Alien Monster`n:space_invader:" )
//    super.RegisterSymbolX( 1, "🤖", "Robot`n:robot:"               )
//    super.RegisterSymbolX( 1, "🎃", "Jack-o-Lantern`n:jack_o_lantern:" )
  }

  // ─── Smileys & Emotion — Hearts & Love ────────────────────────
  private void RegisterHeartsAndEmotion()
  {
    RegisterSymbolX( 1, "❤️",  "Red Heart\n:heart:"                );
    RegisterSymbolX( 1, "🧡",  "Orange Heart\n:orange_heart:"      );
    RegisterSymbolX( 1, "💛",  "Yellow Heart\n:yellow_heart:"      );
    RegisterSymbolX( 1, "💚",  "Green Heart\n:green_heart:"        );
    RegisterSymbolX( 1, "💙",  "Blue Heart\n:blue_heart:"          );
    RegisterSymbolX( 1, "💜",  "Purple Heart\n:purple_heart:"      );
    RegisterSymbolX( 1, "🖤",  "Black Heart\n:black_heart:"        );
    RegisterSymbolX( 1, "🤍",  "White Heart\n:white_heart:"        );
    RegisterSymbolX( 1, "🤎",  "Brown Heart\n:brown_heart:"        );
    RegisterSymbolX( 1, "💔",  "Broken Heart\n:broken_heart:"      );
    RegisterSymbolX( 1, "❣️",  "Heart Exclamation\n:heart_exclamation:" );
    RegisterSymbolX( 1, "💕",  "Two Hearts\n:two_hearts:"          );

    RegisterSymbolX( 1, "💞",  "Revolving Hearts\n:revolving_hearts:"  );
    RegisterSymbolX( 1, "💓",  "Beating Heart\n:heartbeat:"            );
    RegisterSymbolX( 1, "💗",  "Growing Heart\n:heartpulse:"           );
    RegisterSymbolX( 1, "💖",  "Sparkling Heart\n:sparkling_heart:"    );
    RegisterSymbolX( 1, "💘",  "Heart with Arrow\n:cupid:"             );
    RegisterSymbolX( 1, "💝",  "Heart with Ribbon\n:gift_heart:"       );
    RegisterSymbolX( 1, "💟",  "Heart Decoration\n:heart_decoration:"  );
    RegisterSymbolX( 1, "💋",  "Kiss Mark\n:kiss:"                     );
    RegisterSymbolX( 1, "💌",  "Love Letter\n:love_letter:"            );
    RegisterSymbolX( 1, "💯",  "Hundred Points\n:100:"                 );
    RegisterSymbolX( 1, "💢",  "Anger Symbol\n:anger:"                 );
    RegisterSymbolX( 1, "💥",  "Collision\n:boom:"                     );

    RegisterSymbolX( 1, "💫", "Dizzy\n:dizzy:"          );
    RegisterSymbolX( 1, "💦", "Sweat Droplets\n:sweat_drops:" );
    RegisterSymbolX( 1, "💨", "Dashing Away\n:dash:"    );
    RegisterSymbolX( 1, "💬", "Speech Balloon\n:speech_balloon:" );
    RegisterSymbolX( 1, "💭", "Thought Balloon\n:thought_balloon:" );
    RegisterSymbolX( 1, "💤", "ZZZ\n:zzz:"              );
    RegisterSymbolX( 1, "✨", "Sparkles\n:sparkles:"    );
    RegisterSymbolX( 1, "🎵", "Musical Note\n:musical_note:" );
    RegisterSymbolX( 1, "🎶", "Musical Notes\n:notes:"   );
    RegisterSymbolX( 1, "🔥", "Fire\n:fire:"             );
    RegisterSymbolX( 1, "👁️", "Eye\n:eye:"               );
    RegisterSymbolX( 1, "💡", "Light Bulb\n:bulb:"      );
  }

  // ─── People & Body — Gestures ──────────────────────────────────
  private void RegisterGestures()
  {
    RegisterSymbolX( 1, "👋", "Waving Hand\n:wave:" );
    RegisterSymbolX( 1, "🤚", "Raised Back Of Hand\n:raised_back_of_hand:" );
    RegisterSymbolX( 1, "🖐️", "Hand With Fingers Splayed\n:hand_splayed:" );
    RegisterSymbolX( 1, "✋", "Raised Hand\n:raised_hand:" );
    RegisterSymbolX( 1, "🖖", "Vulcan Salute\n:vulcan_salute:" );
    RegisterSymbolX( 1, "🫱", "Rightwards Hand\n:rightwards_hand:" );
    RegisterSymbolX( 1, "🫲", "Leftwards Hand\n:leftwards_hand:" );
    RegisterSymbolX( 1, "🫳", "Palm Down Hand\n:palm_down_hand:" );
    RegisterSymbolX( 1, "🫴", "Palm Up Hand\n:palm_up_hand:" );
    RegisterSymbolX( 1, "🫷", "Leftwards Pushing Hand\n:leftwards_pushing_hand:" );
    RegisterSymbolX( 1, "🫸", "Rightwards Pushing Hand\n:rightwards_pushing_hand:" );
    RegisterSymbolX( 1, "👌", "Ok Hand\n:ok_hand:" );

    RegisterSymbolX( 1, "🤌", "Pinched Fingers\n:pinched_fingers:" );
    RegisterSymbolX( 1, "🤏", "Pinching Hand\n:pinching_hand:" );
    RegisterSymbolX( 1, "✌️", "Victory Hand\n:victory_hand:" );
    RegisterSymbolX( 1, "🤞", "Crossed Fingers\n:crossed_fingers:" );
    RegisterSymbolX( 1, "🫰", "Hand With Index Finger And Thumb Crossed\n:hand_with_index_finger_and_thumb_crossed:" );
    RegisterSymbolX( 1, "🤟", "Love-You Gesture\n:love_you_gesture:" );
    RegisterSymbolX( 1, "🤘", "Sign Of The Horns\n:metal:" );
    RegisterSymbolX( 1, "🤙", "Call Me Hand\n:call_me_hand:" );
    RegisterSymbolX( 1, "👈", "Backhand Index Pointing Left\n:point_left:" );
    RegisterSymbolX( 1, "👉", "Backhand Index Pointing Right\n:point_right:" );
    RegisterSymbolX( 1, "👆", "Backhand Index Pointing Up\n:point_up_2:" );
    RegisterSymbolX( 1, "🖕", "Middle Finger\n:middle_finger:" );

    RegisterSymbolX( 1, "👇", "Backhand Index Pointing Down\n:point_down:" );
    RegisterSymbolX( 1, "☝️", "Index Pointing Up\n:point_up:" );
    RegisterSymbolX( 1, "🫵", "Index Pointing At The Viewer\n:index_pointing_at_the_viewer:" );
    RegisterSymbolX( 1, "👍", "Thumbs Up\n:+1:" );
    RegisterSymbolX( 1, "👎", "Thumbs Down\n:-1:" );
    RegisterSymbolX( 1, "✊", "Raised Fist\n:fist:" );
    RegisterSymbolX( 1, "👊", "Oncoming Fist\n:punch:" );
    RegisterSymbolX( 1, "🤛", "Left-Facing Fist\n:left_facing_fist:" );
    RegisterSymbolX( 1, "🤜", "Right-Facing Fist\n:right_facing_fist:" );
    RegisterSymbolX( 1, "👏", "Clapping Hands\n:clap:" );
    RegisterSymbolX( 1, "🙌", "Raising Hands\n:raised_hands:" );
    RegisterSymbolX( 1, "🫶", "Heart Hands\n:heart_hands:" );

    RegisterSymbolX( 1, "👐", "Open Hands\n:open_hands:" );
    RegisterSymbolX( 1, "🤲", "Palms Up Together\n:palms_up_together:" );
    RegisterSymbolX( 1, "🤝", "Handshake\n:handshake:" );
    RegisterSymbolX( 1, "🙏", "Folded Hands\n:pray:" );
    RegisterSymbolX( 1, "✍️", "Writing Hand\n:writing_hand:" );
    RegisterSymbolX( 1, "💅", "Nail Polish\n:nail_polish:" );
    RegisterSymbolX( 1, "🤳", "Selfie\n:selfie:" );
    RegisterSymbolX( 1, "💪", "Flexed Biceps\n:muscle:" );
    RegisterSymbolX( 1, "🦾", "Mechanical Arm\n:mechanical_arm:" );
    RegisterSymbolX( 1, "🦿", "Mechanical Leg\n:mechanical_leg:" );
    RegisterSymbolX( 1, "🦵", "Leg\n:leg:" );
    RegisterSymbolX( 1, "🦶", "Foot\n:foot:" );

    RegisterSymbolX( 1, "👂", "Ear\n:ear:" );
    RegisterSymbolX( 1, "🦻", "Ear With Hearing Aid\n:ear_with_hearing_aid:" );
    RegisterSymbolX( 1, "👃", "Nose\n:nose:" );
    RegisterSymbolX( 1, "🧠", "Brain\n:brain:" );
    RegisterSymbolX( 1, "🫀", "Anatomical Heart\n:anatomical_heart:" );
    RegisterSymbolX( 1, "🫁", "Lungs\n:lungs:" );
    RegisterSymbolX( 1, "🦷", "Tooth\n:tooth:" );
    RegisterSymbolX( 1, "🦴", "Bone\n:bone:" );
    RegisterSymbolX( 1, "👀", "Eyes\n:eyes:" );
    RegisterSymbolX( 1, "👁️", "Eye\n:eye:" );
    RegisterSymbolX( 1, "👅", "Tongue\n:tongue:" );
    RegisterSymbolX( 1, "👄", "Mouth\n:mouth:" );

    RegisterSymbolX( 1, "🫦", "Biting Lip\n:biting_lip:" );
  }

  // ─── People & Body — People ────────────────────────────────────
  private void RegisterPeople()
  {
    /*;
    RegisterSymbolX( 1, "👶", "Baby\n:baby:" );
    RegisterSymbolX( 1, "🧒", "Child\n:child:" );
    RegisterSymbolX( 1, "👦", "Boy\n:boy:" );
    RegisterSymbolX( 1, "👧", "Girl\n:girl:" );
    RegisterSymbolX( 1, "🧑", "Person\n:person:" );
    RegisterSymbolX( 1, "👱", "Person Blond Hair\n:blond_haired_person:" );
    RegisterSymbolX( 1, "👨", "Man\n:man:" );
    RegisterSymbolX( 1, "🧔", "Person Beard\n:bearded_person:" );
    RegisterSymbolX( 1, "🧔‍♂️", "Man Beard\n:bearded_man:" );
    RegisterSymbolX( 1, "🧔‍♀️", "Woman Beard\n:bearded_woman:" );
    RegisterSymbolX( 1, "👨‍🦰", "Man Red Hair\n:red_haired_man:" );
    RegisterSymbolX( 1, "👨‍🦱", "Man Curly Hair\n:curly_haired_man:" );

    RegisterSymbolX( 1, "👨‍🦳", "Man White Hair\n:white_haired_man:" );
    RegisterSymbolX( 1, "👨‍🦲", "Man Bald\n:bald_man:" );
    RegisterSymbolX( 1, "👩", "Woman\n:woman:" );
    RegisterSymbolX( 1, "👩‍🦰", "Woman Red Hair\n:red_haired_woman:" );
    RegisterSymbolX( 1, "🧑‍🦰", "Person Red Hair\n:red_haired_person:" );
    RegisterSymbolX( 1, "👩‍🦱", "Woman Curly Hair\n:curly_haired_woman:" );
    RegisterSymbolX( 1, "🧑‍🦱", "Person Curly Hair\n:curly_haired_person:" );
    RegisterSymbolX( 1, "👩‍🦳", "Woman White Hair\n:white_haired_woman:" );
    RegisterSymbolX( 1, "🧑‍🦳", "Person White Hair\n:white_haired_person:" );
    RegisterSymbolX( 1, "👩‍🦲", "Woman Bald\n:bald_woman:" );
    RegisterSymbolX( 1, "🧑‍🦲", "Person Bald\n:bald_person:" );
    RegisterSymbolX( 1, "👱‍♀️", "Woman Blond Hair\n:blond_haired_woman:" );

    RegisterSymbolX( 1, "👱‍♂️", "Man Blond Hair\n:blond_haired_man:" );
    RegisterSymbolX( 1, "🧓", "Older Person\n:older_adult:" );
    RegisterSymbolX( 1, "👴", "Old Man\n:older_man:" );
    RegisterSymbolX( 1, "👵", "Old Woman\n:older_woman:" );
    RegisterSymbolX( 1, "🙍", "Person Frowning\n:frowning_person:" );
    RegisterSymbolX( 1, "🙍‍♂️", "Man Frowning\n:frowning_man:" );
    RegisterSymbolX( 1, "🙍‍♀️", "Woman Frowning\n:frowning_woman:" );
    RegisterSymbolX( 1, "🙎", "Person Pouting\n:pouting_person:" );
    RegisterSymbolX( 1, "🙎‍♂️", "Man Pouting\n:pouting_man:" );
    RegisterSymbolX( 1, "🙎‍♀️", "Woman Pouting\n:pouting_woman:" );
    RegisterSymbolX( 1, "🙅", "Person Gesturing No\n:no_good:" );
    RegisterSymbolX( 1, "🙅‍♂️", "Man Gesturing No\n:no_good_man:" );

    RegisterSymbolX( 1, "🙅‍♀️", "Woman Gesturing No\n:no_good_woman:" );
    RegisterSymbolX( 1, "🙆", "Person Gesturing Ok\n:ok_person:" );
    RegisterSymbolX( 1, "🙆‍♂️", "Man Gesturing Ok\n:ok_man:" );
    RegisterSymbolX( 1, "🙆‍♀️", "Woman Gesturing Ok\n:ok_woman:" );
    RegisterSymbolX( 1, "💁", "Person Tipping Hand\n:information_desk_person:" );
    RegisterSymbolX( 1, "💁‍♂️", "Man Tipping Hand\n:information_desk_man:" );
    RegisterSymbolX( 1, "💁‍♀️", "Woman Tipping Hand\n:information_desk_woman:" );
    RegisterSymbolX( 1, "🙋", "Person Raising Hand\n:raising_hand:" );
    RegisterSymbolX( 1, "🙋‍♂️", "Man Raising Hand\n:raising_hand_man:" );
    RegisterSymbolX( 1, "🙋‍♀️", "Woman Raising Hand\n:raising_hand_woman:" );
    RegisterSymbolX( 1, "🧏", "Deaf Person\n:deaf_person:" );
    RegisterSymbolX( 1, "🧏‍♂️", "Deaf Man\n:deaf_man:" );

    RegisterSymbolX( 1, "🧏‍♀️", "Deaf Woman\n:deaf_woman:" );
    RegisterSymbolX( 1, "🙇", "Person Bowing\n:bow:" );
    RegisterSymbolX( 1, "🙇‍♂️", "Man Bowing\n:bowing_man:" );
    RegisterSymbolX( 1, "🙇‍♀️", "Woman Bowing\n:bowing_woman:" );
    RegisterSymbolX( 1, "🤦", "Person Facepalming\n:facepalm:" );
    RegisterSymbolX( 1, "🤦‍♂️", "Man Facepalming\n:man_facepalming:" );
    RegisterSymbolX( 1, "🤦‍♀️", "Woman Facepalming\n:woman_facepalming:" );
    RegisterSymbolX( 1, "🤷", "Person Shrugging\n:shrug:" );
    RegisterSymbolX( 1, "🤷‍♂️", "Man Shrugging\n:man_shrugging:" );
    RegisterSymbolX( 1, "🤷‍♀️", "Woman Shrugging\n:woman_shrugging:" );
    RegisterSymbolX( 1, "🧑‍⚕️", "Health Worker\n:health_worker:" );
    RegisterSymbolX( 1, "👨‍⚕️", "Man Health Worker\n:man_health_worker:" );

    RegisterSymbolX( 1, "👩‍⚕️", "Woman Health Worker\n:woman_health_worker:" );
    RegisterSymbolX( 1, "🧑‍🎓", "Student\n:student:" );
    RegisterSymbolX( 1, "👨‍🎓", "Man Student\n:man_student:" );
    RegisterSymbolX( 1, "👩‍🎓", "Woman Student\n:woman_student:" );
    RegisterSymbolX( 1, "🧑‍🏫", "Teacher\n:teacher:" );
    RegisterSymbolX( 1, "👨‍🏫", "Man Teacher\n:man_teacher:" );
    RegisterSymbolX( 1, "👩‍🏫", "Woman Teacher\n:woman_teacher:" );
    RegisterSymbolX( 1, "🧑‍⚖️", "Judge\n:judge:" );
    RegisterSymbolX( 1, "👨‍⚖️", "Man Judge\n:man_judge:" );
    RegisterSymbolX( 1, "👩‍⚖️", "Woman Judge\n:woman_judge:" );
    RegisterSymbolX( 1, "🧑‍🌾", "Farmer\n:farmer:" );
    RegisterSymbolX( 1, "👨‍🌾", "Man Farmer\n:man_farmer:" );

    RegisterSymbolX( 1, "👩‍🌾", "Woman Farmer\n:woman_farmer:" );
    RegisterSymbolX( 1, "🧑‍🍳", "Cook\n:cook:" );
    RegisterSymbolX( 1, "👨‍🍳", "Man Cook\n:man_cook:" );
    RegisterSymbolX( 1, "👩‍🍳", "Woman Cook\n:woman_cook:" );
    RegisterSymbolX( 1, "🧑‍🔧", "Mechanic\n:mechanic:" );
    RegisterSymbolX( 1, "👨‍🔧", "Man Mechanic\n:man_mechanic:" );
    RegisterSymbolX( 1, "👩‍🔧", "Woman Mechanic\n:woman_mechanic:" );
    RegisterSymbolX( 1, "🧑‍🏭", "Factory Worker\n:factory_worker:" );
    RegisterSymbolX( 1, "👨‍🏭", "Man Factory Worker\n:man_factory_worker:" );
    RegisterSymbolX( 1, "👩‍🏭", "Woman Factory Worker\n:woman_factory_worker:" );
    RegisterSymbolX( 1, "🧑‍💼", "Office Worker\n:office_worker:" );
    RegisterSymbolX( 1, "👨‍💼", "Man Office Worker\n:man_office_worker:" );

    RegisterSymbolX( 1, "👩‍💼", "Woman Office Worker\n:woman_office_worker:" );
    RegisterSymbolX( 1, "🧑‍🔬", "Scientist\n:scientist:" );
    RegisterSymbolX( 1, "👨‍🔬", "Man Scientist\n:man_scientist:" );
    RegisterSymbolX( 1, "👩‍🔬", "Woman Scientist\n:woman_scientist:" );
    RegisterSymbolX( 1, "🧑‍💻", "Technologist\n:technologist:" );
    RegisterSymbolX( 1, "👨‍💻", "Man Technologist\n:man_technologist:" );
    RegisterSymbolX( 1, "👩‍💻", "Woman Technologist\n:woman_technologist:" );
    RegisterSymbolX( 1, "🧑‍🎤", "Singer\n:singer:" );
    RegisterSymbolX( 1, "👨‍🎤", "Man Singer\n:man_singer:" );
    RegisterSymbolX( 1, "👩‍🎤", "Woman Singer\n:woman_singer:" );
    RegisterSymbolX( 1, "🧑‍🎨", "Artist\n:artist:" );
    RegisterSymbolX( 1, "👨‍🎨", "Man Artist\n:man_artist:" );
    RegisterSymbolX( 1, "👩‍🎨", "Woman Artist\n:woman_artist:" );

    RegisterSymbolX( 1, "🧑‍✈️", "Pilot\n:pilot:" );
    RegisterSymbolX( 1, "👨‍✈️", "Man Pilot\n:man_pilot:" );
    RegisterSymbolX( 1, "👩‍✈️", "Woman Pilot\n:woman_pilot:" );
    RegisterSymbolX( 1, "🧑‍🚀", "Astronaut\n:astronaut:" );
    RegisterSymbolX( 1, "👨‍🚀", "Man Astronaut\n:man_astronaut:" );
    RegisterSymbolX( 1, "👩‍🚀", "Woman Astronaut\n:woman_astronaut:" );
    RegisterSymbolX( 1, "🧑‍🚒", "Firefighter\n:firefighter:" );
    RegisterSymbolX( 1, "👨‍🚒", "Man Firefighter\n:man_firefighter:" );
    RegisterSymbolX( 1, "👩‍🚒", "Woman Firefighter\n:woman_firefighter:" );
    RegisterSymbolX( 1, "👮", "Police Officer\n:police_officer:" );
    RegisterSymbolX( 1, "👮‍♂️", "Man Police Officer\n:man_police_officer:" );
    RegisterSymbolX( 1, "👮‍♀️", "Woman Police Officer\n:woman_police_officer:" );

    RegisterSymbolX( 1, "🕵️", "Detective\n:detective:" );
    RegisterSymbolX( 1, "🕵️‍♂️", "Man Detective\n:man_detective:" );
    RegisterSymbolX( 1, "🕵️‍♀️", "Woman Detective\n:woman_detective:" );
    RegisterSymbolX( 1, "💂", "Guard\n:guard:" );
    RegisterSymbolX( 1, "💂‍♂️", "Man Guard\n:man_guard:" );
    RegisterSymbolX( 1, "💂‍♀️", "Woman Guard\n:woman_guard:" );
    RegisterSymbolX( 1, "🥷", "Ninja\n:ninja:" );
    RegisterSymbolX( 1, "👷", "Construction Worker\n:construction_worker:" );
    RegisterSymbolX( 1, "👷‍♂️", "Man Construction Worker\n:man_construction_worker:" );
    RegisterSymbolX( 1, "👷‍♀️", "Woman Construction Worker\n:woman_construction_worker:" );
    RegisterSymbolX( 1, "🫅", "Person With Crown\n:person_with_crown:" );
    RegisterSymbolX( 1, "🤴", "Prince\n:prince:" );

    RegisterSymbolX( 1, "👸", "Princess\n:princess:" );
    RegisterSymbolX( 1, "👳", "Person Wearing Turban\n:person_with_turban:" );
    RegisterSymbolX( 1, "👳‍♂️", "Man Wearing Turban\n:man_with_turban:" );
    RegisterSymbolX( 1, "👳‍♀️", "Woman Wearing Turban\n:woman_with_turban:" );
    RegisterSymbolX( 1, "👲", "Person With Skullcap\n:man_with_gua_pi_mao:" );
    RegisterSymbolX( 1, "🧕", "Woman With Headscarf\n:woman_with_headscarf:" );
    RegisterSymbolX( 1, "🤵", "Person In Tuxedo\n:person_in_tuxedo:" );
    RegisterSymbolX( 1, "🤵‍♂️", "Man In Tuxedo\n:man_in_tuxedo:" );
    RegisterSymbolX( 1, "🤵‍♀️", "Woman In Tuxedo\n:woman_in_tuxedo:" );
    RegisterSymbolX( 1, "👰", "Person With Veil\n:person_with_veil:" );
    RegisterSymbolX( 1, "👰‍♀️", "Woman With Veil\n:bride_with_veil:" );
    RegisterSymbolX( 1, "👰‍♂️", "Man With Veil\n:man_with_veil:" );

    RegisterSymbolX( 1, "🤰", "Pregnant Woman\n:pregnant_woman:" );
    RegisterSymbolX( 1, "🫃", "Pregnant Man\n:pregnant_man:" );
    RegisterSymbolX( 1, "🫄", "Pregnant Person\n:pregnant_person:" );
    RegisterSymbolX( 1, "🤱", "Breast-Feeding\n:breast_feeding:" );
    RegisterSymbolX( 1, "👩‍🍼", "Woman Feeding Baby\n:woman_feeding_baby:" );
    RegisterSymbolX( 1, "👨‍🍼", "Man Feeding Baby\n:man_feeding_baby:" );
    RegisterSymbolX( 1, "🧑‍🍼", "Person Feeding Baby\n:person_feeding_baby:" );
    RegisterSymbolX( 1, "👼", "Baby Angel\n:baby_angel:" );
    RegisterSymbolX( 1, "🎅", "Santa Claus\n:santa:" );
    RegisterSymbolX( 1, "🤶", "Mrs Claus\n:mrs_claus:" );
    RegisterSymbolX( 1, "🧑‍🎄", "Mx Claus\n:mx_claus:" );
    RegisterSymbolX( 1, "🦸", "Superhero\n:superhero:" );

    RegisterSymbolX( 1, "🦸‍♂️", "Man Superhero\n:man_superhero:" );
    RegisterSymbolX( 1, "🦸‍♀️", "Woman Superhero\n:woman_superhero:" );
    RegisterSymbolX( 1, "🦹", "Supervillain\n:supervillain:" );
    RegisterSymbolX( 1, "🦹‍♂️", "Man Supervillain\n:man_supervillain:" );
    RegisterSymbolX( 1, "🦹‍♀️", "Woman Supervillain\n:woman_supervillain:" );
    RegisterSymbolX( 1, "🧙", "Mage\n:mage:" );
    RegisterSymbolX( 1, "🧙‍♂️", "Man Mage\n:man_mage:" );
    RegisterSymbolX( 1, "🧙‍♀️", "Woman Mage\n:woman_mage:" );
    RegisterSymbolX( 1, "🧚", "Fairy\n:fairy:" );
    RegisterSymbolX( 1, "🧚‍♂️", "Man Fairy\n:man_fairy:" );
    RegisterSymbolX( 1, "🧚‍♀️", "Woman Fairy\n:woman_fairy:" );
    RegisterSymbolX( 1, "🧛", "Vampire\n:vampire:" );

    RegisterSymbolX( 1, "🧛‍♂️", "Man Vampire\n:man_vampire:" );
    RegisterSymbolX( 1, "🧛‍♀️", "Woman Vampire\n:woman_vampire:" );
    RegisterSymbolX( 1, "🧜", "Merperson\n:merperson:" );
    RegisterSymbolX( 1, "🧜‍♂️", "Merman\n:merman:" );
    RegisterSymbolX( 1, "🧜‍♀️", "Mermaid\n:mermaid:" );
    RegisterSymbolX( 1, "🧝", "Elf\n:elf:" );
    RegisterSymbolX( 1, "🧝‍♂️", "Man Elf\n:man_elf:" );
    RegisterSymbolX( 1, "🧝‍♀️", "Woman Elf\n:woman_elf:" );
    RegisterSymbolX( 1, "🧞", "Genie\n:genie:" );
    RegisterSymbolX( 1, "🧞‍♂️", "Man Genie\n:man_genie:" );
    RegisterSymbolX( 1, "🧞‍♀️", "Woman Genie\n:woman_genie:" );
    RegisterSymbolX( 1, "🧟", "Zombie\n:zombie:" );

    RegisterSymbolX( 1, "🧟‍♂️", "Man Zombie\n:man_zombie:" );
    RegisterSymbolX( 1, "🧟‍♀️", "Woman Zombie\n:woman_zombie:" );
    RegisterSymbolX( 1, "💆", "Person Getting Massage\n:massage:" );
    RegisterSymbolX( 1, "💆‍♂️", "Man Getting Massage\n:massage_man:" );
    RegisterSymbolX( 1, "💆‍♀️", "Woman Getting Massage\n:massage_woman:" );
    RegisterSymbolX( 1, "💇", "Person Getting Haircut\n:haircut:" );
    RegisterSymbolX( 1, "💇‍♂️", "Man Getting Haircut\n:haircut_man:" );
    RegisterSymbolX( 1, "💇‍♀️", "Woman Getting Haircut\n:haircut_woman:" );
    RegisterSymbolX( 1, "🚶", "Person Walking\n:walking:" );
    RegisterSymbolX( 1, "🚶‍♂️", "Man Walking\n:walking_man:" );
    RegisterSymbolX( 1, "🚶‍♀️", "Woman Walking\n:walking_woman:" );
    RegisterSymbolX( 1, "🧍", "Person Standing\n:standing_person:" );

    RegisterSymbolX( 1, "🧍‍♂️", "Man Standing\n:standing_man:" );
    RegisterSymbolX( 1, "🧍‍♀️", "Woman Standing\n:standing_woman:" );
    RegisterSymbolX( 1, "🧎", "Person Kneeling\n:kneeling_person:" );
    RegisterSymbolX( 1, "🧎‍♂️", "Man Kneeling\n:kneeling_man:" );
    RegisterSymbolX( 1, "🧎‍♀️", "Woman Kneeling\n:kneeling_woman:" );
    RegisterSymbolX( 1, "🧑‍🦯", "Person With White Cane\n:person_with_white_cane:" );
    RegisterSymbolX( 1, "👨‍🦯", "Man With White Cane\n:man_with_white_cane:" );
    RegisterSymbolX( 1, "👩‍🦯", "Woman With White Cane\n:woman_with_white_cane:" );
    RegisterSymbolX( 1, "🧑‍🦼", "Person In Motorized Wheelchair\n:person_in_motorized_wheelchair:" );
    RegisterSymbolX( 1, "👨‍🦼", "Man In Motorized Wheelchair\n:man_in_motorized_wheelchair:" );
    RegisterSymbolX( 1, "👩‍🦼", "Woman In Motorized Wheelchair\n:woman_in_motorized_wheelchair:" );
    RegisterSymbolX( 1, "🧑‍🦽", "Person In Manual Wheelchair\n:person_in_manual_wheelchair:" );

    RegisterSymbolX( 1, "👨‍🦽", "Man In Manual Wheelchair\n:man_in_manual_wheelchair:" );
    RegisterSymbolX( 1, "👩‍🦽", "Woman In Manual Wheelchair\n:woman_in_manual_wheelchair:" );
    RegisterSymbolX( 1, "🏃", "Person Running\n:runner:" );
    RegisterSymbolX( 1, "🏃‍♂️", "Man Running\n:running_man:" );
    RegisterSymbolX( 1, "🏃‍♀️", "Woman Running\n:running_woman:" );
    RegisterSymbolX( 1, "💃", "Woman Dancing\n:dancer:" );
    RegisterSymbolX( 1, "🕺", "Man Dancing\n:man_dancing:" );
    RegisterSymbolX( 1, "🕴️", "Person In Suit Levitating\n:levitating:" );
    RegisterSymbolX( 1, "👯", "People With Bunny Ears\n:people_with_bunny_ears:" );
    RegisterSymbolX( 1, "👯‍♂️", "Men With Bunny Ears\n:men_with_bunny_ears_partying:" );
    RegisterSymbolX( 1, "👯‍♀️", "Women With Bunny Ears\n:women_with_bunny_ears_partying:" );
    RegisterSymbolX( 1, "🧖", "Person In Steamy Room\n:person_in_steamy_room:" );

    RegisterSymbolX( 1, "🧖‍♂️", "Man In Steamy Room\n:man_in_steamy_room:" );
    RegisterSymbolX( 1, "🧖‍♀️", "Woman In Steamy Room\n:woman_in_steamy_room:" );
    RegisterSymbolX( 1, "🧗", "Person Climbing\n:climbing:" );
    RegisterSymbolX( 1, "🧗‍♂️", "Man Climbing\n:man_climbing:" );
    RegisterSymbolX( 1, "🧗‍♀️", "Woman Climbing\n:woman_climbing:" );
    RegisterSymbolX( 1, "🤺", "Person Fencing\n:fencer:" );
    RegisterSymbolX( 1, "🏇", "Horse Racing\n:horse_racing:" );
    RegisterSymbolX( 1, "⛷️", "Skier\n:skier:" );
    RegisterSymbolX( 1, "🏂", "Snowboarder\n:snowboarder:" );
    RegisterSymbolX( 1, "🏌️", "Person Golfing\n:golfing:" );
    RegisterSymbolX( 1, "🏌️‍♂️", "Man Golfing\n:golfing_man:" );
    RegisterSymbolX( 1, "🏌️‍♀️", "Woman Golfing\n:golfing_woman:" );

    RegisterSymbolX( 1, "🏄", "Person Surfing\n:surfer:" );
    RegisterSymbolX( 1, "🏄‍♂️", "Man Surfing\n:surfing_man:" );
    RegisterSymbolX( 1, "🏄‍♀️", "Woman Surfing\n:surfing_woman:" );
    RegisterSymbolX( 1, "🚣", "Person Rowing Boat\n:rowing_person:" );
    RegisterSymbolX( 1, "🚣‍♂️", "Man Rowing Boat\n:rowing_man:" );
    RegisterSymbolX( 1, "🚣‍♀️", "Woman Rowing Boat\n:rowing_woman:" );
    RegisterSymbolX( 1, "🏊", "Person Swimming\n:swimmer:" );
    RegisterSymbolX( 1, "🏊‍♂️", "Man Swimming\n:swimming_man:" );
    RegisterSymbolX( 1, "🏊‍♀️", "Woman Swimming\n:swimming_woman:" );
    RegisterSymbolX( 1, "⛹️", "Person Bouncing Ball\n:bouncing_ball_person:" );
    RegisterSymbolX( 1, "⛹️‍♂️", "Man Bouncing Ball\n:bouncing_ball_man:" );
    RegisterSymbolX( 1, "⛹️‍♀️", "Woman Bouncing Ball\n:bouncing_ball_woman:" );

    RegisterSymbolX( 1, "🏋️", "Person Lifting Weights\n:weight_lifting:" );
    RegisterSymbolX( 1, "🏋️‍♂️", "Man Lifting Weights\n:weight_lifting_man:" );
    RegisterSymbolX( 1, "🏋️‍♀️", "Woman Lifting Weights\n:weight_lifting_woman:" );
    RegisterSymbolX( 1, "🚴", "Person Biking\n:biking_person:" );
    RegisterSymbolX( 1, "🚴‍♂️", "Man Biking\n:biking_man:" );
    RegisterSymbolX( 1, "🚴‍♀️", "Woman Biking\n:biking_woman:" );
    RegisterSymbolX( 1, "🚵", "Person Mountain Biking\n:mountain_biking_person:" );
    RegisterSymbolX( 1, "🚵‍♂️", "Man Mountain Biking\n:mountain_biking_man:" );
    RegisterSymbolX( 1, "🚵‍♀️", "Woman Mountain Biking\n:mountain_biking_woman:" );
    RegisterSymbolX( 1, "🤸", "Person Cartwheeling\n:cartwheeling:" );
    RegisterSymbolX( 1, "🤸‍♂️", "Man Cartwheeling\n:man_cartwheeling:" );
    RegisterSymbolX( 1, "🤸‍♀️", "Woman Cartwheeling\n:woman_cartwheeling:" );

    RegisterSymbolX( 1, "🤼", "People Wrestling\n:wrestling:" );
    RegisterSymbolX( 1, "🤼‍♂️", "Men Wrestling\n:men_wrestling:" );
    RegisterSymbolX( 1, "🤼‍♀️", "Women Wrestling\n:women_wrestling:" );
    RegisterSymbolX( 1, "🤽", "Person Playing Water Polo\n:water_polo:" );
    RegisterSymbolX( 1, "🤽‍♂️", "Man Playing Water Polo\n:man_playing_water_polo:" );
    RegisterSymbolX( 1, "🤽‍♀️", "Woman Playing Water Polo\n:woman_playing_water_polo:" );
    RegisterSymbolX( 1, "🤾", "Person Playing Handball\n:handball_person:" );
    RegisterSymbolX( 1, "🤾‍♂️", "Man Playing Handball\n:man_playing_handball:" );
    RegisterSymbolX( 1, "🤾‍♀️", "Woman Playing Handball\n:woman_playing_handball:" );
    RegisterSymbolX( 1, "🤹", "Person Juggling\n:juggling:" );
    RegisterSymbolX( 1, "🤹‍♂️", "Man Juggling\n:man_juggling:" );
    RegisterSymbolX( 1, "🤹‍♀️", "Woman Juggling\n:woman_juggling:" );

    RegisterSymbolX( 1, "🧘", "Person In Lotus Position\n:lotus_position:" );
    RegisterSymbolX( 1, "🧘‍♂️", "Man In Lotus Position\n:man_in_lotus_position:" );
    RegisterSymbolX( 1, "🧘‍♀️", "Woman In Lotus Position\n:woman_in_lotus_position:" );
    RegisterSymbolX( 1, "🛀", "Person Taking Bath\n:bath:" );
    RegisterSymbolX( 1, "🛌", "Person In Bed\n:person_in_bed:" );
    RegisterSymbolX( 1, "🧑‍🤝‍🧑", "People Holding Hands\n:people_holding_hands:" );
    RegisterSymbolX( 1, "👭", "Women Holding Hands\n:two_women_holding_hands:" );
    RegisterSymbolX( 1, "👫", "Woman And Man Holding Hands\n:couple:" );
    RegisterSymbolX( 1, "👬", "Men Holding Hands\n:two_men_holding_hands:" );
    RegisterSymbolX( 1, "💏", "Kiss\n:couplekiss:" );
    RegisterSymbolX( 1, "👩‍❤️‍💋‍👨", "Kiss Woman Man\n:kiss_woman_man:" );
    RegisterSymbolX( 1, "👨‍❤️‍💋‍👨", "Kiss Man Man\n:kiss_man_man:" );

    RegisterSymbolX( 1, "👩‍❤️‍💋‍👩", "Kiss Woman Woman\n:kiss_woman_woman:" );
    RegisterSymbolX( 1, "💑", "Couple With Heart\n:couple_with_heart:" );
    RegisterSymbolX( 1, "👩‍❤️‍👨", "Couple With Heart Woman Man\n:couple_with_heart_woman_man:" );
    RegisterSymbolX( 1, "👨‍❤️‍👨", "Couple With Heart Man Man\n:couple_with_heart_man_man:" );
    RegisterSymbolX( 1, "👩‍❤️‍👩", "Couple With Heart Woman Woman\n:couple_with_heart_woman_woman:" );
    RegisterSymbolX( 1, "👪", "Family\n:family:" );
    RegisterSymbolX( 1, "👨‍👩‍👦", "Family Man Woman Boy\n:family_man_woman_boy:" );
    RegisterSymbolX( 1, "👨‍👩‍👧", "Family Man Woman Girl\n:family_man_woman_girl:" );
    RegisterSymbolX( 1, "👨‍👩‍👧‍👦", "Family Man Woman Girl Boy\n:family_man_woman_girl_boy:" );
    RegisterSymbolX( 1, "👨‍👩‍👦‍👦", "Family Man Woman Boy Boy\n:family_man_woman_boy_boy:" );
    RegisterSymbolX( 1, "👨‍👩‍👧‍👧", "Family Man Woman Girl Girl\n:family_man_woman_girl_girl:" );
    RegisterSymbolX( 1, "👨‍👨‍👦", "Family Man Man Boy\n:family_man_man_boy:" );

    RegisterSymbolX( 1, "👨‍👨‍👧", "Family Man Man Girl\n:family_man_man_girl:" );
    RegisterSymbolX( 1, "👨‍👨‍👧‍👦", "Family Man Man Girl Boy\n:family_man_man_girl_boy:" );
    RegisterSymbolX( 1, "👨‍👨‍👦‍👦", "Family Man Man Boy Boy\n:family_man_man_boy_boy:" );
    RegisterSymbolX( 1, "👨‍👨‍👧‍👧", "Family Man Man Girl Girl\n:family_man_man_girl_girl:" );
    RegisterSymbolX( 1, "👩‍👩‍👦", "Family Woman Woman Boy\n:family_woman_woman_boy:" );
    RegisterSymbolX( 1, "👩‍👩‍👧", "Family Woman Woman Girl\n:family_woman_woman_girl:" );
    RegisterSymbolX( 1, "👩‍👩‍👧‍👦", "Family Woman Woman Girl Boy\n:family_woman_woman_girl_boy:" );
    RegisterSymbolX( 1, "👩‍👩‍👦‍👦", "Family Woman Woman Boy Boy\n:family_woman_woman_boy_boy:" );
    RegisterSymbolX( 1, "👩‍👩‍👧‍👧", "Family Woman Woman Girl Girl\n:family_woman_woman_girl_girl:" );
    RegisterSymbolX( 1, "👨‍👦", "Family Man Boy\n:family_man_boy:" );
    RegisterSymbolX( 1, "👨‍👦‍👦", "Family Man Boy Boy\n:family_man_boy_boy:" );
    RegisterSymbolX( 1, "👨‍👧", "Family Man Girl\n:family_man_girl:" );

    RegisterSymbolX( 1, "👨‍👧‍👦", "Family Man Girl Boy\n:family_man_girl_boy:" );
    RegisterSymbolX( 1, "👨‍👧‍👧", "Family Man Girl Girl\n:family_man_girl_girl:" );
    RegisterSymbolX( 1, "👩‍👦", "Family Woman Boy\n:family_woman_boy:" );
    RegisterSymbolX( 1, "👩‍👦‍👦", "Family Woman Boy Boy\n:family_woman_boy_boy:" );
    RegisterSymbolX( 1, "👩‍👧", "Family Woman Girl\n:family_woman_girl:" );
    RegisterSymbolX( 1, "👩‍👧‍👦", "Family Woman Girl Boy\n:family_woman_girl_boy:" );
    RegisterSymbolX( 1, "👩‍👧‍👧", "Family Woman Girl Girl\n:family_woman_girl_girl:" );
    RegisterSymbolX( 1, "🗣️", "Speaking Head\n:speaking_head:" );
    RegisterSymbolX( 1, "👤", "Bust In Silhouette\n:bust_in_silhouette:" );
    RegisterSymbolX( 1, "👥", "Busts In Silhouette\n:busts_in_silhouette:" );
    RegisterSymbolX( 1, "🫂", "People Hugging\n:people_hugging:" );
    RegisterSymbolX( 1, "👣", "Footprints\n:footprints:" );

    RegisterSymbolX( 1, "👋🏻", "Waving Hand Light Skin Tone\n:wave_light_skin_tone:" );
    RegisterSymbolX( 1, "👋🏼", "Waving Hand Medium-Light Skin Tone\n:wave_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👋🏽", "Waving Hand Medium Skin Tone\n:wave_medium_skin_tone:" );
    RegisterSymbolX( 1, "👋🏾", "Waving Hand Medium-Dark Skin Tone\n:wave_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👋🏿", "Waving Hand Dark Skin Tone\n:wave_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤚🏻", "Raised Back Of Hand Light Skin Tone\n:raised_back_of_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🤚🏼", "Raised Back Of Hand Medium-Light Skin Tone\n:raised_back_of_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤚🏽", "Raised Back Of Hand Medium Skin Tone\n:raised_back_of_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤚🏾", "Raised Back Of Hand Medium-Dark Skin Tone\n:raised_back_of_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤚🏿", "Raised Back Of Hand Dark Skin Tone\n:raised_back_of_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🖐🏻", "Hand With Fingers Splayed Light Skin Tone\n:hand_splayed_light_skin_tone:" );
    RegisterSymbolX( 1, "🖐🏼", "Hand With Fingers Splayed Medium-Light Skin Tone\n:hand_splayed_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🖐🏽", "Hand With Fingers Splayed Medium Skin Tone\n:hand_splayed_medium_skin_tone:" );
    RegisterSymbolX( 1, "🖐🏾", "Hand With Fingers Splayed Medium-Dark Skin Tone\n:hand_splayed_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🖐🏿", "Hand With Fingers Splayed Dark Skin Tone\n:hand_splayed_dark_skin_tone:" );
    RegisterSymbolX( 1, "✋🏻", "Raised Hand Light Skin Tone\n:raised_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "✋🏼", "Raised Hand Medium-Light Skin Tone\n:raised_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "✋🏽", "Raised Hand Medium Skin Tone\n:raised_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "✋🏾", "Raised Hand Medium-Dark Skin Tone\n:raised_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "✋🏿", "Raised Hand Dark Skin Tone\n:raised_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🖖🏻", "Vulcan Salute Light Skin Tone\n:vulcan_salute_light_skin_tone:" );
    RegisterSymbolX( 1, "🖖🏼", "Vulcan Salute Medium-Light Skin Tone\n:vulcan_salute_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🖖🏽", "Vulcan Salute Medium Skin Tone\n:vulcan_salute_medium_skin_tone:" );
    RegisterSymbolX( 1, "🖖🏾", "Vulcan Salute Medium-Dark Skin Tone\n:vulcan_salute_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🖖🏿", "Vulcan Salute Dark Skin Tone\n:vulcan_salute_dark_skin_tone:" );
    RegisterSymbolX( 1, "👌🏻", "Ok Hand Light Skin Tone\n:ok_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "👌🏼", "Ok Hand Medium-Light Skin Tone\n:ok_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👌🏽", "Ok Hand Medium Skin Tone\n:ok_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "👌🏾", "Ok Hand Medium-Dark Skin Tone\n:ok_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👌🏿", "Ok Hand Dark Skin Tone\n:ok_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤌🏻", "Pinched Fingers Light Skin Tone\n:pinched_fingers_light_skin_tone:" );
    RegisterSymbolX( 1, "🤌🏼", "Pinched Fingers Medium-Light Skin Tone\n:pinched_fingers_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤌🏽", "Pinched Fingers Medium Skin Tone\n:pinched_fingers_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤌🏾", "Pinched Fingers Medium-Dark Skin Tone\n:pinched_fingers_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤌🏿", "Pinched Fingers Dark Skin Tone\n:pinched_fingers_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤏🏻", "Pinching Hand Light Skin Tone\n:pinching_hand_light_skin_tone:" );

    RegisterSymbolX( 1, "🤏🏼", "Pinching Hand Medium-Light Skin Tone\n:pinching_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤏🏽", "Pinching Hand Medium Skin Tone\n:pinching_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤏🏾", "Pinching Hand Medium-Dark Skin Tone\n:pinching_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤏🏿", "Pinching Hand Dark Skin Tone\n:pinching_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "✌🏻", "Victory Hand Light Skin Tone\n:victory_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "✌🏼", "Victory Hand Medium-Light Skin Tone\n:victory_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "✌🏽", "Victory Hand Medium Skin Tone\n:victory_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "✌🏾", "Victory Hand Medium-Dark Skin Tone\n:victory_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "✌🏿", "Victory Hand Dark Skin Tone\n:victory_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤞🏻", "Crossed Fingers Light Skin Tone\n:crossed_fingers_light_skin_tone:" );
    RegisterSymbolX( 1, "🤞🏼", "Crossed Fingers Medium-Light Skin Tone\n:crossed_fingers_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤞🏽", "Crossed Fingers Medium Skin Tone\n:crossed_fingers_medium_skin_tone:" );

    RegisterSymbolX( 1, "🤞🏾", "Crossed Fingers Medium-Dark Skin Tone\n:crossed_fingers_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤞🏿", "Crossed Fingers Dark Skin Tone\n:crossed_fingers_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏻", "Hand With Index Finger And Thumb Crossed Light Skin Tone\n:hand_with_index_finger_and_thumb_crossed_light_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏼", "Hand With Index Finger And Thumb Crossed Medium-Light Skin Tone\n:hand_with_index_finger_and_thumb_crossed_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏽", "Hand With Index Finger And Thumb Crossed Medium Skin Tone\n:hand_with_index_finger_and_thumb_crossed_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏾", "Hand With Index Finger And Thumb Crossed Medium-Dark Skin Tone\n:hand_with_index_finger_and_thumb_crossed_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏿", "Hand With Index Finger And Thumb Crossed Dark Skin Tone\n:hand_with_index_finger_and_thumb_crossed_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤟🏻", "Love-You Gesture Light Skin Tone\n:love_you_gesture_light_skin_tone:" );
    RegisterSymbolX( 1, "🤟🏼", "Love-You Gesture Medium-Light Skin Tone\n:love_you_gesture_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤟🏽", "Love-You Gesture Medium Skin Tone\n:love_you_gesture_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤟🏾", "Love-You Gesture Medium-Dark Skin Tone\n:love_you_gesture_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤟🏿", "Love-You Gesture Dark Skin Tone\n:love_you_gesture_dark_skin_tone:" );

    RegisterSymbolX( 1, "🤘🏻", "Sign Of The Horns Light Skin Tone\n:metal_light_skin_tone:" );
    RegisterSymbolX( 1, "🤘🏼", "Sign Of The Horns Medium-Light Skin Tone\n:metal_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤘🏽", "Sign Of The Horns Medium Skin Tone\n:metal_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤘🏾", "Sign Of The Horns Medium-Dark Skin Tone\n:metal_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤘🏿", "Sign Of The Horns Dark Skin Tone\n:metal_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤙🏻", "Call Me Hand Light Skin Tone\n:call_me_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🤙🏼", "Call Me Hand Medium-Light Skin Tone\n:call_me_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤙🏽", "Call Me Hand Medium Skin Tone\n:call_me_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤙🏾", "Call Me Hand Medium-Dark Skin Tone\n:call_me_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤙🏿", "Call Me Hand Dark Skin Tone\n:call_me_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "👈🏻", "Backhand Index Pointing Left Light Skin Tone\n:point_left_light_skin_tone:" );
    RegisterSymbolX( 1, "👈🏼", "Backhand Index Pointing Left Medium-Light Skin Tone\n:point_left_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "👈🏽", "Backhand Index Pointing Left Medium Skin Tone\n:point_left_medium_skin_tone:" );
    RegisterSymbolX( 1, "👈🏾", "Backhand Index Pointing Left Medium-Dark Skin Tone\n:point_left_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👈🏿", "Backhand Index Pointing Left Dark Skin Tone\n:point_left_dark_skin_tone:" );
    RegisterSymbolX( 1, "👉🏻", "Backhand Index Pointing Right Light Skin Tone\n:point_right_light_skin_tone:" );
    RegisterSymbolX( 1, "👉🏼", "Backhand Index Pointing Right Medium-Light Skin Tone\n:point_right_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👉🏽", "Backhand Index Pointing Right Medium Skin Tone\n:point_right_medium_skin_tone:" );
    RegisterSymbolX( 1, "👉🏾", "Backhand Index Pointing Right Medium-Dark Skin Tone\n:point_right_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👉🏿", "Backhand Index Pointing Right Dark Skin Tone\n:point_right_dark_skin_tone:" );
    RegisterSymbolX( 1, "👆🏻", "Backhand Index Pointing Up Light Skin Tone\n:point_up_2_light_skin_tone:" );
    RegisterSymbolX( 1, "👆🏼", "Backhand Index Pointing Up Medium-Light Skin Tone\n:point_up_2_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👆🏽", "Backhand Index Pointing Up Medium Skin Tone\n:point_up_2_medium_skin_tone:" );
    RegisterSymbolX( 1, "👆🏾", "Backhand Index Pointing Up Medium-Dark Skin Tone\n:point_up_2_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👆🏿", "Backhand Index Pointing Up Dark Skin Tone\n:point_up_2_dark_skin_tone:" );
    RegisterSymbolX( 1, "🖕🏻", "Middle Finger Light Skin Tone\n:middle_finger_light_skin_tone:" );
    RegisterSymbolX( 1, "🖕🏼", "Middle Finger Medium-Light Skin Tone\n:middle_finger_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🖕🏽", "Middle Finger Medium Skin Tone\n:middle_finger_medium_skin_tone:" );
    RegisterSymbolX( 1, "🖕🏾", "Middle Finger Medium-Dark Skin Tone\n:middle_finger_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🖕🏿", "Middle Finger Dark Skin Tone\n:middle_finger_dark_skin_tone:" );
    RegisterSymbolX( 1, "👇🏻", "Backhand Index Pointing Down Light Skin Tone\n:point_down_light_skin_tone:" );
    RegisterSymbolX( 1, "👇🏼", "Backhand Index Pointing Down Medium-Light Skin Tone\n:point_down_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👇🏽", "Backhand Index Pointing Down Medium Skin Tone\n:point_down_medium_skin_tone:" );
    RegisterSymbolX( 1, "👇🏾", "Backhand Index Pointing Down Medium-Dark Skin Tone\n:point_down_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👇🏿", "Backhand Index Pointing Down Dark Skin Tone\n:point_down_dark_skin_tone:" );
    RegisterSymbolX( 1, "☝🏻", "Index Pointing Up Light Skin Tone\n:point_up_light_skin_tone:" );

    RegisterSymbolX( 1, "☝🏼", "Index Pointing Up Medium-Light Skin Tone\n:point_up_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "☝🏽", "Index Pointing Up Medium Skin Tone\n:point_up_medium_skin_tone:" );
    RegisterSymbolX( 1, "☝🏾", "Index Pointing Up Medium-Dark Skin Tone\n:point_up_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "☝🏿", "Index Pointing Up Dark Skin Tone\n:point_up_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏻", "Index Pointing At The Viewer Light Skin Tone\n:index_pointing_at_the_viewer_light_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏼", "Index Pointing At The Viewer Medium-Light Skin Tone\n:index_pointing_at_the_viewer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏽", "Index Pointing At The Viewer Medium Skin Tone\n:index_pointing_at_the_viewer_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏾", "Index Pointing At The Viewer Medium-Dark Skin Tone\n:index_pointing_at_the_viewer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏿", "Index Pointing At The Viewer Dark Skin Tone\n:index_pointing_at_the_viewer_dark_skin_tone:" );
    RegisterSymbolX( 1, "👍🏻", "Thumbs Up Light Skin Tone\n:+1_light_skin_tone:" );
    RegisterSymbolX( 1, "👍🏼", "Thumbs Up Medium-Light Skin Tone\n:+1_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👍🏽", "Thumbs Up Medium Skin Tone\n:+1_medium_skin_tone:" );

    RegisterSymbolX( 1, "👍🏾", "Thumbs Up Medium-Dark Skin Tone\n:+1_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👍🏿", "Thumbs Up Dark Skin Tone\n:+1_dark_skin_tone:" );
    RegisterSymbolX( 1, "👎🏻", "Thumbs Down Light Skin Tone\n:-1_light_skin_tone:" );
    RegisterSymbolX( 1, "👎🏼", "Thumbs Down Medium-Light Skin Tone\n:-1_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👎🏽", "Thumbs Down Medium Skin Tone\n:-1_medium_skin_tone:" );
    RegisterSymbolX( 1, "👎🏾", "Thumbs Down Medium-Dark Skin Tone\n:-1_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👎🏿", "Thumbs Down Dark Skin Tone\n:-1_dark_skin_tone:" );
    RegisterSymbolX( 1, "✊🏻", "Raised Fist Light Skin Tone\n:fist_light_skin_tone:" );
    RegisterSymbolX( 1, "✊🏼", "Raised Fist Medium-Light Skin Tone\n:fist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "✊🏽", "Raised Fist Medium Skin Tone\n:fist_medium_skin_tone:" );
    RegisterSymbolX( 1, "✊🏾", "Raised Fist Medium-Dark Skin Tone\n:fist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "✊🏿", "Raised Fist Dark Skin Tone\n:fist_dark_skin_tone:" );

    RegisterSymbolX( 1, "👊🏻", "Oncoming Fist Light Skin Tone\n:punch_light_skin_tone:" );
    RegisterSymbolX( 1, "👊🏼", "Oncoming Fist Medium-Light Skin Tone\n:punch_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👊🏽", "Oncoming Fist Medium Skin Tone\n:punch_medium_skin_tone:" );
    RegisterSymbolX( 1, "👊🏾", "Oncoming Fist Medium-Dark Skin Tone\n:punch_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👊🏿", "Oncoming Fist Dark Skin Tone\n:punch_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤛🏻", "Left-Facing Fist Light Skin Tone\n:left_facing_fist_light_skin_tone:" );
    RegisterSymbolX( 1, "🤛🏼", "Left-Facing Fist Medium-Light Skin Tone\n:left_facing_fist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤛🏽", "Left-Facing Fist Medium Skin Tone\n:left_facing_fist_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤛🏾", "Left-Facing Fist Medium-Dark Skin Tone\n:left_facing_fist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤛🏿", "Left-Facing Fist Dark Skin Tone\n:left_facing_fist_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤜🏻", "Right-Facing Fist Light Skin Tone\n:right_facing_fist_light_skin_tone:" );
    RegisterSymbolX( 1, "🤜🏼", "Right-Facing Fist Medium-Light Skin Tone\n:right_facing_fist_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🤜🏽", "Right-Facing Fist Medium Skin Tone\n:right_facing_fist_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤜🏾", "Right-Facing Fist Medium-Dark Skin Tone\n:right_facing_fist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤜🏿", "Right-Facing Fist Dark Skin Tone\n:right_facing_fist_dark_skin_tone:" );
    RegisterSymbolX( 1, "👏🏻", "Clapping Hands Light Skin Tone\n:clap_light_skin_tone:" );
    RegisterSymbolX( 1, "👏🏼", "Clapping Hands Medium-Light Skin Tone\n:clap_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👏🏽", "Clapping Hands Medium Skin Tone\n:clap_medium_skin_tone:" );
    RegisterSymbolX( 1, "👏🏾", "Clapping Hands Medium-Dark Skin Tone\n:clap_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👏🏿", "Clapping Hands Dark Skin Tone\n:clap_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙌🏻", "Raising Hands Light Skin Tone\n:raised_hands_light_skin_tone:" );
    RegisterSymbolX( 1, "🙌🏼", "Raising Hands Medium-Light Skin Tone\n:raised_hands_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙌🏽", "Raising Hands Medium Skin Tone\n:raised_hands_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙌🏾", "Raising Hands Medium-Dark Skin Tone\n:raised_hands_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🙌🏿", "Raising Hands Dark Skin Tone\n:raised_hands_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏻", "Heart Hands Light Skin Tone\n:heart_hands_light_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏼", "Heart Hands Medium-Light Skin Tone\n:heart_hands_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏽", "Heart Hands Medium Skin Tone\n:heart_hands_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏾", "Heart Hands Medium-Dark Skin Tone\n:heart_hands_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏿", "Heart Hands Dark Skin Tone\n:heart_hands_dark_skin_tone:" );
    RegisterSymbolX( 1, "👐🏻", "Open Hands Light Skin Tone\n:open_hands_light_skin_tone:" );
    RegisterSymbolX( 1, "👐🏼", "Open Hands Medium-Light Skin Tone\n:open_hands_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👐🏽", "Open Hands Medium Skin Tone\n:open_hands_medium_skin_tone:" );
    RegisterSymbolX( 1, "👐🏾", "Open Hands Medium-Dark Skin Tone\n:open_hands_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👐🏿", "Open Hands Dark Skin Tone\n:open_hands_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤲🏻", "Palms Up Together Light Skin Tone\n:palms_up_together_light_skin_tone:" );

    RegisterSymbolX( 1, "🤲🏼", "Palms Up Together Medium-Light Skin Tone\n:palms_up_together_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤲🏽", "Palms Up Together Medium Skin Tone\n:palms_up_together_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤲🏾", "Palms Up Together Medium-Dark Skin Tone\n:palms_up_together_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤲🏿", "Palms Up Together Dark Skin Tone\n:palms_up_together_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙏🏻", "Folded Hands Light Skin Tone\n:pray_light_skin_tone:" );
    RegisterSymbolX( 1, "🙏🏼", "Folded Hands Medium-Light Skin Tone\n:pray_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙏🏽", "Folded Hands Medium Skin Tone\n:pray_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙏🏾", "Folded Hands Medium-Dark Skin Tone\n:pray_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙏🏿", "Folded Hands Dark Skin Tone\n:pray_dark_skin_tone:" );
    RegisterSymbolX( 1, "✍🏻", "Writing Hand Light Skin Tone\n:writing_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "✍🏼", "Writing Hand Medium-Light Skin Tone\n:writing_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "✍🏽", "Writing Hand Medium Skin Tone\n:writing_hand_medium_skin_tone:" );

    RegisterSymbolX( 1, "✍🏾", "Writing Hand Medium-Dark Skin Tone\n:writing_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "✍🏿", "Writing Hand Dark Skin Tone\n:writing_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "💅🏻", "Nail Polish Light Skin Tone\n:nail_polish_light_skin_tone:" );
    RegisterSymbolX( 1, "💅🏼", "Nail Polish Medium-Light Skin Tone\n:nail_polish_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💅🏽", "Nail Polish Medium Skin Tone\n:nail_polish_medium_skin_tone:" );
    RegisterSymbolX( 1, "💅🏾", "Nail Polish Medium-Dark Skin Tone\n:nail_polish_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💅🏿", "Nail Polish Dark Skin Tone\n:nail_polish_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤳🏻", "Selfie Light Skin Tone\n:selfie_light_skin_tone:" );
    RegisterSymbolX( 1, "🤳🏼", "Selfie Medium-Light Skin Tone\n:selfie_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤳🏽", "Selfie Medium Skin Tone\n:selfie_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤳🏾", "Selfie Medium-Dark Skin Tone\n:selfie_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤳🏿", "Selfie Dark Skin Tone\n:selfie_dark_skin_tone:" );

    RegisterSymbolX( 1, "💪🏻", "Flexed Biceps Light Skin Tone\n:muscle_light_skin_tone:" );
    RegisterSymbolX( 1, "💪🏼", "Flexed Biceps Medium-Light Skin Tone\n:muscle_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💪🏽", "Flexed Biceps Medium Skin Tone\n:muscle_medium_skin_tone:" );
    RegisterSymbolX( 1, "💪🏾", "Flexed Biceps Medium-Dark Skin Tone\n:muscle_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💪🏿", "Flexed Biceps Dark Skin Tone\n:muscle_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦵🏻", "Leg Light Skin Tone\n:leg_light_skin_tone:" );
    RegisterSymbolX( 1, "🦵🏼", "Leg Medium-Light Skin Tone\n:leg_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🦵🏽", "Leg Medium Skin Tone\n:leg_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦵🏾", "Leg Medium-Dark Skin Tone\n:leg_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦵🏿", "Leg Dark Skin Tone\n:leg_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦶🏻", "Foot Light Skin Tone\n:foot_light_skin_tone:" );
    RegisterSymbolX( 1, "🦶🏼", "Foot Medium-Light Skin Tone\n:foot_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🦶🏽", "Foot Medium Skin Tone\n:foot_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦶🏾", "Foot Medium-Dark Skin Tone\n:foot_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦶🏿", "Foot Dark Skin Tone\n:foot_dark_skin_tone:" );
    RegisterSymbolX( 1, "👂🏻", "Ear Light Skin Tone\n:ear_light_skin_tone:" );
    RegisterSymbolX( 1, "👂🏼", "Ear Medium-Light Skin Tone\n:ear_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👂🏽", "Ear Medium Skin Tone\n:ear_medium_skin_tone:" );
    RegisterSymbolX( 1, "👂🏾", "Ear Medium-Dark Skin Tone\n:ear_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👂🏿", "Ear Dark Skin Tone\n:ear_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦻🏻", "Ear With Hearing Aid Light Skin Tone\n:ear_with_hearing_aid_light_skin_tone:" );
    RegisterSymbolX( 1, "🦻🏼", "Ear With Hearing Aid Medium-Light Skin Tone\n:ear_with_hearing_aid_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🦻🏽", "Ear With Hearing Aid Medium Skin Tone\n:ear_with_hearing_aid_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦻🏾", "Ear With Hearing Aid Medium-Dark Skin Tone\n:ear_with_hearing_aid_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🦻🏿", "Ear With Hearing Aid Dark Skin Tone\n:ear_with_hearing_aid_dark_skin_tone:" );
    RegisterSymbolX( 1, "👃🏻", "Nose Light Skin Tone\n:nose_light_skin_tone:" );
    RegisterSymbolX( 1, "👃🏼", "Nose Medium-Light Skin Tone\n:nose_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👃🏽", "Nose Medium Skin Tone\n:nose_medium_skin_tone:" );
    RegisterSymbolX( 1, "👃🏾", "Nose Medium-Dark Skin Tone\n:nose_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👃🏿", "Nose Dark Skin Tone\n:nose_dark_skin_tone:" );
    RegisterSymbolX( 1, "👶🏻", "Baby Light Skin Tone\n:baby_light_skin_tone:" );
    RegisterSymbolX( 1, "👶🏼", "Baby Medium-Light Skin Tone\n:baby_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👶🏽", "Baby Medium Skin Tone\n:baby_medium_skin_tone:" );
    RegisterSymbolX( 1, "👶🏾", "Baby Medium-Dark Skin Tone\n:baby_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👶🏿", "Baby Dark Skin Tone\n:baby_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧒🏻", "Child Light Skin Tone\n:child_light_skin_tone:" );

    RegisterSymbolX( 1, "🧒🏼", "Child Medium-Light Skin Tone\n:child_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧒🏽", "Child Medium Skin Tone\n:child_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧒🏾", "Child Medium-Dark Skin Tone\n:child_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧒🏿", "Child Dark Skin Tone\n:child_dark_skin_tone:" );
    RegisterSymbolX( 1, "👦🏻", "Boy Light Skin Tone\n:boy_light_skin_tone:" );
    RegisterSymbolX( 1, "👦🏼", "Boy Medium-Light Skin Tone\n:boy_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👦🏽", "Boy Medium Skin Tone\n:boy_medium_skin_tone:" );
    RegisterSymbolX( 1, "👦🏾", "Boy Medium-Dark Skin Tone\n:boy_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👦🏿", "Boy Dark Skin Tone\n:boy_dark_skin_tone:" );
    RegisterSymbolX( 1, "👧🏻", "Girl Light Skin Tone\n:girl_light_skin_tone:" );
    RegisterSymbolX( 1, "👧🏼", "Girl Medium-Light Skin Tone\n:girl_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👧🏽", "Girl Medium Skin Tone\n:girl_medium_skin_tone:" );

    RegisterSymbolX( 1, "👧🏾", "Girl Medium-Dark Skin Tone\n:girl_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👧🏿", "Girl Dark Skin Tone\n:girl_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻", "Person Light Skin Tone\n:person_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼", "Person Medium-Light Skin Tone\n:person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽", "Person Medium Skin Tone\n:person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾", "Person Medium-Dark Skin Tone\n:person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿", "Person Dark Skin Tone\n:person_dark_skin_tone:" );
    RegisterSymbolX( 1, "👱🏻", "Person Blond Hair Light Skin Tone\n:blond_haired_person_light_skin_tone:" );
    RegisterSymbolX( 1, "👱🏼", "Person Blond Hair Medium-Light Skin Tone\n:blond_haired_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👱🏽", "Person Blond Hair Medium Skin Tone\n:blond_haired_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "👱🏾", "Person Blond Hair Medium-Dark Skin Tone\n:blond_haired_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👱🏿", "Person Blond Hair Dark Skin Tone\n:blond_haired_person_dark_skin_tone:" );

    RegisterSymbolX( 1, "👨🏻", "Man Light Skin Tone\n:man_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼", "Man Medium-Light Skin Tone\n:man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽", "Man Medium Skin Tone\n:man_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾", "Man Medium-Dark Skin Tone\n:man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿", "Man Dark Skin Tone\n:man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏻", "Person Beard Light Skin Tone\n:bearded_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏼", "Person Beard Medium-Light Skin Tone\n:bearded_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏽", "Person Beard Medium Skin Tone\n:bearded_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏾", "Person Beard Medium-Dark Skin Tone\n:bearded_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏿", "Person Beard Dark Skin Tone\n:bearded_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏻‍♂️", "Man Beard Light Skin Tone\n:bearded_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏼‍♂️", "Man Beard Medium-Light Skin Tone\n:bearded_man_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧔🏽‍♂️", "Man Beard Medium Skin Tone\n:bearded_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏾‍♂️", "Man Beard Medium-Dark Skin Tone\n:bearded_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏿‍♂️", "Man Beard Dark Skin Tone\n:bearded_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏻‍♀️", "Woman Beard Light Skin Tone\n:bearded_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏼‍♀️", "Woman Beard Medium-Light Skin Tone\n:bearded_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏽‍♀️", "Woman Beard Medium Skin Tone\n:bearded_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏾‍♀️", "Woman Beard Medium-Dark Skin Tone\n:bearded_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧔🏿‍♀️", "Woman Beard Dark Skin Tone\n:bearded_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🦰", "Man Red Hair Light Skin Tone\n:red_haired_man_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🦰", "Man Red Hair Medium-Light Skin Tone\n:red_haired_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🦰", "Man Red Hair Medium Skin Tone\n:red_haired_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🦰", "Man Red Hair Medium-Dark Skin Tone\n:red_haired_man_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👨🏿‍🦰", "Man Red Hair Dark Skin Tone\n:red_haired_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🦱", "Man Curly Hair Light Skin Tone\n:curly_haired_man_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🦱", "Man Curly Hair Medium-Light Skin Tone\n:curly_haired_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🦱", "Man Curly Hair Medium Skin Tone\n:curly_haired_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🦱", "Man Curly Hair Medium-Dark Skin Tone\n:curly_haired_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🦱", "Man Curly Hair Dark Skin Tone\n:curly_haired_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🦳", "Man White Hair Light Skin Tone\n:white_haired_man_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🦳", "Man White Hair Medium-Light Skin Tone\n:white_haired_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🦳", "Man White Hair Medium Skin Tone\n:white_haired_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🦳", "Man White Hair Medium-Dark Skin Tone\n:white_haired_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🦳", "Man White Hair Dark Skin Tone\n:white_haired_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🦲", "Man Bald Light Skin Tone\n:bald_man_light_skin_tone:" );

    RegisterSymbolX( 1, "👨🏼‍🦲", "Man Bald Medium-Light Skin Tone\n:bald_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🦲", "Man Bald Medium Skin Tone\n:bald_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🦲", "Man Bald Medium-Dark Skin Tone\n:bald_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🦲", "Man Bald Dark Skin Tone\n:bald_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻", "Woman Light Skin Tone\n:woman_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼", "Woman Medium-Light Skin Tone\n:woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽", "Woman Medium Skin Tone\n:woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾", "Woman Medium-Dark Skin Tone\n:woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿", "Woman Dark Skin Tone\n:woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🦰", "Woman Red Hair Light Skin Tone\n:red_haired_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🦰", "Woman Red Hair Medium-Light Skin Tone\n:red_haired_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🦰", "Woman Red Hair Medium Skin Tone\n:red_haired_woman_medium_skin_tone:" );

    RegisterSymbolX( 1, "👩🏾‍🦰", "Woman Red Hair Medium-Dark Skin Tone\n:red_haired_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🦰", "Woman Red Hair Dark Skin Tone\n:red_haired_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🦰", "Person Red Hair Light Skin Tone\n:red_haired_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🦰", "Person Red Hair Medium-Light Skin Tone\n:red_haired_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🦰", "Person Red Hair Medium Skin Tone\n:red_haired_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🦰", "Person Red Hair Medium-Dark Skin Tone\n:red_haired_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🦰", "Person Red Hair Dark Skin Tone\n:red_haired_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🦱", "Woman Curly Hair Light Skin Tone\n:curly_haired_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🦱", "Woman Curly Hair Medium-Light Skin Tone\n:curly_haired_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🦱", "Woman Curly Hair Medium Skin Tone\n:curly_haired_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🦱", "Woman Curly Hair Medium-Dark Skin Tone\n:curly_haired_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🦱", "Woman Curly Hair Dark Skin Tone\n:curly_haired_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🦱", "Person Curly Hair Light Skin Tone\n:curly_haired_person_light_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏼‍🦱", "Person Curly Hair Medium-Light Skin Tone\n:curly_haired_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🦱", "Person Curly Hair Medium Skin Tone\n:curly_haired_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🦱", "Person Curly Hair Medium-Dark Skin Tone\n:curly_haired_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🦱", "Person Curly Hair Dark Skin Tone\n:curly_haired_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🦳", "Woman White Hair Light Skin Tone\n:white_haired_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🦳", "Woman White Hair Medium-Light Skin Tone\n:white_haired_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🦳", "Woman White Hair Medium Skin Tone\n:white_haired_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🦳", "Woman White Hair Medium-Dark Skin Tone\n:white_haired_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🦳", "Woman White Hair Dark Skin Tone\n:white_haired_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🦳", "Person White Hair Light Skin Tone\n:white_haired_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🦳", "Person White Hair Medium-Light Skin Tone\n:white_haired_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🦳", "Person White Hair Medium Skin Tone\n:white_haired_person_medium_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏾‍🦳", "Person White Hair Medium-Dark Skin Tone\n:white_haired_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🦳", "Person White Hair Dark Skin Tone\n:white_haired_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🦲", "Woman Bald Light Skin Tone\n:bald_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🦲", "Woman Bald Medium-Light Skin Tone\n:bald_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🦲", "Woman Bald Medium Skin Tone\n:bald_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🦲", "Woman Bald Medium-Dark Skin Tone\n:bald_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🦲", "Woman Bald Dark Skin Tone\n:bald_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🦲", "Person Bald Light Skin Tone\n:bald_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🦲", "Person Bald Medium-Light Skin Tone\n:bald_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🦲", "Person Bald Medium Skin Tone\n:bald_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🦲", "Person Bald Medium-Dark Skin Tone\n:bald_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🦲", "Person Bald Dark Skin Tone\n:bald_person_dark_skin_tone:" );

    RegisterSymbolX( 1, "👱🏻‍♀️", "Woman Blond Hair Light Skin Tone\n:blond_haired_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "👱🏼‍♀️", "Woman Blond Hair Medium-Light Skin Tone\n:blond_haired_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👱🏽‍♀️", "Woman Blond Hair Medium Skin Tone\n:blond_haired_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "👱🏾‍♀️", "Woman Blond Hair Medium-Dark Skin Tone\n:blond_haired_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👱🏿‍♀️", "Woman Blond Hair Dark Skin Tone\n:blond_haired_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "👱🏻‍♂️", "Man Blond Hair Light Skin Tone\n:blond_haired_man_light_skin_tone:" );
    RegisterSymbolX( 1, "👱🏼‍♂️", "Man Blond Hair Medium-Light Skin Tone\n:blond_haired_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👱🏽‍♂️", "Man Blond Hair Medium Skin Tone\n:blond_haired_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "👱🏾‍♂️", "Man Blond Hair Medium-Dark Skin Tone\n:blond_haired_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👱🏿‍♂️", "Man Blond Hair Dark Skin Tone\n:blond_haired_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧓🏻", "Older Person Light Skin Tone\n:older_adult_light_skin_tone:" );
    RegisterSymbolX( 1, "🧓🏼", "Older Person Medium-Light Skin Tone\n:older_adult_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧓🏽", "Older Person Medium Skin Tone\n:older_adult_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧓🏾", "Older Person Medium-Dark Skin Tone\n:older_adult_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧓🏿", "Older Person Dark Skin Tone\n:older_adult_dark_skin_tone:" );
    RegisterSymbolX( 1, "👴🏻", "Old Man Light Skin Tone\n:older_man_light_skin_tone:" );
    RegisterSymbolX( 1, "👴🏼", "Old Man Medium-Light Skin Tone\n:older_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👴🏽", "Old Man Medium Skin Tone\n:older_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "👴🏾", "Old Man Medium-Dark Skin Tone\n:older_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👴🏿", "Old Man Dark Skin Tone\n:older_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "👵🏻", "Old Woman Light Skin Tone\n:older_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "👵🏼", "Old Woman Medium-Light Skin Tone\n:older_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👵🏽", "Old Woman Medium Skin Tone\n:older_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "👵🏾", "Old Woman Medium-Dark Skin Tone\n:older_woman_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👵🏿", "Old Woman Dark Skin Tone\n:older_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏻", "Person Frowning Light Skin Tone\n:frowning_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏼", "Person Frowning Medium-Light Skin Tone\n:frowning_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏽", "Person Frowning Medium Skin Tone\n:frowning_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏾", "Person Frowning Medium-Dark Skin Tone\n:frowning_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏿", "Person Frowning Dark Skin Tone\n:frowning_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏻‍♂️", "Man Frowning Light Skin Tone\n:frowning_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏼‍♂️", "Man Frowning Medium-Light Skin Tone\n:frowning_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏽‍♂️", "Man Frowning Medium Skin Tone\n:frowning_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏾‍♂️", "Man Frowning Medium-Dark Skin Tone\n:frowning_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏿‍♂️", "Man Frowning Dark Skin Tone\n:frowning_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏻‍♀️", "Woman Frowning Light Skin Tone\n:frowning_woman_light_skin_tone:" );

    RegisterSymbolX( 1, "🙍🏼‍♀️", "Woman Frowning Medium-Light Skin Tone\n:frowning_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏽‍♀️", "Woman Frowning Medium Skin Tone\n:frowning_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏾‍♀️", "Woman Frowning Medium-Dark Skin Tone\n:frowning_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙍🏿‍♀️", "Woman Frowning Dark Skin Tone\n:frowning_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏻", "Person Pouting Light Skin Tone\n:pouting_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏼", "Person Pouting Medium-Light Skin Tone\n:pouting_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏽", "Person Pouting Medium Skin Tone\n:pouting_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏾", "Person Pouting Medium-Dark Skin Tone\n:pouting_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏿", "Person Pouting Dark Skin Tone\n:pouting_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏻‍♂️", "Man Pouting Light Skin Tone\n:pouting_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏼‍♂️", "Man Pouting Medium-Light Skin Tone\n:pouting_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏽‍♂️", "Man Pouting Medium Skin Tone\n:pouting_man_medium_skin_tone:" );

    RegisterSymbolX( 1, "🙎🏾‍♂️", "Man Pouting Medium-Dark Skin Tone\n:pouting_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏿‍♂️", "Man Pouting Dark Skin Tone\n:pouting_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏻‍♀️", "Woman Pouting Light Skin Tone\n:pouting_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏼‍♀️", "Woman Pouting Medium-Light Skin Tone\n:pouting_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏽‍♀️", "Woman Pouting Medium Skin Tone\n:pouting_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏾‍♀️", "Woman Pouting Medium-Dark Skin Tone\n:pouting_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙎🏿‍♀️", "Woman Pouting Dark Skin Tone\n:pouting_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏻", "Person Gesturing No Light Skin Tone\n:no_good_light_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏼", "Person Gesturing No Medium-Light Skin Tone\n:no_good_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏽", "Person Gesturing No Medium Skin Tone\n:no_good_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏾", "Person Gesturing No Medium-Dark Skin Tone\n:no_good_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏿", "Person Gesturing No Dark Skin Tone\n:no_good_dark_skin_tone:" );

    RegisterSymbolX( 1, "🙅🏻‍♂️", "Man Gesturing No Light Skin Tone\n:no_good_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏼‍♂️", "Man Gesturing No Medium-Light Skin Tone\n:no_good_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏽‍♂️", "Man Gesturing No Medium Skin Tone\n:no_good_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏾‍♂️", "Man Gesturing No Medium-Dark Skin Tone\n:no_good_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏿‍♂️", "Man Gesturing No Dark Skin Tone\n:no_good_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏻‍♀️", "Woman Gesturing No Light Skin Tone\n:no_good_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏼‍♀️", "Woman Gesturing No Medium-Light Skin Tone\n:no_good_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏽‍♀️", "Woman Gesturing No Medium Skin Tone\n:no_good_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏾‍♀️", "Woman Gesturing No Medium-Dark Skin Tone\n:no_good_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙅🏿‍♀️", "Woman Gesturing No Dark Skin Tone\n:no_good_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏻", "Person Gesturing Ok Light Skin Tone\n:ok_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏼", "Person Gesturing Ok Medium-Light Skin Tone\n:ok_person_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🙆🏽", "Person Gesturing Ok Medium Skin Tone\n:ok_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏾", "Person Gesturing Ok Medium-Dark Skin Tone\n:ok_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏿", "Person Gesturing Ok Dark Skin Tone\n:ok_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏻‍♂️", "Man Gesturing Ok Light Skin Tone\n:ok_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏼‍♂️", "Man Gesturing Ok Medium-Light Skin Tone\n:ok_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏽‍♂️", "Man Gesturing Ok Medium Skin Tone\n:ok_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏾‍♂️", "Man Gesturing Ok Medium-Dark Skin Tone\n:ok_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏿‍♂️", "Man Gesturing Ok Dark Skin Tone\n:ok_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏻‍♀️", "Woman Gesturing Ok Light Skin Tone\n:ok_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏼‍♀️", "Woman Gesturing Ok Medium-Light Skin Tone\n:ok_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏽‍♀️", "Woman Gesturing Ok Medium Skin Tone\n:ok_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙆🏾‍♀️", "Woman Gesturing Ok Medium-Dark Skin Tone\n:ok_woman_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🙆🏿‍♀️", "Woman Gesturing Ok Dark Skin Tone\n:ok_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "💁🏻", "Person Tipping Hand Light Skin Tone\n:information_desk_person_light_skin_tone:" );
    RegisterSymbolX( 1, "💁🏼", "Person Tipping Hand Medium-Light Skin Tone\n:information_desk_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💁🏽", "Person Tipping Hand Medium Skin Tone\n:information_desk_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "💁🏾", "Person Tipping Hand Medium-Dark Skin Tone\n:information_desk_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💁🏿", "Person Tipping Hand Dark Skin Tone\n:information_desk_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "💁🏻‍♂️", "Man Tipping Hand Light Skin Tone\n:information_desk_man_light_skin_tone:" );
    RegisterSymbolX( 1, "💁🏼‍♂️", "Man Tipping Hand Medium-Light Skin Tone\n:information_desk_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💁🏽‍♂️", "Man Tipping Hand Medium Skin Tone\n:information_desk_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "💁🏾‍♂️", "Man Tipping Hand Medium-Dark Skin Tone\n:information_desk_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💁🏿‍♂️", "Man Tipping Hand Dark Skin Tone\n:information_desk_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "💁🏻‍♀️", "Woman Tipping Hand Light Skin Tone\n:information_desk_woman_light_skin_tone:" );

    RegisterSymbolX( 1, "💁🏼‍♀️", "Woman Tipping Hand Medium-Light Skin Tone\n:information_desk_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💁🏽‍♀️", "Woman Tipping Hand Medium Skin Tone\n:information_desk_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "💁🏾‍♀️", "Woman Tipping Hand Medium-Dark Skin Tone\n:information_desk_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💁🏿‍♀️", "Woman Tipping Hand Dark Skin Tone\n:information_desk_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏻", "Person Raising Hand Light Skin Tone\n:raising_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏼", "Person Raising Hand Medium-Light Skin Tone\n:raising_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏽", "Person Raising Hand Medium Skin Tone\n:raising_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏾", "Person Raising Hand Medium-Dark Skin Tone\n:raising_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏿", "Person Raising Hand Dark Skin Tone\n:raising_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏻‍♂️", "Man Raising Hand Light Skin Tone\n:raising_hand_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏼‍♂️", "Man Raising Hand Medium-Light Skin Tone\n:raising_hand_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏽‍♂️", "Man Raising Hand Medium Skin Tone\n:raising_hand_man_medium_skin_tone:" );

    RegisterSymbolX( 1, "🙋🏾‍♂️", "Man Raising Hand Medium-Dark Skin Tone\n:raising_hand_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏿‍♂️", "Man Raising Hand Dark Skin Tone\n:raising_hand_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏻‍♀️", "Woman Raising Hand Light Skin Tone\n:raising_hand_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏼‍♀️", "Woman Raising Hand Medium-Light Skin Tone\n:raising_hand_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏽‍♀️", "Woman Raising Hand Medium Skin Tone\n:raising_hand_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏾‍♀️", "Woman Raising Hand Medium-Dark Skin Tone\n:raising_hand_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙋🏿‍♀️", "Woman Raising Hand Dark Skin Tone\n:raising_hand_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏻", "Deaf Person Light Skin Tone\n:deaf_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏼", "Deaf Person Medium-Light Skin Tone\n:deaf_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏽", "Deaf Person Medium Skin Tone\n:deaf_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏾", "Deaf Person Medium-Dark Skin Tone\n:deaf_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏿", "Deaf Person Dark Skin Tone\n:deaf_person_dark_skin_tone:" );

    RegisterSymbolX( 1, "🧏🏻‍♂️", "Deaf Man Light Skin Tone\n:deaf_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏼‍♂️", "Deaf Man Medium-Light Skin Tone\n:deaf_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏽‍♂️", "Deaf Man Medium Skin Tone\n:deaf_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏾‍♂️", "Deaf Man Medium-Dark Skin Tone\n:deaf_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏿‍♂️", "Deaf Man Dark Skin Tone\n:deaf_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏻‍♀️", "Deaf Woman Light Skin Tone\n:deaf_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏼‍♀️", "Deaf Woman Medium-Light Skin Tone\n:deaf_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏽‍♀️", "Deaf Woman Medium Skin Tone\n:deaf_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏾‍♀️", "Deaf Woman Medium-Dark Skin Tone\n:deaf_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧏🏿‍♀️", "Deaf Woman Dark Skin Tone\n:deaf_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏻", "Person Bowing Light Skin Tone\n:bow_light_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏼", "Person Bowing Medium-Light Skin Tone\n:bow_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🙇🏽", "Person Bowing Medium Skin Tone\n:bow_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏾", "Person Bowing Medium-Dark Skin Tone\n:bow_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏿", "Person Bowing Dark Skin Tone\n:bow_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏻‍♂️", "Man Bowing Light Skin Tone\n:bowing_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏼‍♂️", "Man Bowing Medium-Light Skin Tone\n:bowing_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏽‍♂️", "Man Bowing Medium Skin Tone\n:bowing_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏾‍♂️", "Man Bowing Medium-Dark Skin Tone\n:bowing_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏿‍♂️", "Man Bowing Dark Skin Tone\n:bowing_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏻‍♀️", "Woman Bowing Light Skin Tone\n:bowing_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏼‍♀️", "Woman Bowing Medium-Light Skin Tone\n:bowing_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏽‍♀️", "Woman Bowing Medium Skin Tone\n:bowing_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🙇🏾‍♀️", "Woman Bowing Medium-Dark Skin Tone\n:bowing_woman_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🙇🏿‍♀️", "Woman Bowing Dark Skin Tone\n:bowing_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏻", "Person Facepalming Light Skin Tone\n:facepalm_light_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏼", "Person Facepalming Medium-Light Skin Tone\n:facepalm_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏽", "Person Facepalming Medium Skin Tone\n:facepalm_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏾", "Person Facepalming Medium-Dark Skin Tone\n:facepalm_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏿", "Person Facepalming Dark Skin Tone\n:facepalm_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏻‍♂️", "Man Facepalming Light Skin Tone\n:man_facepalming_light_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏼‍♂️", "Man Facepalming Medium-Light Skin Tone\n:man_facepalming_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏽‍♂️", "Man Facepalming Medium Skin Tone\n:man_facepalming_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏾‍♂️", "Man Facepalming Medium-Dark Skin Tone\n:man_facepalming_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏿‍♂️", "Man Facepalming Dark Skin Tone\n:man_facepalming_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏻‍♀️", "Woman Facepalming Light Skin Tone\n:woman_facepalming_light_skin_tone:" );

    RegisterSymbolX( 1, "🤦🏼‍♀️", "Woman Facepalming Medium-Light Skin Tone\n:woman_facepalming_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏽‍♀️", "Woman Facepalming Medium Skin Tone\n:woman_facepalming_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏾‍♀️", "Woman Facepalming Medium-Dark Skin Tone\n:woman_facepalming_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤦🏿‍♀️", "Woman Facepalming Dark Skin Tone\n:woman_facepalming_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏻", "Person Shrugging Light Skin Tone\n:shrug_light_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏼", "Person Shrugging Medium-Light Skin Tone\n:shrug_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏽", "Person Shrugging Medium Skin Tone\n:shrug_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏾", "Person Shrugging Medium-Dark Skin Tone\n:shrug_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏿", "Person Shrugging Dark Skin Tone\n:shrug_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏻‍♂️", "Man Shrugging Light Skin Tone\n:man_shrugging_light_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏼‍♂️", "Man Shrugging Medium-Light Skin Tone\n:man_shrugging_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏽‍♂️", "Man Shrugging Medium Skin Tone\n:man_shrugging_medium_skin_tone:" );

    RegisterSymbolX( 1, "🤷🏾‍♂️", "Man Shrugging Medium-Dark Skin Tone\n:man_shrugging_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏿‍♂️", "Man Shrugging Dark Skin Tone\n:man_shrugging_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏻‍♀️", "Woman Shrugging Light Skin Tone\n:woman_shrugging_light_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏼‍♀️", "Woman Shrugging Medium-Light Skin Tone\n:woman_shrugging_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏽‍♀️", "Woman Shrugging Medium Skin Tone\n:woman_shrugging_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏾‍♀️", "Woman Shrugging Medium-Dark Skin Tone\n:woman_shrugging_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤷🏿‍♀️", "Woman Shrugging Dark Skin Tone\n:woman_shrugging_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍⚕️", "Health Worker Light Skin Tone\n:health_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍⚕️", "Health Worker Medium-Light Skin Tone\n:health_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍⚕️", "Health Worker Medium Skin Tone\n:health_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍⚕️", "Health Worker Medium-Dark Skin Tone\n:health_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍⚕️", "Health Worker Dark Skin Tone\n:health_worker_dark_skin_tone:" );

    RegisterSymbolX( 1, "👨🏻‍⚕️", "Man Health Worker Light Skin Tone\n:man_health_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍⚕️", "Man Health Worker Medium-Light Skin Tone\n:man_health_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍⚕️", "Man Health Worker Medium Skin Tone\n:man_health_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍⚕️", "Man Health Worker Medium-Dark Skin Tone\n:man_health_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍⚕️", "Man Health Worker Dark Skin Tone\n:man_health_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍⚕️", "Woman Health Worker Light Skin Tone\n:woman_health_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍⚕️", "Woman Health Worker Medium-Light Skin Tone\n:woman_health_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍⚕️", "Woman Health Worker Medium Skin Tone\n:woman_health_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍⚕️", "Woman Health Worker Medium-Dark Skin Tone\n:woman_health_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍⚕️", "Woman Health Worker Dark Skin Tone\n:woman_health_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🎓", "Student Light Skin Tone\n:student_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🎓", "Student Medium-Light Skin Tone\n:student_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏽‍🎓", "Student Medium Skin Tone\n:student_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🎓", "Student Medium-Dark Skin Tone\n:student_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🎓", "Student Dark Skin Tone\n:student_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🎓", "Man Student Light Skin Tone\n:man_student_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🎓", "Man Student Medium-Light Skin Tone\n:man_student_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🎓", "Man Student Medium Skin Tone\n:man_student_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🎓", "Man Student Medium-Dark Skin Tone\n:man_student_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🎓", "Man Student Dark Skin Tone\n:man_student_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🎓", "Woman Student Light Skin Tone\n:woman_student_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🎓", "Woman Student Medium-Light Skin Tone\n:woman_student_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🎓", "Woman Student Medium Skin Tone\n:woman_student_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🎓", "Woman Student Medium-Dark Skin Tone\n:woman_student_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏿‍🎓", "Woman Student Dark Skin Tone\n:woman_student_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🏫", "Teacher Light Skin Tone\n:teacher_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🏫", "Teacher Medium-Light Skin Tone\n:teacher_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🏫", "Teacher Medium Skin Tone\n:teacher_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🏫", "Teacher Medium-Dark Skin Tone\n:teacher_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🏫", "Teacher Dark Skin Tone\n:teacher_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🏫", "Man Teacher Light Skin Tone\n:man_teacher_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🏫", "Man Teacher Medium-Light Skin Tone\n:man_teacher_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🏫", "Man Teacher Medium Skin Tone\n:man_teacher_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🏫", "Man Teacher Medium-Dark Skin Tone\n:man_teacher_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🏫", "Man Teacher Dark Skin Tone\n:man_teacher_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🏫", "Woman Teacher Light Skin Tone\n:woman_teacher_light_skin_tone:" );

    RegisterSymbolX( 1, "👩🏼‍🏫", "Woman Teacher Medium-Light Skin Tone\n:woman_teacher_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🏫", "Woman Teacher Medium Skin Tone\n:woman_teacher_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🏫", "Woman Teacher Medium-Dark Skin Tone\n:woman_teacher_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🏫", "Woman Teacher Dark Skin Tone\n:woman_teacher_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍⚖️", "Judge Light Skin Tone\n:judge_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍⚖️", "Judge Medium-Light Skin Tone\n:judge_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍⚖️", "Judge Medium Skin Tone\n:judge_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍⚖️", "Judge Medium-Dark Skin Tone\n:judge_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍⚖️", "Judge Dark Skin Tone\n:judge_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍⚖️", "Man Judge Light Skin Tone\n:man_judge_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍⚖️", "Man Judge Medium-Light Skin Tone\n:man_judge_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍⚖️", "Man Judge Medium Skin Tone\n:man_judge_medium_skin_tone:" );

    RegisterSymbolX( 1, "👨🏾‍⚖️", "Man Judge Medium-Dark Skin Tone\n:man_judge_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍⚖️", "Man Judge Dark Skin Tone\n:man_judge_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍⚖️", "Woman Judge Light Skin Tone\n:woman_judge_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍⚖️", "Woman Judge Medium-Light Skin Tone\n:woman_judge_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍⚖️", "Woman Judge Medium Skin Tone\n:woman_judge_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍⚖️", "Woman Judge Medium-Dark Skin Tone\n:woman_judge_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍⚖️", "Woman Judge Dark Skin Tone\n:woman_judge_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🌾", "Farmer Light Skin Tone\n:farmer_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🌾", "Farmer Medium-Light Skin Tone\n:farmer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🌾", "Farmer Medium Skin Tone\n:farmer_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🌾", "Farmer Medium-Dark Skin Tone\n:farmer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🌾", "Farmer Dark Skin Tone\n:farmer_dark_skin_tone:" );

    RegisterSymbolX( 1, "👨🏻‍🌾", "Man Farmer Light Skin Tone\n:man_farmer_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🌾", "Man Farmer Medium-Light Skin Tone\n:man_farmer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🌾", "Man Farmer Medium Skin Tone\n:man_farmer_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🌾", "Man Farmer Medium-Dark Skin Tone\n:man_farmer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🌾", "Man Farmer Dark Skin Tone\n:man_farmer_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🌾", "Woman Farmer Light Skin Tone\n:woman_farmer_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🌾", "Woman Farmer Medium-Light Skin Tone\n:woman_farmer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🌾", "Woman Farmer Medium Skin Tone\n:woman_farmer_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🌾", "Woman Farmer Medium-Dark Skin Tone\n:woman_farmer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🌾", "Woman Farmer Dark Skin Tone\n:woman_farmer_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🍳", "Cook Light Skin Tone\n:cook_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🍳", "Cook Medium-Light Skin Tone\n:cook_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏽‍🍳", "Cook Medium Skin Tone\n:cook_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🍳", "Cook Medium-Dark Skin Tone\n:cook_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🍳", "Cook Dark Skin Tone\n:cook_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🍳", "Man Cook Light Skin Tone\n:man_cook_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🍳", "Man Cook Medium-Light Skin Tone\n:man_cook_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🍳", "Man Cook Medium Skin Tone\n:man_cook_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🍳", "Man Cook Medium-Dark Skin Tone\n:man_cook_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🍳", "Man Cook Dark Skin Tone\n:man_cook_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🍳", "Woman Cook Light Skin Tone\n:woman_cook_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🍳", "Woman Cook Medium-Light Skin Tone\n:woman_cook_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🍳", "Woman Cook Medium Skin Tone\n:woman_cook_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🍳", "Woman Cook Medium-Dark Skin Tone\n:woman_cook_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏿‍🍳", "Woman Cook Dark Skin Tone\n:woman_cook_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🔧", "Mechanic Light Skin Tone\n:mechanic_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🔧", "Mechanic Medium-Light Skin Tone\n:mechanic_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🔧", "Mechanic Medium Skin Tone\n:mechanic_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🔧", "Mechanic Medium-Dark Skin Tone\n:mechanic_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🔧", "Mechanic Dark Skin Tone\n:mechanic_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🔧", "Man Mechanic Light Skin Tone\n:man_mechanic_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🔧", "Man Mechanic Medium-Light Skin Tone\n:man_mechanic_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🔧", "Man Mechanic Medium Skin Tone\n:man_mechanic_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🔧", "Man Mechanic Medium-Dark Skin Tone\n:man_mechanic_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🔧", "Man Mechanic Dark Skin Tone\n:man_mechanic_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🔧", "Woman Mechanic Light Skin Tone\n:woman_mechanic_light_skin_tone:" );

    RegisterSymbolX( 1, "👩🏼‍🔧", "Woman Mechanic Medium-Light Skin Tone\n:woman_mechanic_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🔧", "Woman Mechanic Medium Skin Tone\n:woman_mechanic_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🔧", "Woman Mechanic Medium-Dark Skin Tone\n:woman_mechanic_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🔧", "Woman Mechanic Dark Skin Tone\n:woman_mechanic_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🏭", "Factory Worker Light Skin Tone\n:factory_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🏭", "Factory Worker Medium-Light Skin Tone\n:factory_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🏭", "Factory Worker Medium Skin Tone\n:factory_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🏭", "Factory Worker Medium-Dark Skin Tone\n:factory_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🏭", "Factory Worker Dark Skin Tone\n:factory_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🏭", "Man Factory Worker Light Skin Tone\n:man_factory_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🏭", "Man Factory Worker Medium-Light Skin Tone\n:man_factory_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🏭", "Man Factory Worker Medium Skin Tone\n:man_factory_worker_medium_skin_tone:" );

    RegisterSymbolX( 1, "👨🏾‍🏭", "Man Factory Worker Medium-Dark Skin Tone\n:man_factory_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🏭", "Man Factory Worker Dark Skin Tone\n:man_factory_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🏭", "Woman Factory Worker Light Skin Tone\n:woman_factory_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🏭", "Woman Factory Worker Medium-Light Skin Tone\n:woman_factory_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🏭", "Woman Factory Worker Medium Skin Tone\n:woman_factory_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🏭", "Woman Factory Worker Medium-Dark Skin Tone\n:woman_factory_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🏭", "Woman Factory Worker Dark Skin Tone\n:woman_factory_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍💼", "Office Worker Light Skin Tone\n:office_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍💼", "Office Worker Medium-Light Skin Tone\n:office_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍💼", "Office Worker Medium Skin Tone\n:office_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍💼", "Office Worker Medium-Dark Skin Tone\n:office_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍💼", "Office Worker Dark Skin Tone\n:office_worker_dark_skin_tone:" );

    RegisterSymbolX( 1, "👨🏻‍💼", "Man Office Worker Light Skin Tone\n:man_office_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍💼", "Man Office Worker Medium-Light Skin Tone\n:man_office_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍💼", "Man Office Worker Medium Skin Tone\n:man_office_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍💼", "Man Office Worker Medium-Dark Skin Tone\n:man_office_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍💼", "Man Office Worker Dark Skin Tone\n:man_office_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍💼", "Woman Office Worker Light Skin Tone\n:woman_office_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍💼", "Woman Office Worker Medium-Light Skin Tone\n:woman_office_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍💼", "Woman Office Worker Medium Skin Tone\n:woman_office_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍💼", "Woman Office Worker Medium-Dark Skin Tone\n:woman_office_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍💼", "Woman Office Worker Dark Skin Tone\n:woman_office_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🔬", "Scientist Light Skin Tone\n:scientist_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🔬", "Scientist Medium-Light Skin Tone\n:scientist_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏽‍🔬", "Scientist Medium Skin Tone\n:scientist_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🔬", "Scientist Medium-Dark Skin Tone\n:scientist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🔬", "Scientist Dark Skin Tone\n:scientist_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🔬", "Man Scientist Light Skin Tone\n:man_scientist_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🔬", "Man Scientist Medium-Light Skin Tone\n:man_scientist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🔬", "Man Scientist Medium Skin Tone\n:man_scientist_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🔬", "Man Scientist Medium-Dark Skin Tone\n:man_scientist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🔬", "Man Scientist Dark Skin Tone\n:man_scientist_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🔬", "Woman Scientist Light Skin Tone\n:woman_scientist_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🔬", "Woman Scientist Medium-Light Skin Tone\n:woman_scientist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🔬", "Woman Scientist Medium Skin Tone\n:woman_scientist_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🔬", "Woman Scientist Medium-Dark Skin Tone\n:woman_scientist_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏿‍🔬", "Woman Scientist Dark Skin Tone\n:woman_scientist_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍💻", "Technologist Light Skin Tone\n:technologist_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍💻", "Technologist Medium-Light Skin Tone\n:technologist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍💻", "Technologist Medium Skin Tone\n:technologist_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍💻", "Technologist Medium-Dark Skin Tone\n:technologist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍💻", "Technologist Dark Skin Tone\n:technologist_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍💻", "Man Technologist Light Skin Tone\n:man_technologist_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍💻", "Man Technologist Medium-Light Skin Tone\n:man_technologist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍💻", "Man Technologist Medium Skin Tone\n:man_technologist_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍💻", "Man Technologist Medium-Dark Skin Tone\n:man_technologist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍💻", "Man Technologist Dark Skin Tone\n:man_technologist_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍💻", "Woman Technologist Light Skin Tone\n:woman_technologist_light_skin_tone:" );

    RegisterSymbolX( 1, "👩🏼‍💻", "Woman Technologist Medium-Light Skin Tone\n:woman_technologist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍💻", "Woman Technologist Medium Skin Tone\n:woman_technologist_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍💻", "Woman Technologist Medium-Dark Skin Tone\n:woman_technologist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍💻", "Woman Technologist Dark Skin Tone\n:woman_technologist_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🎤", "Singer Light Skin Tone\n:singer_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🎤", "Singer Medium-Light Skin Tone\n:singer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🎤", "Singer Medium Skin Tone\n:singer_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🎤", "Singer Medium-Dark Skin Tone\n:singer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🎤", "Singer Dark Skin Tone\n:singer_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🎤", "Man Singer Light Skin Tone\n:man_singer_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🎤", "Man Singer Medium-Light Skin Tone\n:man_singer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🎤", "Man Singer Medium Skin Tone\n:man_singer_medium_skin_tone:" );

    RegisterSymbolX( 1, "👨🏾‍🎤", "Man Singer Medium-Dark Skin Tone\n:man_singer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🎤", "Man Singer Dark Skin Tone\n:man_singer_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🎤", "Woman Singer Light Skin Tone\n:woman_singer_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🎤", "Woman Singer Medium-Light Skin Tone\n:woman_singer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🎤", "Woman Singer Medium Skin Tone\n:woman_singer_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🎤", "Woman Singer Medium-Dark Skin Tone\n:woman_singer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🎤", "Woman Singer Dark Skin Tone\n:woman_singer_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🎨", "Artist Light Skin Tone\n:artist_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🎨", "Artist Medium-Light Skin Tone\n:artist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🎨", "Artist Medium Skin Tone\n:artist_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🎨", "Artist Medium-Dark Skin Tone\n:artist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🎨", "Artist Dark Skin Tone\n:artist_dark_skin_tone:" );

    RegisterSymbolX( 1, "👨🏻‍🎨", "Man Artist Light Skin Tone\n:man_artist_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🎨", "Man Artist Medium-Light Skin Tone\n:man_artist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🎨", "Man Artist Medium Skin Tone\n:man_artist_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🎨", "Man Artist Medium-Dark Skin Tone\n:man_artist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🎨", "Man Artist Dark Skin Tone\n:man_artist_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🎨", "Woman Artist Light Skin Tone\n:woman_artist_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🎨", "Woman Artist Medium-Light Skin Tone\n:woman_artist_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🎨", "Woman Artist Medium Skin Tone\n:woman_artist_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🎨", "Woman Artist Medium-Dark Skin Tone\n:woman_artist_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🎨", "Woman Artist Dark Skin Tone\n:woman_artist_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍✈️", "Pilot Light Skin Tone\n:pilot_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍✈️", "Pilot Medium-Light Skin Tone\n:pilot_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏽‍✈️", "Pilot Medium Skin Tone\n:pilot_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍✈️", "Pilot Medium-Dark Skin Tone\n:pilot_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍✈️", "Pilot Dark Skin Tone\n:pilot_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍✈️", "Man Pilot Light Skin Tone\n:man_pilot_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍✈️", "Man Pilot Medium-Light Skin Tone\n:man_pilot_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍✈️", "Man Pilot Medium Skin Tone\n:man_pilot_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍✈️", "Man Pilot Medium-Dark Skin Tone\n:man_pilot_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍✈️", "Man Pilot Dark Skin Tone\n:man_pilot_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍✈️", "Woman Pilot Light Skin Tone\n:woman_pilot_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍✈️", "Woman Pilot Medium-Light Skin Tone\n:woman_pilot_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍✈️", "Woman Pilot Medium Skin Tone\n:woman_pilot_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍✈️", "Woman Pilot Medium-Dark Skin Tone\n:woman_pilot_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏿‍✈️", "Woman Pilot Dark Skin Tone\n:woman_pilot_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🚀", "Astronaut Light Skin Tone\n:astronaut_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🚀", "Astronaut Medium-Light Skin Tone\n:astronaut_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🚀", "Astronaut Medium Skin Tone\n:astronaut_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🚀", "Astronaut Medium-Dark Skin Tone\n:astronaut_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🚀", "Astronaut Dark Skin Tone\n:astronaut_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🚀", "Man Astronaut Light Skin Tone\n:man_astronaut_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🚀", "Man Astronaut Medium-Light Skin Tone\n:man_astronaut_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🚀", "Man Astronaut Medium Skin Tone\n:man_astronaut_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🚀", "Man Astronaut Medium-Dark Skin Tone\n:man_astronaut_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🚀", "Man Astronaut Dark Skin Tone\n:man_astronaut_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🚀", "Woman Astronaut Light Skin Tone\n:woman_astronaut_light_skin_tone:" );

    RegisterSymbolX( 1, "👩🏼‍🚀", "Woman Astronaut Medium-Light Skin Tone\n:woman_astronaut_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🚀", "Woman Astronaut Medium Skin Tone\n:woman_astronaut_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🚀", "Woman Astronaut Medium-Dark Skin Tone\n:woman_astronaut_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🚀", "Woman Astronaut Dark Skin Tone\n:woman_astronaut_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🚒", "Firefighter Light Skin Tone\n:firefighter_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🚒", "Firefighter Medium-Light Skin Tone\n:firefighter_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🚒", "Firefighter Medium Skin Tone\n:firefighter_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🚒", "Firefighter Medium-Dark Skin Tone\n:firefighter_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🚒", "Firefighter Dark Skin Tone\n:firefighter_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🚒", "Man Firefighter Light Skin Tone\n:man_firefighter_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🚒", "Man Firefighter Medium-Light Skin Tone\n:man_firefighter_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🚒", "Man Firefighter Medium Skin Tone\n:man_firefighter_medium_skin_tone:" );

    RegisterSymbolX( 1, "👨🏾‍🚒", "Man Firefighter Medium-Dark Skin Tone\n:man_firefighter_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🚒", "Man Firefighter Dark Skin Tone\n:man_firefighter_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🚒", "Woman Firefighter Light Skin Tone\n:woman_firefighter_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🚒", "Woman Firefighter Medium-Light Skin Tone\n:woman_firefighter_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🚒", "Woman Firefighter Medium Skin Tone\n:woman_firefighter_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🚒", "Woman Firefighter Medium-Dark Skin Tone\n:woman_firefighter_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🚒", "Woman Firefighter Dark Skin Tone\n:woman_firefighter_dark_skin_tone:" );
    RegisterSymbolX( 1, "👮🏻", "Police Officer Light Skin Tone\n:police_officer_light_skin_tone:" );
    RegisterSymbolX( 1, "👮🏼", "Police Officer Medium-Light Skin Tone\n:police_officer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👮🏽", "Police Officer Medium Skin Tone\n:police_officer_medium_skin_tone:" );
    RegisterSymbolX( 1, "👮🏾", "Police Officer Medium-Dark Skin Tone\n:police_officer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👮🏿", "Police Officer Dark Skin Tone\n:police_officer_dark_skin_tone:" );

    RegisterSymbolX( 1, "👮🏻‍♂️", "Man Police Officer Light Skin Tone\n:man_police_officer_light_skin_tone:" );
    RegisterSymbolX( 1, "👮🏼‍♂️", "Man Police Officer Medium-Light Skin Tone\n:man_police_officer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👮🏽‍♂️", "Man Police Officer Medium Skin Tone\n:man_police_officer_medium_skin_tone:" );
    RegisterSymbolX( 1, "👮🏾‍♂️", "Man Police Officer Medium-Dark Skin Tone\n:man_police_officer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👮🏿‍♂️", "Man Police Officer Dark Skin Tone\n:man_police_officer_dark_skin_tone:" );
    RegisterSymbolX( 1, "👮🏻‍♀️", "Woman Police Officer Light Skin Tone\n:woman_police_officer_light_skin_tone:" );
    RegisterSymbolX( 1, "👮🏼‍♀️", "Woman Police Officer Medium-Light Skin Tone\n:woman_police_officer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👮🏽‍♀️", "Woman Police Officer Medium Skin Tone\n:woman_police_officer_medium_skin_tone:" );
    RegisterSymbolX( 1, "👮🏾‍♀️", "Woman Police Officer Medium-Dark Skin Tone\n:woman_police_officer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👮🏿‍♀️", "Woman Police Officer Dark Skin Tone\n:woman_police_officer_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏻", "Detective Light Skin Tone\n:detective_light_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏼", "Detective Medium-Light Skin Tone\n:detective_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🕵🏽", "Detective Medium Skin Tone\n:detective_medium_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏾", "Detective Medium-Dark Skin Tone\n:detective_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏿", "Detective Dark Skin Tone\n:detective_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏻‍♂️", "Man Detective Light Skin Tone\n:man_detective_light_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏼‍♂️", "Man Detective Medium-Light Skin Tone\n:man_detective_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏽‍♂️", "Man Detective Medium Skin Tone\n:man_detective_medium_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏾‍♂️", "Man Detective Medium-Dark Skin Tone\n:man_detective_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏿‍♂️", "Man Detective Dark Skin Tone\n:man_detective_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏻‍♀️", "Woman Detective Light Skin Tone\n:woman_detective_light_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏼‍♀️", "Woman Detective Medium-Light Skin Tone\n:woman_detective_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏽‍♀️", "Woman Detective Medium Skin Tone\n:woman_detective_medium_skin_tone:" );
    RegisterSymbolX( 1, "🕵🏾‍♀️", "Woman Detective Medium-Dark Skin Tone\n:woman_detective_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🕵🏿‍♀️", "Woman Detective Dark Skin Tone\n:woman_detective_dark_skin_tone:" );
    RegisterSymbolX( 1, "💂🏻", "Guard Light Skin Tone\n:guard_light_skin_tone:" );
    RegisterSymbolX( 1, "💂🏼", "Guard Medium-Light Skin Tone\n:guard_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💂🏽", "Guard Medium Skin Tone\n:guard_medium_skin_tone:" );
    RegisterSymbolX( 1, "💂🏾", "Guard Medium-Dark Skin Tone\n:guard_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💂🏿", "Guard Dark Skin Tone\n:guard_dark_skin_tone:" );
    RegisterSymbolX( 1, "💂🏻‍♂️", "Man Guard Light Skin Tone\n:man_guard_light_skin_tone:" );
    RegisterSymbolX( 1, "💂🏼‍♂️", "Man Guard Medium-Light Skin Tone\n:man_guard_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💂🏽‍♂️", "Man Guard Medium Skin Tone\n:man_guard_medium_skin_tone:" );
    RegisterSymbolX( 1, "💂🏾‍♂️", "Man Guard Medium-Dark Skin Tone\n:man_guard_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💂🏿‍♂️", "Man Guard Dark Skin Tone\n:man_guard_dark_skin_tone:" );
    RegisterSymbolX( 1, "💂🏻‍♀️", "Woman Guard Light Skin Tone\n:woman_guard_light_skin_tone:" );

    RegisterSymbolX( 1, "💂🏼‍♀️", "Woman Guard Medium-Light Skin Tone\n:woman_guard_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💂🏽‍♀️", "Woman Guard Medium Skin Tone\n:woman_guard_medium_skin_tone:" );
    RegisterSymbolX( 1, "💂🏾‍♀️", "Woman Guard Medium-Dark Skin Tone\n:woman_guard_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💂🏿‍♀️", "Woman Guard Dark Skin Tone\n:woman_guard_dark_skin_tone:" );
    RegisterSymbolX( 1, "🥷🏻", "Ninja Light Skin Tone\n:ninja_light_skin_tone:" );
    RegisterSymbolX( 1, "🥷🏼", "Ninja Medium-Light Skin Tone\n:ninja_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🥷🏽", "Ninja Medium Skin Tone\n:ninja_medium_skin_tone:" );
    RegisterSymbolX( 1, "🥷🏾", "Ninja Medium-Dark Skin Tone\n:ninja_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🥷🏿", "Ninja Dark Skin Tone\n:ninja_dark_skin_tone:" );
    RegisterSymbolX( 1, "👷🏻", "Construction Worker Light Skin Tone\n:construction_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👷🏼", "Construction Worker Medium-Light Skin Tone\n:construction_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👷🏽", "Construction Worker Medium Skin Tone\n:construction_worker_medium_skin_tone:" );

    RegisterSymbolX( 1, "👷🏾", "Construction Worker Medium-Dark Skin Tone\n:construction_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👷🏿", "Construction Worker Dark Skin Tone\n:construction_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "👷🏻‍♂️", "Man Construction Worker Light Skin Tone\n:man_construction_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👷🏼‍♂️", "Man Construction Worker Medium-Light Skin Tone\n:man_construction_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👷🏽‍♂️", "Man Construction Worker Medium Skin Tone\n:man_construction_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "👷🏾‍♂️", "Man Construction Worker Medium-Dark Skin Tone\n:man_construction_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👷🏿‍♂️", "Man Construction Worker Dark Skin Tone\n:man_construction_worker_dark_skin_tone:" );
    RegisterSymbolX( 1, "👷🏻‍♀️", "Woman Construction Worker Light Skin Tone\n:woman_construction_worker_light_skin_tone:" );
    RegisterSymbolX( 1, "👷🏼‍♀️", "Woman Construction Worker Medium-Light Skin Tone\n:woman_construction_worker_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👷🏽‍♀️", "Woman Construction Worker Medium Skin Tone\n:woman_construction_worker_medium_skin_tone:" );
    RegisterSymbolX( 1, "👷🏾‍♀️", "Woman Construction Worker Medium-Dark Skin Tone\n:woman_construction_worker_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👷🏿‍♀️", "Woman Construction Worker Dark Skin Tone\n:woman_construction_worker_dark_skin_tone:" );

    RegisterSymbolX( 1, "🤴🏻", "Prince Light Skin Tone\n:prince_light_skin_tone:" );
    RegisterSymbolX( 1, "🤴🏼", "Prince Medium-Light Skin Tone\n:prince_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤴🏽", "Prince Medium Skin Tone\n:prince_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤴🏾", "Prince Medium-Dark Skin Tone\n:prince_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤴🏿", "Prince Dark Skin Tone\n:prince_dark_skin_tone:" );
    RegisterSymbolX( 1, "👸🏻", "Princess Light Skin Tone\n:princess_light_skin_tone:" );
    RegisterSymbolX( 1, "👸🏼", "Princess Medium-Light Skin Tone\n:princess_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👸🏽", "Princess Medium Skin Tone\n:princess_medium_skin_tone:" );
    RegisterSymbolX( 1, "👸🏾", "Princess Medium-Dark Skin Tone\n:princess_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👸🏿", "Princess Dark Skin Tone\n:princess_dark_skin_tone:" );
    RegisterSymbolX( 1, "👳🏻", "Person Wearing Turban Light Skin Tone\n:person_with_turban_light_skin_tone:" );
    RegisterSymbolX( 1, "👳🏼", "Person Wearing Turban Medium-Light Skin Tone\n:person_with_turban_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "👳🏽", "Person Wearing Turban Medium Skin Tone\n:person_with_turban_medium_skin_tone:" );
    RegisterSymbolX( 1, "👳🏾", "Person Wearing Turban Medium-Dark Skin Tone\n:person_with_turban_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👳🏿", "Person Wearing Turban Dark Skin Tone\n:person_with_turban_dark_skin_tone:" );
    RegisterSymbolX( 1, "👳🏻‍♂️", "Man Wearing Turban Light Skin Tone\n:man_with_turban_light_skin_tone:" );
    RegisterSymbolX( 1, "👳🏼‍♂️", "Man Wearing Turban Medium-Light Skin Tone\n:man_with_turban_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👳🏽‍♂️", "Man Wearing Turban Medium Skin Tone\n:man_with_turban_medium_skin_tone:" );
    RegisterSymbolX( 1, "👳🏾‍♂️", "Man Wearing Turban Medium-Dark Skin Tone\n:man_with_turban_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👳🏿‍♂️", "Man Wearing Turban Dark Skin Tone\n:man_with_turban_dark_skin_tone:" );
    RegisterSymbolX( 1, "👳🏻‍♀️", "Woman Wearing Turban Light Skin Tone\n:woman_with_turban_light_skin_tone:" );
    RegisterSymbolX( 1, "👳🏼‍♀️", "Woman Wearing Turban Medium-Light Skin Tone\n:woman_with_turban_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👳🏽‍♀️", "Woman Wearing Turban Medium Skin Tone\n:woman_with_turban_medium_skin_tone:" );
    RegisterSymbolX( 1, "👳🏾‍♀️", "Woman Wearing Turban Medium-Dark Skin Tone\n:woman_with_turban_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👳🏿‍♀️", "Woman Wearing Turban Dark Skin Tone\n:woman_with_turban_dark_skin_tone:" );
    RegisterSymbolX( 1, "👲🏻", "Person With Skullcap Light Skin Tone\n:person_with_skullcap_light_skin_tone:" );
    RegisterSymbolX( 1, "👲🏼", "Person With Skullcap Medium-Light Skin Tone\n:person_with_skullcap_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👲🏽", "Person With Skullcap Medium Skin Tone\n:person_with_skullcap_medium_skin_tone:" );
    RegisterSymbolX( 1, "👲🏾", "Person With Skullcap Medium-Dark Skin Tone\n:person_with_skullcap_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👲🏿", "Person With Skullcap Dark Skin Tone\n:person_with_skullcap_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧕🏻", "Woman With Headscarf Light Skin Tone\n:woman_with_headscarf_light_skin_tone:" );
    RegisterSymbolX( 1, "🧕🏼", "Woman With Headscarf Medium-Light Skin Tone\n:woman_with_headscarf_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧕🏽", "Woman With Headscarf Medium Skin Tone\n:woman_with_headscarf_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧕🏾", "Woman With Headscarf Medium-Dark Skin Tone\n:woman_with_headscarf_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧕🏿", "Woman With Headscarf Dark Skin Tone\n:woman_with_headscarf_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏻", "Person In Tuxedo Light Skin Tone\n:person_in_tuxedo_light_skin_tone:" );

    RegisterSymbolX( 1, "🤵🏼", "Person In Tuxedo Medium-Light Skin Tone\n:person_in_tuxedo_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏽", "Person In Tuxedo Medium Skin Tone\n:person_in_tuxedo_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏾", "Person In Tuxedo Medium-Dark Skin Tone\n:person_in_tuxedo_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏿", "Person In Tuxedo Dark Skin Tone\n:person_in_tuxedo_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏻‍♂️", "Man In Tuxedo Light Skin Tone\n:man_in_tuxedo_light_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏼‍♂️", "Man In Tuxedo Medium-Light Skin Tone\n:man_in_tuxedo_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏽‍♂️", "Man In Tuxedo Medium Skin Tone\n:man_in_tuxedo_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏾‍♂️", "Man In Tuxedo Medium-Dark Skin Tone\n:man_in_tuxedo_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏿‍♂️", "Man In Tuxedo Dark Skin Tone\n:man_in_tuxedo_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏻‍♀️", "Woman In Tuxedo Light Skin Tone\n:woman_in_tuxedo_light_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏼‍♀️", "Woman In Tuxedo Medium-Light Skin Tone\n:woman_in_tuxedo_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏽‍♀️", "Woman In Tuxedo Medium Skin Tone\n:woman_in_tuxedo_medium_skin_tone:" );

    RegisterSymbolX( 1, "🤵🏾‍♀️", "Woman In Tuxedo Medium-Dark Skin Tone\n:woman_in_tuxedo_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤵🏿‍♀️", "Woman In Tuxedo Dark Skin Tone\n:woman_in_tuxedo_dark_skin_tone:" );
    RegisterSymbolX( 1, "👰🏻", "Person With Veil Light Skin Tone\n:person_with_veil_light_skin_tone:" );
    RegisterSymbolX( 1, "👰🏼", "Person With Veil Medium-Light Skin Tone\n:person_with_veil_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👰🏽", "Person With Veil Medium Skin Tone\n:person_with_veil_medium_skin_tone:" );
    RegisterSymbolX( 1, "👰🏾", "Person With Veil Medium-Dark Skin Tone\n:person_with_veil_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👰🏿", "Person With Veil Dark Skin Tone\n:person_with_veil_dark_skin_tone:" );
    RegisterSymbolX( 1, "👰🏻‍♂️", "Man With Veil Light Skin Tone\n:man_with_veil_light_skin_tone:" );
    RegisterSymbolX( 1, "👰🏼‍♂️", "Man With Veil Medium-Light Skin Tone\n:man_with_veil_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👰🏽‍♂️", "Man With Veil Medium Skin Tone\n:man_with_veil_medium_skin_tone:" );
    RegisterSymbolX( 1, "👰🏾‍♂️", "Man With Veil Medium-Dark Skin Tone\n:man_with_veil_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👰🏿‍♂️", "Man With Veil Dark Skin Tone\n:man_with_veil_dark_skin_tone:" );

    RegisterSymbolX( 1, "👰🏻‍♀️", "Woman With Veil Light Skin Tone\n:woman_with_veil_light_skin_tone:" );
    RegisterSymbolX( 1, "👰🏼‍♀️", "Woman With Veil Medium-Light Skin Tone\n:woman_with_veil_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👰🏽‍♀️", "Woman With Veil Medium Skin Tone\n:woman_with_veil_medium_skin_tone:" );
    RegisterSymbolX( 1, "👰🏾‍♀️", "Woman With Veil Medium-Dark Skin Tone\n:woman_with_veil_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👰🏿‍♀️", "Woman With Veil Dark Skin Tone\n:woman_with_veil_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤰🏻", "Pregnant Woman Light Skin Tone\n:pregnant_woman_light_skin_tone:" );
    RegisterSymbolX( 1, "🤰🏼", "Pregnant Woman Medium-Light Skin Tone\n:pregnant_woman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤰🏽", "Pregnant Woman Medium Skin Tone\n:pregnant_woman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤰🏾", "Pregnant Woman Medium-Dark Skin Tone\n:pregnant_woman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤰🏿", "Pregnant Woman Dark Skin Tone\n:pregnant_woman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫃🏻", "Pregnant Man Light Skin Tone\n:pregnant_man_light_skin_tone:" );
    RegisterSymbolX( 1, "🫃🏼", "Pregnant Man Medium-Light Skin Tone\n:pregnant_man_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🫃🏽", "Pregnant Man Medium Skin Tone\n:pregnant_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫃🏾", "Pregnant Man Medium-Dark Skin Tone\n:pregnant_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫃🏿", "Pregnant Man Dark Skin Tone\n:pregnant_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏻", "Pregnant Person Light Skin Tone\n:pregnant_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏼", "Pregnant Person Medium-Light Skin Tone\n:pregnant_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏽", "Pregnant Person Medium Skin Tone\n:pregnant_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏾", "Pregnant Person Medium-Dark Skin Tone\n:pregnant_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏿", "Pregnant Person Dark Skin Tone\n:pregnant_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤱🏻", "Breast-Feeding Light Skin Tone\n:breast_feeding_light_skin_tone:" );
    RegisterSymbolX( 1, "🤱🏼", "Breast-Feeding Medium-Light Skin Tone\n:breast_feeding_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤱🏽", "Breast-Feeding Medium Skin Tone\n:breast_feeding_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤱🏾", "Breast-Feeding Medium-Dark Skin Tone\n:breast_feeding_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🤱🏿", "Breast-Feeding Dark Skin Tone\n:breast_feeding_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🍼", "Woman Feeding Baby Light Skin Tone\n:woman_feeding_baby_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🍼", "Woman Feeding Baby Medium-Light Skin Tone\n:woman_feeding_baby_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🍼", "Woman Feeding Baby Medium Skin Tone\n:woman_feeding_baby_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🍼", "Woman Feeding Baby Medium-Dark Skin Tone\n:woman_feeding_baby_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🍼", "Woman Feeding Baby Dark Skin Tone\n:woman_feeding_baby_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🍼", "Man Feeding Baby Light Skin Tone\n:man_feeding_baby_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🍼", "Man Feeding Baby Medium-Light Skin Tone\n:man_feeding_baby_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🍼", "Man Feeding Baby Medium Skin Tone\n:man_feeding_baby_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🍼", "Man Feeding Baby Medium-Dark Skin Tone\n:man_feeding_baby_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🍼", "Man Feeding Baby Dark Skin Tone\n:man_feeding_baby_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🍼", "Person Feeding Baby Light Skin Tone\n:person_feeding_baby_light_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏼‍🍼", "Person Feeding Baby Medium-Light Skin Tone\n:person_feeding_baby_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🍼", "Person Feeding Baby Medium Skin Tone\n:person_feeding_baby_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🍼", "Person Feeding Baby Medium-Dark Skin Tone\n:person_feeding_baby_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🍼", "Person Feeding Baby Dark Skin Tone\n:person_feeding_baby_dark_skin_tone:" );
    RegisterSymbolX( 1, "👼🏻", "Baby Angel Light Skin Tone\n:baby_angel_light_skin_tone:" );
    RegisterSymbolX( 1, "👼🏼", "Baby Angel Medium-Light Skin Tone\n:baby_angel_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👼🏽", "Baby Angel Medium Skin Tone\n:baby_angel_medium_skin_tone:" );
    RegisterSymbolX( 1, "👼🏾", "Baby Angel Medium-Dark Skin Tone\n:baby_angel_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👼🏿", "Baby Angel Dark Skin Tone\n:baby_angel_dark_skin_tone:" );
    RegisterSymbolX( 1, "🎅🏻", "Santa Claus Light Skin Tone\n:santa_light_skin_tone:" );
    RegisterSymbolX( 1, "🎅🏼", "Santa Claus Medium-Light Skin Tone\n:santa_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🎅🏽", "Santa Claus Medium Skin Tone\n:santa_medium_skin_tone:" );

    RegisterSymbolX( 1, "🎅🏾", "Santa Claus Medium-Dark Skin Tone\n:santa_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🎅🏿", "Santa Claus Dark Skin Tone\n:santa_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🎄", "Mx Claus Light Skin Tone\n:mx_claus_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🎄", "Mx Claus Medium-Light Skin Tone\n:mx_claus_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🎄", "Mx Claus Medium Skin Tone\n:mx_claus_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🎄", "Mx Claus Medium-Dark Skin Tone\n:mx_claus_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🎄", "Mx Claus Dark Skin Tone\n:mx_claus_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏻", "Superhero Light Skin Tone\n:superhero_light_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏼", "Superhero Medium-Light Skin Tone\n:superhero_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏽", "Superhero Medium Skin Tone\n:superhero_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏾", "Superhero Medium-Dark Skin Tone\n:superhero_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏿", "Superhero Dark Skin Tone\n:superhero_dark_skin_tone:" );

    RegisterSymbolX( 1, "🦸🏻‍♂️", "Man Superhero Light Skin Tone\n:man_superhero_light_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏼‍♂️", "Man Superhero Medium-Light Skin Tone\n:man_superhero_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏽‍♂️", "Man Superhero Medium Skin Tone\n:man_superhero_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏾‍♂️", "Man Superhero Medium-Dark Skin Tone\n:man_superhero_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏿‍♂️", "Man Superhero Dark Skin Tone\n:man_superhero_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏻‍♀️", "Woman Superhero Light Skin Tone\n:woman_superhero_light_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏼‍♀️", "Woman Superhero Medium-Light Skin Tone\n:woman_superhero_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏽‍♀️", "Woman Superhero Medium Skin Tone\n:woman_superhero_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏾‍♀️", "Woman Superhero Medium-Dark Skin Tone\n:woman_superhero_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦸🏿‍♀️", "Woman Superhero Dark Skin Tone\n:woman_superhero_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏻", "Supervillain Light Skin Tone\n:supervillain_light_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏼", "Supervillain Medium-Light Skin Tone\n:supervillain_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🦹🏽", "Supervillain Medium Skin Tone\n:supervillain_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏾", "Supervillain Medium-Dark Skin Tone\n:supervillain_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏿", "Supervillain Dark Skin Tone\n:supervillain_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏻‍♂️", "Man Supervillain Light Skin Tone\n:man_supervillain_light_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏼‍♂️", "Man Supervillain Medium-Light Skin Tone\n:man_supervillain_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏽‍♂️", "Man Supervillain Medium Skin Tone\n:man_supervillain_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏾‍♂️", "Man Supervillain Medium-Dark Skin Tone\n:man_supervillain_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏿‍♂️", "Man Supervillain Dark Skin Tone\n:man_supervillain_dark_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏻‍♀️", "Woman Supervillain Light Skin Tone\n:woman_supervillain_light_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏼‍♀️", "Woman Supervillain Medium-Light Skin Tone\n:woman_supervillain_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏽‍♀️", "Woman Supervillain Medium Skin Tone\n:woman_supervillain_medium_skin_tone:" );
    RegisterSymbolX( 1, "🦹🏾‍♀️", "Woman Supervillain Medium-Dark Skin Tone\n:woman_supervillain_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🦹🏿‍♀️", "Woman Supervillain Dark Skin Tone\n:woman_supervillain_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏻", "Mage Light Skin Tone\n:mage_light_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏼", "Mage Medium-Light Skin Tone\n:mage_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏽", "Mage Medium Skin Tone\n:mage_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏾", "Mage Medium-Dark Skin Tone\n:mage_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏿", "Mage Dark Skin Tone\n:mage_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏻‍♂️", "Man Mage Light Skin Tone\n:man_mage_light_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏼‍♂️", "Man Mage Medium-Light Skin Tone\n:man_mage_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏽‍♂️", "Man Mage Medium Skin Tone\n:man_mage_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏾‍♂️", "Man Mage Medium-Dark Skin Tone\n:man_mage_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏿‍♂️", "Man Mage Dark Skin Tone\n:man_mage_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏻‍♀️", "Woman Mage Light Skin Tone\n:woman_mage_light_skin_tone:" );

    RegisterSymbolX( 1, "🧙🏼‍♀️", "Woman Mage Medium-Light Skin Tone\n:woman_mage_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏽‍♀️", "Woman Mage Medium Skin Tone\n:woman_mage_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏾‍♀️", "Woman Mage Medium-Dark Skin Tone\n:woman_mage_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧙🏿‍♀️", "Woman Mage Dark Skin Tone\n:woman_mage_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏻", "Fairy Light Skin Tone\n:fairy_light_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏼", "Fairy Medium-Light Skin Tone\n:fairy_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏽", "Fairy Medium Skin Tone\n:fairy_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏾", "Fairy Medium-Dark Skin Tone\n:fairy_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏿", "Fairy Dark Skin Tone\n:fairy_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏻‍♂️", "Man Fairy Light Skin Tone\n:man_fairy_light_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏼‍♂️", "Man Fairy Medium-Light Skin Tone\n:man_fairy_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏽‍♂️", "Man Fairy Medium Skin Tone\n:man_fairy_medium_skin_tone:" );

    RegisterSymbolX( 1, "🧚🏾‍♂️", "Man Fairy Medium-Dark Skin Tone\n:man_fairy_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏿‍♂️", "Man Fairy Dark Skin Tone\n:man_fairy_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏻‍♀️", "Woman Fairy Light Skin Tone\n:woman_fairy_light_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏼‍♀️", "Woman Fairy Medium-Light Skin Tone\n:woman_fairy_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏽‍♀️", "Woman Fairy Medium Skin Tone\n:woman_fairy_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏾‍♀️", "Woman Fairy Medium-Dark Skin Tone\n:woman_fairy_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧚🏿‍♀️", "Woman Fairy Dark Skin Tone\n:woman_fairy_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏻", "Vampire Light Skin Tone\n:vampire_light_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏼", "Vampire Medium-Light Skin Tone\n:vampire_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏽", "Vampire Medium Skin Tone\n:vampire_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏾", "Vampire Medium-Dark Skin Tone\n:vampire_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏿", "Vampire Dark Skin Tone\n:vampire_dark_skin_tone:" );

    RegisterSymbolX( 1, "🧛🏻‍♂️", "Man Vampire Light Skin Tone\n:man_vampire_light_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏼‍♂️", "Man Vampire Medium-Light Skin Tone\n:man_vampire_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏽‍♂️", "Man Vampire Medium Skin Tone\n:man_vampire_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏾‍♂️", "Man Vampire Medium-Dark Skin Tone\n:man_vampire_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏿‍♂️", "Man Vampire Dark Skin Tone\n:man_vampire_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏻‍♀️", "Woman Vampire Light Skin Tone\n:woman_vampire_light_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏼‍♀️", "Woman Vampire Medium-Light Skin Tone\n:woman_vampire_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏽‍♀️", "Woman Vampire Medium Skin Tone\n:woman_vampire_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏾‍♀️", "Woman Vampire Medium-Dark Skin Tone\n:woman_vampire_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧛🏿‍♀️", "Woman Vampire Dark Skin Tone\n:woman_vampire_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏻", "Merperson Light Skin Tone\n:merperson_light_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏼", "Merperson Medium-Light Skin Tone\n:merperson_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧜🏽", "Merperson Medium Skin Tone\n:merperson_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏾", "Merperson Medium-Dark Skin Tone\n:merperson_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏿", "Merperson Dark Skin Tone\n:merperson_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏻‍♂️", "Merman Light Skin Tone\n:merman_light_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏼‍♂️", "Merman Medium-Light Skin Tone\n:merman_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏽‍♂️", "Merman Medium Skin Tone\n:merman_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏾‍♂️", "Merman Medium-Dark Skin Tone\n:merman_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏿‍♂️", "Merman Dark Skin Tone\n:merman_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏻‍♀️", "Mermaid Light Skin Tone\n:mermaid_light_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏼‍♀️", "Mermaid Medium-Light Skin Tone\n:mermaid_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏽‍♀️", "Mermaid Medium Skin Tone\n:mermaid_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧜🏾‍♀️", "Mermaid Medium-Dark Skin Tone\n:mermaid_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🧜🏿‍♀️", "Mermaid Dark Skin Tone\n:mermaid_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏻", "Elf Light Skin Tone\n:elf_light_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏼", "Elf Medium-Light Skin Tone\n:elf_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏽", "Elf Medium Skin Tone\n:elf_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏾", "Elf Medium-Dark Skin Tone\n:elf_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏿", "Elf Dark Skin Tone\n:elf_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏻‍♂️", "Man Elf Light Skin Tone\n:man_elf_light_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏼‍♂️", "Man Elf Medium-Light Skin Tone\n:man_elf_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏽‍♂️", "Man Elf Medium Skin Tone\n:man_elf_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏾‍♂️", "Man Elf Medium-Dark Skin Tone\n:man_elf_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏿‍♂️", "Man Elf Dark Skin Tone\n:man_elf_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏻‍♀️", "Woman Elf Light Skin Tone\n:woman_elf_light_skin_tone:" );

    RegisterSymbolX( 1, "🧝🏼‍♀️", "Woman Elf Medium-Light Skin Tone\n:woman_elf_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏽‍♀️", "Woman Elf Medium Skin Tone\n:woman_elf_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏾‍♀️", "Woman Elf Medium-Dark Skin Tone\n:woman_elf_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧝🏿‍♀️", "Woman Elf Dark Skin Tone\n:woman_elf_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧞‍♂️", "Man Genie\n:man_genie:" );
    RegisterSymbolX( 1, "🧞‍♀️", "Woman Genie\n:woman_genie:" );
    RegisterSymbolX( 1, "🧟‍♂️", "Man Zombie\n:man_zombie:" );
    RegisterSymbolX( 1, "🧟‍♀️", "Woman Zombie\n:woman_zombie:" );
    RegisterSymbolX( 1, "💆🏻", "Person Getting Massage Light Skin Tone\n:massage_light_skin_tone:" );
    RegisterSymbolX( 1, "💆🏼", "Person Getting Massage Medium-Light Skin Tone\n:massage_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💆🏽", "Person Getting Massage Medium Skin Tone\n:massage_medium_skin_tone:" );
    RegisterSymbolX( 1, "💆🏾", "Person Getting Massage Medium-Dark Skin Tone\n:massage_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "💆🏿", "Person Getting Massage Dark Skin Tone\n:massage_dark_skin_tone:" );
    RegisterSymbolX( 1, "💇🏻", "Person Getting Haircut Light Skin Tone\n:haircut_light_skin_tone:" );
    RegisterSymbolX( 1, "💇🏼", "Person Getting Haircut Medium-Light Skin Tone\n:haircut_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💇🏽", "Person Getting Haircut Medium Skin Tone\n:haircut_medium_skin_tone:" );
    RegisterSymbolX( 1, "💇🏾", "Person Getting Haircut Medium-Dark Skin Tone\n:haircut_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💇🏿", "Person Getting Haircut Dark Skin Tone\n:haircut_dark_skin_tone:" );
    RegisterSymbolX( 1, "💆🏻‍♂️", "Man Getting Massage Light Skin Tone\n:man_getting_massage_light_skin_tone:" );
    RegisterSymbolX( 1, "💆🏼‍♂️", "Man Getting Massage Medium-Light Skin Tone\n:man_getting_massage_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💆🏽‍♂️", "Man Getting Massage Medium Skin Tone\n:man_getting_massage_medium_skin_tone:" );
    RegisterSymbolX( 1, "💆🏾‍♂️", "Man Getting Massage Medium-Dark Skin Tone\n:man_getting_massage_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💆🏿‍♂️", "Man Getting Massage Dark Skin Tone\n:man_getting_massage_dark_skin_tone:" );
    RegisterSymbolX( 1, "💆🏻‍♀️", "Woman Getting Massage Light Skin Tone\n:woman_getting_massage_light_skin_tone:" );

    RegisterSymbolX( 1, "💆🏼‍♀️", "Woman Getting Massage Medium-Light Skin Tone\n:woman_getting_massage_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💆🏽‍♀️", "Woman Getting Massage Medium Skin Tone\n:woman_getting_massage_medium_skin_tone:" );
    RegisterSymbolX( 1, "💆🏾‍♀️", "Woman Getting Massage Medium-Dark Skin Tone\n:woman_getting_massage_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💆🏿‍♀️", "Woman Getting Massage Dark Skin Tone\n:woman_getting_massage_dark_skin_tone:" );
    RegisterSymbolX( 1, "💇🏻‍♂️", "Man Getting Haircut Light Skin Tone\n:man_getting_haircut_light_skin_tone:" );
    RegisterSymbolX( 1, "💇🏼‍♂️", "Man Getting Haircut Medium-Light Skin Tone\n:man_getting_haircut_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💇🏽‍♂️", "Man Getting Haircut Medium Skin Tone\n:man_getting_haircut_medium_skin_tone:" );
    RegisterSymbolX( 1, "💇🏾‍♂️", "Man Getting Haircut Medium-Dark Skin Tone\n:man_getting_haircut_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💇🏿‍♂️", "Man Getting Haircut Dark Skin Tone\n:man_getting_haircut_dark_skin_tone:" );
    RegisterSymbolX( 1, "💇🏻‍♀️", "Woman Getting Haircut Light Skin Tone\n:woman_getting_haircut_light_skin_tone:" );
    RegisterSymbolX( 1, "💇🏼‍♀️", "Woman Getting Haircut Medium-Light Skin Tone\n:woman_getting_haircut_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💇🏽‍♀️", "Woman Getting Haircut Medium Skin Tone\n:woman_getting_haircut_medium_skin_tone:" );

    RegisterSymbolX( 1, "💇🏾‍♀️", "Woman Getting Haircut Medium-Dark Skin Tone\n:woman_getting_haircut_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💇🏿‍♀️", "Woman Getting Haircut Dark Skin Tone\n:woman_getting_haircut_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏻", "Person Walking Light Skin Tone\n:walking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏼", "Person Walking Medium-Light Skin Tone\n:walking_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏽", "Person Walking Medium Skin Tone\n:walking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏾", "Person Walking Medium-Dark Skin Tone\n:walking_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏿", "Person Walking Dark Skin Tone\n:walking_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏻‍♂️", "Man Walking Light Skin Tone\n:man_walking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏼‍♂️", "Man Walking Medium-Light Skin Tone\n:man_walking_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏽‍♂️", "Man Walking Medium Skin Tone\n:man_walking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏾‍♂️", "Man Walking Medium-Dark Skin Tone\n:man_walking_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏿‍♂️", "Man Walking Dark Skin Tone\n:man_walking_dark_skin_tone:" );

    RegisterSymbolX( 1, "🚶🏻‍♀️", "Woman Walking Light Skin Tone\n:woman_walking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏼‍♀️", "Woman Walking Medium-Light Skin Tone\n:woman_walking_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏽‍♀️", "Woman Walking Medium Skin Tone\n:woman_walking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏾‍♀️", "Woman Walking Medium-Dark Skin Tone\n:woman_walking_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚶🏿‍♀️", "Woman Walking Dark Skin Tone\n:woman_walking_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏻", "Person Standing Light Skin Tone\n:standing_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏼", "Person Standing Medium-Light Skin Tone\n:standing_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏽", "Person Standing Medium Skin Tone\n:standing_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏾", "Person Standing Medium-Dark Skin Tone\n:standing_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏿", "Person Standing Dark Skin Tone\n:standing_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏻‍♂️", "Man Standing Light Skin Tone\n:man_standing_light_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏼‍♂️", "Man Standing Medium-Light Skin Tone\n:man_standing_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧍🏽‍♂️", "Man Standing Medium Skin Tone\n:man_standing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏾‍♂️", "Man Standing Medium-Dark Skin Tone\n:man_standing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏿‍♂️", "Man Standing Dark Skin Tone\n:man_standing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏻‍♀️", "Woman Standing Light Skin Tone\n:woman_standing_light_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏼‍♀️", "Woman Standing Medium-Light Skin Tone\n:woman_standing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏽‍♀️", "Woman Standing Medium Skin Tone\n:woman_standing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏾‍♀️", "Woman Standing Medium-Dark Skin Tone\n:woman_standing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧍🏿‍♀️", "Woman Standing Dark Skin Tone\n:woman_standing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏻", "Person Kneeling Light Skin Tone\n:kneeling_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏼", "Person Kneeling Medium-Light Skin Tone\n:kneeling_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏽", "Person Kneeling Medium Skin Tone\n:kneeling_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏾", "Person Kneeling Medium-Dark Skin Tone\n:kneeling_person_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🧎🏿", "Person Kneeling Dark Skin Tone\n:kneeling_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏻‍♂️", "Man Kneeling Light Skin Tone\n:man_kneeling_light_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏼‍♂️", "Man Kneeling Medium-Light Skin Tone\n:man_kneeling_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏽‍♂️", "Man Kneeling Medium Skin Tone\n:man_kneeling_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏾‍♂️", "Man Kneeling Medium-Dark Skin Tone\n:man_kneeling_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏿‍♂️", "Man Kneeling Dark Skin Tone\n:man_kneeling_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏻‍♀️", "Woman Kneeling Light Skin Tone\n:woman_kneeling_light_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏼‍♀️", "Woman Kneeling Medium-Light Skin Tone\n:woman_kneeling_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏽‍♀️", "Woman Kneeling Medium Skin Tone\n:woman_kneeling_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏾‍♀️", "Woman Kneeling Medium-Dark Skin Tone\n:woman_kneeling_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧎🏿‍♀️", "Woman Kneeling Dark Skin Tone\n:woman_kneeling_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🦯", "Person With White Cane Light Skin Tone\n:person_with_white_cane_light_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏼‍🦯", "Person With White Cane Medium-Light Skin Tone\n:person_with_white_cane_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🦯", "Person With White Cane Medium Skin Tone\n:person_with_white_cane_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🦯", "Person With White Cane Medium-Dark Skin Tone\n:person_with_white_cane_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🦯", "Person With White Cane Dark Skin Tone\n:person_with_white_cane_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🦯", "Man With White Cane Light Skin Tone\n:man_with_white_cane_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🦯", "Man With White Cane Medium-Light Skin Tone\n:man_with_white_cane_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🦯", "Man With White Cane Medium Skin Tone\n:man_with_white_cane_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🦯", "Man With White Cane Medium-Dark Skin Tone\n:man_with_white_cane_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🦯", "Man With White Cane Dark Skin Tone\n:man_with_white_cane_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🦯", "Woman With White Cane Light Skin Tone\n:woman_with_white_cane_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🦯", "Woman With White Cane Medium-Light Skin Tone\n:woman_with_white_cane_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🦯", "Woman With White Cane Medium Skin Tone\n:woman_with_white_cane_medium_skin_tone:" );

    RegisterSymbolX( 1, "👩🏾‍🦯", "Woman With White Cane Medium-Dark Skin Tone\n:woman_with_white_cane_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🦯", "Woman With White Cane Dark Skin Tone\n:woman_with_white_cane_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🦼", "Person In Motorized Wheelchair Light Skin Tone\n:person_in_motorized_wheelchair_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🦼", "Person In Motorized Wheelchair Medium-Light Skin Tone\n:person_in_motorized_wheelchair_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🦼", "Person In Motorized Wheelchair Medium Skin Tone\n:person_in_motorized_wheelchair_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🦼", "Person In Motorized Wheelchair Medium-Dark Skin Tone\n:person_in_motorized_wheelchair_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🦼", "Person In Motorized Wheelchair Dark Skin Tone\n:person_in_motorized_wheelchair_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🦼", "Man In Motorized Wheelchair Light Skin Tone\n:man_in_motorized_wheelchair_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🦼", "Man In Motorized Wheelchair Medium-Light Skin Tone\n:man_in_motorized_wheelchair_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍🦼", "Man In Motorized Wheelchair Medium Skin Tone\n:man_in_motorized_wheelchair_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🦼", "Man In Motorized Wheelchair Medium-Dark Skin Tone\n:man_in_motorized_wheelchair_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🦼", "Man In Motorized Wheelchair Dark Skin Tone\n:man_in_motorized_wheelchair_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏻‍🦼", "Woman In Motorized Wheelchair Light Skin Tone\n:woman_in_motorized_wheelchair_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🦼", "Woman In Motorized Wheelchair Medium-Light Skin Tone\n:woman_in_motorized_wheelchair_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🦼", "Woman In Motorized Wheelchair Medium Skin Tone\n:woman_in_motorized_wheelchair_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🦼", "Woman In Motorized Wheelchair Medium-Dark Skin Tone\n:woman_in_motorized_wheelchair_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🦼", "Woman In Motorized Wheelchair Dark Skin Tone\n:woman_in_motorized_wheelchair_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🦽", "Person In Manual Wheelchair Light Skin Tone\n:person_in_manual_wheelchair_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🦽", "Person In Manual Wheelchair Medium-Light Skin Tone\n:person_in_manual_wheelchair_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🦽", "Person In Manual Wheelchair Medium Skin Tone\n:person_in_manual_wheelchair_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🦽", "Person In Manual Wheelchair Medium-Dark Skin Tone\n:person_in_manual_wheelchair_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🦽", "Person In Manual Wheelchair Dark Skin Tone\n:person_in_manual_wheelchair_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍🦽", "Man In Manual Wheelchair Light Skin Tone\n:man_in_manual_wheelchair_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍🦽", "Man In Manual Wheelchair Medium-Light Skin Tone\n:man_in_manual_wheelchair_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "👨🏽‍🦽", "Man In Manual Wheelchair Medium Skin Tone\n:man_in_manual_wheelchair_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍🦽", "Man In Manual Wheelchair Medium-Dark Skin Tone\n:man_in_manual_wheelchair_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍🦽", "Man In Manual Wheelchair Dark Skin Tone\n:man_in_manual_wheelchair_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍🦽", "Woman In Manual Wheelchair Light Skin Tone\n:woman_in_manual_wheelchair_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍🦽", "Woman In Manual Wheelchair Medium-Light Skin Tone\n:woman_in_manual_wheelchair_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍🦽", "Woman In Manual Wheelchair Medium Skin Tone\n:woman_in_manual_wheelchair_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍🦽", "Woman In Manual Wheelchair Medium-Dark Skin Tone\n:woman_in_manual_wheelchair_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍🦽", "Woman In Manual Wheelchair Dark Skin Tone\n:woman_in_manual_wheelchair_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏻", "Person Running Light Skin Tone\n:running_light_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏼", "Person Running Medium-Light Skin Tone\n:running_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏽", "Person Running Medium Skin Tone\n:running_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏾", "Person Running Medium-Dark Skin Tone\n:running_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🏃🏿", "Person Running Dark Skin Tone\n:running_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏻‍♂️", "Man Running Light Skin Tone\n:man_running_light_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏼‍♂️", "Man Running Medium-Light Skin Tone\n:man_running_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏽‍♂️", "Man Running Medium Skin Tone\n:man_running_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏾‍♂️", "Man Running Medium-Dark Skin Tone\n:man_running_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏿‍♂️", "Man Running Dark Skin Tone\n:man_running_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏻‍♀️", "Woman Running Light Skin Tone\n:woman_running_light_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏼‍♀️", "Woman Running Medium-Light Skin Tone\n:woman_running_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏽‍♀️", "Woman Running Medium Skin Tone\n:woman_running_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏾‍♀️", "Woman Running Medium-Dark Skin Tone\n:woman_running_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏃🏿‍♀️", "Woman Running Dark Skin Tone\n:woman_running_dark_skin_tone:" );
    RegisterSymbolX( 1, "💃🏻", "Woman Dancing Light Skin Tone\n:dancer_light_skin_tone:" );

    RegisterSymbolX( 1, "💃🏼", "Woman Dancing Medium-Light Skin Tone\n:dancer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "💃🏽", "Woman Dancing Medium Skin Tone\n:dancer_medium_skin_tone:" );
    RegisterSymbolX( 1, "💃🏾", "Woman Dancing Medium-Dark Skin Tone\n:dancer_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "💃🏿", "Woman Dancing Dark Skin Tone\n:dancer_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕺🏻", "Man Dancing Light Skin Tone\n:man_dancing_light_skin_tone:" );
    RegisterSymbolX( 1, "🕺🏼", "Man Dancing Medium-Light Skin Tone\n:man_dancing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🕺🏽", "Man Dancing Medium Skin Tone\n:man_dancing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🕺🏾", "Man Dancing Medium-Dark Skin Tone\n:man_dancing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕺🏿", "Man Dancing Dark Skin Tone\n:man_dancing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕴🏻", "Person In Suit Levitating Light Skin Tone\n:levitating_light_skin_tone:" );
    RegisterSymbolX( 1, "🕴🏼", "Person In Suit Levitating Medium-Light Skin Tone\n:levitating_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🕴🏽", "Person In Suit Levitating Medium Skin Tone\n:levitating_medium_skin_tone:" );

    RegisterSymbolX( 1, "🕴🏾", "Person In Suit Levitating Medium-Dark Skin Tone\n:levitating_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🕴🏿", "Person In Suit Levitating Dark Skin Tone\n:levitating_dark_skin_tone:" );
    RegisterSymbolX( 1, "👯", "People With Bunny Ears\n:people_with_bunny_ears:" );
    RegisterSymbolX( 1, "👯‍♂️", "Men With Bunny Ears\n:men_with_bunny_ears:" );
    RegisterSymbolX( 1, "👯‍♀️", "Women With Bunny Ears\n:women_with_bunny_ears:" );
    RegisterSymbolX( 1, "🧖🏻", "Person In Steamy Room Light Skin Tone\n:person_in_steamy_room_light_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏼", "Person In Steamy Room Medium-Light Skin Tone\n:person_in_steamy_room_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏽", "Person In Steamy Room Medium Skin Tone\n:person_in_steamy_room_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏾", "Person In Steamy Room Medium-Dark Skin Tone\n:person_in_steamy_room_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏿", "Person In Steamy Room Dark Skin Tone\n:person_in_steamy_room_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏻‍♂️", "Man In Steamy Room Light Skin Tone\n:man_in_steamy_room_light_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏼‍♂️", "Man In Steamy Room Medium-Light Skin Tone\n:man_in_steamy_room_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🧖🏽‍♂️", "Man In Steamy Room Medium Skin Tone\n:man_in_steamy_room_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏾‍♂️", "Man In Steamy Room Medium-Dark Skin Tone\n:man_in_steamy_room_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏿‍♂️", "Man In Steamy Room Dark Skin Tone\n:man_in_steamy_room_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏻‍♀️", "Woman In Steamy Room Light Skin Tone\n:woman_in_steamy_room_light_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏼‍♀️", "Woman In Steamy Room Medium-Light Skin Tone\n:woman_in_steamy_room_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏽‍♀️", "Woman In Steamy Room Medium Skin Tone\n:woman_in_steamy_room_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏾‍♀️", "Woman In Steamy Room Medium-Dark Skin Tone\n:woman_in_steamy_room_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧖🏿‍♀️", "Woman In Steamy Room Dark Skin Tone\n:woman_in_steamy_room_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏻", "Person Climbing Light Skin Tone\n:climbing_light_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏼", "Person Climbing Medium-Light Skin Tone\n:climbing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏽", "Person Climbing Medium Skin Tone\n:climbing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏾", "Person Climbing Medium-Dark Skin Tone\n:climbing_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🧗🏿", "Person Climbing Dark Skin Tone\n:climbing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏻‍♂️", "Man Climbing Light Skin Tone\n:man_climbing_light_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏼‍♂️", "Man Climbing Medium-Light Skin Tone\n:man_climbing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏽‍♂️", "Man Climbing Medium Skin Tone\n:man_climbing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏾‍♂️", "Man Climbing Medium-Dark Skin Tone\n:man_climbing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏿‍♂️", "Man Climbing Dark Skin Tone\n:man_climbing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏻‍♀️", "Woman Climbing Light Skin Tone\n:woman_climbing_light_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏼‍♀️", "Woman Climbing Medium-Light Skin Tone\n:woman_climbing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏽‍♀️", "Woman Climbing Medium Skin Tone\n:woman_climbing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏾‍♀️", "Woman Climbing Medium-Dark Skin Tone\n:woman_climbing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧗🏿‍♀️", "Woman Climbing Dark Skin Tone\n:woman_climbing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏻", "Person Golfing Light Skin Tone\n:golfing_light_skin_tone:" );

    RegisterSymbolX( 1, "🏌🏼", "Person Golfing Medium-Light Skin Tone\n:golfing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏽", "Person Golfing Medium Skin Tone\n:golfing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏾", "Person Golfing Medium-Dark Skin Tone\n:golfing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏿", "Person Golfing Dark Skin Tone\n:golfing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏻‍♂️", "Man Golfing Light Skin Tone\n:man_golfing_light_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏼‍♂️", "Man Golfing Medium-Light Skin Tone\n:man_golfing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏽‍♂️", "Man Golfing Medium Skin Tone\n:man_golfing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏾‍♂️", "Man Golfing Medium-Dark Skin Tone\n:man_golfing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏿‍♂️", "Man Golfing Dark Skin Tone\n:man_golfing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏻‍♀️", "Woman Golfing Light Skin Tone\n:woman_golfing_light_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏼‍♀️", "Woman Golfing Medium-Light Skin Tone\n:woman_golfing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏽‍♀️", "Woman Golfing Medium Skin Tone\n:woman_golfing_medium_skin_tone:" );

    RegisterSymbolX( 1, "🏌🏾‍♀️", "Woman Golfing Medium-Dark Skin Tone\n:woman_golfing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏌🏿‍♀️", "Woman Golfing Dark Skin Tone\n:woman_golfing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏻", "Person Surfing Light Skin Tone\n:surfing_light_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏼", "Person Surfing Medium-Light Skin Tone\n:surfing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏽", "Person Surfing Medium Skin Tone\n:surfing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏾", "Person Surfing Medium-Dark Skin Tone\n:surfing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏿", "Person Surfing Dark Skin Tone\n:surfing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏻‍♂️", "Man Surfing Light Skin Tone\n:man_surfing_light_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏼‍♂️", "Man Surfing Medium-Light Skin Tone\n:man_surfing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏽‍♂️", "Man Surfing Medium Skin Tone\n:man_surfing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏾‍♂️", "Man Surfing Medium-Dark Skin Tone\n:man_surfing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏿‍♂️", "Man Surfing Dark Skin Tone\n:man_surfing_dark_skin_tone:" );

    RegisterSymbolX( 1, "🏄🏻‍♀️", "Woman Surfing Light Skin Tone\n:woman_surfing_light_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏼‍♀️", "Woman Surfing Medium-Light Skin Tone\n:woman_surfing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏽‍♀️", "Woman Surfing Medium Skin Tone\n:woman_surfing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏾‍♀️", "Woman Surfing Medium-Dark Skin Tone\n:woman_surfing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏄🏿‍♀️", "Woman Surfing Dark Skin Tone\n:woman_surfing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏻", "Person Rowing Boat Light Skin Tone\n:rowing_light_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏼", "Person Rowing Boat Medium-Light Skin Tone\n:rowing_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏽", "Person Rowing Boat Medium Skin Tone\n:rowing_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏾", "Person Rowing Boat Medium-Dark Skin Tone\n:rowing_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏿", "Person Rowing Boat Dark Skin Tone\n:rowing_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏻‍♂️", "Man Rowing Boat Light Skin Tone\n:man_rowing_boat_light_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏼‍♂️", "Man Rowing Boat Medium-Light Skin Tone\n:man_rowing_boat_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🚣🏽‍♂️", "Man Rowing Boat Medium Skin Tone\n:man_rowing_boat_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏾‍♂️", "Man Rowing Boat Medium-Dark Skin Tone\n:man_rowing_boat_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏿‍♂️", "Man Rowing Boat Dark Skin Tone\n:man_rowing_boat_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏻‍♀️", "Woman Rowing Boat Light Skin Tone\n:woman_rowing_boat_light_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏼‍♀️", "Woman Rowing Boat Medium-Light Skin Tone\n:woman_rowing_boat_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏽‍♀️", "Woman Rowing Boat Medium Skin Tone\n:woman_rowing_boat_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏾‍♀️", "Woman Rowing Boat Medium-Dark Skin Tone\n:woman_rowing_boat_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚣🏿‍♀️", "Woman Rowing Boat Dark Skin Tone\n:woman_rowing_boat_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏻", "Person Swimming Light Skin Tone\n:swimming_light_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏼", "Person Swimming Medium-Light Skin Tone\n:swimming_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏽", "Person Swimming Medium Skin Tone\n:swimming_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏾", "Person Swimming Medium-Dark Skin Tone\n:swimming_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🏊🏿", "Person Swimming Dark Skin Tone\n:swimming_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏻‍♂️", "Man Swimming Light Skin Tone\n:man_swimming_light_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏼‍♂️", "Man Swimming Medium-Light Skin Tone\n:man_swimming_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏽‍♂️", "Man Swimming Medium Skin Tone\n:man_swimming_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏾‍♂️", "Man Swimming Medium-Dark Skin Tone\n:man_swimming_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏿‍♂️", "Man Swimming Dark Skin Tone\n:man_swimming_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏻‍♀️", "Woman Swimming Light Skin Tone\n:woman_swimming_light_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏼‍♀️", "Woman Swimming Medium-Light Skin Tone\n:woman_swimming_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏽‍♀️", "Woman Swimming Medium Skin Tone\n:woman_swimming_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏾‍♀️", "Woman Swimming Medium-Dark Skin Tone\n:woman_swimming_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏊🏿‍♀️", "Woman Swimming Dark Skin Tone\n:woman_swimming_dark_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏻", "Person Bouncing Ball Light Skin Tone\n:person_bouncing_ball_light_skin_tone:" );

    RegisterSymbolX( 1, "⛹🏼", "Person Bouncing Ball Medium-Light Skin Tone\n:person_bouncing_ball_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏽", "Person Bouncing Ball Medium Skin Tone\n:person_bouncing_ball_medium_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏾", "Person Bouncing Ball Medium-Dark Skin Tone\n:person_bouncing_ball_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏿", "Person Bouncing Ball Dark Skin Tone\n:person_bouncing_ball_dark_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏻‍♂️", "Man Bouncing Ball Light Skin Tone\n:man_bouncing_ball_light_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏼‍♂️", "Man Bouncing Ball Medium-Light Skin Tone\n:man_bouncing_ball_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏽‍♂️", "Man Bouncing Ball Medium Skin Tone\n:man_bouncing_ball_medium_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏾‍♂️", "Man Bouncing Ball Medium-Dark Skin Tone\n:man_bouncing_ball_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏿‍♂️", "Man Bouncing Ball Dark Skin Tone\n:man_bouncing_ball_dark_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏻‍♀️", "Woman Bouncing Ball Light Skin Tone\n:woman_bouncing_ball_light_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏼‍♀️", "Woman Bouncing Ball Medium-Light Skin Tone\n:woman_bouncing_ball_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏽‍♀️", "Woman Bouncing Ball Medium Skin Tone\n:woman_bouncing_ball_medium_skin_tone:" );

    RegisterSymbolX( 1, "⛹🏾‍♀️", "Woman Bouncing Ball Medium-Dark Skin Tone\n:woman_bouncing_ball_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "⛹🏿‍♀️", "Woman Bouncing Ball Dark Skin Tone\n:woman_bouncing_ball_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏻", "Person Lifting Weights Light Skin Tone\n:weight_lifting_light_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏼", "Person Lifting Weights Medium-Light Skin Tone\n:weight_lifting_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏽", "Person Lifting Weights Medium Skin Tone\n:weight_lifting_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏾", "Person Lifting Weights Medium-Dark Skin Tone\n:weight_lifting_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏿", "Person Lifting Weights Dark Skin Tone\n:weight_lifting_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏻‍♂️", "Man Lifting Weights Light Skin Tone\n:man_lifting_weights_light_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏼‍♂️", "Man Lifting Weights Medium-Light Skin Tone\n:man_lifting_weights_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏽‍♂️", "Man Lifting Weights Medium Skin Tone\n:man_lifting_weights_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏾‍♂️", "Man Lifting Weights Medium-Dark Skin Tone\n:man_lifting_weights_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏿‍♂️", "Man Lifting Weights Dark Skin Tone\n:man_lifting_weights_dark_skin_tone:" );

    RegisterSymbolX( 1, "🏋🏻‍♀️", "Woman Lifting Weights Light Skin Tone\n:woman_lifting_weights_light_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏼‍♀️", "Woman Lifting Weights Medium-Light Skin Tone\n:woman_lifting_weights_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏽‍♀️", "Woman Lifting Weights Medium Skin Tone\n:woman_lifting_weights_medium_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏾‍♀️", "Woman Lifting Weights Medium-Dark Skin Tone\n:woman_lifting_weights_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🏋🏿‍♀️", "Woman Lifting Weights Dark Skin Tone\n:woman_lifting_weights_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏻", "Person Biking Light Skin Tone\n:biking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏼", "Person Biking Medium-Light Skin Tone\n:biking_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏽", "Person Biking Medium Skin Tone\n:biking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏾", "Person Biking Medium-Dark Skin Tone\n:biking_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏿", "Person Biking Dark Skin Tone\n:biking_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏻‍♂️", "Man Biking Light Skin Tone\n:man_biking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏼‍♂️", "Man Biking Medium-Light Skin Tone\n:man_biking_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🚴🏽‍♂️", "Man Biking Medium Skin Tone\n:man_biking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏾‍♂️", "Man Biking Medium-Dark Skin Tone\n:man_biking_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏿‍♂️", "Man Biking Dark Skin Tone\n:man_biking_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏻‍♀️", "Woman Biking Light Skin Tone\n:woman_biking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏼‍♀️", "Woman Biking Medium-Light Skin Tone\n:woman_biking_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏽‍♀️", "Woman Biking Medium Skin Tone\n:woman_biking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏾‍♀️", "Woman Biking Medium-Dark Skin Tone\n:woman_biking_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚴🏿‍♀️", "Woman Biking Dark Skin Tone\n:woman_biking_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏻", "Person Mountain Biking Light Skin Tone\n:mountain_biking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏼", "Person Mountain Biking Medium-Light Skin Tone\n:mountain_biking_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏽", "Person Mountain Biking Medium Skin Tone\n:mountain_biking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏾", "Person Mountain Biking Medium-Dark Skin Tone\n:mountain_biking_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🚵🏿", "Person Mountain Biking Dark Skin Tone\n:mountain_biking_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏻‍♂️", "Man Mountain Biking Light Skin Tone\n:man_mountain_biking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏼‍♂️", "Man Mountain Biking Medium-Light Skin Tone\n:man_mountain_biking_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏽‍♂️", "Man Mountain Biking Medium Skin Tone\n:man_mountain_biking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏾‍♂️", "Man Mountain Biking Medium-Dark Skin Tone\n:man_mountain_biking_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏿‍♂️", "Man Mountain Biking Dark Skin Tone\n:man_mountain_biking_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏻‍♀️", "Woman Mountain Biking Light Skin Tone\n:woman_mountain_biking_light_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏼‍♀️", "Woman Mountain Biking Medium-Light Skin Tone\n:woman_mountain_biking_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏽‍♀️", "Woman Mountain Biking Medium Skin Tone\n:woman_mountain_biking_medium_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏾‍♀️", "Woman Mountain Biking Medium-Dark Skin Tone\n:woman_mountain_biking_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🚵🏿‍♀️", "Woman Mountain Biking Dark Skin Tone\n:woman_mountain_biking_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏻", "Person Cartwheeling Light Skin Tone\n:person_doing_cartwheel_light_skin_tone:" );

    RegisterSymbolX( 1, "🤸🏼", "Person Cartwheeling Medium-Light Skin Tone\n:person_doing_cartwheel_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏽", "Person Cartwheeling Medium Skin Tone\n:person_doing_cartwheel_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏾", "Person Cartwheeling Medium-Dark Skin Tone\n:person_doing_cartwheel_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏿", "Person Cartwheeling Dark Skin Tone\n:person_doing_cartwheel_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏻‍♂️", "Man Cartwheeling Light Skin Tone\n:man_doing_cartwheel_light_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏼‍♂️", "Man Cartwheeling Medium-Light Skin Tone\n:man_doing_cartwheel_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏽‍♂️", "Man Cartwheeling Medium Skin Tone\n:man_doing_cartwheel_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏾‍♂️", "Man Cartwheeling Medium-Dark Skin Tone\n:man_doing_cartwheel_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏿‍♂️", "Man Cartwheeling Dark Skin Tone\n:man_doing_cartwheel_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏻‍♀️", "Woman Cartwheeling Light Skin Tone\n:woman_doing_cartwheel_light_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏼‍♀️", "Woman Cartwheeling Medium-Light Skin Tone\n:woman_doing_cartwheel_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏽‍♀️", "Woman Cartwheeling Medium Skin Tone\n:woman_doing_cartwheel_medium_skin_tone:" );

    RegisterSymbolX( 1, "🤸🏾‍♀️", "Woman Cartwheeling Medium-Dark Skin Tone\n:woman_doing_cartwheel_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤸🏿‍♀️", "Woman Cartwheeling Dark Skin Tone\n:woman_doing_cartwheel_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤼", "People Wrestling\n:people_wrestling:" );
    RegisterSymbolX( 1, "🤼‍♂️", "Men Wrestling\n:men_wrestling:" );
    RegisterSymbolX( 1, "🤼‍♀️", "Women Wrestling\n:women_wrestling:" );
    RegisterSymbolX( 1, "🤽🏻", "Person Playing Water Polo Light Skin Tone\n:water_polo_light_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏼", "Person Playing Water Polo Medium-Light Skin Tone\n:water_polo_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏽", "Person Playing Water Polo Medium Skin Tone\n:water_polo_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏾", "Person Playing Water Polo Medium-Dark Skin Tone\n:water_polo_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏿", "Person Playing Water Polo Dark Skin Tone\n:water_polo_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏻‍♂️", "Man Playing Water Polo Light Skin Tone\n:man_playing_water_polo_light_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏼‍♂️", "Man Playing Water Polo Medium-Light Skin Tone\n:man_playing_water_polo_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🤽🏽‍♂️", "Man Playing Water Polo Medium Skin Tone\n:man_playing_water_polo_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏾‍♂️", "Man Playing Water Polo Medium-Dark Skin Tone\n:man_playing_water_polo_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏿‍♂️", "Man Playing Water Polo Dark Skin Tone\n:man_playing_water_polo_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏻‍♀️", "Woman Playing Water Polo Light Skin Tone\n:woman_playing_water_polo_light_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏼‍♀️", "Woman Playing Water Polo Medium-Light Skin Tone\n:woman_playing_water_polo_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏽‍♀️", "Woman Playing Water Polo Medium Skin Tone\n:woman_playing_water_polo_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏾‍♀️", "Woman Playing Water Polo Medium-Dark Skin Tone\n:woman_playing_water_polo_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤽🏿‍♀️", "Woman Playing Water Polo Dark Skin Tone\n:woman_playing_water_polo_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏻", "Person Playing Handball Light Skin Tone\n:handball_light_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏼", "Person Playing Handball Medium-Light Skin Tone\n:handball_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏽", "Person Playing Handball Medium Skin Tone\n:handball_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏾", "Person Playing Handball Medium-Dark Skin Tone\n:handball_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🤾🏿", "Person Playing Handball Dark Skin Tone\n:handball_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏻‍♂️", "Man Playing Handball Light Skin Tone\n:man_playing_handball_light_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏼‍♂️", "Man Playing Handball Medium-Light Skin Tone\n:man_playing_handball_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏽‍♂️", "Man Playing Handball Medium Skin Tone\n:man_playing_handball_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏾‍♂️", "Man Playing Handball Medium-Dark Skin Tone\n:man_playing_handball_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏿‍♂️", "Man Playing Handball Dark Skin Tone\n:man_playing_handball_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏻‍♀️", "Woman Playing Handball Light Skin Tone\n:woman_playing_handball_light_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏼‍♀️", "Woman Playing Handball Medium-Light Skin Tone\n:woman_playing_handball_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏽‍♀️", "Woman Playing Handball Medium Skin Tone\n:woman_playing_handball_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏾‍♀️", "Woman Playing Handball Medium-Dark Skin Tone\n:woman_playing_handball_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤾🏿‍♀️", "Woman Playing Handball Dark Skin Tone\n:woman_playing_handball_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏻", "Person Juggling Light Skin Tone\n:juggling_light_skin_tone:" );

    RegisterSymbolX( 1, "🤹🏼", "Person Juggling Medium-Light Skin Tone\n:juggling_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏽", "Person Juggling Medium Skin Tone\n:juggling_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏾", "Person Juggling Medium-Dark Skin Tone\n:juggling_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏿", "Person Juggling Dark Skin Tone\n:juggling_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏻‍♂️", "Man Juggling Light Skin Tone\n:man_juggling_light_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏼‍♂️", "Man Juggling Medium-Light Skin Tone\n:man_juggling_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏽‍♂️", "Man Juggling Medium Skin Tone\n:man_juggling_medium_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏾‍♂️", "Man Juggling Medium-Dark Skin Tone\n:man_juggling_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏿‍♂️", "Man Juggling Dark Skin Tone\n:man_juggling_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏻‍♀️", "Woman Juggling Light Skin Tone\n:woman_juggling_light_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏼‍♀️", "Woman Juggling Medium-Light Skin Tone\n:woman_juggling_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏽‍♀️", "Woman Juggling Medium Skin Tone\n:woman_juggling_medium_skin_tone:" );

    RegisterSymbolX( 1, "🤹🏾‍♀️", "Woman Juggling Medium-Dark Skin Tone\n:woman_juggling_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🤹🏿‍♀️", "Woman Juggling Dark Skin Tone\n:woman_juggling_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏻", "Person In Lotus Position Light Skin Tone\n:lotus_position_light_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏼", "Person In Lotus Position Medium-Light Skin Tone\n:lotus_position_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏽", "Person In Lotus Position Medium Skin Tone\n:lotus_position_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏾", "Person In Lotus Position Medium-Dark Skin Tone\n:lotus_position_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏿", "Person In Lotus Position Dark Skin Tone\n:lotus_position_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏻‍♂️", "Man In Lotus Position Light Skin Tone\n:man_in_lotus_position_light_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏼‍♂️", "Man In Lotus Position Medium-Light Skin Tone\n:man_in_lotus_position_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏽‍♂️", "Man In Lotus Position Medium Skin Tone\n:man_in_lotus_position_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏾‍♂️", "Man In Lotus Position Medium-Dark Skin Tone\n:man_in_lotus_position_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏿‍♂️", "Man In Lotus Position Dark Skin Tone\n:man_in_lotus_position_dark_skin_tone:" );

    RegisterSymbolX( 1, "🧘🏻‍♀️", "Woman In Lotus Position Light Skin Tone\n:woman_in_lotus_position_light_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏼‍♀️", "Woman In Lotus Position Medium-Light Skin Tone\n:woman_in_lotus_position_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏽‍♀️", "Woman In Lotus Position Medium Skin Tone\n:woman_in_lotus_position_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏾‍♀️", "Woman In Lotus Position Medium-Dark Skin Tone\n:woman_in_lotus_position_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧘🏿‍♀️", "Woman In Lotus Position Dark Skin Tone\n:woman_in_lotus_position_dark_skin_tone:" );
    RegisterSymbolX( 1, "🛀🏻", "Person Taking Bath Light Skin Tone\n:bath_light_skin_tone:" );
    RegisterSymbolX( 1, "🛀🏼", "Person Taking Bath Medium-Light Skin Tone\n:bath_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🛀🏽", "Person Taking Bath Medium Skin Tone\n:bath_medium_skin_tone:" );
    RegisterSymbolX( 1, "🛀🏾", "Person Taking Bath Medium-Dark Skin Tone\n:bath_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🛀🏿", "Person Taking Bath Dark Skin Tone\n:bath_dark_skin_tone:" );
    RegisterSymbolX( 1, "🛌🏻", "Person In Bed Light Skin Tone\n:person_in_bed_light_skin_tone:" );
    RegisterSymbolX( 1, "🛌🏼", "Person In Bed Medium-Light Skin Tone\n:person_in_bed_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🛌🏽", "Person In Bed Medium Skin Tone\n:person_in_bed_medium_skin_tone:" );
    RegisterSymbolX( 1, "🛌🏾", "Person In Bed Medium-Dark Skin Tone\n:person_in_bed_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🛌🏿", "Person In Bed Dark Skin Tone\n:person_in_bed_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🤝‍🧑🏻", "People Holding Hands Light-Light Skin Tone\n:people_holding_hands_light_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🤝‍🧑🏼", "People Holding Hands Light-Medium-Light Skin Tone\n:people_holding_hands_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🤝‍🧑🏽", "People Holding Hands Light-Medium Skin Tone\n:people_holding_hands_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🤝‍🧑🏾", "People Holding Hands Light-Medium-Dark Skin Tone\n:people_holding_hands_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏻‍🤝‍🧑🏿", "People Holding Hands Light-Dark Skin Tone\n:people_holding_hands_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🤝‍🧑🏻", "People Holding Hands Medium-Light-Light Skin Tone\n:people_holding_hands_medium_light_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🤝‍🧑🏼", "People Holding Hands Medium-Light-Medium-Light Skin Tone\n:people_holding_hands_medium_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🤝‍🧑🏽", "People Holding Hands Medium-Light-Medium Skin Tone\n:people_holding_hands_medium_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏼‍🤝‍🧑🏾", "People Holding Hands Medium-Light-Medium-Dark Skin Tone\n:people_holding_hands_medium_light_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏼‍🤝‍🧑🏿", "People Holding Hands Medium-Light-Dark Skin Tone\n:people_holding_hands_medium_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🤝‍🧑🏻", "People Holding Hands Medium-Light Skin Tone\n:people_holding_hands_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🤝‍🧑🏼", "People Holding Hands Medium-Medium-Light Skin Tone\n:people_holding_hands_medium_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🤝‍🧑🏽", "People Holding Hands Medium-Medium Skin Tone\n:people_holding_hands_medium_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🤝‍🧑🏾", "People Holding Hands Medium-Medium-Dark Skin Tone\n:people_holding_hands_medium_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏽‍🤝‍🧑🏿", "People Holding Hands Medium-Dark Skin Tone\n:people_holding_hands_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🤝‍🧑🏻", "People Holding Hands Medium-Dark-Light Skin Tone\n:people_holding_hands_medium_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🤝‍🧑🏼", "People Holding Hands Medium-Dark-Medium-Light Skin Tone\n:people_holding_hands_medium_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🤝‍🧑🏽", "People Holding Hands Medium-Dark-Medium Skin Tone\n:people_holding_hands_medium_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🤝‍🧑🏾", "People Holding Hands Medium-Dark-Medium-Dark Skin Tone\n:people_holding_hands_medium_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏾‍🤝‍🧑🏿", "People Holding Hands Medium-Dark-Dark Skin Tone\n:people_holding_hands_medium_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🤝‍🧑🏻", "People Holding Hands Dark-Light Skin Tone\n:people_holding_hands_dark_light_skin_tone:" );

    RegisterSymbolX( 1, "🧑🏿‍🤝‍🧑🏼", "People Holding Hands Dark-Medium-Light Skin Tone\n:people_holding_hands_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🤝‍🧑🏽", "People Holding Hands Dark-Medium Skin Tone\n:people_holding_hands_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🤝‍🧑🏾", "People Holding Hands Dark-Medium-Dark Skin Tone\n:people_holding_hands_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🧑🏿‍🤝‍🧑🏿", "People Holding Hands Dark-Dark Skin Tone\n:people_holding_hands_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👭🏻", "Women Holding Hands Light Skin Tone\n:women_holding_hands_light_skin_tone:" );
    RegisterSymbolX( 1, "👭🏼", "Women Holding Hands Medium-Light Skin Tone\n:women_holding_hands_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👭🏽", "Women Holding Hands Medium Skin Tone\n:women_holding_hands_medium_skin_tone:" );
    RegisterSymbolX( 1, "👭🏾", "Women Holding Hands Medium-Dark Skin Tone\n:women_holding_hands_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👭🏿", "Women Holding Hands Dark Skin Tone\n:women_holding_hands_dark_skin_tone:" );
    RegisterSymbolX( 1, "👬🏻", "Men Holding Hands Light Skin Tone\n:men_holding_hands_light_skin_tone:" );
    RegisterSymbolX( 1, "👬🏼", "Men Holding Hands Medium-Light Skin Tone\n:men_holding_hands_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👬🏽", "Men Holding Hands Medium Skin Tone\n:men_holding_hands_medium_skin_tone:" );

    RegisterSymbolX( 1, "👬🏾", "Men Holding Hands Medium-Dark Skin Tone\n:men_holding_hands_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👬🏿", "Men Holding Hands Dark Skin Tone\n:men_holding_hands_dark_skin_tone:" );
    RegisterSymbolX( 1, "👫🏻", "Woman And Man Holding Hands Light Skin Tone\n:woman_man_holding_hands_light_skin_tone:" );
    RegisterSymbolX( 1, "👫🏼", "Woman And Man Holding Hands Medium-Light Skin Tone\n:woman_man_holding_hands_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👫🏽", "Woman And Man Holding Hands Medium Skin Tone\n:woman_man_holding_hands_medium_skin_tone:" );
    RegisterSymbolX( 1, "👫🏾", "Woman And Man Holding Hands Medium-Dark Skin Tone\n:woman_man_holding_hands_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👫🏿", "Woman And Man Holding Hands Dark Skin Tone\n:woman_man_holding_hands_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👩🏻", "Couple With Heart Women Light-Light Skin Tone\n:couple_with_heart_women_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👩🏼", "Couple With Heart Women Light-Medium-Light Skin Tone\n:couple_with_heart_women_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👩🏽", "Couple With Heart Women Light-Medium Skin Tone\n:couple_with_heart_women_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👩🏾", "Couple With Heart Women Light-Medium-Dark Skin Tone\n:couple_with_heart_women_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👩🏿", "Couple With Heart Women Light-Dark Skin Tone\n:couple_with_heart_women_light_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏼‍❤️‍👩🏻", "Couple With Heart Women Medium-Light-Light Skin Tone\n:couple_with_heart_women_medium_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👩🏼", "Couple With Heart Women Medium-Light-Medium-Light Skin Tone\n:couple_with_heart_women_medium_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👩🏽", "Couple With Heart Women Medium-Light-Medium Skin Tone\n:couple_with_heart_women_medium_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👩🏾", "Couple With Heart Women Medium-Light-Medium-Dark Skin Tone\n:couple_with_heart_women_medium_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👩🏿", "Couple With Heart Women Medium-Light-Dark Skin Tone\n:couple_with_heart_women_medium_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👩🏻", "Couple With Heart Women Medium-Light Skin Tone\n:couple_with_heart_women_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👩🏼", "Couple With Heart Women Medium-Medium-Light Skin Tone\n:couple_with_heart_women_medium_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👩🏽", "Couple With Heart Women Medium-Medium Skin Tone\n:couple_with_heart_women_medium_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👩🏾", "Couple With Heart Women Medium-Medium-Dark Skin Tone\n:couple_with_heart_women_medium_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👩🏿", "Couple With Heart Women Medium-Dark Skin Tone\n:couple_with_heart_women_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍👩🏻", "Couple With Heart Women Medium-Dark-Light Skin Tone\n:couple_with_heart_women_medium_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍👩🏼", "Couple With Heart Women Medium-Dark-Medium-Light Skin Tone\n:couple_with_heart_women_medium_dark_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "👩🏾‍❤️‍👩🏽", "Couple With Heart Women Medium-Dark-Medium Skin Tone\n:couple_with_heart_women_medium_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍👩🏾", "Couple With Heart Women Medium-Dark-Medium-Dark Skin Tone\n:couple_with_heart_women_medium_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍👩🏿", "Couple With Heart Women Medium-Dark-Dark Skin Tone\n:couple_with_heart_women_medium_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👩🏻", "Couple With Heart Women Dark-Light Skin Tone\n:couple_with_heart_women_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👩🏼", "Couple With Heart Women Dark-Medium-Light Skin Tone\n:couple_with_heart_women_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👩🏽", "Couple With Heart Women Dark-Medium Skin Tone\n:couple_with_heart_women_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👩🏾", "Couple With Heart Women Dark-Medium-Dark Skin Tone\n:couple_with_heart_women_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👩🏿", "Couple With Heart Women Dark-Dark Skin Tone\n:couple_with_heart_women_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍❤️‍👨🏻", "Couple With Heart Men Light-Light Skin Tone\n:couple_with_heart_men_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍❤️‍👨🏼", "Couple With Heart Men Light-Medium-Light Skin Tone\n:couple_with_heart_men_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍❤️‍👨🏽", "Couple With Heart Men Light-Medium Skin Tone\n:couple_with_heart_men_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍❤️‍👨🏾", "Couple With Heart Men Light-Medium-Dark Skin Tone\n:couple_with_heart_men_light_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👨🏻‍❤️‍👨🏿", "Couple With Heart Men Light-Dark Skin Tone\n:couple_with_heart_men_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍👨🏻", "Couple With Heart Men Medium-Light-Light Skin Tone\n:couple_with_heart_men_medium_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍👨🏼", "Couple With Heart Men Medium-Light-Medium-Light Skin Tone\n:couple_with_heart_men_medium_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍👨🏽", "Couple With Heart Men Medium-Light-Medium Skin Tone\n:couple_with_heart_men_medium_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍👨🏾", "Couple With Heart Men Medium-Light-Medium-Dark Skin Tone\n:couple_with_heart_men_medium_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍👨🏿", "Couple With Heart Men Medium-Light-Dark Skin Tone\n:couple_with_heart_men_medium_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍👨🏻", "Couple With Heart Men Medium-Light Skin Tone\n:couple_with_heart_men_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍👨🏼", "Couple With Heart Men Medium-Medium-Light Skin Tone\n:couple_with_heart_men_medium_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍👨🏽", "Couple With Heart Men Medium-Medium Skin Tone\n:couple_with_heart_men_medium_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍👨🏾", "Couple With Heart Men Medium-Medium-Dark Skin Tone\n:couple_with_heart_men_medium_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍👨🏿", "Couple With Heart Men Medium-Dark Skin Tone\n:couple_with_heart_men_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍👨🏻", "Couple With Heart Men Medium-Dark-Light Skin Tone\n:couple_with_heart_men_medium_dark_light_skin_tone:" );

    RegisterSymbolX( 1, "👨🏾‍❤️‍👨🏼", "Couple With Heart Men Medium-Dark-Medium-Light Skin Tone\n:couple_with_heart_men_medium_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍👨🏽", "Couple With Heart Men Medium-Dark-Medium Skin Tone\n:couple_with_heart_men_medium_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍👨🏾", "Couple With Heart Men Medium-Dark-Medium-Dark Skin Tone\n:couple_with_heart_men_medium_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍👨🏿", "Couple With Heart Men Medium-Dark-Dark Skin Tone\n:couple_with_heart_men_medium_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍👨🏻", "Couple With Heart Men Dark-Light Skin Tone\n:couple_with_heart_men_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍👨🏼", "Couple With Heart Men Dark-Medium-Light Skin Tone\n:couple_with_heart_men_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍👨🏽", "Couple With Heart Men Dark-Medium Skin Tone\n:couple_with_heart_men_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍👨🏾", "Couple With Heart Men Dark-Medium-Dark Skin Tone\n:couple_with_heart_men_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍👨🏿", "Couple With Heart Men Dark-Dark Skin Tone\n:couple_with_heart_men_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👨🏻", "Couple With Heart Woman Man Light-Light Skin Tone\n:couple_with_heart_woman_man_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👨🏼", "Couple With Heart Woman Man Light-Medium-Light Skin Tone\n:couple_with_heart_woman_man_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👨🏽", "Couple With Heart Woman Man Light-Medium Skin Tone\n:couple_with_heart_woman_man_light_medium_skin_tone:" );

    RegisterSymbolX( 1, "👩🏻‍❤️‍👨🏾", "Couple With Heart Woman Man Light-Medium-Dark Skin Tone\n:couple_with_heart_woman_man_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍👨🏿", "Couple With Heart Woman Man Light-Dark Skin Tone\n:couple_with_heart_woman_man_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👨🏻", "Couple With Heart Woman Man Medium-Light-Light Skin Tone\n:couple_with_heart_woman_man_medium_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👨🏼", "Couple With Heart Woman Man Medium-Light-Medium-Light Skin Tone\n:couple_with_heart_woman_man_medium_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👨🏽", "Couple With Heart Woman Man Medium-Light-Medium Skin Tone\n:couple_with_heart_woman_man_medium_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👨🏾", "Couple With Heart Woman Man Medium-Light-Medium-Dark Skin Tone\n:couple_with_heart_woman_man_medium_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍👨🏿", "Couple With Heart Woman Man Medium-Light-Dark Skin Tone\n:couple_with_heart_woman_man_medium_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👨🏻", "Couple With Heart Woman Man Medium-Light Skin Tone\n:couple_with_heart_woman_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👨🏼", "Couple With Heart Woman Man Medium-Medium-Light Skin Tone\n:couple_with_heart_woman_man_medium_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👨🏽", "Couple With Heart Woman Man Medium-Medium Skin Tone\n:couple_with_heart_woman_man_medium_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👨🏾", "Couple With Heart Woman Man Medium-Medium-Dark Skin Tone\n:couple_with_heart_woman_man_medium_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍👨🏿", "Couple With Heart Woman Man Medium-Dark Skin Tone\n:couple_with_heart_woman_man_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏾‍❤️‍👨🏻", "Couple With Heart Woman Man Medium-Dark-Light Skin Tone\n:couple_with_heart_woman_man_medium_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍👨🏼", "Couple With Heart Woman Man Medium-Dark-Medium-Light Skin Tone\n:couple_with_heart_woman_man_medium_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍👨🏽", "Couple With Heart Woman Man Medium-Dark-Medium Skin Tone\n:couple_with_heart_woman_man_medium_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍👨🏾", "Couple With Heart Woman Man Medium-Dark-Medium-Dark Skin Tone\n:couple_with_heart_woman_man_medium_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍👨🏿", "Couple With Heart Woman Man Medium-Dark-Dark Skin Tone\n:couple_with_heart_woman_man_medium_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👨🏻", "Couple With Heart Woman Man Dark-Light Skin Tone\n:couple_with_heart_woman_man_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👨🏼", "Couple With Heart Woman Man Dark-Medium-Light Skin Tone\n:couple_with_heart_woman_man_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👨🏽", "Couple With Heart Woman Man Dark-Medium Skin Tone\n:couple_with_heart_woman_man_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👨🏾", "Couple With Heart Woman Man Dark-Medium-Dark Skin Tone\n:couple_with_heart_woman_man_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍👨🏿", "Couple With Heart Woman Man Dark-Dark Skin Tone\n:couple_with_heart_woman_man_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👩🏻", "Kiss Women Light-Light Skin Tone\n:kiss_women_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👩🏼", "Kiss Women Light-Medium-Light Skin Tone\n:kiss_women_light_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👩🏽", "Kiss Women Light-Medium Skin Tone\n:kiss_women_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👩🏾", "Kiss Women Light-Medium-Dark Skin Tone\n:kiss_women_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👩🏿", "Kiss Women Light-Dark Skin Tone\n:kiss_women_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👩🏻", "Kiss Women Medium-Light-Light Skin Tone\n:kiss_women_medium_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👩🏼", "Kiss Women Medium-Light-Medium-Light Skin Tone\n:kiss_women_medium_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👩🏽", "Kiss Women Medium-Light-Medium Skin Tone\n:kiss_women_medium_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👩🏾", "Kiss Women Medium-Light-Medium-Dark Skin Tone\n:kiss_women_medium_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👩🏿", "Kiss Women Medium-Light-Dark Skin Tone\n:kiss_women_medium_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👩🏻", "Kiss Women Medium-Light Skin Tone\n:kiss_women_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👩🏼", "Kiss Women Medium-Medium-Light Skin Tone\n:kiss_women_medium_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👩🏽", "Kiss Women Medium-Medium Skin Tone\n:kiss_women_medium_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👩🏾", "Kiss Women Medium-Medium-Dark Skin Tone\n:kiss_women_medium_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👩🏿", "Kiss Women Medium-Dark Skin Tone\n:kiss_women_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👩🏻", "Kiss Women Medium-Dark-Light Skin Tone\n:kiss_women_medium_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👩🏼", "Kiss Women Medium-Dark-Medium-Light Skin Tone\n:kiss_women_medium_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👩🏽", "Kiss Women Medium-Dark-Medium Skin Tone\n:kiss_women_medium_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👩🏾", "Kiss Women Medium-Dark-Medium-Dark Skin Tone\n:kiss_women_medium_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👩🏿", "Kiss Women Medium-Dark-Dark Skin Tone\n:kiss_women_medium_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👩🏻", "Kiss Women Dark-Light Skin Tone\n:kiss_women_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👩🏼", "Kiss Women Dark-Medium-Light Skin Tone\n:kiss_women_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👩🏽", "Kiss Women Dark-Medium Skin Tone\n:kiss_women_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👩🏾", "Kiss Women Dark-Medium-Dark Skin Tone\n:kiss_women_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👩🏿", "Kiss Women Dark-Dark Skin Tone\n:kiss_women_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍❤️‍💋‍👨🏻", "Kiss Men Light-Light Skin Tone\n:kiss_men_light_light_skin_tone:" );

    RegisterSymbolX( 1, "👨🏻‍❤️‍💋‍👨🏼", "Kiss Men Light-Medium-Light Skin Tone\n:kiss_men_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍❤️‍💋‍👨🏽", "Kiss Men Light-Medium Skin Tone\n:kiss_men_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍❤️‍💋‍👨🏾", "Kiss Men Light-Medium-Dark Skin Tone\n:kiss_men_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏻‍❤️‍💋‍👨🏿", "Kiss Men Light-Dark Skin Tone\n:kiss_men_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍💋‍👨🏻", "Kiss Men Medium-Light-Light Skin Tone\n:kiss_men_medium_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍💋‍👨🏼", "Kiss Men Medium-Light-Medium-Light Skin Tone\n:kiss_men_medium_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍💋‍👨🏽", "Kiss Men Medium-Light-Medium Skin Tone\n:kiss_men_medium_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍💋‍👨🏾", "Kiss Men Medium-Light-Medium-Dark Skin Tone\n:kiss_men_medium_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏼‍❤️‍💋‍👨🏿", "Kiss Men Medium-Light-Dark Skin Tone\n:kiss_men_medium_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍💋‍👨🏻", "Kiss Men Medium-Light Skin Tone\n:kiss_men_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍💋‍👨🏼", "Kiss Men Medium-Medium-Light Skin Tone\n:kiss_men_medium_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍💋‍👨🏽", "Kiss Men Medium-Medium Skin Tone\n:kiss_men_medium_medium_skin_tone:" );

    RegisterSymbolX( 1, "👨🏽‍❤️‍💋‍👨🏾", "Kiss Men Medium-Medium-Dark Skin Tone\n:kiss_men_medium_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏽‍❤️‍💋‍👨🏿", "Kiss Men Medium-Dark Skin Tone\n:kiss_men_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍💋‍👨🏻", "Kiss Men Medium-Dark-Light Skin Tone\n:kiss_men_medium_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍💋‍👨🏼", "Kiss Men Medium-Dark-Medium-Light Skin Tone\n:kiss_men_medium_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍💋‍👨🏽", "Kiss Men Medium-Dark-Medium Skin Tone\n:kiss_men_medium_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍💋‍👨🏾", "Kiss Men Medium-Dark-Medium-Dark Skin Tone\n:kiss_men_medium_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏾‍❤️‍💋‍👨🏿", "Kiss Men Medium-Dark-Dark Skin Tone\n:kiss_men_medium_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍💋‍👨🏻", "Kiss Men Dark-Light Skin Tone\n:kiss_men_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍💋‍👨🏼", "Kiss Men Dark-Medium-Light Skin Tone\n:kiss_men_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍💋‍👨🏽", "Kiss Men Dark-Medium Skin Tone\n:kiss_men_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍💋‍👨🏾", "Kiss Men Dark-Medium-Dark Skin Tone\n:kiss_men_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨🏿‍❤️‍💋‍👨🏿", "Kiss Men Dark-Dark Skin Tone\n:kiss_men_dark_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👨🏻", "Kiss Woman Man Light-Light Skin Tone\n:kiss_woman_man_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👨🏼", "Kiss Woman Man Light-Medium-Light Skin Tone\n:kiss_woman_man_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👨🏽", "Kiss Woman Man Light-Medium Skin Tone\n:kiss_woman_man_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👨🏾", "Kiss Woman Man Light-Medium-Dark Skin Tone\n:kiss_woman_man_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏻‍❤️‍💋‍👨🏿", "Kiss Woman Man Light-Dark Skin Tone\n:kiss_woman_man_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👨🏻", "Kiss Woman Man Medium-Light-Light Skin Tone\n:kiss_woman_man_medium_light_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👨🏼", "Kiss Woman Man Medium-Light-Medium-Light Skin Tone\n:kiss_woman_man_medium_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👨🏽", "Kiss Woman Man Medium-Light-Medium Skin Tone\n:kiss_woman_man_medium_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👨🏾", "Kiss Woman Man Medium-Light-Medium-Dark Skin Tone\n:kiss_woman_man_medium_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏼‍❤️‍💋‍👨🏿", "Kiss Woman Man Medium-Light-Dark Skin Tone\n:kiss_woman_man_medium_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👨🏻", "Kiss Woman Man Medium-Light Skin Tone\n:kiss_woman_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👨🏼", "Kiss Woman Man Medium-Medium-Light Skin Tone\n:kiss_woman_man_medium_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👨🏽", "Kiss Woman Man Medium-Medium Skin Tone\n:kiss_woman_man_medium_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👨🏾", "Kiss Woman Man Medium-Medium-Dark Skin Tone\n:kiss_woman_man_medium_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏽‍❤️‍💋‍👨🏿", "Kiss Woman Man Medium-Dark Skin Tone\n:kiss_woman_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👨🏻", "Kiss Woman Man Medium-Dark-Light Skin Tone\n:kiss_woman_man_medium_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👨🏼", "Kiss Woman Man Medium-Dark-Medium-Light Skin Tone\n:kiss_woman_man_medium_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👨🏽", "Kiss Woman Man Medium-Dark-Medium Skin Tone\n:kiss_woman_man_medium_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👨🏾", "Kiss Woman Man Medium-Dark-Medium-Dark Skin Tone\n:kiss_woman_man_medium_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏾‍❤️‍💋‍👨🏿", "Kiss Woman Man Medium-Dark-Dark Skin Tone\n:kiss_woman_man_medium_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👨🏻", "Kiss Woman Man Dark-Light Skin Tone\n:kiss_woman_man_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👨🏼", "Kiss Woman Man Dark-Medium-Light Skin Tone\n:kiss_woman_man_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👨🏽", "Kiss Woman Man Dark-Medium Skin Tone\n:kiss_woman_man_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👨🏾", "Kiss Woman Man Dark-Medium-Dark Skin Tone\n:kiss_woman_man_dark_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "👩🏿‍❤️‍💋‍👨🏿", "Kiss Woman Man Dark-Dark Skin Tone\n:kiss_woman_man_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "👨‍👩‍👦", "Family Man Woman Boy\n:family_man_woman_boy:" );
    RegisterSymbolX( 1, "👨‍👩‍👧", "Family Man Woman Girl\n:family_man_woman_girl:" );
    RegisterSymbolX( 1, "👨‍👩‍👧‍👦", "Family Man Woman Girl Boy\n:family_man_woman_girl_boy:" );
    RegisterSymbolX( 1, "👨‍👩‍👦‍👦", "Family Man Woman Boy Boy\n:family_man_woman_boy_boy:" );
    RegisterSymbolX( 1, "👨‍👩‍👧‍👧", "Family Man Woman Girl Girl\n:family_man_woman_girl_girl:" );
    RegisterSymbolX( 1, "👨‍👨‍👦", "Family Man Man Boy\n:family_man_man_boy:" );
    RegisterSymbolX( 1, "👨‍👨‍👧", "Family Man Man Girl\n:family_man_man_girl:" );
    RegisterSymbolX( 1, "👨‍👨‍👧‍👦", "Family Man Man Girl Boy\n:family_man_man_girl_boy:" );
    RegisterSymbolX( 1, "👨‍👨‍👦‍👦", "Family Man Man Boy Boy\n:family_man_man_boy_boy:" );
    RegisterSymbolX( 1, "👨‍👨‍👧‍👧", "Family Man Man Girl Girl\n:family_man_man_girl_girl:" );
    RegisterSymbolX( 1, "👩‍👩‍👦", "Family Woman Woman Boy\n:family_woman_woman_boy:" );

    RegisterSymbolX( 1, "👩‍👩‍👧", "Family Woman Woman Girl\n:family_woman_woman_girl:" );
    RegisterSymbolX( 1, "👩‍👩‍👧‍👦", "Family Woman Woman Girl Boy\n:family_woman_woman_girl_boy:" );
    RegisterSymbolX( 1, "👩‍👩‍👦‍👦", "Family Woman Woman Boy Boy\n:family_woman_woman_boy_boy:" );
    RegisterSymbolX( 1, "👩‍👩‍👧‍👧", "Family Woman Woman Girl Girl\n:family_woman_woman_girl_girl:" );
    RegisterSymbolX( 1, "👨‍👦", "Family Man Boy\n:family_man_boy:" );
    RegisterSymbolX( 1, "👨‍👦‍👦", "Family Man Boy Boy\n:family_man_boy_boy:" );
    RegisterSymbolX( 1, "👨‍👧", "Family Man Girl\n:family_man_girl:" );
    RegisterSymbolX( 1, "👨‍👧‍👦", "Family Man Girl Boy\n:family_man_girl_boy:" );
    RegisterSymbolX( 1, "👨‍👧‍👧", "Family Man Girl Girl\n:family_man_girl_girl:" );
    RegisterSymbolX( 1, "👩‍👦", "Family Woman Boy\n:family_woman_boy:" );
    RegisterSymbolX( 1, "👩‍👦‍👦", "Family Woman Boy Boy\n:family_woman_boy_boy:" );
    RegisterSymbolX( 1, "👩‍👧", "Family Woman Girl\n:family_woman_girl:" );

    RegisterSymbolX( 1, "👩‍👧‍👦", "Family Woman Girl Boy\n:family_woman_girl_boy:" );
    RegisterSymbolX( 1, "👩‍👧‍👧", "Family Woman Girl Girl\n:family_woman_girl_girl:" );
    RegisterSymbolX( 1, "🫱🏻‍🫲🏻", "Handshake Light-Light Skin Tone\n:handshake_light_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏻‍🫲🏼", "Handshake Light-Medium-Light Skin Tone\n:handshake_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏻‍🫲🏽", "Handshake Light-Medium Skin Tone\n:handshake_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏻‍🫲🏾", "Handshake Light-Medium-Dark Skin Tone\n:handshake_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏻‍🫲🏿", "Handshake Light-Dark Skin Tone\n:handshake_light_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏼‍🫲🏻", "Handshake Medium-Light-Light Skin Tone\n:handshake_medium_light_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏼‍🫲🏼", "Handshake Medium-Light-Medium-Light Skin Tone\n:handshake_medium_light_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏼‍🫲🏽", "Handshake Medium-Light-Medium Skin Tone\n:handshake_medium_light_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏼‍🫲🏾", "Handshake Medium-Light-Medium-Dark Skin Tone\n:handshake_medium_light_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏼‍🫲🏿", "Handshake Medium-Light-Dark Skin Tone\n:handshake_medium_light_dark_skin_tone:" );

    RegisterSymbolX( 1, "🫱🏽‍🫲🏻", "Handshake Medium-Light Skin Tone\n:handshake_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏽‍🫲🏼", "Handshake Medium-Medium-Light Skin Tone\n:handshake_medium_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏽‍🫲🏽", "Handshake Medium-Medium Skin Tone\n:handshake_medium_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏽‍🫲🏾", "Handshake Medium-Medium-Dark Skin Tone\n:handshake_medium_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏽‍🫲🏿", "Handshake Medium-Dark Skin Tone\n:handshake_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏾‍🫲🏻", "Handshake Medium-Dark-Light Skin Tone\n:handshake_medium_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏾‍🫲🏼", "Handshake Medium-Dark-Medium-Light Skin Tone\n:handshake_medium_dark_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏾‍🫲🏽", "Handshake Medium-Dark-Medium Skin Tone\n:handshake_medium_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏾‍🫲🏾", "Handshake Medium-Dark-Medium-Dark Skin Tone\n:handshake_medium_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏾‍🫲🏿", "Handshake Medium-Dark-Dark Skin Tone\n:handshake_medium_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏿‍🫲🏻", "Handshake Dark-Light Skin Tone\n:handshake_dark_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏿‍🫲🏼", "Handshake Dark-Medium-Light Skin Tone\n:handshake_dark_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🫱🏿‍🫲🏽", "Handshake Dark-Medium Skin Tone\n:handshake_dark_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏿‍🫲🏾", "Handshake Dark-Medium-Dark Skin Tone\n:handshake_dark_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏿‍🫲🏿", "Handshake Dark-Dark Skin Tone\n:handshake_dark_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫂", "People Hugging\n:people_hugging:" );
    RegisterSymbolX( 1, "👤", "Bust In Silhouette\n:bust_in_silhouette:" );
    RegisterSymbolX( 1, "👥", "Busts In Silhouette\n:busts_in_silhouette:" );
    RegisterSymbolX( 1, "🗣️", "Speaking Head\n:speaking_head:" );
    RegisterSymbolX( 1, "👣", "Footprints\n:footprints:" );
    RegisterSymbolX( 1, "🫦", "Biting Lip\n:biting_lip:" );
    RegisterSymbolX( 1, "🫄", "Pregnant Person\n:pregnant_person:" );
    RegisterSymbolX( 1, "🫃", "Pregnant Man\n:pregnant_man:" );
    RegisterSymbolX( 1, "🫅", "Person With Crown\n:person_with_crown:" );

    RegisterSymbolX( 1, "🫶", "Heart Hands\n:heart_hands:" );
    RegisterSymbolX( 1, "🫱", "Rightwards Hand\n:rightwards_hand:" );
    RegisterSymbolX( 1, "🫲", "Leftwards Hand\n:leftwards_hand:" );
    RegisterSymbolX( 1, "🫳", "Palm Down Hand\n:palm_down_hand:" );
    RegisterSymbolX( 1, "🫴", "Palm Up Hand\n:palm_up_hand:" );
    RegisterSymbolX( 1, "🫰", "Hand With Index Finger And Thumb Crossed\n:hand_with_index_finger_and_thumb_crossed:" );
    RegisterSymbolX( 1, "🫵", "Index Pointing At The Viewer\n:index_pointing_at_the_viewer:" );
    RegisterSymbolX( 1, "🫸", "Rightwards Pushing Hand\n:rightwards_pushing_hand:" );
    RegisterSymbolX( 1, "🫷", "Leftwards Pushing Hand\n:leftwards_pushing_hand:" );
    RegisterSymbolX( 1, "🫷🏻", "Leftwards Pushing Hand Light Skin Tone\n:leftwards_pushing_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🫷🏼", "Leftwards Pushing Hand Medium-Light Skin Tone\n:leftwards_pushing_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫷🏽", "Leftwards Pushing Hand Medium Skin Tone\n:leftwards_pushing_hand_medium_skin_tone:" );

    RegisterSymbolX( 1, "🫷🏾", "Leftwards Pushing Hand Medium-Dark Skin Tone\n:leftwards_pushing_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫷🏿", "Leftwards Pushing Hand Dark Skin Tone\n:leftwards_pushing_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫸🏻", "Rightwards Pushing Hand Light Skin Tone\n:rightwards_pushing_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🫸🏼", "Rightwards Pushing Hand Medium-Light Skin Tone\n:rightwards_pushing_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫸🏽", "Rightwards Pushing Hand Medium Skin Tone\n:rightwards_pushing_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫸🏾", "Rightwards Pushing Hand Medium-Dark Skin Tone\n:rightwards_pushing_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫸🏿", "Rightwards Pushing Hand Dark Skin Tone\n:rightwards_pushing_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏻", "Rightwards Hand Light Skin Tone\n:rightwards_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏼", "Rightwards Hand Medium-Light Skin Tone\n:rightwards_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏽", "Rightwards Hand Medium Skin Tone\n:rightwards_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏾", "Rightwards Hand Medium-Dark Skin Tone\n:rightwards_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫱🏿", "Rightwards Hand Dark Skin Tone\n:rightwards_hand_dark_skin_tone:" );

    RegisterSymbolX( 1, "🫲🏻", "Leftwards Hand Light Skin Tone\n:leftwards_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🫲🏼", "Leftwards Hand Medium-Light Skin Tone\n:leftwards_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫲🏽", "Leftwards Hand Medium Skin Tone\n:leftwards_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫲🏾", "Leftwards Hand Medium-Dark Skin Tone\n:leftwards_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫲🏿", "Leftwards Hand Dark Skin Tone\n:leftwards_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫳🏻", "Palm Down Hand Light Skin Tone\n:palm_down_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🫳🏼", "Palm Down Hand Medium-Light Skin Tone\n:palm_down_hand_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫳🏽", "Palm Down Hand Medium Skin Tone\n:palm_down_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫳🏾", "Palm Down Hand Medium-Dark Skin Tone\n:palm_down_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫳🏿", "Palm Down Hand Dark Skin Tone\n:palm_down_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫴🏻", "Palm Up Hand Light Skin Tone\n:palm_up_hand_light_skin_tone:" );
    RegisterSymbolX( 1, "🫴🏼", "Palm Up Hand Medium-Light Skin Tone\n:palm_up_hand_medium_light_skin_tone:" );

    RegisterSymbolX( 1, "🫴🏽", "Palm Up Hand Medium Skin Tone\n:palm_up_hand_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫴🏾", "Palm Up Hand Medium-Dark Skin Tone\n:palm_up_hand_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫴🏿", "Palm Up Hand Dark Skin Tone\n:palm_up_hand_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏻", "Hand With Index Finger And Thumb Crossed Light Skin Tone\n:hand_with_index_finger_and_thumb_crossed_light_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏼", "Hand With Index Finger And Thumb Crossed Medium-Light Skin Tone\n:hand_with_index_finger_and_thumb_crossed_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏽", "Hand With Index Finger And Thumb Crossed Medium Skin Tone\n:hand_with_index_finger_and_thumb_crossed_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏾", "Hand With Index Finger And Thumb Crossed Medium-Dark Skin Tone\n:hand_with_index_finger_and_thumb_crossed_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫰🏿", "Hand With Index Finger And Thumb Crossed Dark Skin Tone\n:hand_with_index_finger_and_thumb_crossed_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏻", "Index Pointing At The Viewer Light Skin Tone\n:index_pointing_at_the_viewer_light_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏼", "Index Pointing At The Viewer Medium-Light Skin Tone\n:index_pointing_at_the_viewer_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏽", "Index Pointing At The Viewer Medium Skin Tone\n:index_pointing_at_the_viewer_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫵🏾", "Index Pointing At The Viewer Medium-Dark Skin Tone\n:index_pointing_at_the_viewer_medium_dark_skin_tone:" );

    RegisterSymbolX( 1, "🫵🏿", "Index Pointing At The Viewer Dark Skin Tone\n:index_pointing_at_the_viewer_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏻", "Heart Hands Light Skin Tone\n:heart_hands_light_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏼", "Heart Hands Medium-Light Skin Tone\n:heart_hands_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏽", "Heart Hands Medium Skin Tone\n:heart_hands_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏾", "Heart Hands Medium-Dark Skin Tone\n:heart_hands_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫶🏿", "Heart Hands Dark Skin Tone\n:heart_hands_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫅🏻", "Person With Crown Light Skin Tone\n:person_with_crown_light_skin_tone:" );
    RegisterSymbolX( 1, "🫅🏼", "Person With Crown Medium-Light Skin Tone\n:person_with_crown_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫅🏽", "Person With Crown Medium Skin Tone\n:person_with_crown_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫅🏾", "Person With Crown Medium-Dark Skin Tone\n:person_with_crown_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫅🏿", "Person With Crown Dark Skin Tone\n:person_with_crown_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫃🏻", "Pregnant Man Light Skin Tone\n:pregnant_man_light_skin_tone:" );

    RegisterSymbolX( 1, "🫃🏼", "Pregnant Man Medium-Light Skin Tone\n:pregnant_man_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫃🏽", "Pregnant Man Medium Skin Tone\n:pregnant_man_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫃🏾", "Pregnant Man Medium-Dark Skin Tone\n:pregnant_man_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫃🏿", "Pregnant Man Dark Skin Tone\n:pregnant_man_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏻", "Pregnant Person Light Skin Tone\n:pregnant_person_light_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏼", "Pregnant Person Medium-Light Skin Tone\n:pregnant_person_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏽", "Pregnant Person Medium Skin Tone\n:pregnant_person_medium_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏾", "Pregnant Person Medium-Dark Skin Tone\n:pregnant_person_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫄🏿", "Pregnant Person Dark Skin Tone\n:pregnant_person_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫦🏻", "Biting Lip Light Skin Tone\n:biting_lip_light_skin_tone:" );
    RegisterSymbolX( 1, "🫦🏼", "Biting Lip Medium-Light Skin Tone\n:biting_lip_medium_light_skin_tone:" );
    RegisterSymbolX( 1, "🫦🏽", "Biting Lip Medium Skin Tone\n:biting_lip_medium_skin_tone:" );

    RegisterSymbolX( 1, "🫦🏾", "Biting Lip Medium-Dark Skin Tone\n:biting_lip_medium_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫦🏿", "Biting Lip Dark Skin Tone\n:biting_lip_dark_skin_tone:" );
    RegisterSymbolX( 1, "🫄‍♂️", "Pregnant Man Variant\n:pregnant_man_variant:" );
    RegisterSymbolX( 1, "🫄‍♀️", "Pregnant Woman Variant\n:pregnant_woman_variant:" );
    RegisterSymbolX( 1, "🫃‍♂️", "Pregnant Man Male Variant\n:pregnant_man_male_variant:" );
    RegisterSymbolX( 1, "🫃‍♀️", "Pregnant Man Female Variant\n:pregnant_man_female_variant:" );
    RegisterSymbolX( 1, "🫅‍♂️", "Person With Crown Male Variant\n:person_with_crown_male_variant:" );
    RegisterSymbolX( 1, "🫅‍♀️", "Person With Crown Female Variant\n:person_with_crown_female_variant:" );
    RegisterSymbolX( 1, "🫶🏻‍🫶🏼", "Heart Hands Mixed Light Medium-Light\n:heart_hands_mixed_light_medium_light:" );
    RegisterSymbolX( 1, "🫶🏻‍🫶🏽", "Heart Hands Mixed Light Medium\n:heart_hands_mixed_light_medium:" );
    RegisterSymbolX( 1, "🫶🏻‍🫶🏾", "Heart Hands Mixed Light Medium-Dark\n:heart_hands_mixed_light_medium_dark:" );
    RegisterSymbolX( 1, "🫶🏻‍🫶🏿", "Heart Hands Mixed Light Dark\n:heart_hands_mixed_light_dark:" );

    RegisterSymbolX( 1, "🫶🏼‍🫶🏻", "Heart Hands Mixed Medium-Light Light\n:heart_hands_mixed_medium_light_light:" );
    RegisterSymbolX( 1, "🫶🏼‍🫶🏽", "Heart Hands Mixed Medium-Light Medium\n:heart_hands_mixed_medium_light_medium:" );
    RegisterSymbolX( 1, "🫶🏼‍🫶🏾", "Heart Hands Mixed Medium-Light Medium-Dark\n:heart_hands_mixed_medium_light_medium_dark:" );
    RegisterSymbolX( 1, "🫶🏼‍🫶🏿", "Heart Hands Mixed Medium-Light Dark\n:heart_hands_mixed_medium_light_dark:" );
    RegisterSymbolX( 1, "🫶🏽‍🫶🏻", "Heart Hands Mixed Medium Light\n:heart_hands_mixed_medium_light:" );
    RegisterSymbolX( 1, "🫶🏽‍🫶🏼", "Heart Hands Mixed Medium Medium-Light\n:heart_hands_mixed_medium_medium_light:" );
    RegisterSymbolX( 1, "🫶🏽‍🫶🏾", "Heart Hands Mixed Medium Medium-Dark\n:heart_hands_mixed_medium_medium_dark:" );
    RegisterSymbolX( 1, "🫶🏽‍🫶🏿", "Heart Hands Mixed Medium Dark\n:heart_hands_mixed_medium_dark:" );
    RegisterSymbolX( 1, "🫶🏾‍🫶🏻", "Heart Hands Mixed Medium-Dark Light\n:heart_hands_mixed_medium_dark_light:" );
    RegisterSymbolX( 1, "🫶🏾‍🫶🏼", "Heart Hands Mixed Medium-Dark Medium-Light\n:heart_hands_mixed_medium_dark_medium_light:" );
    RegisterSymbolX( 1, "🫶🏾‍🫶🏽", "Heart Hands Mixed Medium-Dark Medium\n:heart_hands_mixed_medium_dark_medium:" );
    RegisterSymbolX( 1, "🫶🏾‍🫶🏿", "Heart Hands Mixed Medium-Dark Dark\n:heart_hands_mixed_medium_dark_dark:" );

    RegisterSymbolX( 1, "🫶🏿‍🫶🏻", "Heart Hands Mixed Dark Light\n:heart_hands_mixed_dark_light:" );
    RegisterSymbolX( 1, "🫶🏿‍🫶🏼", "Heart Hands Mixed Dark Medium-Light\n:heart_hands_mixed_dark_medium_light:" );
    RegisterSymbolX( 1, "🫶🏿‍🫶🏽", "Heart Hands Mixed Dark Medium\n:heart_hands_mixed_dark_medium:" );
    RegisterSymbolX( 1, "🫶🏿‍🫶🏾", "Heart Hands Mixed Dark Medium-Dark\n:heart_hands_mixed_dark_medium_dark:" );
    RegisterSymbolX( 1, "🫱🏻‍🫲", "Handshake Left Light\n:handshake_left_light:" );
    RegisterSymbolX( 1, "🫱🏼‍🫲", "Handshake Left Medium-Light\n:handshake_left_medium_light:" );
    RegisterSymbolX( 1, "🫱🏽‍🫲", "Handshake Left Medium\n:handshake_left_medium:" );
    RegisterSymbolX( 1, "🫱🏾‍🫲", "Handshake Left Medium-Dark\n:handshake_left_medium_dark:" );
    RegisterSymbolX( 1, "🫱🏿‍🫲", "Handshake Left Dark\n:handshake_left_dark:" );
    RegisterSymbolX( 1, "🫱‍🫲🏻", "Handshake Right Light\n:handshake_right_light:" );
    RegisterSymbolX( 1, "🫱‍🫲🏼", "Handshake Right Medium-Light\n:handshake_right_medium_light:" );
    RegisterSymbolX( 1, "🫱‍🫲🏽", "Handshake Right Medium\n:handshake_right_medium:" );

    RegisterSymbolX( 1, "🫱‍🫲🏾", "Handshake Right Medium-Dark\n:handshake_right_medium_dark:" );
    RegisterSymbolX( 1, "🫱‍🫲🏿", "Handshake Right Dark\n:handshake_right_dark:" );
    RegisterSymbolX( 1, "🫷‍🫸", "Pushing Hands\n:pushing_hands:" );
    RegisterSymbolX( 1, "🫷🏻‍🫸🏻", "Pushing Hands Light-Light\n:pushing_hands_light_light:" );
    RegisterSymbolX( 1, "🫷🏼‍🫸🏼", "Pushing Hands Medium-Light-Medium-Light\n:pushing_hands_medium_light_medium_light:" );
    RegisterSymbolX( 1, "🫷🏽‍🫸🏽", "Pushing Hands Medium-Medium\n:pushing_hands_medium_medium:" );
    RegisterSymbolX( 1, "🫷🏾‍🫸🏾", "Pushing Hands Medium-Dark-Medium-Dark\n:pushing_hands_medium_dark_medium_dark:" );
    RegisterSymbolX( 1, "🫷🏿‍🫸🏿", "Pushing Hands Dark-Dark\n:pushing_hands_dark_dark:" );
    RegisterSymbolX( 1, "🫷🏻‍🫸🏿", "Pushing Hands Light-Dark\n:pushing_hands_light_dark:" );
    RegisterSymbolX( 1, "🫷🏿‍🫸🏻", "Pushing Hands Dark-Light\n:pushing_hands_dark_light:" );
    RegisterSymbolX( 1, "🫷🏻‍🫸🏼", "Pushing Hands Light-Medium-Light\n:pushing_hands_light_medium_light:" );
    RegisterSymbolX( 1, "🫷🏻‍🫸🏽", "Pushing Hands Light-Medium\n:pushing_hands_light_medium:" );

    RegisterSymbolX( 1, "🫷🏻‍🫸🏾", "Pushing Hands Light-Medium-Dark\n:pushing_hands_light_medium_dark:" );
    RegisterSymbolX( 1, "🫷🏼‍🫸🏻", "Pushing Hands Medium-Light-Light\n:pushing_hands_medium_light_light:" );
    RegisterSymbolX( 1, "🫷🏼‍🫸🏽", "Pushing Hands Medium-Light-Medium\n:pushing_hands_medium_light_medium:" );
    RegisterSymbolX( 1, "🫷🏼‍🫸🏾", "Pushing Hands Medium-Light-Medium-Dark\n:pushing_hands_medium_light_medium_dark:" );
    RegisterSymbolX( 1, "🫷🏼‍🫸🏿", "Pushing Hands Medium-Light-Dark\n:pushing_hands_medium_light_dark:" );
    RegisterSymbolX( 1, "🫷🏽‍🫸🏻", "Pushing Hands Medium-Light\n:pushing_hands_medium_light:" );
    RegisterSymbolX( 1, "🫷🏽‍🫸🏼", "Pushing Hands Medium-Medium-Light\n:pushing_hands_medium_medium_light:" );
    RegisterSymbolX( 1, "🫷🏽‍🫸🏾", "Pushing Hands Medium-Medium-Dark\n:pushing_hands_medium_medium_dark:" );
    RegisterSymbolX( 1, "🫷🏽‍🫸🏿", "Pushing Hands Medium-Dark\n:pushing_hands_medium_dark:" );
    RegisterSymbolX( 1, "🫷🏾‍🫸🏻", "Pushing Hands Medium-Dark-Light\n:pushing_hands_medium_dark_light:" );
    RegisterSymbolX( 1, "🫷🏾‍🫸🏼", "Pushing Hands Medium-Dark-Medium-Light\n:pushing_hands_medium_dark_medium_light:" );
    RegisterSymbolX( 1, "🫷🏾‍🫸🏽", "Pushing Hands Medium-Dark-Medium\n:pushing_hands_medium_dark_medium:" );

    RegisterSymbolX( 1, "🫷🏾‍🫸🏿", "Pushing Hands Medium-Dark-Dark\n:pushing_hands_medium_dark_dark:" );
    RegisterSymbolX( 1, "🫷🏿‍🫸🏼", "Pushing Hands Dark-Medium-Light\n:pushing_hands_dark_medium_light:" );
    RegisterSymbolX( 1, "🫷🏿‍🫸🏽", "Pushing Hands Dark-Medium\n:pushing_hands_dark_medium:" );
    RegisterSymbolX( 1, "🫷🏿‍🫸🏾", "Pushing Hands Dark-Medium-Dark\n:pushing_hands_dark_medium_dark:" );
    RegisterSymbolX( 1, "🫦🏻‍🫦🏼", "Biting Lip Mixed Light Medium-Light\n:biting_lip_mixed_light_medium_light:" );
    RegisterSymbolX( 1, "🫦🏻‍🫦🏽", "Biting Lip Mixed Light Medium\n:biting_lip_mixed_light_medium:" );
    RegisterSymbolX( 1, "🫦🏻‍🫦🏾", "Biting Lip Mixed Light Medium-Dark\n:biting_lip_mixed_light_medium_dark:" );
    RegisterSymbolX( 1, "🫦🏻‍🫦🏿", "Biting Lip Mixed Light Dark\n:biting_lip_mixed_light_dark:" );
    RegisterSymbolX( 1, "🫦🏼‍🫦🏽", "Biting Lip Mixed Medium-Light Medium\n:biting_lip_mixed_medium_light_medium:" );
    RegisterSymbolX( 1, "🫦🏼‍🫦🏾", "Biting Lip Mixed Medium-Light Medium-Dark\n:biting_lip_mixed_medium_light_medium_dark:" );
    RegisterSymbolX( 1, "🫦🏼‍🫦🏿", "Biting Lip Mixed Medium-Light Dark\n:biting_lip_mixed_medium_light_dark:" );
    RegisterSymbolX( 1, "🫦🏽‍🫦🏾", "Biting Lip Mixed Medium Medium-Dark\n:biting_lip_mixed_medium_medium_dark:" );

    RegisterSymbolX( 1, "🫦🏽‍🫦🏿", "Biting Lip Mixed Medium Dark\n:biting_lip_mixed_medium_dark:" );
    RegisterSymbolX( 1, "🫦🏾‍🫦🏿", "Biting Lip Mixed Medium-Dark Dark\n:biting_lip_mixed_medium_dark_dark:" );
    RegisterSymbolX( 1, "🫱🏻‍🫲🏻‍🫱🏻", "Triple Handshake Light\n:triple_handshake_light:" );
    RegisterSymbolX( 1, "🫱🏼‍🫲🏼‍🫱🏼", "Triple Handshake Medium-Light\n:triple_handshake_medium_light:" );
    RegisterSymbolX( 1, "🫱🏽‍🫲🏽‍🫱🏽", "Triple Handshake Medium\n:triple_handshake_medium:" );
    RegisterSymbolX( 1, "🫱🏾‍🫲🏾‍🫱🏾", "Triple Handshake Medium-Dark\n:triple_handshake_medium_dark:" );
    RegisterSymbolX( 1, "🫱🏿‍🫲🏿‍🫱🏿", "Triple Handshake Dark\n:triple_handshake_dark:" );
    RegisterSymbolX( 1, "🫷🏻‍🫸🏻‍🫷🏻", "Triple Push Light\n:triple_push_light:" );
    RegisterSymbolX( 1, "🫷🏼‍🫸🏼‍🫷🏼", "Triple Push Medium-Light\n:triple_push_medium_light:" );
    RegisterSymbolX( 1, "🫷🏽‍🫸🏽‍🫷🏽", "Triple Push Medium\n:triple_push_medium:" );
    */;
  }

  // ─── Animals & Nature — Animals ────────────────────────────────
  private void RegisterAnimals()
  {
    RegisterSymbolX( 1, "🐶", "Dog\n:dog:"             );
    RegisterSymbolX( 1, "🐱", "Cat\n:cat:"             );
    RegisterSymbolX( 1, "🐭", "Mouse\n:mouse:"         );
    RegisterSymbolX( 1, "🐹", "Hamster\n:hamster:"     );
    RegisterSymbolX( 1, "🐰", "Rabbit\n:rabbit:"       );
    RegisterSymbolX( 1, "🦊", "Fox\n:fox_face:"        );
    RegisterSymbolX( 1, "🐻", "Bear\n:bear:"           );
    RegisterSymbolX( 1, "🐼", "Panda\n:panda_face:"    );
    RegisterSymbolX( 1, "🐨", "Koala\n:koala:"         );
    RegisterSymbolX( 1, "🐯", "Tiger\n:tiger:"         );
    RegisterSymbolX( 1, "🦁", "Lion\n:lion:"           );
    RegisterSymbolX( 1, "🐮", "Cow\n:cow:"             );

    RegisterSymbolX( 1, "🐷", "Pig\n:pig:"             );
    RegisterSymbolX( 1, "🐸", "Frog\n:frog:"           );
    RegisterSymbolX( 1, "🐵", "Monkey Face\n:monkey_face:" );
    RegisterSymbolX( 1, "🙈", "See-No-Evil Monkey\n:see_no_evil:" );
    RegisterSymbolX( 1, "🙉", "Hear-No-Evil Monkey\n:hear_no_evil:" );
    RegisterSymbolX( 1, "🙊", "Speak-No-Evil Monkey\n:speak_no_evil:" );
    RegisterSymbolX( 1, "🐔", "Chicken\n:chicken:"     );
    RegisterSymbolX( 1, "🐧", "Penguin\n:penguin:"     );
    RegisterSymbolX( 1, "🐦", "Bird\n:bird:"           );
    RegisterSymbolX( 1, "🐤", "Baby Chick\n:baby_chick:" );
    RegisterSymbolX( 1, "🦆", "Duck\n:duck:"           );
    RegisterSymbolX( 1, "🦅", "Eagle\n:eagle:"         );

    RegisterSymbolX( 1, "🦉", "Owl\n:owl:"             );
    RegisterSymbolX( 1, "🦇", "Bat\n:bat:"             );
    RegisterSymbolX( 1, "🐺", "Wolf\n:wolf:"           );
    RegisterSymbolX( 1, "🐗", "Boar\n:boar:"           );
    RegisterSymbolX( 1, "🐴", "Horse\n:horse:"         );
    RegisterSymbolX( 1, "🦄", "Unicorn\n:unicorn:"     );
    RegisterSymbolX( 1, "🐝", "Honeybee\n:bee:"        );
    RegisterSymbolX( 1, "🐛", "Bug\n:bug:"             );
    RegisterSymbolX( 1, "🦋", "Butterfly\n:butterfly:" );
    RegisterSymbolX( 1, "🐌", "Snail\n:snail:"         );
    RegisterSymbolX( 1, "🐞", "Lady Beetle\n:beetle:"  );
    RegisterSymbolX( 1, "🐜", "Ant\n:ant:"             );

    RegisterSymbolX( 1, "🦟", "Mosquito\n:mosquito:"   );
    RegisterSymbolX( 1, "🦗", "Cricket\n:cricket:"     );
    RegisterSymbolX( 1, "🕷️", "Spider\n:spider:"       );
    RegisterSymbolX( 1, "🦂", "Scorpion\n:scorpion:"   );
    RegisterSymbolX( 1, "🐢", "Turtle\n:turtle:"       );
    RegisterSymbolX( 1, "🐍", "Snake\n:snake:"         );
    RegisterSymbolX( 1, "🦎", "Lizard\n:lizard:"       );
    RegisterSymbolX( 1, "🐊", "Crocodile\n:crocodile:" );
    RegisterSymbolX( 1, "🐙", "Octopus\n:octopus:"     );
    RegisterSymbolX( 1, "🦑", "Squid\n:squid:"         );
    RegisterSymbolX( 1, "🦐", "Shrimp\n:shrimp:"       );
    RegisterSymbolX( 1, "🦞", "Lobster\n:lobster:"     );

    RegisterSymbolX( 1, "🦀", "Crab\n:crab:"           );
    RegisterSymbolX( 1, "🐡", "Blowfish\n:blowfish:"   );
    RegisterSymbolX( 1, "🐠", "Tropical Fish\n:tropical_fish:" );
    RegisterSymbolX( 1, "🐟", "Fish\n:fish:"           );
    RegisterSymbolX( 1, "🐬", "Dolphin\n:dolphin:"     );
    RegisterSymbolX( 1, "🐳", "Spouting Whale\n:whale:" );
    RegisterSymbolX( 1, "🐋", "Whale\n:whale2:"        );
    RegisterSymbolX( 1, "🦈", "Shark\n:shark:"         );
    RegisterSymbolX( 1, "🦓", "Zebra\n:zebra:"         );
    RegisterSymbolX( 1, "🦍", "Gorilla\n:gorilla:"     );
    RegisterSymbolX( 1, "🦧", "Orangutan\n:orangutan:" );
    RegisterSymbolX( 1, "🐘", "Elephant\n:elephant:"   );

    RegisterSymbolX( 1, "🦛", "Hippopotamus\n:hippopotamus:" );
    RegisterSymbolX( 1, "🦏", "Rhinoceros\n:rhinoceros:"     );
    RegisterSymbolX( 1, "🐪", "Camel\n:dromedary_camel:"     );
    RegisterSymbolX( 1, "🐫", "Two-Hump Camel\n:camel:"      );
    RegisterSymbolX( 1, "🦒", "Giraffe\n:giraffe:"           );
    RegisterSymbolX( 1, "🦘", "Kangaroo\n:kangaroo:"         );
    RegisterSymbolX( 1, "🐃", "Water Buffalo\n:water_buffalo:" );
    RegisterSymbolX( 1, "🐂", "Ox\n:ox:"                     );
    RegisterSymbolX( 1, "🐄", "Cow\n:cow2:"                  );
    RegisterSymbolX( 1, "🐎", "Horse\n:horse2:"               );
    RegisterSymbolX( 1, "🐖", "Pig\n:pig2:"                   );
    RegisterSymbolX( 1, "🐏", "Ram\n:ram:"                    );

    RegisterSymbolX( 1, "🐑", "Ewe\n:sheep:"           );
    RegisterSymbolX( 1, "🦙", "Llama\n:llama:"         );
    RegisterSymbolX( 1, "🐐", "Goat\n:goat:"           );
    RegisterSymbolX( 1, "🦌", "Deer\n:deer:"           );
    RegisterSymbolX( 1, "🐕", "Dog\n:dog2:"            );
    RegisterSymbolX( 1, "🐩", "Poodle\n:poodle:"       );
    RegisterSymbolX( 1, "🐈", "Cat\n:cat2:"            );
    RegisterSymbolX( 1, "🦚", "Peacock\n:peacock:"     );
    RegisterSymbolX( 1, "🦜", "Parrot\n:parrot:"       );
    RegisterSymbolX( 1, "🦢", "Swan\n:swan:"           );
    RegisterSymbolX( 1, "🦩", "Flamingo\n:flamingo:"   );
    RegisterSymbolX( 1, "🕊️", "Dove\n:dove:"           );

    RegisterSymbolX( 1, "🐇", "Rabbit\n:rabbit2:"      );
    RegisterSymbolX( 1, "🦝", "Raccoon\n:raccoon:"     );
    RegisterSymbolX( 1, "🦨", "Skunk\n:skunk:"         );
    RegisterSymbolX( 1, "🦡", "Badger\n:badger:"       );
    RegisterSymbolX( 1, "🦫", "Beaver\n:beaver:"       );
    RegisterSymbolX( 1, "🦦", "Otter\n:otter:"         );
    RegisterSymbolX( 1, "🦥", "Sloth\n:sloth:"         );
    RegisterSymbolX( 1, "🐁", "Mouse\n:mouse2:"        );
    RegisterSymbolX( 1, "🐀", "Rat\n:rat:"             );
    RegisterSymbolX( 1, "🐿️", "Chipmunk\n:chipmunk:"  );
    RegisterSymbolX( 1, "🦔", "Hedgehog\n:hedgehog:"   );
    RegisterSymbolX( 1, "🐾", "Paw Prints\n:paw_prints:" );
  }

  // ─── Animals & Nature — Plants & Sky ───────────────────────────
  private void RegisterPlantsAndNature()
  {
    RegisterSymbolX( 1, "🌵", "Cactus\n:cactus:"          );
    RegisterSymbolX( 1, "🎄", "Christmas Tree\n:christmas_tree:" );
    RegisterSymbolX( 1, "🌲", "Evergreen Tree\n:evergreen_tree:" );
    RegisterSymbolX( 1, "🌳", "Deciduous Tree\n:deciduous_tree:" );
    RegisterSymbolX( 1, "🌴", "Palm Tree\n:palm_tree:"    );
    RegisterSymbolX( 1, "🌱", "Seedling\n:seedling:"      );
    RegisterSymbolX( 1, "🌿", "Herb\n:herb:"              );
    RegisterSymbolX( 1, "☘️", "Shamrock\n:shamrock:"      );
    RegisterSymbolX( 1, "🍀", "Four Leaf Clover\n:four_leaf_clover:" );
    RegisterSymbolX( 1, "🎋", "Tanabata Tree\n:tanabata_tree:" );
    RegisterSymbolX( 1, "🎍", "Pine Decoration\n:bamboo:" );
    RegisterSymbolX( 1, "🍃", "Leaf Fluttering\n:leaves:" );

    RegisterSymbolX( 1, "🍂", "Fallen Leaf\n:fallen_leaf:"  );
    RegisterSymbolX( 1, "🍁", "Maple Leaf\n:maple_leaf:"    );
    RegisterSymbolX( 1, "🍄", "Mushroom\n:mushroom:"        );
    RegisterSymbolX( 1, "🌾", "Sheaf of Rice\n:ear_of_rice:" );
    RegisterSymbolX( 1, "💐", "Bouquet\n:bouquet:"          );
    RegisterSymbolX( 1, "🌷", "Tulip\n:tulip:"              );
    RegisterSymbolX( 1, "🌹", "Rose\n:rose:"                );
    RegisterSymbolX( 1, "🥀", "Wilted Flower\n:wilted_flower:" );
    RegisterSymbolX( 1, "🌺", "Hibiscus\n:hibiscus:"        );
    RegisterSymbolX( 1, "🌸", "Cherry Blossom\n:cherry_blossom:" );
    RegisterSymbolX( 1, "🌼", "Blossom\n:blossom:"          );
    RegisterSymbolX( 1, "🌻", "Sunflower\n:sunflower:"      );

    RegisterSymbolX( 1, "🌞", "Sun with Face\n:sun_with_face:" );
    RegisterSymbolX( 1, "🌝", "Full Moon with Face\n:full_moon_face:" );
    RegisterSymbolX( 1, "🌛", "First Quarter Moon\n:first_quarter_moon_with_face:" );
    RegisterSymbolX( 1, "🌜", "Last Quarter Moon\n:last_quarter_moon_with_face:" );
    RegisterSymbolX( 1, "🌚", "New Moon with Face\n:new_moon_with_face:" );
    RegisterSymbolX( 1, "🌕", "Full Moon\n:full_moon:"      );
    RegisterSymbolX( 1, "🌙", "Crescent Moon\n:crescent_moon:" );
    RegisterSymbolX( 1, "🌟", "Glowing Star\n:star2:"       );
    RegisterSymbolX( 1, "⭐", "Star\n:star:"                );
    RegisterSymbolX( 1, "🌠", "Shooting Star\n:stars:"      );
    RegisterSymbolX( 1, "☀️", "Sun\n:sunny:"                );
    RegisterSymbolX( 1, "🌈", "Rainbow\n:rainbow:"          );

    RegisterSymbolX( 1, "⛅", "Sun Behind Cloud\n:partly_sunny:"  );
    RegisterSymbolX( 1, "🌤️", "Sun Behind Small Cloud\n:sun_small_cloud:" );
    RegisterSymbolX( 1, "☁️", "Cloud\n:cloud:"                    );
    RegisterSymbolX( 1, "🌦️", "Sun Behind Rain Cloud\n:sun_rain:" );
    RegisterSymbolX( 1, "🌧️", "Cloud with Rain\n:rain_cloud:"     );
    RegisterSymbolX( 1, "⛈️", "Thunder Cloud and Rain\n:thunder_cloud:" );
    RegisterSymbolX( 1, "🌩️", "Cloud with Lightning\n:lightning:" );
    RegisterSymbolX( 1, "🌨️", "Cloud with Snow\n:snow_cloud:"     );
    RegisterSymbolX( 1, "❄️", "Snowflake\n:snowflake:"            );
    RegisterSymbolX( 1, "☃️", "Snowman with Snow\n:snowman_with_snow:" );
    RegisterSymbolX( 1, "⛄", "Snowman\n:snowman:"                );
    RegisterSymbolX( 1, "🌬️", "Wind Face\n:wind_blowing_face:"    );

    RegisterSymbolX( 1, "🌊", "Water Wave\n:ocean:"      );
    RegisterSymbolX( 1, "🌀", "Cyclone\n:cyclone:"       );
    RegisterSymbolX( 1, "🌪️", "Tornado\n:tornado:"       );
    RegisterSymbolX( 1, "🌫️", "Fog\n:fog:"               );
    RegisterSymbolX( 1, "🌈", "Rainbow\n:rainbow2:"      );
    RegisterSymbolX( 1, "☔", "Umbrella with Rain\n:umbrella:" );
    RegisterSymbolX( 1, "⚡", "Lightning\n:zap:"          );
    RegisterSymbolX( 1, "🌋", "Volcano\n:volcano:"        );
    RegisterSymbolX( 1, "🏔️", "Snow-Capped Mountain\n:snow_capped_mountain:" );
    RegisterSymbolX( 1, "⛰️", "Mountain\n:mountain:"     );
    RegisterSymbolX( 1, "🏕️", "Camping\n:camping:"       );
    RegisterSymbolX( 1, "🌌", "Milky Way\n:milky_way:"   );
  }

  // ─── Food & Drink ──────────────────────────────────────────────
  private void RegisterFoodAndDrink()
  {
    RegisterSymbolX( 1, "🍎", "Red Apple\n:apple:"          );
    RegisterSymbolX( 1, "🍐", "Pear\n:pear:"                );
    RegisterSymbolX( 1, "🍊", "Tangerine\n:tangerine:"      );
    RegisterSymbolX( 1, "🍋", "Lemon\n:lemon:"              );
    RegisterSymbolX( 1, "🍌", "Banana\n:banana:"            );
    RegisterSymbolX( 1, "🍉", "Watermelon\n:watermelon:"    );
    RegisterSymbolX( 1, "🍇", "Grapes\n:grapes:"            );
    RegisterSymbolX( 1, "🍓", "Strawberry\n:strawberry:"    );
    RegisterSymbolX( 1, "🫐", "Blueberries\n:blueberries:"  );
    RegisterSymbolX( 1, "🍈", "Melon\n:melon:"              );
    RegisterSymbolX( 1, "🍒", "Cherries\n:cherries:"        );
    RegisterSymbolX( 1, "🍑", "Peach\n:peach:"              );

    RegisterSymbolX( 1, "🥭", "Mango\n:mango:"              );
    RegisterSymbolX( 1, "🍍", "Pineapple\n:pineapple:"      );
    RegisterSymbolX( 1, "🥥", "Coconut\n:coconut:"          );
    RegisterSymbolX( 1, "🥝", "Kiwi\n:kiwi:"                );
    RegisterSymbolX( 1, "🍅", "Tomato\n:tomato:"            );
    RegisterSymbolX( 1, "🍆", "Eggplant\n:eggplant:"        );
    RegisterSymbolX( 1, "🥑", "Avocado\n:avocado:"          );
    RegisterSymbolX( 1, "🥦", "Broccoli\n:broccoli:"        );
    RegisterSymbolX( 1, "🥬", "Leafy Green\n:leafy_green:"  );
    RegisterSymbolX( 1, "🥒", "Cucumber\n:cucumber:"        );
    RegisterSymbolX( 1, "🌶️", "Hot Pepper\n:hot_pepper:"    );
    RegisterSymbolX( 1, "🧄", "Garlic\n:garlic:"            );

    RegisterSymbolX( 1, "🧅", "Onion\n:onion:"              );
    RegisterSymbolX( 1, "🥔", "Potato\n:potato:"            );
    RegisterSymbolX( 1, "🥕", "Carrot\n:carrot:"            );
    RegisterSymbolX( 1, "🌽", "Ear of Corn\n:corn:"         );
    RegisterSymbolX( 1, "🥜", "Peanuts\n:peanuts:"          );
    RegisterSymbolX( 1, "🍞", "Bread\n:bread:"              );
    RegisterSymbolX( 1, "🥐", "Croissant\n:croissant:"      );
    RegisterSymbolX( 1, "🧀", "Cheese Wedge\n:cheese:"      );
    RegisterSymbolX( 1, "🥚", "Egg\n:egg:"                  );
    RegisterSymbolX( 1, "🍳", "Cooking\n:fried_egg:"        );
    RegisterSymbolX( 1, "🥞", "Pancakes\n:pancakes:"        );
    RegisterSymbolX( 1, "🧇", "Waffle\n:waffle:"            );

    RegisterSymbolX( 1, "🥓", "Bacon\n:bacon:"              );
    RegisterSymbolX( 1, "🥩", "Cut of Meat\n:cut_of_meat:"  );
    RegisterSymbolX( 1, "🍗", "Poultry Leg\n:poultry_leg:"  );
    RegisterSymbolX( 1, "🍖", "Meat on Bone\n:meat_on_bone:" );
    RegisterSymbolX( 1, "🌭", "Hot Dog\n:hotdog:"           );
    RegisterSymbolX( 1, "🍔", "Hamburger\n:hamburger:"      );
    RegisterSymbolX( 1, "🍟", "French Fries\n:fries:"       );
    RegisterSymbolX( 1, "🍕", "Pizza\n:pizza:"              );
    RegisterSymbolX( 1, "🌮", "Taco\n:taco:"                );
    RegisterSymbolX( 1, "🌯", "Burrito\n:burrito:"          );
    RegisterSymbolX( 1, "🥙", "Stuffed Flatbread\n:stuffed_flatbread:" );
    RegisterSymbolX( 1, "🥗", "Green Salad\n:green_salad:"  );

    RegisterSymbolX( 1, "🍝", "Spaghetti\n:spaghetti:"      );
    RegisterSymbolX( 1, "🍜", "Steaming Bowl\n:ramen:"      );
    RegisterSymbolX( 1, "🍲", "Pot of Food\n:stew:"         );
    RegisterSymbolX( 1, "🍛", "Curry Rice\n:curry:"         );
    RegisterSymbolX( 1, "🍣", "Sushi\n:sushi:"              );
    RegisterSymbolX( 1, "🍱", "Bento Box\n:bento:"          );
    RegisterSymbolX( 1, "🥟", "Dumpling\n:dumpling:"        );
    RegisterSymbolX( 1, "🍤", "Fried Shrimp\n:fried_shrimp:" );
    RegisterSymbolX( 1, "🍙", "Rice Ball\n:rice_ball:"      );
    RegisterSymbolX( 1, "🍚", "Cooked Rice\n:rice:"         );
    RegisterSymbolX( 1, "🍦", "Soft Ice Cream\n:icecream:"  );
    RegisterSymbolX( 1, "🍧", "Shaved Ice\n:shaved_ice:"    );

    RegisterSymbolX( 1, "🍨", "Ice Cream\n:ice_cream:"      );
    RegisterSymbolX( 1, "🧁", "Cupcake\n:cupcake:"          );
    RegisterSymbolX( 1, "🎂", "Birthday Cake\n:birthday:"   );
    RegisterSymbolX( 1, "🍰", "Shortcake\n:cake:"           );
    RegisterSymbolX( 1, "🍮", "Custard\n:custard:"          );
    RegisterSymbolX( 1, "🍭", "Lollipop\n:lollipop:"        );
    RegisterSymbolX( 1, "🍬", "Candy\n:candy:"              );
    RegisterSymbolX( 1, "🍫", "Chocolate Bar\n:chocolate_bar:" );
    RegisterSymbolX( 1, "🍿", "Popcorn\n:popcorn:"          );
    RegisterSymbolX( 1, "🍩", "Doughnut\n:doughnut:"        );
    RegisterSymbolX( 1, "🍪", "Cookie\n:cookie:"            );
    RegisterSymbolX( 1, "🌰", "Chestnut\n:chestnut:"        );

    RegisterSymbolX( 1, "☕", "Hot Beverage\n:coffee:"     );
    RegisterSymbolX( 1, "🍵", "Teacup\n:tea:"              );
    RegisterSymbolX( 1, "🧃", "Beverage Box\n:beverage_box:" );
    RegisterSymbolX( 1, "🥤", "Cup with Straw\n:cup_with_straw:" );
    RegisterSymbolX( 1, "🧋", "Bubble Tea\n:bubble_tea:"   );
    RegisterSymbolX( 1, "🍺", "Beer Mug\n:beer:"           );
    RegisterSymbolX( 1, "🍻", "Clinking Beer Mugs\n:beers:" );
    RegisterSymbolX( 1, "🥂", "Clinking Glasses\n:clinking_glasses:" );
    RegisterSymbolX( 1, "🍷", "Wine Glass\n:wine_glass:"   );
    RegisterSymbolX( 1, "🥃", "Tumbler Glass\n:tumbler_glass:" );
    RegisterSymbolX( 1, "🍸", "Cocktail Glass\n:cocktail:" );
    RegisterSymbolX( 1, "🍹", "Tropical Drink\n:tropical_drink:" );
  }

  // ─── Travel & Places ───────────────────────────────────────────
  private void RegisterTravelAndPlaces()
  {
    RegisterSymbolX( 1, "🚗", "Automobile\n:car:"           );
    RegisterSymbolX( 1, "🚕", "Taxi\n:taxi:"                );
    RegisterSymbolX( 1, "🚙", "Sport Utility Vehicle\n:blue_car:" );
    RegisterSymbolX( 1, "🚌", "Bus\n:bus:"                  );
    RegisterSymbolX( 1, "🚎", "Trolleybus\n:trolleybus:"    );
    RegisterSymbolX( 1, "🏎️", "Racing Car\n:racing_car:"    );
    RegisterSymbolX( 1, "🚓", "Police Car\n:police_car:"    );
    RegisterSymbolX( 1, "🚑", "Ambulance\n:ambulance:"      );
    RegisterSymbolX( 1, "🚒", "Fire Engine\n:fire_engine:"  );
    RegisterSymbolX( 1, "🚐", "Minibus\n:minibus:"          );
    RegisterSymbolX( 1, "🚚", "Delivery Truck\n:truck:"     );
    RegisterSymbolX( 1, "🚛", "Articulated Lorry\n:articulated_lorry:" );

    RegisterSymbolX( 1, "🚜", "Tractor\n:tractor:"          );
    RegisterSymbolX( 1, "🏍️", "Motorcycle\n:motorcycle:"    );
    RegisterSymbolX( 1, "🛵", "Motor Scooter\n:motor_scooter:" );
    RegisterSymbolX( 1, "🚲", "Bicycle\n:bike:"             );
    RegisterSymbolX( 1, "🛴", "Kick Scooter\n:scooter:"     );
    RegisterSymbolX( 1, "🚁", "Helicopter\n:helicopter:"    );
    RegisterSymbolX( 1, "🛸", "Flying Saucer\n:flying_saucer:" );
    RegisterSymbolX( 1, "✈️", "Airplane\n:airplane:"        );
    RegisterSymbolX( 1, "🚀", "Rocket\n:rocket:"            );
    RegisterSymbolX( 1, "🛩️", "Small Airplane\n:small_airplane:" );
    RegisterSymbolX( 1, "⛵", "Sailboat\n:sailboat:"        );
    RegisterSymbolX( 1, "🚤", "Speedboat\n:speedboat:"      );

    RegisterSymbolX( 1, "🛳️", "Passenger Ship\n:passenger_ship:" );
    RegisterSymbolX( 1, "🚢", "Ship\n:ship:"                );
    RegisterSymbolX( 1, "🚂", "Locomotive\n:steam_locomotive:" );
    RegisterSymbolX( 1, "🚄", "High-Speed Train\n:bullettrain_side:" );
    RegisterSymbolX( 1, "🚇", "Metro\n:metro:"              );
    RegisterSymbolX( 1, "🚊", "Tram\n:tram:"                );
    RegisterSymbolX( 1, "🚞", "Mountain Railway\n:mountain_railway:" );
    RegisterSymbolX( 1, "⛽", "Fuel Pump\n:fuelpump:"       );
    RegisterSymbolX( 1, "🚦", "Vertical Traffic Light\n:vertical_traffic_light:" );
    RegisterSymbolX( 1, "🚥", "Horizontal Traffic Light\n:traffic_light:" );
    RegisterSymbolX( 1, "🚧", "Construction\n:construction:" );
    RegisterSymbolX( 1, "🗺️", "World Map\n:world_map:"      );

    RegisterSymbolX( 1, "🧭", "Compass\n:compass:"          );
    RegisterSymbolX( 1, "🏠", "House\n:house:"              );
    RegisterSymbolX( 1, "🏡", "House with Garden\n:house_with_garden:" );
    RegisterSymbolX( 1, "🏢", "Office Building\n:office:"   );
    RegisterSymbolX( 1, "🏥", "Hospital\n:hospital:"        );
    RegisterSymbolX( 1, "🏦", "Bank\n:bank:"                );
    RegisterSymbolX( 1, "🏨", "Hotel\n:hotel:"              );
    RegisterSymbolX( 1, "🏪", "Convenience Store\n:convenience_store:" );
    RegisterSymbolX( 1, "🏫", "School\n:school:"            );
    RegisterSymbolX( 1, "🏰", "Castle\n:european_castle:"   );
    RegisterSymbolX( 1, "🏯", "Japanese Castle\n:japanese_castle:" );
    RegisterSymbolX( 1, "🗼", "Tokyo Tower\n:tokyo_tower:"  );

    RegisterSymbolX( 1, "🗽", "Statue of Liberty\n:statue_of_liberty:" );
    RegisterSymbolX( 1, "⛪", "Church\n:church:"            );
    RegisterSymbolX( 1, "🕌", "Mosque\n:mosque:"            );
    RegisterSymbolX( 1, "🕍", "Synagogue\n:synagogue:"      );
    RegisterSymbolX( 1, "⛩️", "Shinto Shrine\n:shinto_shrine:" );
    RegisterSymbolX( 1, "🕋", "Kaaba\n:kaaba:"              );
    RegisterSymbolX( 1, "⛲", "Fountain\n:fountain:"        );
    RegisterSymbolX( 1, "🏟️", "Stadium\n:stadium:"          );
    RegisterSymbolX( 1, "🌁", "Foggy\n:foggy:"              );
    RegisterSymbolX( 1, "🌃", "Night with Stars\n:night_with_stars:" );
    RegisterSymbolX( 1, "🏙️", "Cityscape\n:cityscape:"      );
    RegisterSymbolX( 1, "🌄", "Sunrise over Mountains\n:sunrise_over_mountains:" );

    RegisterSymbolX( 1, "🌅", "Sunrise\n:sunrise:"          );
    RegisterSymbolX( 1, "🌆", "Cityscape at Dusk\n:city_sunrise:" );
    RegisterSymbolX( 1, "🌇", "Sunset\n:city_sunset:"       );
    RegisterSymbolX( 1, "🌉", "Bridge at Night\n:bridge_at_night:" );
    RegisterSymbolX( 1, "🏔️", "Snow-Capped Mountain\n:mountain_snow:" );
    RegisterSymbolX( 1, "🏜️", "Desert\n:desert:"            );
    RegisterSymbolX( 1, "🏝️", "Desert Island\n:desert_island:" );
    RegisterSymbolX( 1, "🏞️", "National Park\n:national_park:" );
    RegisterSymbolX( 1, "🗻", "Mount Fuji\n:mount_fuji:"    );
    RegisterSymbolX( 1, "🌐", "Globe with Meridians\n:globe_with_meridians:" );
    RegisterSymbolX( 1, "🌍", "Globe Europe-Africa\n:earth_africa:" );
    RegisterSymbolX( 1, "🌎", "Globe Americas\n:earth_americas:" );
  }

  // ─── Activities ────────────────────────────────────────────────
  private void RegisterActivities()
  {
    RegisterSymbolX( 1, "⚽", "Soccer Ball\n:soccer:"       );
    RegisterSymbolX( 1, "🏀", "Basketball\n:basketball:"    );
    RegisterSymbolX( 1, "🏈", "American Football\n:football:" );
    RegisterSymbolX( 1, "⚾", "Baseball\n:baseball:"        );
    RegisterSymbolX( 1, "🥎", "Softball\n:softball:"        );
    RegisterSymbolX( 1, "🎾", "Tennis\n:tennis:"            );
    RegisterSymbolX( 1, "🏐", "Volleyball\n:volleyball:"    );
    RegisterSymbolX( 1, "🏉", "Rugby Football\n:rugby_football:" );
    RegisterSymbolX( 1, "🥏", "Flying Disc\n:flying_disc:"  );
    RegisterSymbolX( 1, "🎱", "Pool 8 Ball\n:8ball:"        );
    RegisterSymbolX( 1, "🏓", "Ping Pong\n:ping_pong:"      );
    RegisterSymbolX( 1, "🏸", "Badminton\n:badminton:"      );

    RegisterSymbolX( 1, "🥍", "Lacrosse\n:lacrosse:"        );
    RegisterSymbolX( 1, "🏒", "Ice Hockey\n:ice_hockey:"    );
    RegisterSymbolX( 1, "🏑", "Field Hockey\n:field_hockey:" );
    RegisterSymbolX( 1, "🏏", "Cricket Game\n:cricket_game:" );
    RegisterSymbolX( 1, "⛳", "Flag in Hole\n:golf:"         );
    RegisterSymbolX( 1, "🎣", "Fishing Pole\n:fishing_pole_and_fish:" );
    RegisterSymbolX( 1, "🤿", "Diving Mask\n:diving_mask:"  );
    RegisterSymbolX( 1, "🥊", "Boxing Glove\n:boxing_glove:" );
    RegisterSymbolX( 1, "🥋", "Martial Arts Uniform\n:martial_arts_uniform:" );
    RegisterSymbolX( 1, "🎯", "Direct Hit\n:dart:"          );
    RegisterSymbolX( 1, "🪃", "Boomerang\n:boomerang:"      );
    RegisterSymbolX( 1, "🏹", "Bow and Arrow\n:bow_and_arrow:" );

    RegisterSymbolX( 1, "🎿", "Skis\n:ski:"                 );
    RegisterSymbolX( 1, "🛷", "Sled\n:sled:"                );
    RegisterSymbolX( 1, "🥌", "Curling Stone\n:curling_stone:" );
    RegisterSymbolX( 1, "🛹", "Skateboard\n:skateboard:"    );
    RegisterSymbolX( 1, "🪂", "Parachute\n:parachute:"      );
    RegisterSymbolX( 1, "🏋️", "Weightlifter\n:weight_lifter:" );
    RegisterSymbolX( 1, "🤼", "Wrestlers\n:wrestlers:"      );
    RegisterSymbolX( 1, "🤸", "Gymnast\n:person_doing_cartwheel:" );
    RegisterSymbolX( 1, "⛷️", "Skier\n:skier:"              );
    RegisterSymbolX( 1, "🏂", "Snowboarder\n:snowboarder:"   );
    RegisterSymbolX( 1, "🏊", "Swimmer\n:swimmer:"          );
    RegisterSymbolX( 1, "🚵", "Mountain Biker\n:mountain_bicyclist:" );

    RegisterSymbolX( 1, "🚴", "Bicyclist\n:bicyclist:"      );
    RegisterSymbolX( 1, "🧘", "Person Meditating\n:person_in_lotus_position:" );
    RegisterSymbolX( 1, "🎮", "Video Game\n:video_game:"    );
    RegisterSymbolX( 1, "🕹️", "Joystick\n:joystick:"        );
    RegisterSymbolX( 1, "🎰", "Slot Machine\n:slot_machine:" );
    RegisterSymbolX( 1, "🧩", "Puzzle Piece\n:jigsaw:"      );
    RegisterSymbolX( 1, "🎲", "Game Die\n:game_die:"        );
    RegisterSymbolX( 1, "♟️", "Chess Pawn\n:chess_pawn:"    );
    RegisterSymbolX( 1, "🎭", "Performing Arts\n:performing_arts:" );
    RegisterSymbolX( 1, "🎨", "Artist Palette\n:art:"       );
    RegisterSymbolX( 1, "🖌️", "Paintbrush\n:paintbrush:"    );
    RegisterSymbolX( 1, "🎬", "Clapper Board\n:clapper:"    );

    RegisterSymbolX( 1, "🎤", "Microphone\n:microphone:"    );
    RegisterSymbolX( 1, "🎧", "Headphones\n:headphones:"    );
    RegisterSymbolX( 1, "🎼", "Musical Score\n:musical_score:" );
    RegisterSymbolX( 1, "🎷", "Saxophone\n:saxophone:"      );
    RegisterSymbolX( 1, "🎸", "Guitar\n:guitar:"            );
    RegisterSymbolX( 1, "🎹", "Musical Keyboard\n:musical_keyboard:" );
    RegisterSymbolX( 1, "🎺", "Trumpet\n:trumpet:"          );
    RegisterSymbolX( 1, "🎻", "Violin\n:violin:"            );
    RegisterSymbolX( 1, "🥁", "Drum\n:drum_with_drumsticks:" );
    RegisterSymbolX( 1, "🪘", "Long Drum\n:long_drum:"      );
    RegisterSymbolX( 1, "🪗", "Accordion\n:accordion:"      );
    RegisterSymbolX( 1, "🎪", "Circus Tent\n:circus_tent:"  );

    RegisterSymbolX( 1, "🎠", "Carousel Horse\n:carousel_horse:" );
    RegisterSymbolX( 1, "🎡", "Ferris Wheel\n:ferris_wheel:" );
    RegisterSymbolX( 1, "🎢", "Roller Coaster\n:roller_coaster:" );
    RegisterSymbolX( 1, "🎟️", "Admission Ticket\n:tickets:"  );
    RegisterSymbolX( 1, "🎈", "Balloon\n:balloon:"          );
    RegisterSymbolX( 1, "🎉", "Party Popper\n:tada:"        );
    RegisterSymbolX( 1, "🎊", "Confetti Ball\n:confetti_ball:" );
    RegisterSymbolX( 1, "🎁", "Wrapped Gift\n:gift:"        );
    RegisterSymbolX( 1, "🎀", "Ribbon\n:ribbon:"            );
    RegisterSymbolX( 1, "🎗️", "Reminder Ribbon\n:reminder_ribbon:" );
    RegisterSymbolX( 1, "🏆", "Trophy\n:trophy:"            );
    RegisterSymbolX( 1, "🥇", "1st Place Medal\n:1st_place_medal:" );

    RegisterSymbolX( 1, "🥈", "2nd Place Medal\n:2nd_place_medal:" );
    RegisterSymbolX( 1, "🥉", "3rd Place Medal\n:3rd_place_medal:" );
    RegisterSymbolX( 1, "🏅", "Sports Medal\n:medal_sports:" );
  }

  // ─── Objects ───────────────────────────────────────────────────
  private void RegisterObjects()
  {
    RegisterSymbolX( 1, "💻", "Laptop\n:computer:"           );
    RegisterSymbolX( 1, "🖥️", "Desktop Computer\n:desktop_computer:" );
    RegisterSymbolX( 1, "🖨️", "Printer\n:printer:"           );
    RegisterSymbolX( 1, "⌨️", "Keyboard\n:keyboard:"         );
    RegisterSymbolX( 1, "🖱️", "Computer Mouse\n:three_button_mouse:" );
    RegisterSymbolX( 1, "💾", "Floppy Disk\n:floppy_disk:"   );
    RegisterSymbolX( 1, "💿", "Optical Disk\n:cd:"           );
    RegisterSymbolX( 1, "📀", "DVD\n:dvd:"                   );
    RegisterSymbolX( 1, "📱", "Mobile Phone\n:iphone:"       );
    RegisterSymbolX( 1, "📞", "Telephone Receiver\n:telephone_receiver:" );
    RegisterSymbolX( 1, "☎️", "Telephone\n:telephone:"       );
    RegisterSymbolX( 1, "📺", "Television\n:tv:"             );

    RegisterSymbolX( 1, "📷", "Camera\n:camera:"             );
    RegisterSymbolX( 1, "📸", "Camera with Flash\n:camera_flash:" );
    RegisterSymbolX( 1, "📹", "Video Camera\n:video_camera:" );
    RegisterSymbolX( 1, "🎥", "Movie Camera\n:movie_camera:" );
    RegisterSymbolX( 1, "📡", "Satellite Antenna\n:satellite:" );
    RegisterSymbolX( 1, "🔭", "Telescope\n:telescope:"       );
    RegisterSymbolX( 1, "🔬", "Microscope\n:microscope:"     );
    RegisterSymbolX( 1, "💡", "Light Bulb\n:bulb2:"          );
    RegisterSymbolX( 1, "🔦", "Flashlight\n:flashlight:"     );
    RegisterSymbolX( 1, "🕯️", "Candle\n:candle:"             );
    RegisterSymbolX( 1, "🔋", "Battery\n:battery:"           );
    RegisterSymbolX( 1, "🔌", "Electric Plug\n:electric_plug:" );

    RegisterSymbolX( 1, "🧲", "Magnet\n:magnet:"             );
    RegisterSymbolX( 1, "⚗️", "Alembic\n:alembic:"           );
    RegisterSymbolX( 1, "🧬", "DNA\n:dna:"                   );
    RegisterSymbolX( 1, "🩺", "Stethoscope\n:stethoscope:"   );
    RegisterSymbolX( 1, "💊", "Pill\n:pill:"                 );
    RegisterSymbolX( 1, "🩹", "Adhesive Bandage\n:adhesive_bandage:" );
    RegisterSymbolX( 1, "💉", "Syringe\n:syringe:"           );
    RegisterSymbolX( 1, "🧪", "Test Tube\n:test_tube:"       );
    RegisterSymbolX( 1, "🔨", "Hammer\n:hammer:"             );
    RegisterSymbolX( 1, "🪓", "Axe\n:axe:"                   );
    RegisterSymbolX( 1, "⚒️", "Hammer and Pick\n:hammer_and_pick:" );
    RegisterSymbolX( 1, "🛠️", "Hammer and Wrench\n:hammer_and_wrench:" );

    RegisterSymbolX( 1, "🔧", "Wrench\n:wrench:"             );
    RegisterSymbolX( 1, "🔩", "Nut and Bolt\n:nut_and_bolt:" );
    RegisterSymbolX( 1, "⚙️", "Gear\n:gear:"                 );
    RegisterSymbolX( 1, "🔑", "Key\n:key:"                   );
    RegisterSymbolX( 1, "🗝️", "Old Key\n:old_key:"           );
    RegisterSymbolX( 1, "🔒", "Locked\n:lock:"               );
    RegisterSymbolX( 1, "🔓", "Unlocked\n:unlock:"           );
    RegisterSymbolX( 1, "✂️", "Scissors\n:scissors:"         );
    RegisterSymbolX( 1, "📦", "Package\n:package:"           );
    RegisterSymbolX( 1, "📫", "Closed Mailbox\n:mailbox:"    );
    RegisterSymbolX( 1, "📝", "Memo\n:memo:"                 );
    RegisterSymbolX( 1, "📋", "Clipboard\n:clipboard:"       );

    RegisterSymbolX( 1, "📊", "Bar Chart\n:bar_chart:"       );
    RegisterSymbolX( 1, "📈", "Chart Increasing\n:chart_with_upwards_trend:" );
    RegisterSymbolX( 1, "📉", "Chart Decreasing\n:chart_with_downwards_trend:" );
    RegisterSymbolX( 1, "📅", "Calendar\n:calendar:"         );
    RegisterSymbolX( 1, "📌", "Pushpin\n:pushpin:"           );
    RegisterSymbolX( 1, "📍", "Round Pushpin\n:round_pushpin:" );
    RegisterSymbolX( 1, "📎", "Paperclip\n:paperclip:"       );
    RegisterSymbolX( 1, "📏", "Straight Ruler\n:straight_ruler:" );
    RegisterSymbolX( 1, "📐", "Triangular Ruler\n:triangular_ruler:" );
    RegisterSymbolX( 1, "🖊️", "Pen\n:pen_ballpoint:"        );
    RegisterSymbolX( 1, "✒️", "Black Nib\n:black_nib:"      );
    RegisterSymbolX( 1, "🔍", "Magnifying Glass Left\n:mag:" );

    RegisterSymbolX( 1, "📚", "Books\n:books:"               );
    RegisterSymbolX( 1, "📖", "Open Book\n:open_book:"       );
    RegisterSymbolX( 1, "📰", "Newspaper\n:newspaper:"       );
    RegisterSymbolX( 1, "🗂️", "Card Index Dividers\n:card_index_dividers:" );
    RegisterSymbolX( 1, "🗑️", "Wastebasket\n:wastebasket:"   );
    RegisterSymbolX( 1, "🔐", "Locked with Key\n:closed_lock_with_key:" );
    RegisterSymbolX( 1, "🪬", "Hamsa\n:hamsa:"               );
    RegisterSymbolX( 1, "🧿", "Nazar Amulet\n:nazar_amulet:" );
    RegisterSymbolX( 1, "🏮", "Red Paper Lantern\n:izakaya_lantern:" );
    RegisterSymbolX( 1, "💈", "Barber Pole\n:barber:"        );
    RegisterSymbolX( 1, "🪞", "Mirror\n:mirror:"             );
    RegisterSymbolX( 1, "🪑", "Chair\n:chair:"               );
  }

  // ─── Symbols ───────────────────────────────────────────────────
  private void RegisterSymbols()
  {
    RegisterSymbolX( 1, "⛔", "No Entry\n:no_entry:"         );
    RegisterSymbolX( 1, "🚫", "Prohibited\n:no_entry_sign:"  );
    RegisterSymbolX( 1, "🚳", "No Bicycles\n:no_bicycles:"   );
    RegisterSymbolX( 1, "🚭", "No Smoking\n:no_smoking:"     );
    RegisterSymbolX( 1, "🚯", "No Littering\n:do_not_litter:" );
    RegisterSymbolX( 1, "🚱", "Non-Potable Water\n:non_potable_water:" );
    RegisterSymbolX( 1, "🚷", "No Pedestrians\n:no_pedestrians:" );
    RegisterSymbolX( 1, "📵", "No Mobile Phones\n:no_mobile_phones:" );
    RegisterSymbolX( 1, "🔞", "No One Under 18\n:underage:"  );
    RegisterSymbolX( 1, "✅", "Check Mark Button\n:white_check_mark:" );
    RegisterSymbolX( 1, "❌", "Cross Mark\n:x:"              );
    RegisterSymbolX( 1, "❎", "Cross Mark Button\n:negative_squared_cross_mark:" );

    RegisterSymbolX( 1, "‼️", "Double Exclamation\n:bangbang:"  );
    RegisterSymbolX( 1, "⁉️", "Exclamation Question\n:interrobang:" );
    RegisterSymbolX( 1, "❓", "Question Mark\n:question:"     );
    RegisterSymbolX( 1, "❔", "White Question Mark\n:grey_question:" );
    RegisterSymbolX( 1, "❕", "White Exclamation Mark\n:grey_exclamation:" );
    RegisterSymbolX( 1, "❗", "Exclamation Mark\n:exclamation:" );
    RegisterSymbolX( 1, "🔔", "Bell\n:bell:"                 );
    RegisterSymbolX( 1, "🔕", "Bell with Slash\n:no_bell:"   );
    RegisterSymbolX( 1, "🔇", "Muted Speaker\n:mute:"        );
    RegisterSymbolX( 1, "🔈", "Speaker Low Volume\n:sound:"  );
    RegisterSymbolX( 1, "🔉", "Speaker Medium Volume\n:loudspeaker:" );
    RegisterSymbolX( 1, "🔊", "Speaker High Volume\n:loud_sound:" );

    RegisterSymbolX( 1, "🔴", "Red Circle\n:red_circle:"     );
    RegisterSymbolX( 1, "🟠", "Orange Circle\n:orange_circle:" );
    RegisterSymbolX( 1, "🟡", "Yellow Circle\n:yellow_circle:" );
    RegisterSymbolX( 1, "🟢", "Green Circle\n:green_circle:" );
    RegisterSymbolX( 1, "🔵", "Blue Circle\n:blue_circle:"   );
    RegisterSymbolX( 1, "🟣", "Purple Circle\n:purple_circle:" );
    RegisterSymbolX( 1, "⚫", "Black Circle\n:black_circle:" );
    RegisterSymbolX( 1, "⚪", "White Circle\n:white_circle:" );
    RegisterSymbolX( 1, "🟤", "Brown Circle\n:brown_circle:" );
    RegisterSymbolX( 1, "🔶", "Large Orange Diamond\n:large_orange_diamond:" );
    RegisterSymbolX( 1, "🔷", "Large Blue Diamond\n:large_blue_diamond:" );
    RegisterSymbolX( 1, "🔸", "Small Orange Diamond\n:small_orange_diamond:" );

    RegisterSymbolX( 1, "🔹", "Small Blue Diamond\n:small_blue_diamond:" );
    RegisterSymbolX( 1, "🔺", "Red Triangle Up\n:small_red_triangle:" );
    RegisterSymbolX( 1, "🔻", "Red Triangle Down\n:small_red_triangle_down:" );
    RegisterSymbolX( 1, "💠", "Diamond with Dot\n:diamond_shape_with_a_dot_inside:" );
    RegisterSymbolX( 1, "🔘", "Radio Button\n:radio_button:" );
    RegisterSymbolX( 1, "🔲", "Black Square Button\n:black_square_button:" );
    RegisterSymbolX( 1, "🔳", "White Square Button\n:white_square_button:" );
    RegisterSymbolX( 1, "♻️", "Recycling Symbol\n:recycle:"  );
    RegisterSymbolX( 1, "⚜️", "Fleur-de-Lis\n:fleur_de_lis:" );
    RegisterSymbolX( 1, "🔱", "Trident Emblem\n:trident:"    );
    RegisterSymbolX( 1, "📛", "Name Badge\n:name_badge:"     );
    RegisterSymbolX( 1, "🔰", "Japanese Symbol for Beginner\n:beginner:" );

    RegisterSymbolX( 1, "⬆️", "Up Arrow\n:arrow_up:"         );
    RegisterSymbolX( 1, "↗️", "Up-Right Arrow\n:arrow_upper_right:" );
    RegisterSymbolX( 1, "➡️", "Right Arrow\n:arrow_right:"   );
    RegisterSymbolX( 1, "↘️", "Down-Right Arrow\n:arrow_lower_right:" );
    RegisterSymbolX( 1, "⬇️", "Down Arrow\n:arrow_down:"     );
    RegisterSymbolX( 1, "↙️", "Down-Left Arrow\n:arrow_lower_left:" );
    RegisterSymbolX( 1, "⬅️", "Left Arrow\n:arrow_left:"     );
    RegisterSymbolX( 1, "↖️", "Up-Left Arrow\n:arrow_upper_left:" );
    RegisterSymbolX( 1, "↕️", "Up-Down Arrow\n:arrows_up_down:" );
    RegisterSymbolX( 1, "↔️", "Left-Right Arrow\n:arrows_left_right:" );
    RegisterSymbolX( 1, "↩️", "Right Arrow Curving Left\n:leftwards_arrow_with_hook:" );
    RegisterSymbolX( 1, "↪️", "Left Arrow Curving Right\n:arrow_right_hook:" );

    RegisterSymbolX( 1, "🔁", "Repeat Button\n:repeat:"      );
    RegisterSymbolX( 1, "🔂", "Repeat Single Button\n:repeat_one:" );
    RegisterSymbolX( 1, "🔀", "Shuffle\n:twisted_rightwards_arrows:" );
    RegisterSymbolX( 1, "🔃", "Clockwise Arrows\n:arrows_clockwise:" );
    RegisterSymbolX( 1, "♾️", "Infinity\n:infinity:"          );
    RegisterSymbolX( 1, "🆗", "OK Button\n:ok:"               );
    RegisterSymbolX( 1, "🆕", "NEW Button\n:new:"             );
    RegisterSymbolX( 1, "🆙", "UP Button\n:up:"               );
    RegisterSymbolX( 1, "🆒", "COOL Button\n:cool:"           );
    RegisterSymbolX( 1, "🆓", "FREE Button\n:free:"           );
    RegisterSymbolX( 1, "🆖", "NG Button\n:ng:"               );
    RegisterSymbolX( 1, "🔮", "Crystal Ball\n:crystal_ball:"  );
  }

}
