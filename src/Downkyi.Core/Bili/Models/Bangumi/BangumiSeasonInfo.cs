using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiSeasonInfo
{
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("season_title")] public string SeasonTitle { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
}
