using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class SubtitleTrack
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("lan")]
    public string Lan { get; set; } = string.Empty;
    [JsonPropertyName("lan_doc")]
    public string LanDoc { get; set; } = string.Empty;
    [JsonPropertyName("is_lock")]
    public bool IsLock { get; set; }
    [JsonPropertyName("subtitle_url")]
    public string SubtitleUrl { get; set; } = string.Empty;
    [JsonPropertyName("id_str")]
    public string IdStr { get; set; } = string.Empty;
}

public class SubtitleInfo
{
    [JsonPropertyName("allow_submit")]
    public bool AllowSubmit { get; set; }
    [JsonPropertyName("subtitles")]
    public List<SubtitleTrack>? Subtitles { get; set; }
}
