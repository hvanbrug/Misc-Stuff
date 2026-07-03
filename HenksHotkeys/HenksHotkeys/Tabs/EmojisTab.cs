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
    ApplySkinTone();          // re-tint toneable emoji to the active tone (#27)
    RecalcSizes();
  }

  // Re-tint every toneable catalog emoji to the active skin tone (#27) — the picture and the sent
  // text both change. Favourites are left as the user stored them; non-toneable emoji are untouched
  // (EmojiSkin.Apply only tones an emoji that has a real toned image).
  private void ApplySkinTone()
  {
    string tone = Core.AppState.SkinTone;
    if( string.IsNullOrEmpty( tone ) )
    {
      return;
    }
    foreach( SymbolElement s in Symbols )
    {
      if( s.IsFavourite )
      {
        continue;
      }
      s.Char = Emoji.EmojiSkin.Apply( s.BaseChar, tone ); // image uses this; the click reads it live
    }
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
  // a heading, then the category's buttons flowing below it — unless the section is collapsed (#26),
  // in which case the buttons are skipped entirely and everything below flows up.
  private void Category( string name, Action register )
  {
    bool collapsed = Core.EmojiSectionStore.IsCollapsed( name );
    NextLine( true );                        // finish the previous category's partial row
    if( m_placedCategory ) ShiftLineByHalf(); // gap above the heading (not for the first one)
    RegisterSectionHeader( name, collapsible: true, collapsed: collapsed );
    if( !collapsed ) register();
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

    bool collapsed = Core.EmojiSectionStore.IsCollapsed( "Favourites" );
    RegisterSectionHeader( "Favourites", collapsible: true, collapsed: collapsed );
    if( collapsed )
    {
      return true; // heading only; the favourites are hidden
    }

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
    // Base (yellow) only; skin-tone variants come from the global tone selector (#27).
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
    RegisterSymbolX( 1, "🧞‍♂️", "Man Genie\n:man_genie:" );
    RegisterSymbolX( 1, "🧞‍♀️", "Woman Genie\n:woman_genie:" );
    RegisterSymbolX( 1, "🧟‍♂️", "Man Zombie\n:man_zombie:" );
    RegisterSymbolX( 1, "🧟‍♀️", "Woman Zombie\n:woman_zombie:" );
    RegisterSymbolX( 1, "👯", "People With Bunny Ears\n:people_with_bunny_ears:" );
    RegisterSymbolX( 1, "👯‍♂️", "Men With Bunny Ears\n:men_with_bunny_ears:" );
    RegisterSymbolX( 1, "👯‍♀️", "Women With Bunny Ears\n:women_with_bunny_ears:" );
    RegisterSymbolX( 1, "🤼", "People Wrestling\n:people_wrestling:" );
    RegisterSymbolX( 1, "🤼‍♂️", "Men Wrestling\n:men_wrestling:" );
    RegisterSymbolX( 1, "🤼‍♀️", "Women Wrestling\n:women_wrestling:" );
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
    RegisterSymbolX( 1, "🫄‍♂️", "Pregnant Man Variant\n:pregnant_man_variant:" );
    RegisterSymbolX( 1, "🫄‍♀️", "Pregnant Woman Variant\n:pregnant_woman_variant:" );
    RegisterSymbolX( 1, "🫃‍♂️", "Pregnant Man Male Variant\n:pregnant_man_male_variant:" );
    RegisterSymbolX( 1, "🫃‍♀️", "Pregnant Man Female Variant\n:pregnant_man_female_variant:" );
    RegisterSymbolX( 1, "🫅‍♂️", "Person With Crown Male Variant\n:person_with_crown_male_variant:" );
    RegisterSymbolX( 1, "🫅‍♀️", "Person With Crown Female Variant\n:person_with_crown_female_variant:" );
    RegisterSymbolX( 1, "🫷‍🫸", "Pushing Hands\n:pushing_hands:" );
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
