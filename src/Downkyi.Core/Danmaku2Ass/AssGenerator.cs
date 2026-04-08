using Downkyi.Core.Bili.Models.Danmaku;

namespace Downkyi.Core.Danmaku2Ass;

/// <summary>
/// Converts a list of DanmakuItem into an ASS subtitle file string.
/// </summary>
public static class AssGenerator
{
    public static string Generate(List<DanmakuItem> items, AssConfig config)
    {
        var entries = items
            .Where(d => !ShouldBlock(d, config))
            .Select(d => DanmakuEntry.FromDanmakuItem(d, config.FontSize))
            .OrderBy(d => d.Time)
            .ToList();

        if (config.ReduceCount > 0 && entries.Count > config.ReduceCount)
        {
            entries = ReduceDensity(entries, config.ReduceCount);
        }

        var scrollCollision = new CollisionManager(config.VideoHeight, config.FontSize, config.BottomMargin);
        var topCollision = new CollisionManager(config.VideoHeight, config.FontSize, config.BottomMargin);
        var bottomCollision = new CollisionManager(config.VideoHeight, config.FontSize, config.BottomMargin);

        var sb = new System.Text.StringBuilder();
        WriteHeader(sb, config);

        foreach (var entry in entries)
        {
            string assLine = entry.Type switch
            {
                1 => GenerateScrollLine(entry, config, scrollCollision),
                4 => GenerateFixedLine(entry, config, bottomCollision, isBottom: true),
                5 => GenerateFixedLine(entry, config, topCollision, isBottom: false),
                _ => GenerateScrollLine(entry, config, scrollCollision), // treat unknown as scroll
            };
            sb.AppendLine(assLine);
        }

        return sb.ToString();
    }

    private static bool ShouldBlock(DanmakuItem d, AssConfig cfg)
    {
        if (cfg.BlockScrolling && d.Type == 1) return true;
        if (cfg.BlockTop && d.Type == 5) return true;
        if (cfg.BlockBottom && d.Type == 4) return true;
        if (cfg.BlockColorful && d.Color != 16777215) return true;
        return false;
    }

    private static List<DanmakuEntry> ReduceDensity(List<DanmakuEntry> entries, int target)
    {
        int step = entries.Count / target;
        return entries.Where((_, i) => i % step == 0).Take(target).ToList();
    }

    private static void WriteHeader(System.Text.StringBuilder sb, AssConfig cfg)
    {
        int alpha = (int)((1 - cfg.Opacity) * 255);
        string alphaHex = alpha.ToString("X2");

        sb.AppendLine("[Script Info]");
        sb.AppendLine("ScriptType: v4.00+");
        sb.AppendLine("Collisions: Normal");
        sb.AppendLine($"PlayResX: {cfg.VideoWidth}");
        sb.AppendLine($"PlayResY: {cfg.VideoHeight}");
        sb.AppendLine();
        sb.AppendLine("[V4+ Styles]");
        sb.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");
        sb.AppendLine($"Style: Default,{cfg.FontFamily},{(int)cfg.FontSize},&H{alphaHex}FFFFFF,&H{alphaHex}FFFFFF,&H{alphaHex}000000,&H{alphaHex}000000,0,0,0,0,100,100,0,0,1,1,0,2,0,0,0,0");
        sb.AppendLine();
        sb.AppendLine("[Events]");
        sb.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
    }

    private static string GenerateScrollLine(DanmakuEntry entry, AssConfig cfg, CollisionManager collision)
    {
        float duration = cfg.ScrollDuration;
        // Time for the text to fully clear (be off-screen): when the tail exits right edge
        float clearTime = entry.Time + duration + (entry.TextWidth / cfg.VideoWidth) * duration;
        int row = collision.AssignRow(entry, clearTime);
        if (row < 0) row = 0; // fallback: show on row 0

        float y = collision.RowY(row);
        string start = FormatTime(entry.Time);
        string end = FormatTime(entry.Time + duration);
        string color = ColorToAss(entry.Color);
        // Scroll: move from right edge (x=PlayResX) to left (-textWidth)
        string move = $"\\move({cfg.VideoWidth},{y},{-entry.TextWidth},{y})";
        string text = EscapeAss(entry.Content);
        return $"Dialogue: 0,{start},{end},Default,,0,0,0,,{{{move}{color}}}{text}";
    }

    private static string GenerateFixedLine(DanmakuEntry entry, AssConfig cfg, CollisionManager collision, bool isBottom)
    {
        float duration = cfg.FixedDuration;
        int row = collision.AssignRow(entry, entry.Time + duration);
        if (row < 0) row = 0;

        float y = isBottom
            ? cfg.VideoHeight - collision.RowY(row)
            : collision.RowY(row);
        int an = isBottom ? 2 : 8; // ASS alignment: 2=bottom-center, 8=top-center
        string start = FormatTime(entry.Time);
        string end = FormatTime(entry.Time + duration);
        string color = ColorToAss(entry.Color);
        string pos = $"\\an{an}\\pos({cfg.VideoWidth / 2},{y})";
        string text = EscapeAss(entry.Content);
        return $"Dialogue: 0,{start},{end},Default,,0,0,0,,{{{pos}{color}}}{text}";
    }

    private static string FormatTime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)(seconds % 3600 / 60);
        float s = seconds % 60;
        return $"{h}:{m:D2}:{s:00.00}";
    }

    private static string ColorToAss(int color)
    {
        if (color == 16777215) return string.Empty; // white = default, no override needed
        // ASS color is &HBBGGRR
        int r = (color >> 16) & 0xFF;
        int g = (color >> 8) & 0xFF;
        int b = color & 0xFF;
        return $"\\c&H{b:X2}{g:X2}{r:X2}&";
    }

    private static string EscapeAss(string text)
        => text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Replace("\n", "\\N");
}
