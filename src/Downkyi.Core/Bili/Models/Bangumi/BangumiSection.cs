using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiSection
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("episodes")] public List<BangumiEpisode>? Episodes { get; set; }
}
