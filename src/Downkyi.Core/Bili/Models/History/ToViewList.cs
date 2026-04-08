using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.History;

public class ToViewOrigin
{
    [JsonPropertyName("data")] public ToViewData? Data { get; set; }
}

public class ToViewData
{
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("list")] public List<ToViewList>? List { get; set; }
}

public class ToViewList
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("pic")] public string Pic { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("add_at")] public long AddAt { get; set; }
}
