using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiEpisode
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("duration")] public long Duration { get; set; }
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("long_title")] public string LongTitle { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("badge")] public string Badge { get; set; } = string.Empty;
    [JsonPropertyName("pub_time")] public long PubTime { get; set; }
    [JsonPropertyName("status")] public int Status { get; set; }
}
