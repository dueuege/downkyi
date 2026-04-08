using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class SpaceSeasonsSeriesOrigin
{
    [JsonPropertyName("data")] public SpaceSeasonsSeriesData? Data { get; set; }
}

public class SpaceSeasonsSeriesData
{
    [JsonPropertyName("items_lists")] public SpaceSeasonsSeriesItems? ItemsLists { get; set; }
}

public class SpaceSeasonsSeriesItems
{
    [JsonPropertyName("seasons_list")] public List<SpaceSeasonItem>? SeasonsList { get; set; }
    [JsonPropertyName("series_list")] public List<SpaceSeriesItem>? SeriesList { get; set; }
    [JsonPropertyName("page")] public SpaceSeasonsSeriesPage? Page { get; set; }
}

public class SpaceSeasonsSeriesPage
{
    [JsonPropertyName("page_num")] public int PageNum { get; set; }
    [JsonPropertyName("page_size")] public int PageSize { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
}

public class SpaceSeasonItem
{
    [JsonPropertyName("meta")] public SpaceSeasonMeta? Meta { get; set; }
}

public class SpaceSeasonMeta
{
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("total")] public int Total { get; set; }
}

public class SpaceSeriesItem
{
    [JsonPropertyName("meta")] public SpaceSeriesMeta? Meta { get; set; }
}

public class SpaceSeriesMeta
{
    [JsonPropertyName("series_id")] public long SeriesId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("total")] public int Total { get; set; }
}
