using System.ComponentModel;
using Newtonsoft.Json;

namespace HenksHotkeys.Core;

/// <summary>
/// The on-disk tab/button content (tabs.json). Hand-editable: each entry is either
/// a data tab (name + columns + rows of buttons) or a reference to a built-in code
/// tab (<see cref="Builtin"/>, e.g. "Emojis" / "Tools"). Defaults are omitted from
/// the file by the serializer so it stays terse.
/// </summary>
internal sealed class TabFile
{
  /// <summary>Free-text help shown at the top of the file. Ignored by the app but
  /// preserved across saves so the cheat-sheet survives re-serialisation.</summary>
  [JsonProperty( "_readme", NullValueHandling = NullValueHandling.Ignore )]
  public string[]? Readme { get; set; }

  /// <summary>Crypto parameters for the passphrase-encrypted secrets (travels with
  /// the file so the same passphrase decrypts on another machine). Null until the
  /// first secret is sealed.</summary>
  [JsonProperty( "crypto", NullValueHandling = NullValueHandling.Ignore )]
  public CryptoHeader? Crypto { get; set; }

  [JsonProperty( "tabs" )]
  public List<TabEntry> Tabs { get; set; } = new();

  /// <summary>Tombstones for tabs/buttons deleted on some machine, so the deletion
  /// wins over a stale copy on merge. App-managed; prunable.</summary>
  [JsonProperty( "_deleted", NullValueHandling = NullValueHandling.Ignore )]
  public List<Tombstone>? Deleted { get; set; }
}

internal sealed class Tombstone
{
  [JsonProperty( "id" )]  public string Id  { get; set; } = "";
  [JsonProperty( "mod" )] public long   Mod { get; set; }
}

internal sealed class CryptoHeader
{
  [JsonProperty( "salt" )]
  public string Salt { get; set; } = "";

  [JsonProperty( "iterations" )]
  public int Iterations { get; set; } = Secrets.DefaultIterations;

  /// <summary>A known token encrypted with the key, used to detect a wrong passphrase.</summary>
  [JsonProperty( "verifier", NullValueHandling = NullValueHandling.Ignore )]
  public string? Verifier { get; set; }
}

internal sealed class TabEntry
{
  /// <summary>Name of a built-in code tab to insert here (e.g. "Emojis", "Tools").
  /// When set, all the data-tab fields below are ignored.</summary>
  [JsonProperty( "builtin", NullValueHandling = NullValueHandling.Ignore )]
  public string? Builtin { get; set; }

  [JsonProperty( "name", NullValueHandling = NullValueHandling.Ignore )]
  public string? Name { get; set; }

  /// <summary>Buttons per row (the tab's column count).</summary>
  [JsonProperty( "columns" )]
  public int Columns { get; set; }

  [JsonProperty( "fontSize" ), DefaultValue( 14.0 )]
  public double FontSize { get; set; } = 14.0;

  [JsonProperty( "fontName" ), DefaultValue( "Segoe UI" )]
  public string FontName { get; set; } = "Segoe UI";

  [JsonProperty( "buttonWidth" ), DefaultValue( 35 )]
  public int ButtonWidth { get; set; } = 35;

  [JsonProperty( "buttonHeight" ), DefaultValue( 35 )]
  public int ButtonHeight { get; set; } = 35;

  [JsonProperty( "gap" ), DefaultValue( 3 )]
  public int Gap { get; set; } = 3;

  [JsonProperty( "originX" ), DefaultValue( 15 )]
  public int OriginX { get; set; } = 15;

  [JsonProperty( "originY" ), DefaultValue( 35 )]
  public int OriginY { get; set; } = 35;

  [JsonProperty( "emojiImages" ), DefaultValue( false )]
  public bool EmojiImages { get; set; }

  [JsonProperty( "stripEmojis" ), DefaultValue( false )]
  public bool StripEmojis { get; set; }

  [JsonProperty( "rows", NullValueHandling = NullValueHandling.Ignore )]
  public List<RowDef>? Rows { get; set; }

  // ── Merge metadata (app-managed; keeps sharing across machines safe) ──
  [JsonProperty( "id", NullValueHandling = NullValueHandling.Ignore )]
  public string? Id { get; set; }

  /// <summary>Last-modified clock for this tab's own fields + layout.</summary>
  [JsonProperty( "mod", DefaultValueHandling = DefaultValueHandling.Ignore )]
  public long Mod { get; set; }
}

internal sealed class RowDef
{
  /// <summary>Extra vertical space before this row, in button-height units
  /// (0 = normal single-row spacing). Used for the small gaps between groups.</summary>
  [JsonProperty( "gapBefore" ), DefaultValue( 0.0 )]
  public double GapBefore { get; set; }

  /// <summary>Leading empty cells before the first button.</summary>
  [JsonProperty( "indent" ), DefaultValue( 0 )]
  public int Indent { get; set; }

  [JsonProperty( "buttons" )]
  public List<ButtonDef> Buttons { get; set; } = new();
}

internal sealed class ButtonDef
{
  /// <summary>The character / text the button sends (and shows, unless ShowDesc).</summary>
  [JsonProperty( "text" ), DefaultValue( "" )]
  public string Text { get; set; } = "";

  /// <summary>
  /// A sensitive value (password / private text) sent instead of <see cref="Text"/>.
  /// Stored encrypted at rest ("enc:..."); a plaintext value here is sealed on the
  /// next load. Secret buttons never display their value (face shows the desc).
  /// </summary>
  [JsonProperty( "secret", NullValueHandling = NullValueHandling.Ignore )]
  public string? Secret { get; set; }

  /// <summary>Runtime-only decrypted secret (never serialized).</summary>
  [JsonIgnore]
  public string? Plain { get; set; }

  /// <summary>True when this button carries a secret value.</summary>
  [JsonIgnore]
  public bool IsSecret => !string.IsNullOrEmpty( Secret );

  /// <summary>Tooltip / label. When omitted, the text is used.</summary>
  [JsonProperty( "desc", NullValueHandling = NullValueHandling.Ignore )]
  public string? Desc { get; set; }

  /// <summary>Global hotkey (AutoHotkey-style), e.g. "#!1". Omitted = none.</summary>
  [JsonProperty( "hotkey", NullValueHandling = NullValueHandling.Ignore )]
  public string? Hotkey { get; set; }

  [JsonProperty( "width" ), DefaultValue( 1 )]
  public int Width { get; set; } = 1;

  [JsonProperty( "align" ), DefaultValue( "center" )]
  public string Align { get; set; } = "center";

  /// <summary>Show the text on the button face. False = show the description
  /// instead (used by the wide text tabs and the hidden Sensitive entries).</summary>
  [JsonProperty( "showText" ), DefaultValue( true )]
  public bool ShowText { get; set; } = true;

  /// <summary>Include the sent text in the tooltip.</summary>
  [JsonProperty( "tipText" ), DefaultValue( false )]
  public bool TipText { get; set; }

  // ── Merge metadata (app-managed) ──
  [JsonProperty( "id", NullValueHandling = NullValueHandling.Ignore )]
  public string? Id { get; set; }

  /// <summary>Last-modified clock for this button's content.</summary>
  [JsonProperty( "mod", DefaultValueHandling = DefaultValueHandling.Ignore )]
  public long Mod { get; set; }
}
