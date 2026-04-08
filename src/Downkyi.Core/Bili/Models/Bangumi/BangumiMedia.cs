using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiMediaOrigin
{
    [JsonPropertyName("result")] public BangumiMediaResult? Result { get; set; }
}

public class BangumiMediaResult
{
    [JsonPropertyName("media")] public BangumiMedia? Media { get; set; }
}

public class BangumiMedia
{
    [JsonPropertyName("media_id")] public long MediaId { get; set; }
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("type_name")] public string TypeName { get; set; } = string.Empty;
}
