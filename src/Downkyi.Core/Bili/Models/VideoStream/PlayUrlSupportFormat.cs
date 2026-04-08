using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlSupportFormat
{
    [JsonPropertyName("quality")]
    public int Quality { get; set; }
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;
    [JsonPropertyName("new_description")]
    public string NewDescription { get; set; } = string.Empty;
    [JsonPropertyName("display_desc")]
    public string DisplayDesc { get; set; } = string.Empty;
    [JsonPropertyName("superscript")]
    public string Superscript { get; set; } = string.Empty;
}
