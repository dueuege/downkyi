using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Favorites;

public class FavStatus
{
    [JsonPropertyName("collect")] public int Collect { get; set; }
    [JsonPropertyName("play")] public int Play { get; set; }
    [JsonPropertyName("thumb_up")] public int ThumbUp { get; set; }
    [JsonPropertyName("share")] public int Share { get; set; }
}
