using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downkyi.Core.Database.Download;
using Downkyi.UI.Models;
using Downkyi.UI.Mvvm;

namespace Downkyi.UI.ViewModels.DownloadManager;

public partial class DownloadingViewModel : ViewModelBase
{
    public const string Key = "DownloadManager_Downloading";

    [ObservableProperty]
    private ObservableCollection<DownloadingItem> _downloadingList = new();

    public DownloadingViewModel(BaseServices baseServices) : base(baseServices)
    {
    }

    /// <summary>Loads persisted downloading items from DB on app start (S2-06).</summary>
    public async Task LoadAsync()
    {
        var entities = await DownloadDatabase.Instance.GetAllDownloadingAsync();
        DownloadingList.Clear();
        foreach (var e in entities)
        {
            // Items that were actively downloading when the app closed are reset to Waiting
            var status = e.Status == DownloadStatus.Downloading ? DownloadStatus.Waiting : e.Status;
            DownloadingList.Add(new DownloadingItem
            {
                Uuid = e.Uuid,
                Avid = e.Avid,
                Bvid = e.Bvid,
                Cid = e.Cid,
                Epid = e.Epid,
                ContentType = e.ContentType,
                Quality = e.Quality,
                Title = e.Title,
                CoverUrl = e.CoverUrl,
                UpperName = e.UpperName,
                FilePath = e.FilePath,
                Status = status,
                DownloadVideo = e.DownloadVideo,
                DownloadAudio = e.DownloadAudio,
            });
        }
    }

    /// <summary>Adds an item to the observable download list (called by AddToDownloadService).</summary>
    public void AddItem(DownloadingItem item)
    {
        DownloadingList.Add(item);
    }

    /// <summary>Removes a completed/cancelled item from the list.</summary>
    public void RemoveItem(string uuid)
    {
        var item = DownloadingList.FirstOrDefault(x => x.Uuid == uuid);
        if (item != null) DownloadingList.Remove(item);
    }

    [RelayCommand]
    private async Task PauseAllAsync()
    {
        foreach (var item in DownloadingList.Where(x => x.IsActive))
        {
            item.Status = DownloadStatus.Paused;
            item.Cts.Cancel();
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ResumeAllAsync()
    {
        // TODO: resume each paused item's download task
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ClearAllAsync()
    {
        var toCancel = DownloadingList.ToList();
        foreach (var item in toCancel)
        {
            item.Cts.Cancel();
        }
        DownloadingList.Clear();
        // Remove all from DB
        var db = DownloadDatabase.Instance;
        foreach (var item in toCancel)
        {
            await db.DeleteDownloadingAsync(item.Uuid);
        }
    }

    [RelayCommand]
    private async Task CancelItemAsync(DownloadingItem item)
    {
        item.Status = DownloadStatus.Cancelled;
        item.Cts.Cancel();
        DownloadingList.Remove(item);
        await DownloadDatabase.Instance.DeleteDownloadingAsync(item.Uuid);
    }
}
