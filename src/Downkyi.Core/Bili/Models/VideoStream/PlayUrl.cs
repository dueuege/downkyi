using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlOrigin
{
    [JsonPropertyName("data")]
    public PlayUrl? Data { get; set; }
    [JsonPropertyName("result")]
    public PlayUrl? Result { get; set; }
}

public class PlayUrl
{
    [JsonPropertyName("accept_description")]
    public List<string> AcceptDescription { get; set; } = new();
    [JsonPropertyName("accept_quality")]
    public List<int> AcceptQuality { get; set; } = new();
    [JsonPropertyName("durl")]
    public List<PlayUrlDurl>? Durl { get; set; }
    [JsonPropertyName("dash")]
    public PlayUrlDash? Dash { get; set; }
    [JsonPropertyName("support_formats")]
    public List<PlayUrlSupportFormat>? SupportFormats { get; set; }
}
