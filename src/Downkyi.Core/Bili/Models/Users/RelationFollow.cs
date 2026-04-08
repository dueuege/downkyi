using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class RelationFollowOrigin
{
    [JsonPropertyName("data")] public RelationFollow? Data { get; set; }
}

public class RelationFollow
{
    [JsonPropertyName("list")] public List<RelationFollowInfo>? List { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
}

public class RelationFollowInfo
{
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("uname")] public string Uname { get; set; } = string.Empty;
    [JsonPropertyName("face")] public string Face { get; set; } = string.Empty;
    [JsonPropertyName("sign")] public string Sign { get; set; } = string.Empty;
}
