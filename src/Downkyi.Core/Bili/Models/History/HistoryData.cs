using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.History;

public class HistoryOrigin
{
    [JsonPropertyName("data")] public HistoryData? Data { get; set; }
}

public class HistoryData
{
    [JsonPropertyName("cursor")] public HistoryCursor? Cursor { get; set; }
    [JsonPropertyName("list")] public List<HistoryList>? List { get; set; }
}
