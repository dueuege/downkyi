using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDash
{
    [JsonPropertyName("duration")]
    public long Duration { get; set; }
    [JsonPropertyName("video")]
    public List<PlayUrlDashVideo>? Video { get; set; }
    [JsonPropertyName("audio")]
    public List<PlayUrlDashVideo>? Audio { get; set; }
    [JsonPropertyName("dolby")]
    public PlayUrlDashDolby? Dolby { get; set; }
    [JsonPropertyName("flac")]
    public PlayUrlDashFlac? Flac { get; set; }
}
