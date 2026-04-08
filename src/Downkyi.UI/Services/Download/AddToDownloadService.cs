using Downkyi.Core.Database.Download;
using Downkyi.Core.FileName;
using Downkyi.Core.Settings;
using Downkyi.Core.Utils;
using Downkyi.UI.Models;
using Downkyi.UI.ViewModels.DownloadManager;

namespace Downkyi.UI.Services.Download;

/// <summary>
/// Converts selected video pages from VideoDetailViewModel into DownloadingItem entries
/// and enqueues them in the DownloadingViewModel and DownloadDatabase.
/// The actual download engine (Slice 2) polls DownloadingViewModel and executes the downloads.
/// </summary>
public class AddToDownloadService
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    private readonly DownloadingViewModel _downloadingViewModel;
    private readonly DownloadFinishedViewModel _downloadFinishedViewModel;

    public AddToDownloadService(DownloadingViewModel downloadingViewModel,
        DownloadFinishedViewModel downloadFinishedViewModel)
    {
        _downloadingViewModel = downloadingViewModel;
        _downloadFinishedViewModel = downloadFinishedViewModel;
    }

    /// <summary>
    /// Enqueues selected pages as download jobs.
    /// </summary>
    /// <param name="videoInfoView">The video metadata (title, cover, uploader, etc.)</param>
    /// <param name="sections">All sections with their pages (IsSelected flags respected).</param>
    /// <param name="directory">The root download directory chosen by the user.</param>
    /// <param name="downloadAll">If true, ignore IsSelected flags and queue everything.</param>
    /// <returns>Number of items enqueued.</returns>
    public async Task<int> AddToDownloadAsync(
        VideoInfoView videoInfoView,
        IEnumerable<VideoSectionItem> sections,
        string directory,
        bool downloadAll = false)
    {
        if (string.IsNullOrEmpty(directory)) return 0;
        if (!Directory.Exists(directory))
        {
            try { Directory.CreateDirectory(directory); }
            catch (Exception e) { Log.Error(e, "Cannot create directory {Dir}", directory); return 0; }
        }

        var fileNameParts = SettingsManager.Instance.GetFileNameParts();
        var orderFormat = SettingsManager.Instance.GetOrderFormat();
        var videoContent = SettingsManager.Instance.GetVideoContent();

        int count = 0;
        var allSections = sections.ToList();

        foreach (var section in allSections)
        {
            foreach (var page in section.VideoPages)
            {
                if (!downloadAll && !page.IsSelected) continue;

                // Build the file path using the FileName template system
                string sectionName = allSections.Count > 1 ? section.Title : string.Empty;
                string zonePrefix = videoInfoView.VideoZone.Contains('>')
                    ? videoInfoView.VideoZone.Split('>')[0]
                    : videoInfoView.VideoZone;

                string relPath = FileName.Builder(fileNameParts)
                    .SetSection(Format.FormatFileName(sectionName))
                    .SetMainTitle(Format.FormatFileName(videoInfoView.Title))
                    .SetPageTitle(Format.FormatFileName(page.Title))
                    .SetVideoZone(zonePrefix)
                    .SetCid(page.Cid)
                    .SetOrder(page.Page, section.VideoPages.Count)
                    .RelativePath();

                string filePath = Path.Combine(directory, relPath);

                string uuid = Guid.NewGuid().ToString("N");

                var entity = new DownloadingEntity
                {
                    Uuid = uuid,
                    Cid = page.Cid,
                    Title = page.Title,
                    CoverUrl = videoInfoView.CoverUrl,
                    UpperName = videoInfoView.UpName,
                    Quality = SettingsManager.Instance.GetQuality(),
                    Status = DownloadStatus.Waiting,
                    FilePath = filePath,
                    DownloadAudio = videoContent.DownloadAudio,
                    DownloadVideo = videoContent.DownloadVideo,
                    DownloadDanmaku = videoContent.DownloadDanmaku,
                    DownloadSubtitle = videoContent.DownloadSubtitle,
                    DownloadCover = videoContent.DownloadCover,
                };

                await DownloadDatabase.Instance.InsertDownloadingAsync(entity);

                var item = new DownloadingItem
                {
                    Uuid = uuid,
                    Cid = page.Cid,
                    Title = page.Title,
                    CoverUrl = videoInfoView.CoverUrl,
                    UpperName = videoInfoView.UpName,
                    FilePath = filePath,
                    Status = DownloadStatus.Waiting,
                };

                _downloadingViewModel.AddItem(item);
                count++;
            }
        }

        Log.Info("AddToDownloadAsync: {Count} items enqueued to {Dir}", count, directory);
        return count;
    }
}
