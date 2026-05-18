; PromptHelpers.ahk
; A collection of helper shortcuts for writing specific prompts.




class PromptsTabPage extends TabPage
{
  __New()
  {
    super.__New( "Prompt Helpers" )

    super.m_fontSize    := "s10"
    super.m_symBtnSizeX := 320
    super.m_symBtnSizeY := 24

    this.LoadLargePrompts()

    super.SetRowsOf( 2 )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {

    super.RegisterSymbolX( 1, "no speckled noise, random particles, or visual artifacts",             "Avoid noise and artifacts",  unset, unset, "left", 0, 1 )
    super.RegisterSymbolX( 1, "Clean up all speckled noise, random particles, and visual artifacts.", "Remove noise and artifacts", unset, unset, "left", 0, 1 )
    super.RegisterSymbolX( 1, "A semi-realistic digital illustration of ",                            "Semi-realistic",             unset, unset, "left", 0, 1 )
    super.RegisterSpace()

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, this.GPT2UpscalePromptSoft, "GPT 2 - Soft Upscale",            unset, unset, "left", 0, 1 )
    super.RegisterSymbolX( 1, this.GPT2UpscalePainterly,  "GPT 2 - Painterly Upscale addon", unset, unset, "left", 0, 1 )
    super.RegisterSymbolX( 1, this.GPT2UpscalePromptHard, "GPT 2 - Hard Upscale",            unset, unset, "left", 0, 1 )
    super.RegisterSymbolX( 1, this.GPT2UpscaleRealistic,  "GPT 2 - Realistic Upscale addon", unset, unset, "left", 0, 1 )

    super.ShiftLineByThird()
    super.RegisterSymbolX( 1, "abc of def - score - DC `n", "Daily Challenge description",     unset, unset, "left", 0, 1 )
    super.RegisterSymbolX( 1, "abc of def (ghi) - score`n", "Community Challenge description", unset, unset, "left", 0, 1 )
  }

  LoadLargePrompts()
  {
    this.GPT2UpscalePromptSoft := "Refine and lightly upscale this image while preserving the "
    this.GPT2UpscalePromptSoft .= "original composition, character identity, pose, lighting, "
    this.GPT2UpscalePromptSoft .= "colors, and artistic style.`n`n"
    this.GPT2UpscalePromptSoft .= "Clean up minor noise, compression artifacts, blurry edges, "
    this.GPT2UpscalePromptSoft .= "low-resolution defects, and small rendering inconsistencies. "
    this.GPT2UpscalePromptSoft .= "Improve overall clarity, texture definition, and detail "
    this.GPT2UpscalePromptSoft .= "cohesion while keeping the image natural and faithful to the "
    this.GPT2UpscalePromptSoft .= "original.`n`n"
    this.GPT2UpscalePromptSoft .= "Correct subtle anatomy or rendering issues only where "
    this.GPT2UpscalePromptSoft .= "necessary. Do not redesign characters, alter proportions, "
    this.GPT2UpscalePromptSoft .= "change the scene layout, or reinterpret the image.`n`n"
    this.GPT2UpscalePromptSoft .= "Natural detail, coherent textures, soft clean rendering, "
    this.GPT2UpscalePromptSoft .= "subtle sharpening, consistent quality."

    this.GPT2UpscalePromptHard := "Upscale and refine this image while preserving the original "
    this.GPT2UpscalePromptHard .= "composition, character identity, pose, camera angle, lighting, "
    this.GPT2UpscalePromptHard .= "colors, clothing, and overall artistic style.`n`n"
    this.GPT2UpscalePromptHard .= "Clean up compression artifacts, noise, pixelation, blurry "
    this.GPT2UpscalePromptHard .= "details, jagged edges, muddy textures, and low-resolution "
    this.GPT2UpscalePromptHard .= "defects. Improve fine detail, edge definition, skin texture, "
    this.GPT2UpscalePromptHard .= "hair strands, fabric texture, metallic surfaces, and "
    this.GPT2UpscalePromptHard .= "environmental detail while keeping the image natural and "
    this.GPT2UpscalePromptHard .= "cohesive.`n`n"
    this.GPT2UpscalePromptHard .= "Correct malformed anatomy, warped fingers, distorted eyes, "
    this.GPT2UpscalePromptHard .= "asymmetry, duplicated features, and small rendering defects "
    this.GPT2UpscalePromptHard .= "without redesigning the scene.`n`n"
    this.GPT2UpscalePromptHard .= "Preserve the original mood and framing exactly. Do not change "
    this.GPT2UpscalePromptHard .= "the scene layout, add new objects, alter proportions, crop "
    this.GPT2UpscalePromptHard .= "the image, or reinterpret the art style.`n`n"
    this.GPT2UpscalePromptHard .= "High clarity, clean detail, polished rendering, coherent "
    this.GPT2UpscalePromptHard .= "textures, subtle natural sharpening, artifact-free image, "
    this.GPT2UpscalePromptHard .= "professional quality."

    this.GPT2UpscalePainterly := "Maintain painterly texture and natural artistic brushwork. "
    this.GPT2UpscalePainterly .= "Avoid plastic smoothing or photorealistic conversion."

    this.GPT2UpscaleRealistic := "Maintain realistic skin texture, pores, and natural "
    this.GPT2UpscaleRealistic .= "imperfections. Avoid waxy or overprocessed surfaces."
  }
}










; Ctrl + Shift + 3 => Basic prompt template
RegisterAction( "Ctrl+Shift+3", "Basic prompt template", PromptBasicTemplate )
^+3::PromptBasicTemplate()
PromptBasicTemplate()
{
  DoSendText( "STYLE:`n`n`n"
              "SUBJECT:`n`n`n"
              "SETTING:`n`n" )
}

; Ctrl + Shift + 4 => Edit prompt closer
RegisterAction( "Ctrl+Shift+4", "Edit prompt closer", EditPromptCloser )
^+4::EditPromptCloser()
EditPromptCloser()
{
  DoSendText( "Keep all other aspects of the original image "
              "unchanged. Make no additional changes." )
}

; Ctrl + Shift + 5 => Semi-realistic digital illustration prompt
RegisterAction( "Ctrl+Shift+5", "Semi-realistic digital illustration", SemiRealDigIllust )
^+5::SemiRealDigIllust()
SemiRealDigIllust()
{
  DoSendText( "semi-realistic digital illustration" )
}

; Ctrl + Shift + 6 => Short negative prompt
RegisterAction( "Ctrl+Shift+6", "Short negative prompt", NegativePromptShort )
^+6::NegativePromptShort()
NegativePromptShort()
{
  DoSendText( "ugly, tiling, out of frame, disfigured, deformed, out of frame,  body out of frame, blurry,`n"
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

; Ctrl + Shift + Alt + 6 => Long negative prompt
RegisterAction( "Ctrl+Shift+Alt+6", "Long negative prompt", NegativePromptLong )
^+!6::NegativePromptLong()
NegativePromptLong()
{
  DoSendText( "[QUALITY]`n"
              "ugly, low quality, grainy, blurry, blurred, jpeg artifact, oversaturated, overexposed, plastic-like,`n"
              "unrealistic, lowres, draft, ms paint, bad art,`n"
              "error lighting, error shadow, error reflection,`n"
              "`n"
              "[FACIAL]`n"
              "poorly drawn face,  missing face,  extra face,  error face,  bad face,  multiple face,  merged face,  mutated face,  mangled face,`n"
              "poorly drawn eyes,  missing eyes,  extra eyes,  error eyes,  bad eyes,  multiple eyes,  merged eyes,  mutated eyes,  mangled eyes,`n"
              "poorly drawn mouth, missing mouth, extra mouth, error mouth, bad mouth, multiple mouth, merged mouth, mutated mouth, mangled mouth,`n"
              "`n"
              "crooked face, duplicate faces, cloned face,`n"
              "mismatched eyes, bug eyes, cross-eyed,`n"
              "`n"
              "[ANATOMY]`n"
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
              "[COMPOSITION_FRAMING]`n"
              "body out of frame, out of frame, off-center, poor composition, unclear details, cropped, canvas frame,`n"
              "cut off, cut-off body parts, strange body positions, impossible body positioning,`n"
              "`n"
              "[ARTIFACTS]`n"
              "tiling, watermark, logo, signature, cartoonish, error, duplicate, text, username," )
}

; Ctrl + Shift + 7 => Pony Promp modifier
RegisterAction( "Ctrl+Shift+7", "Pony prompt modifier", PonyPrompModifier )
^+7::PonyPrompModifier()
PonyPrompModifier()
{
  DoSendText( "score_9, score_8_up, score_7_up, score_6_up, "
              "score_5_up, score_4_up, score_3_up, score_2_up, score_1_up," )
}


; Ctrl + Shift + S => SREF to full prompt conversion
RegisterAction( "Ctrl+Shift+S", "SREF to full prompt conversion", SREFtoFullPrompt )
^+s::SREFtoFullPrompt()
SREFtoFullPrompt()
{
  Haystack    := GetSelectedTextThroughClipboard()
  NeedleRegEx := "ms)\s*\|\s*"
  Replacement := ",`s"
  NewStr      := RegExReplace( Haystack, NeedleRegEx, Replacement )
  A_Clipboard := NewStr
}
