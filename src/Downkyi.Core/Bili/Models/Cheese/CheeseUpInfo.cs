using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseUpInfo
{
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("uname")] public string Uname { get; set; } = string.Empty;
    [JsonPropertyName("avatar")] public string Avatar { get; set; } = string.Empty;
}
