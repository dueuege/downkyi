using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiSeasonOrigin
{
    [JsonPropertyName("result")] public BangumiSeason? Result { get; set; }
}

public class BangumiSeason
{
    [JsonPropertyName("areas")] public List<BangumiArea>? Areas { get; set; }
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("evaluate")] public string Evaluate { get; set; } = string.Empty;
    [JsonPropertyName("episodes")] public List<BangumiEpisode>? Episodes { get; set; }
    [JsonPropertyName("media_id")] public long MediaId { get; set; }
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("season_title")] public string SeasonTitle { get; set; } = string.Empty;
    [JsonPropertyName("seasons")] public List<BangumiSeasonInfo>? Seasons { get; set; }
    [JsonPropertyName("section")] public List<BangumiSection>? Section { get; set; }
    [JsonPropertyName("stat")] public BangumiStat? Stat { get; set; }
    [JsonPropertyName("subtitle")] public string Subtitle { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("up_info")] public BangumiUpInfo? UpInfo { get; set; }
}
