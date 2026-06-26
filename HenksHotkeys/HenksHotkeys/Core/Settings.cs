namespace HenksHotkeys.Core;

/// <summary>
/// Serializable settings model (window state + toggles), persisted as JSON in
/// %LocalAppData%\HenksHotkeys\settings.json. Nullable position/size fields are
/// "unset" until the window has been placed or resized at least once.
/// </summary>
internal sealed class Settings
{
  public bool Collapsed          { get; set; }
  public bool ClipSendMode       { get; set; }
  public bool StripCommentEmojis { get; set; }

  public int? Height { get; set; }
  public int? X      { get; set; }
  public int? Y      { get; set; }
  public int? FavX   { get; set; }
  public int? FavY   { get; set; }

  public int LastTab { get; set; } = 1;
}
