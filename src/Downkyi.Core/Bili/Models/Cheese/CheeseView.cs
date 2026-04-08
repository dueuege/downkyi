using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseViewOrigin
{
    [JsonPropertyName("data")] public CheeseView? Data { get; set; }
}

public class CheeseView
{
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("episodes")] public List<CheeseEpisode>? Episodes { get; set; }
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("stat")] public CheeseStat? Stat { get; set; }
    [JsonPropertyName("subtitle")] public string Subtitle { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("up_info")] public CheeseUpInfo? UpInfo { get; set; }
}
