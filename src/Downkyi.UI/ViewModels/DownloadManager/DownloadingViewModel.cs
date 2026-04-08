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
