using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseStat
{
    [JsonPropertyName("play")] public long Play { get; set; }
}
