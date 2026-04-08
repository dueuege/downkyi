using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downkyi.Core.Database.Download;
using Downkyi.UI.Models;
using Downkyi.UI.Mvvm;

namespace Downkyi.UI.ViewModels.DownloadManager;

public partial class DownloadFinishedViewModel : ViewModelBase
{
    public const string Key = "DownloadManager_DownloadFinished";

    [ObservableProperty]
    private ObservableCollection<DownloadedItem> _downloadedList = new();

    public DownloadFinishedViewModel(BaseServices baseServices) : base(baseServices)
    {
    }

    /// <summary>Loads completed downloads from the database.</summary>
    public async Task LoadAsync()
    {
        var entities = await DownloadDatabase.Instance.GetAllDownloadedAsync();
        DownloadedList.Clear();
        foreach (var e in entities)
        {
            var finishedTime = DateTimeOffset.FromUnixTimeSeconds(e.FinishedAt)
                .ToLocalTime()
                .ToString("yyyy-MM-dd HH:mm:ss");
            DownloadedList.Add(new DownloadedItem
            {
                Id = e.Id,
                Bvid = e.Bvid,
                CoverUrl = e.CoverUrl,
                Title = e.Title,
                UpperName = e.UpperName,
                FilePath = e.FilePath,
                FinishedTime = finishedTime,
            });
        }
    }

    /// <summary>Adds a newly completed item to the top of the list.</summary>
    public void AddItem(DownloadedItem item)
    {
        DownloadedList.Insert(0, item);
    }

    [RelayCommand]
    private void OpenFile(DownloadedItem item)
    {
        if (string.IsNullOrEmpty(item.FilePath) || !File.Exists(item.FilePath)) return;
        try
        {
            Process.Start(new ProcessStartInfo(item.FilePath) { UseShellExecute = true });
        }
        catch { /* ignore */ }
    }

    [RelayCommand]
    private void OpenFolder(DownloadedItem item)
    {
        string? dir = Path.GetDirectoryName(item.FilePath);
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
        try
        {
            if (OperatingSystem.IsWindows())
                Process.Start("explorer.exe", $"/select,\"{item.FilePath}\"");
            else
                Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
        }
        catch { /* ignore */ }
    }

    [RelayCommand]
    private async Task DeleteItemAsync(DownloadedItem item)
    {
        DownloadedList.Remove(item);
        await DownloadDatabase.Instance.DeleteDownloadedAsync(item.Id);
    }

    [RelayCommand]
    private async Task ClearAllAsync()
    {
        // Only clear UI list; keep files on disk
        var ids = DownloadedList.Select(x => x.Id).ToList();
        DownloadedList.Clear();
        foreach (var id in ids)
        {
            await DownloadDatabase.Instance.DeleteDownloadedAsync(id);
        }
    }
}
