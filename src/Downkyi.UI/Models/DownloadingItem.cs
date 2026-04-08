using CommunityToolkit.Mvvm.ComponentModel;
using Downkyi.Core.Database.Download;

namespace Downkyi.UI.Models;

/// <summary>
/// Observable UI model representing one item in the download queue.
/// Backed by <see cref="DownloadingEntity"/> in the SQLite database.
/// </summary>
public partial class DownloadingItem : ObservableObject
{
    public string Uuid { get; set; } = string.Empty;
    public long Avid { get; set; }
    public string Bvid { get; set; } = string.Empty;
    public long Cid { get; set; }
    public long Epid { get; set; }
    public int ContentType { get; set; }   // 0=video, 1=bangumi, 2=cheese
    public int Quality { get; set; }
    public string CoverUrl { get; set; } = string.Empty;
    public bool DownloadVideo { get; set; } = true;
    public bool DownloadAudio { get; set; } = true;

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _upperName = string.Empty;
    [ObservableProperty] private DownloadStatus _status = DownloadStatus.NotStarted;
    [ObservableProperty] private float _progress;        // 0–100
    [ObservableProperty] private string _speed = string.Empty;
    [ObservableProperty] private string _filePath = string.Empty;

    /// <summary>Token source for cancelling this download.</summary>
    public CancellationTokenSource Cts { get; } = new CancellationTokenSource();

    public bool IsCompleted => Status == DownloadStatus.Succeed;
    public bool IsFailed => Status == DownloadStatus.Failed;
    public bool IsPaused => Status == DownloadStatus.Paused;
    public bool IsActive => Status == DownloadStatus.Downloading || Status == DownloadStatus.Waiting;
}
