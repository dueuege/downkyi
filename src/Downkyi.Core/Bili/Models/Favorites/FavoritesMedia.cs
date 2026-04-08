using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Favorites;

public class FavoritesMediaResourceOrigin
{
    [JsonPropertyName("data")] public FavoritesMediaResourceData? Data { get; set; }
}

public class FavoritesMediaResourceData
{
    [JsonPropertyName("medias")] public List<FavoritesMedia>? Medias { get; set; }
    [JsonPropertyName("has_more")] public bool HasMore { get; set; }
}

public class FavoritesMedia
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("duration")] public long Duration { get; set; }
    [JsonPropertyName("upper")] public FavUpper? Upper { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("bv_id")] public string BvId { get; set; } = string.Empty;
    [JsonPropertyName("fav_time")] public long FavTime { get; set; }
}
