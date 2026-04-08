namespace Downkyi.Core.Danmaku2Ass;

public class AssConfig
{
    public int VideoWidth { get; set; } = 1280;
    public int VideoHeight { get; set; } = 720;
    public float FontSize { get; set; } = 25f;
    public string FontFamily { get; set; } = "黑体";
    public float Opacity { get; set; } = 0.7f;       // 0.0 (transparent) to 1.0 (opaque)
    public int ScrollDuration { get; set; } = 8;      // seconds for rolling danmaku
    public int FixedDuration { get; set; } = 4;       // seconds for top/bottom danmaku
    public bool BlockScrolling { get; set; } = false;
    public bool BlockTop { get; set; } = false;
    public bool BlockBottom { get; set; } = false;
    public bool BlockColorful { get; set; } = false;  // block non-white danmaku
    public int ReduceCount { get; set; } = 0;         // 0 = no reduction
    /// <summary>Vertical margin as fraction of video height reserved at bottom.</summary>
    public float BottomMargin { get; set; } = 0.02f;
}
