using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDashVideo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("base_url")]
    public string BaseUrl { get; set; } = string.Empty;
    [JsonPropertyName("backup_url")]
    public List<string>? BackupUrl { get; set; }
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;
    [JsonPropertyName("codecs")]
    public string Codecs { get; set; } = string.Empty;
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
    [JsonPropertyName("frameRate")]
    public string FrameRate { get; set; } = string.Empty;
    [JsonPropertyName("codecid")]
    public int CodecId { get; set; }
}
