namespace Downkyi.Core.Bili.Models.Danmaku;

/// <summary>
/// A single danmaku item parsed from Bilibili XML danmaku format.
/// Format of the 'p' attribute: time,type,size,color,timestamp,pool,userHash,dmid
/// </summary>
public class DanmakuItem
{
    public float Time { get; set; }        // seconds from start
    public int Type { get; set; }          // 1=rolling, 4=bottom, 5=top, 6=reverse, 7=advanced
    public int Size { get; set; }          // font size (default 25)
    public int Color { get; set; }         // decimal color (e.g. 16777215 = white)
    public long Timestamp { get; set; }    // unix timestamp when sent
    public int Pool { get; set; }          // 0=normal pool
    public string UserHash { get; set; } = string.Empty;
    public string DmId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
