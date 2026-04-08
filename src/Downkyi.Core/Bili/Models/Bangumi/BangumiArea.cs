using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiArea
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
