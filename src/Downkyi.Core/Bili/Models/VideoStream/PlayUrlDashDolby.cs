using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDashDolby
{
    [JsonPropertyName("audio")]
    public List<PlayUrlDashVideo>? Audio { get; set; }
}
