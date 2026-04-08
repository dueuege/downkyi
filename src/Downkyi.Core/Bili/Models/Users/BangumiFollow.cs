using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class BangumiFollowOrigin
{
    [JsonPropertyName("data")] public BangumiFollowData? Data { get; set; }
}

public class BangumiFollowData
{
    [JsonPropertyName("follow_list")] public List<BangumiFollow>? FollowList { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("has_next")] public bool HasNext { get; set; }
}

public class BangumiFollow
{
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("media_id")] public long MediaId { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("badge")] public string Badge { get; set; } = string.Empty;
    [JsonPropertyName("season_type_name")] public string SeasonTypeName { get; set; } = string.Empty;
    [JsonPropertyName("is_finish")] public int IsFinish { get; set; }
    [JsonPropertyName("total_count")] public int TotalCount { get; set; }
}
