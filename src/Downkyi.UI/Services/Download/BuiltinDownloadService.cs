using Downkyi.Core.Bili.Models.VideoStream;
using Downkyi.Core.Bili.Web;
using Downkyi.Core.Database.Download;
using Downkyi.Core.Downloader;
using Downkyi.Core.FFmpeg;
using Downkyi.Core.Settings;
using Downkyi.Core.Settings.Enum;
using Downkyi.UI.Models;
using Downkyi.UI.ViewModels.DownloadManager;

namespace Downkyi.UI.Services.Download;

/// <summary>
/// S2-02/S2-03: Background download engine.
/// Polls DownloadingViewModel for Waiting items, downloads DASH streams,
/// merges with FFmpeg, and moves completed items to DownloadFinishedViewModel.
/// </summary>
public class BuiltinDownloadService
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    // Bilibili CDN stream download requires these headers
    private const string Referer = "https://www.bilibili.com";

    private readonly DownloadingViewModel _downloadingVm;
    private readonly DownloadFinishedViewModel _downloadFinishedVm;

    // Captured on construction (must be created on UI thread) for cross-thread UI dispatch
    private readonly SynchronizationContext? _uiContext;

    private CancellationTokenSource _cts = new();
    private bool _running;

    public BuiltinDownloadService(
        DownloadingViewModel downloadingVm,
        DownloadFinishedViewModel downloadFinishedVm)
    {
        _downloadingVm = downloadingVm;
        _downloadFinishedVm = downloadFinishedVm;
        _uiContext = SynchronizationContext.Current;
    }

    /// <summary>Starts the background download loop.</summary>
    public void Start()
    {
        if (_running) return;
        _running = true;
        _cts = new CancellationTokenSource();
        Task.Run(() => ProcessLoopAsync(_cts.Token));
        Log.Info("BuiltinDownloadService started");
    }

    /// <summary>Signals the loop to stop gracefully.</summary>
    public void Stop()
    {
        _cts.Cancel();
        _running = false;
    }

    // ─── Main loop ────────────────────────────────────────────────────────────

    private async Task ProcessLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            DownloadingItem? item = null;
            RunOnUiThread(() =>
                item = _downloadingVm.DownloadingList.FirstOrDefault(x => x.Status == DownloadStatus.Waiting));

            if (item != null)
            {
                await ProcessItemAsync(item);
            }
            else
            {
                try { await Task.Delay(1000, ct); }
                catch (OperationCanceledException) { break; }
            }
        }
        Log.Info("BuiltinDownloadService loop exited");
    }

    // ─── Per-item processing ──────────────────────────────────────────────────

    private async Task ProcessItemAsync(DownloadingItem item)
    {
        SetStatus(item, DownloadStatus.Downloading);
        await UpdateDbStatusAsync(item.Uuid, DownloadStatus.Downloading);

        try
        {
            // 1. Fetch play URL
            var playUrl = await FetchPlayUrlAsync(item);
            if (playUrl?.Dash == null)
            {
                Log.Warn("No DASH play URL for cid={Cid}", item.Cid);
                SetStatus(item, DownloadStatus.Failed);
                await UpdateDbStatusAsync(item.Uuid, DownloadStatus.Failed);
                return;
            }

            // 2. Select streams
            var videoStream = item.DownloadVideo ? SelectVideoStream(playUrl, item.Quality) : null;
            var audioStream = item.DownloadAudio ? SelectAudioStream(playUrl) : null;

            string tempDir = Path.GetTempPath();
            string videoTemp = Path.Combine(tempDir, item.Uuid + "_video.m4s");
            string audioTemp = Path.Combine(tempDir, item.Uuid + "_audio.m4s");

            var headers = await BuildHeadersAsync();

            // 3. Download video stream (0–50%)
            if (videoStream != null)
            {
                var prog = new Progress<(long r, long t)>(p =>
                {
                    if (p.t > 0) SetProgress(item, p.r * 50f / p.t);
                });
                await DownloadWithFallbackAsync(videoStream, videoTemp, headers, prog, item.Cts.Token);
            }

            // 4. Download audio stream (50–100%)
            if (audioStream != null)
            {
                var prog = new Progress<(long r, long t)>(p =>
                {
                    if (p.t > 0) SetProgress(item, 50f + p.r * 50f / p.t);
                });
                await DownloadWithFallbackAsync(audioStream, audioTemp, headers, prog, item.Cts.Token);
            }

            // 5. Merge with FFmpeg
            SetProgress(item, 100f);
            string finalPath = item.FilePath + ".mp4";

            string? dir = Path.GetDirectoryName(finalPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            bool merged = FFmpegHelper.MergeVideo(
                File.Exists(audioTemp) ? audioTemp : null!,
                File.Exists(videoTemp) ? videoTemp : null!,
                finalPath);

            if (!merged)
            {
                Log.Error("FFmpeg merge failed for {FilePath}", finalPath);
                SetStatus(item, DownloadStatus.Failed);
                await UpdateDbStatusAsync(item.Uuid, DownloadStatus.Failed);
                return;
            }

            // Clean up temp files
            TryDelete(videoTemp);
            TryDelete(audioTemp);

            // 6. Mark succeeded; move to downloaded list
            RunOnUiThread(() => item.FilePath = finalPath);
            SetStatus(item, DownloadStatus.Succeed);

            var finishedEntity = new DownloadedEntity
            {
                Uuid = item.Uuid,
                Bvid = item.Bvid,
                Title = item.Title,
                CoverUrl = item.CoverUrl,
                UpperName = item.UpperName,
                FilePath = finalPath,
            };
            await DownloadDatabase.Instance.InsertDownloadedAsync(finishedEntity);
            await DownloadDatabase.Instance.DeleteDownloadingAsync(item.Uuid);

            var finishedItem = new DownloadedItem
            {
                Bvid = item.Bvid,
                CoverUrl = item.CoverUrl,
                Title = item.Title,
                UpperName = item.UpperName,
                FilePath = finalPath,
                FinishedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };

            RunOnUiThread(() =>
            {
                _downloadingVm.RemoveItem(item.Uuid);
                _downloadFinishedVm.AddItem(finishedItem);
            });

            Log.Info("Download complete: {Title} -> {FilePath}", item.Title, finalPath);
        }
        catch (OperationCanceledException)
        {
            SetStatus(item, DownloadStatus.Cancelled);
            await UpdateDbStatusAsync(item.Uuid, DownloadStatus.Cancelled);
            TryDelete(Path.Combine(Path.GetTempPath(), item.Uuid + "_video.m4s"));
            TryDelete(Path.Combine(Path.GetTempPath(), item.Uuid + "_audio.m4s"));
            Log.Info("Download cancelled: {Title}", item.Title);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Download failed for cid={Cid} title={Title}", item.Cid, item.Title);
            SetStatus(item, DownloadStatus.Failed);
            await UpdateDbStatusAsync(item.Uuid, DownloadStatus.Failed);
        }
    }

    // ─── Play URL ─────────────────────────────────────────────────────────────

    private static async Task<PlayUrl?> FetchPlayUrlAsync(DownloadingItem item)
    {
        return item.ContentType switch
        {
            1 => await VideoStreamApi.GetBangumiPlayUrlAsync(item.Avid, item.Bvid, item.Cid, item.Quality),
            2 => await VideoStreamApi.GetCheesePlayUrlAsync(item.Avid, item.Bvid, item.Cid, item.Epid, item.Quality),
            _ => await VideoStreamApi.GetVideoPlayUrlAsync(item.Avid, item.Bvid, item.Cid, item.Quality),
        };
    }

    // ─── Stream selection ─────────────────────────────────────────────────────

    private static PlayUrlDashVideo? SelectVideoStream(PlayUrl playUrl, int requestedQuality)
    {
        var videos = playUrl.Dash?.Video;
        if (videos == null || videos.Count == 0) return null;

        int codecPref = SettingsManager.Instance.GetVideoCodecs() == (int)VideoCodecs.HEVC ? 12 : 13;

        // Pick best quality at or below requested, then prefer AVC/HEVC per settings
        var candidates = videos
            .Where(v => v.Id <= requestedQuality)
            .OrderByDescending(v => v.Id)
            .ToList();

        if (candidates.Count == 0)
            candidates = videos.OrderByDescending(v => v.Id).ToList();

        int bestQuality = candidates[0].Id;
        var atBestQuality = candidates.Where(v => v.Id == bestQuality).ToList();

        return atBestQuality.FirstOrDefault(v => v.CodecId == codecPref)
               ?? atBestQuality.First();
    }

    private static PlayUrlDashVideo? SelectAudioStream(PlayUrl playUrl)
    {
        var audios = playUrl.Dash?.Audio;
        if (audios == null || audios.Count == 0) return null;

        int maxAudio = SettingsManager.Instance.GetAudioQuality();

        // Prefer highest standard bitrate at or below setting (exclude Dolby/FLAC)
        return audios
            .Where(a => a.Id <= maxAudio)
            .OrderByDescending(a => a.Id)
            .FirstOrDefault()
               ?? audios.OrderByDescending(a => a.Id).First();
    }

    // ─── Download helpers ─────────────────────────────────────────────────────

    private static async Task DownloadWithFallbackAsync(
        PlayUrlDashVideo stream,
        string filePath,
        Dictionary<string, string> headers,
        IProgress<(long, long)> progress,
        CancellationToken ct)
    {
        var urls = new List<string> { stream.BaseUrl };
        if (stream.BackupUrl != null) urls.AddRange(stream.BackupUrl);

        foreach (var url in urls)
        {
            try
            {
                await FileDownloader.DownloadAsync(url, filePath, headers, progress, ct);
                return;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Log.Warn(ex, "Stream download failed for URL, trying backup: {Url}", url);
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
        throw new IOException($"All stream URLs failed for {Path.GetFileName(filePath)}");
    }

    private static async Task<Dictionary<string, string>> BuildHeadersAsync()
    {
        string cookies = await Downkyi.Core.Bili.Web.LoginHelperV2.GetLoginInfoCookiesString();
        var headers = new Dictionary<string, string>
        {
            ["Referer"] = Referer,
        };
        if (!string.IsNullOrEmpty(cookies))
            headers["Cookie"] = cookies;
        return headers;
    }

    // ─── DB helpers ───────────────────────────────────────────────────────────

    private static async Task UpdateDbStatusAsync(string uuid, DownloadStatus status)
    {
        var entity = await DownloadDatabase.Instance.GetDownloadingByUuidAsync(uuid);
        if (entity != null)
        {
            entity.Status = status;
            await DownloadDatabase.Instance.UpdateDownloadingAsync(entity);
        }
    }

    // ─── UI thread helpers ────────────────────────────────────────────────────

    private void RunOnUiThread(Action action)
    {
        if (_uiContext != null)
            _uiContext.Post(_ => action(), null);
        else
            action();
    }

    private void SetStatus(DownloadingItem item, DownloadStatus status)
        => RunOnUiThread(() => item.Status = status);

    private void SetProgress(DownloadingItem item, float value)
        => RunOnUiThread(() => item.Progress = value);

    // ─── Misc ─────────────────────────────────────────────────────────────────

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* best-effort */ }
    }
}
