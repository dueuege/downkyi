using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class SpaceChannelListOrigin
{
    [JsonPropertyName("data")] public SpaceChannelListData? Data { get; set; }
}

public class SpaceChannelListData
{
    [JsonPropertyName("list")] public List<SpaceChannel>? List { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
}

public class SpaceChannel
{
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("intro")] public string Intro { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
}

public class SpaceChannelVideoOrigin
{
    [JsonPropertyName("data")] public SpaceChannelVideoData? Data { get; set; }
}

public class SpaceChannelVideoData
{
    [JsonPropertyName("list")] public SpaceChannelVideoList? List { get; set; }
    [JsonPropertyName("page")] public SpaceChannelVideoPage? Page { get; set; }
}

public class SpaceChannelVideoList
{
    [JsonPropertyName("archives")] public List<SpaceChannelArchive>? Archives { get; set; }
}

public class SpaceChannelVideoPage
{
    [JsonPropertyName("page_num")] public int PageNum { get; set; }
    [JsonPropertyName("page_size")] public int PageSize { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
}

public class SpaceChannelArchive
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("pic")] public string Pic { get; set; } = string.Empty;
    [JsonPropertyName("duration")] public long Duration { get; set; }
    [JsonPropertyName("pubdate")] public long Pubdate { get; set; }
}
