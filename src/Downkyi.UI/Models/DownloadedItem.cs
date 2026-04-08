using CommunityToolkit.Mvvm.ComponentModel;

namespace Downkyi.UI.Models;

/// <summary>Observable UI model for a completed download in the history list.</summary>
public partial class DownloadedItem : ObservableObject
{
    public long Id { get; set; }
    public string Bvid { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _upperName = string.Empty;
    [ObservableProperty] private string _filePath = string.Empty;
    [ObservableProperty] private string _finishedTime = string.Empty;
}
