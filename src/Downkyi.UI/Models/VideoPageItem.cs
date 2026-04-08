using CommunityToolkit.Mvvm.ComponentModel;

namespace Downkyi.UI.Models;

/// <summary>Observable UI wrapper for a single video page/episode in the episode list.</summary>
public partial class VideoPageItem : ObservableObject
{
    public long Cid { get; set; }
    public int Page { get; set; }
    public long Duration { get; set; }
    public long Avid { get; set; }
    public string Bvid { get; set; } = string.Empty;
    public long Epid { get; set; }

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private bool _isSelected;

    public string DurationText => FormatDuration(Duration);

    private static string FormatDuration(long seconds)
    {
        if (seconds <= 0) return "--:--";
        long h = seconds / 3600;
        long m = seconds % 3600 / 60;
        long s = seconds % 60;
        return h > 0
            ? $"{h:D2}:{m:D2}:{s:D2}"
            : $"{m:D2}:{s:D2}";
    }
}
