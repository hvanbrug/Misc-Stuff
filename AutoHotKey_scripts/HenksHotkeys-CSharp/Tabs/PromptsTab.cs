using HenksHotkeys.Core;
using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

/// <summary>Prompt-helper tab (PromptHelpers.ahk): canned prompt fragments and templates.</summary>
internal sealed class PromptsTab : TabModel
{
  public PromptsTab() : base( "Prompt Helpers" )
  {
    FontSize    = 10f;
    SymBtnSizeX = 214;
    SymBtnSizeY = 24;

    SetRowsOf( 3 );
    RegisterButtons();
    RecalcSizes();
  }

  private void RegisterButtons()
  {
    RegisterSymbolX( 1, "No speckled noise, random particles, or visual artifacts.",                            "Avoid noise and artifacts",   null, null, "left", 0, 1 );
    RegisterSymbolX( 1, "Clean up all speckled noise, random particles, and visual artifacts.",                 "Remove noise and artifacts",  null, null, "left", 0, 1 );
    RegisterSymbolX( 1, "Keep all other aspects of the original image unchanged. Make no additional changes.",  "Edit prompt closer",          "^+4", null, "left", 0, 1 );
    RegisterSymbolX( 1, "A semi-realistic digital illustration of \n\n\n\nNo speckled noise, random particles, or visual artifacts.", "Prompt basic template",       "^+3", null, "left", 0, 1 );
    RegisterSymbolX( 1, "STYLE:\n\n\nSUBJECT:\n\n\nSETTING:\n\n",                                               "Prompt basic template (old)", null, null, "left", 0, 1 );
    RegisterSymbolX( 1, "",                                                                                     "Pony prompt modifier",        "^+7", PonyPromptModifier,   "left", 0, 1 );
    RegisterSymbolX( 1, "A semi-realistic digital illustration of ",                                            "Semi-realistic",              "^+5", null, "left", 0, 1 );
    RegisterSymbolX( 1, "",                                                                                     "Short negative prompt",       "^+6", NegativePromptShort,  "left", 0, 1 );
    RegisterSymbolX( 1, "",                                                                                     "Long negative prompt",       "^!+6", NegativePromptLong,   "left", 0, 1 );

    ShiftLineByThird();
    RegisterSymbolX( 1, "", "GPT 2 - Soft Upscale",            null, GptUpscaleSoft,      "left", 0, 1 );
    RegisterSymbolX( 1, "", "GPT 2 - Hard Upscale",            null, GptUpscaleHard,      "left", 0, 1 );
    RegisterSpace();
    RegisterSymbolX( 1, "", "GPT 2 - Painterly Upscale addon", null, GptUpscalePainterly, "left", 0, 1 );
    RegisterSymbolX( 1, "", "GPT 2 - Realistic Upscale addon", null, GptUpscaleRealistic, "left", 0, 1 );
    RegisterSpace();

    ShiftLineByThird();
    RegisterSymbolX( 1, "abc of def - score - DC \n", "Daily Challenge description", null, null, "left", 0, 1 );
    NextLine();
    RegisterSymbolX( 1, "abc of def (ghi) - score\n", "Community Challenge desc.",   null, null, "left", 0, 1 );
  }

  private static void GptUpscaleSoft()
  {
    TextSender.SendText(
      "Refine and lightly upscale this image while preserving the " +
      "original composition, character identity, pose, lighting, " +
      "colors, and artistic style.\n\n" +
      "Clean up minor noise, compression artifacts, blurry edges, " +
      "low-resolution defects, and small rendering inconsistencies. " +
      "Improve overall clarity, texture definition, and detail " +
      "cohesion while keeping the image natural and faithful to the " +
      "original.\n\n" +
      "Correct subtle anatomy or rendering issues only where " +
      "necessary. Do not redesign characters, alter proportions, " +
      "change the scene layout, or reinterpret the image.\n\n" +
      "Natural detail, coherent textures, soft clean rendering, " +
      "subtle sharpening, consistent quality." );
  }

  private static void GptUpscaleHard()
  {
    TextSender.SendText(
      "Upscale and refine this image while preserving the original " +
      "composition, character identity, pose, camera angle, lighting, " +
      "colors, clothing, and overall artistic style.\n\n" +
      "Clean up compression artifacts, noise, pixelation, blurry " +
      "details, jagged edges, muddy textures, and low-resolution " +
      "defects. Improve fine detail, edge definition, skin texture, " +
      "hair strands, fabric texture, metallic surfaces, and " +
      "environmental detail while keeping the image natural and " +
      "cohesive.\n\n" +
      "Correct malformed anatomy, warped fingers, distorted eyes, " +
      "asymmetry, duplicated features, and small rendering defects " +
      "without redesigning the scene.\n\n" +
      "Preserve the original mood and framing exactly. Do not change " +
      "the scene layout, add new objects, alter proportions, crop " +
      "the image, or reinterpret the art style.\n\n" +
      "High clarity, clean detail, polished rendering, coherent " +
      "textures, subtle natural sharpening, artifact-free image, " +
      "professional quality." );
  }

  private static void GptUpscalePainterly()
  {
    TextSender.SendText(
      "Maintain painterly texture and natural artistic brushwork. " +
      "Avoid plastic smoothing or photorealistic conversion." );
  }

  private static void GptUpscaleRealistic()
  {
    TextSender.SendText(
      "Maintain realistic skin texture, pores, and natural " +
      "imperfections. Avoid waxy or overprocessed surfaces." );
  }

  private static void NegativePromptShort()
  {
    TextSender.SendText(
      "ugly, tiling, out of frame, disfigured, deformed, out of frame,  body out of frame, blurry,\n" +
      "blurred, watermark, grainy, canvas frame, ms paint, disfigured, bad art, close up, duplicate,\n" +
      "letterbox, lowres, text, error, cropped, jpeg artifacts, username, logo, signature, cut off, draft,\n" +
      "\n" +
      "bad anatomy, bad proportions, cloned face, gross proportions, extra limbs, malformed limbs,\n" +
      "morbid, mutilated, mutation, long neck, surgery scars,\n" +
      "\n" +
      "poorly drawn fingers, missing fingers, extra fingers, error fingers, bad fingers, fused fingers,\n" +
      "poorly drawn toes,    missing toes,    extra toes,    error toes,    bad toes,    fused toes,\n" +
      "\n" +
      "poorly drawn hands, missing hands, extra hands, error hands, bad hands, multiple hands, merged hands, mutated hands,\n" +
      "poorly drawn feet,  missing feet,  extra feet,  error feet,  bad feet,  multiple feet,  merged feet,  mutated feet,\n" +
      "poorly drawn arms,  missing arms,  extra arms,  error arms,  bad arms,  multiple arms,  merged arms,  mutated arms,\n" +
      "poorly drawn legs,  missing legs,  extra legs,  error legs,  bad legs,  multiple legs,  merged legs,  mutated legs,\n" +
      "poorly drawn face,  missing face,  extra face,  error face,  bad face,  multiple face,  merged face,  mutated face,\n" +
      "\n" +
      "error eyes,  bad eyes, bug eyes, cross-eyed,\n" +
      "error mouth, bad mouth,\n" +
      "error body,\n" +
      "error hair,\n" +
      "\n" +
      "error lighting, error shadow, error reflection," );
  }

  private static void NegativePromptLong()
  {
    TextSender.SendText(
      "[QUALITY]\n" +
      "ugly, low quality, grainy, blurry, blurred, jpeg artifact, oversaturated, overexposed, plastic-like,\n" +
      "unrealistic, lowres, draft, ms paint, bad art,\n" +
      "error lighting, error shadow, error reflection,\n" +
      "\n" +
      "[FACIAL]\n" +
      "poorly drawn face,  missing face,  extra face,  error face,  bad face,  multiple face,  merged face,  mutated face,  mangled face,\n" +
      "poorly drawn eyes,  missing eyes,  extra eyes,  error eyes,  bad eyes,  multiple eyes,  merged eyes,  mutated eyes,  mangled eyes,\n" +
      "poorly drawn mouth, missing mouth, extra mouth, error mouth, bad mouth, multiple mouth, merged mouth, mutated mouth, mangled mouth,\n" +
      "\n" +
      "crooked face, duplicate faces, cloned face,\n" +
      "mismatched eyes, bug eyes, cross-eyed,\n" +
      "\n" +
      "[ANATOMY]\n" +
      "bad anatomy, incorrect anatomy, bad proportions, gross proportions, extra limbs, malformed limbs, disfigured, deformed,\n" +
      "morbid, mutilated, mutation, long neck, surgical scars, backwards limbs, unexpected bumps, unexpected markings, unexpected growth,\n" +
      "error hair, female body hair,\n" +
      "\n" +
      "poorly drawn fingers, missing fingers, extra fingers, error fingers, bad fingers, fused fingers,\n" +
      "poorly drawn toes,    missing toes,    extra toes,    error toes,    bad toes,    fused toes,\n" +
      "(seven fingers, six fingers, four fingers, three fingers:1.5),\n" +
      "(seven toes,    six toes,    four toes,    three toes:1.5),\n" +
      "\n" +
      "poorly drawn hands, missing hands, extra hands, error hands, bad hands, multiple hands, merged hands, mutated hands, mangled hands,\n" +
      "poorly drawn feet,  missing feet,  extra feet,  error feet,  bad feet,  multiple feet,  merged feet,  mutated feet,  mangled feet,\n" +
      "poorly drawn arms,  missing arms,  extra arms,  error arms,  bad arms,  multiple arms,  merged arms,  mutated arms,  mangled arms,\n" +
      "poorly drawn legs,  missing legs,  extra legs,  error legs,  bad legs,  multiple legs,  merged legs,  mutated legs,  mangled legs,\n" +
      "poorly drawn body,  missing body,  extra body,  error body,  bad body,  multiple body,  merged body,  mutated body,  mangled body,\n" +
      "\n" +
      "[COMPOSITION_FRAMING]\n" +
      "body out of frame, out of frame, off-center, poor composition, unclear details, cropped, canvas frame,\n" +
      "cut off, cut-off body parts, strange body positions, impossible body positioning,\n" +
      "\n" +
      "[ARTIFACTS]\n" +
      "tiling, watermark, logo, signature, cartoonish, error, duplicate, text, username," );
  }

  private static void PonyPromptModifier()
  {
    TextSender.SendText(
      "score_9, score_8_up, score_7_up, score_6_up, " +
      "score_5_up, score_4_up, score_3_up, score_2_up, score_1_up," );
  }
}
