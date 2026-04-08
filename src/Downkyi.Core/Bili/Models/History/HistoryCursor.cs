using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.History;

public class HistoryCursor
{
    [JsonPropertyName("max")] public long Max { get; set; }
    [JsonPropertyName("view_at")] public long ViewAt { get; set; }
    [JsonPropertyName("business")] public string Business { get; set; } = string.Empty;
}
