using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.History;

public class HistoryListHistory
{
    [JsonPropertyName("oid")] public long Oid { get; set; }
    [JsonPropertyName("epid")] public long Epid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("business")] public string Business { get; set; } = string.Empty;
    [JsonPropertyName("dt")] public int Dt { get; set; }
}

public class HistoryList
{
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("uri")] public string Uri { get; set; } = string.Empty;
    [JsonPropertyName("history")] public HistoryListHistory? History { get; set; }
    [JsonPropertyName("author_name")] public string AuthorName { get; set; } = string.Empty;
    [JsonPropertyName("author_mid")] public long AuthorMid { get; set; }
    [JsonPropertyName("view_at")] public long ViewAt { get; set; }
    [JsonPropertyName("duration")] public long Duration { get; set; }
}
