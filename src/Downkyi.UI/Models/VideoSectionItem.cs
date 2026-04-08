using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Downkyi.UI.Models;

/// <summary>Observable UI wrapper for a video section (e.g. "正片", "PV", UGC series section).</summary>
public partial class VideoSectionItem : ObservableObject
{
    public long Id { get; set; }

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private ObservableCollection<VideoPageItem> _videoPages = new();
}
