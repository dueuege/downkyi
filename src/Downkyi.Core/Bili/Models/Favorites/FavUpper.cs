using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Favorites;

public class FavUpper
{
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("face")] public string Face { get; set; } = string.Empty;
}
