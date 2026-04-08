using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDurl
{
    [JsonPropertyName("order")]
    public int Order { get; set; }
    [JsonPropertyName("length")]
    public long Length { get; set; }
    [JsonPropertyName("size")]
    public long Size { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    [JsonPropertyName("backup_url")]
    public List<string>? BackupUrl { get; set; }
}
