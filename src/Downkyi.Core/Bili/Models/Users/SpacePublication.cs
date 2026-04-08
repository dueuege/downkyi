using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class SpacePublicationOrigin
{
    [JsonPropertyName("data")] public SpacePublicationData? Data { get; set; }
}

public class SpacePublicationData
{
    [JsonPropertyName("list")] public SpacePublicationList? List { get; set; }
    [JsonPropertyName("page")] public SpacePublicationPage? Page { get; set; }
}

public class SpacePublicationPage
{
    [JsonPropertyName("pn")] public int Pn { get; set; }
    [JsonPropertyName("ps")] public int Ps { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
}

public class SpacePublicationList
{
    [JsonPropertyName("vlist")] public List<SpacePublicationVideo>? Vlist { get; set; }
}

public class SpacePublicationVideo
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("pic")] public string Pic { get; set; } = string.Empty;
    [JsonPropertyName("length")] public string Length { get; set; } = string.Empty;
    [JsonPropertyName("created")] public long Created { get; set; }
    [JsonPropertyName("play")] public long Play { get; set; }
    [JsonPropertyName("author")] public string Author { get; set; } = string.Empty;
    [JsonPropertyName("mid")] public long Mid { get; set; }
}
