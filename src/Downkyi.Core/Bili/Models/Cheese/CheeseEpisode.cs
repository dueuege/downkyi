using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseEpisode
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("duration")] public long Duration { get; set; }
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("release_date")] public long ReleaseDate { get; set; }
    [JsonPropertyName("status")] public int Status { get; set; }
}
