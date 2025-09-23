#Requires AutoHotkey v2.0
#SingleInstance Force

::-hvb-::
{
  SendInput( 'Henk vanBruggen' )
}


; ^ = Ctrl key
; + = Shift key
; # = Windows key
; ! = Alt key

^#+.::BulletCharacter()
BulletCharacter()
{
  SendInput( '•' )
}

^#`::AlmostEqualCharacter()
AlmostEqualCharacter()
{
  SendInput( '≈' )
}

^#,::DoubleArrowLeftCharacter()
DoubleArrowLeftCharacter()
{
  SendInput( '⇐' )
}
^#.::DoubleArrowRightCharacter()
DoubleArrowRightCharacter()
{
  SendInput( '⇒' )
}

^+,::DoubleLongArrowLeftCharacter()
DoubleLongArrowLeftCharacter()
{
  SendInput( '⟸' )
}

^+.::DoubleLongArrowRightCharacter()
DoubleLongArrowRightCharacter()
{
  SendInput( '⟹' )
}

!+1::AppData()
AppData()
{
  SendInput( "%appdata%" )
}

!+2::LocalAppData()
LocalAppData()
{
  SendInput( "%localappdata%" )
}

^+1::DailyChallengeDescription()
DailyChallengeDescription()
{
  SendInput( "abc of def - score - DC `n" )
}

^+2::CommunityChallengeDescription()
CommunityChallengeDescription()
{
  SendInput( "abc of def (ghi) - score`n" )
}

^+3::PromptBasicTemplate()
PromptBasicTemplate()
{
  SendInput( "STYLE:`n`n`n"
             "SUBJECT:`n`n`n"
             "SETTING:`n`n" )
}

^+!3::PromptBasicTemplateShift()
PromptBasicTemplateShift()
{
  SendInput( "[S.T.Y.L.E]+`n+`n+`n"
             "[S.U.B.J.E.C.T]+`n+`n+`n"
             "[S.E.T.T.I.N.G]+`n+`n" )
}

^+4::KontextEditCloser()
KontextEditCloser()
{
  SendInput( "Keep all other aspects of the original image "
             "exactly the same. Make no other changes." )
}

^+5::NegativePromptShort()
NegativePromptShort()
{
  SendInput( "ugly, tiling, out of frame, disfigured, deformed, out of frame,  body out of frame, blurry,`n"
             "blurred, watermark, grainy, canvas frame, ms paint, disfigured, bad art, close up, duplicate,`n"
             "letterbox, lowres, text, error, cropped, jpeg artifacts, username, logo, signature, cut off, draft,`n"
             "`n"
             "bad anatomy, bad proportions, cloned face, gross proportions, extra limbs, malformed limbs,`n"
             "morbid, mutilated, mutation, long neck, surgery scars,`n"
             "`n"
             "poorly drawn fingers, missing fingers, extra fingers, error fingers, bad fingers, fused fingers,`n"
             "poorly drawn toes,    missing toes,    extra toes,    error toes,    bad toes,    fused toes,`n"
             "`n"
             "poorly drawn hands, missing hands, extra hands, error hands, bad hands, multiple hands, merged hands, mutated hands,`n"
             "poorly drawn feet,  missing feet,  extra feet,  error feet,  bad feet,  multiple feet,  merged feet,  mutated feet,`n"
             "poorly drawn arms,  missing arms,  extra arms,  error arms,  bad arms,  multiple arms,  merged arms,  mutated arms,`n"
             "poorly drawn legs,  missing legs,  extra legs,  error legs,  bad legs,  multiple legs,  merged legs,  mutated legs,`n"
             "poorly drawn face,  missing face,  extra face,  error face,  bad face,  multiple face,  merged face,  mutated face,`n"
             "`n"
             "error eyes,  bad eyes, bug eyes, cross-eyed,`n"
             "error mouth, bad mouth,`n"
             "error body,`n"
             "error hair,`n"
             "`n"
             "error lighting, error shadow, error reflection," )
}

^+!5::NegativePromptLong()
NegativePromptLong()
{
  SendInput( "[Q.U.A.L.I.T.Y]`n"
             "ugly, low quality, grainy, blurry, blurred, jpeg artifact, oversaturated, overexposed, plastic-like,`n"
             "unrealistic, lowres, draft, ms paint, bad art,`n"
             "error lighting, error shadow, error reflection,`n"
             "`n"
             "[F.A.C.I.A.L]`n"
             "poorly drawn face,  missing face,  extra face,  error face,  bad face,  multiple face,  merged face,  mutated face,  mangled face,`n"
             "poorly drawn eyes,  missing eyes,  extra eyes,  error eyes,  bad eyes,  multiple eyes,  merged eyes,  mutated eyes,  mangled eyes,`n"
             "poorly drawn mouth, missing mouth, extra mouth, error mouth, bad mouth, multiple mouth, merged mouth, mutated mouth, mangled mouth,`n"
             "`n"
             "crooked face, duplicate faces, cloned face,`n"
             "mismatched eyes, bug eyes, cross-eyed,`n"
             "`n"
             "[A.N.A.T.O.M.Y]`n"
             "bad anatomy, incorrect anatomy, bad proportions, gross proportions, extra limbs, malformed limbs, disfigured, deformed,`n"
             "morbid, mutilated, mutation, long neck, surgical scars, backwards limbs, unexpected bumps, unexpected markings, unexpected growth,`n"
             "error hair, female body hair,`n"
             "`n"
             "poorly drawn fingers, missing fingers, extra fingers, error fingers, bad fingers, fused fingers,`n"
             "poorly drawn toes,    missing toes,    extra toes,    error toes,    bad toes,    fused toes,`n"
             "(seven fingers, six fingers, four fingers, three fingers:1.5),`n"
             "(seven toes,    six toes,    four toes,    three toes:1.5),`n"
             "`n"
             "poorly drawn hands, missing hands, extra hands, error hands, bad hands, multiple hands, merged hands, mutated hands, mangled hands,`n"
             "poorly drawn feet,  missing feet,  extra feet,  error feet,  bad feet,  multiple feet,  merged feet,  mutated feet,  mangled feet,`n"
             "poorly drawn arms,  missing arms,  extra arms,  error arms,  bad arms,  multiple arms,  merged arms,  mutated arms,  mangled arms,`n"
             "poorly drawn legs,  missing legs,  extra legs,  error legs,  bad legs,  multiple legs,  merged legs,  mutated legs,  mangled legs,`n"
             "poorly drawn body,  missing body,  extra body,  error body,  bad body,  multiple body,  merged body,  mutated body,  mangled body,`n"
             "`n"
             "[C.O.M.P.O.S.I.T.I.O.N_F.R.A.M.I.N.G]`n"
             "body out of frame, out of frame, off-center, poor composition, unclear details, cropped, canvas frame,`n"
             "cut off, cut-off body parts, strange body positions, impossible body positioning,`n"
             "`n"
             "[A.R.T.I.F.A.C.T.S]`n"
             "tiling, watermark, logo, signature, cartoonish, error, duplicate, text, username," )
}

^+6::InstagramTagForNC()
InstagramTagForNC()
{
  SendInput( "@nightcafestudio" )
}

^+7::PonyPrompModifier()
PonyPrompModifier()
{
  SendInput( "score_9, score_8_up, score_7_up, score_6_up, "
             "score_5_up, score_4_up, score_3_up, score_2_up, score_1_up," )
}

^+0::CreationPrefix()
CreationPrefix()
{
  SendInput( "https://creator.nightcafe.studio/creation/" )
}

^+s::SREFtoFullPrompt()
SREFtoFullPrompt()
{
  Haystack    := GetSelectedTextThroughClipboard()
  NeedleRegEx := "ms)\s*\|\s*"
  Replacement := ",`s"
  NewStr      := RegExReplace( Haystack, NeedleRegEx, Replacement )
  A_Clipboard := NewStr
}

GetSelectedTextThroughClipboard()
{
  backup := ClipboardAll()
  A_Clipboard := ''
  Send( '^c' )
  if !ClipWait( 1, 1 )
  {
    return (A_Clipboard := backup)
  }
  txt := A_Clipboard
  A_Clipboard := backup
  return txt
}



^+#F6::SendInput( "MyDogIs1Cut3Puppy" )
^+#F7::SendInput( "buildprogrammer.geo@microsurvey.com" )
^+#F8::SendInput( "MS2023bp{!}run" )
^+#F9::SendInput( "henk.vanbruggen@microsurvey.com" )
^+#F10::SendInput( "H{!}Pircotha29" )
^+#F11::SendInput( "henk.vanbruggen@microsurvey.onmicrosoft.com" )
^+#F12::SendInput( "HVB13{#}dvlp{@}" )
