using Downkyi.Core.Bili.Models.Danmaku;

namespace Downkyi.Core.Danmaku2Ass;

/// <summary>Internal representation of a danmaku with computed layout properties.</summary>
internal class DanmakuEntry
{
    public float Time { get; set; }
    public int Type { get; set; }       // 1=scroll, 4=bottom, 5=top
    public float FontSize { get; set; }
    public int Color { get; set; }
    public string Content { get; set; } = string.Empty;
    // Layout (computed)
    public float TextWidth { get; set; }
    public int Row { get; set; } = -1;  // -1 = unassigned

    internal static DanmakuEntry FromDanmakuItem(DanmakuItem item, float fontSize)
    {
        return new DanmakuEntry
        {
            Time = item.Time,
            Type = item.Type,
            FontSize = fontSize,
            Color = item.Color,
            Content = item.Content,
            TextWidth = EstimateTextWidth(item.Content, fontSize),
        };
    }

    /// <summary>
    /// Rough text width estimate: each CJK char ≈ fontSize px wide, ASCII ≈ fontSize*0.6.
    /// Good enough for collision detection without a real text renderer.
    /// </summary>
    private static float EstimateTextWidth(string text, float fontSize)
    {
        float w = 0;
        foreach (char c in text)
        {
            w += c > 0x2E7F ? fontSize : fontSize * 0.6f;
        }
        return w;
    }
}
