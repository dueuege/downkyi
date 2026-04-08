using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Favorites;

public class FavoritesMetaInfoOrigin
{
    [JsonPropertyName("data")] public FavoritesMetaInfo? Data { get; set; }
}

public class FavoritesListOrigin
{
    [JsonPropertyName("data")] public FavoritesListData? Data { get; set; }
}

public class FavoritesListData
{
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("list")] public List<FavoritesMetaInfo>? List { get; set; }
}

public class FavoritesMetaInfo
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("fid")] public long Fid { get; set; }
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("upper")] public FavUpper? Upper { get; set; }
    [JsonPropertyName("cnt_info")] public FavStatus? CntInfo { get; set; }
    [JsonPropertyName("intro")] public string Intro { get; set; } = string.Empty;
    [JsonPropertyName("media_count")] public int MediaCount { get; set; }
}
