using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiStat
{
    [JsonPropertyName("coins")] public long Coins { get; set; }
    [JsonPropertyName("danmakus")] public long Danmakus { get; set; }
    [JsonPropertyName("favorites")] public long Favorites { get; set; }
    [JsonPropertyName("likes")] public long Likes { get; set; }
    [JsonPropertyName("views")] public long Views { get; set; }
    [JsonPropertyName("reply")] public long Reply { get; set; }
    [JsonPropertyName("share")] public long Share { get; set; }
}
