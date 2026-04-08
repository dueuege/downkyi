using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseEpisodeListOrigin
{
    [JsonPropertyName("data")] public CheeseEpisodeListData? Data { get; set; }
}

public class CheeseEpisodeListData
{
    [JsonPropertyName("items")] public List<CheeseEpisode>? Items { get; set; }
    [JsonPropertyName("page")] public CheesePage? Page { get; set; }
}

public class CheesePage
{
    [JsonPropertyName("next")] public bool Next { get; set; }
    [JsonPropertyName("num")] public int Num { get; set; }
    [JsonPropertyName("size")] public int Size { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
}
