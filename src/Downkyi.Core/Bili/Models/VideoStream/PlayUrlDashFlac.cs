using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDashFlac
{
    [JsonPropertyName("audio")]
    public PlayUrlDashVideo? Audio { get; set; }
}
