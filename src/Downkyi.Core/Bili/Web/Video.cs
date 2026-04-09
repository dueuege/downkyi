using Downkyi.BiliSharp.Api.Login;
using Downkyi.BiliSharp.Api.Models.Video;
using Downkyi.BiliSharp.Api.Sign;
using Downkyi.Core.Bili.Models;
using Downkyi.Core.Bili.Models.VideoStream;
using Downkyi.Core.Bili.Utils;

namespace Downkyi.Core.Bili.Web;

internal class Video : IVideo
{
    private readonly VideoView? videoView;

    private readonly string _input = string.Empty;

    internal Video(string input)
    {
        if (input == null)
        {
            return;
        }

        _input = input;

        if (ParseEntrance.IsAvId(input) || ParseEntrance.IsAvUrl(input))
        {
            long avid = ParseEntrance.GetAvId(input);
            videoView = BiliSharp.Api.Video.VideoInfo.GetVideoViewInfo(null, avid);
        }

        if (ParseEntrance.IsBvId(input) || ParseEntrance.IsBvUrl(input))
        {
            string bvid = ParseEntrance.GetBvId(input);
            videoView = BiliSharp.Api.Video.VideoInfo.GetVideoViewInfo(bvid);
        }
    }

    public string Input()
    {
        return _input;
    }

    /// <summary>
    /// 获取视频详情页信息
    /// </summary>
    public VideoInfo? GetVideoInfo(string? bvid = null, long aid = -1)
    {
        if (videoView == null) { return null; }
        if (videoView.Data == null) { return null; }
        if (videoView.Data.View == null) { return null; }

        /* 视频发布时间 */
        DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
        DateTime dateTime = startTime.AddSeconds(videoView.Data.View.Pubdate);
        string publishTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

        VideoInfo videoInfo = new()
        {
            Aid = videoView.Data.View.Aid,
            Bvid = videoView.Data.View.Bvid,
            Cid = videoView.Data.View.Cid,
            Title = videoView.Data.View.Title,
            Description = videoView.Data.View.Desc,
            PublishTime = publishTime,
            CoverUrl = videoView.Data.View.Pic ?? string.Empty,
            UpperMid = videoView.Data.View.Owner?.Mid ?? 0,
            UpperName = videoView.Data.View.Owner?.Name ?? string.Empty,
            TypeId = videoView.Data.View.Tid,
            PlayCount = videoView.Data.View.Stat?.View ?? 0,
            DanmakuCount = videoView.Data.View.Stat?.Danmaku ?? 0,
            LikeCount = videoView.Data.View.Stat?.Like ?? 0,
            CoinCount = videoView.Data.View.Stat?.Coin ?? 0,
            FavoriteCount = videoView.Data.View.Stat?.Favorite ?? 0,
            ShareCount = videoView.Data.View.Stat?.Share ?? 0,
            ReplyCount = videoView.Data.View.Stat?.Reply ?? 0,
        };

        return videoInfo;
    }

    /// <summary>Returns all video pages (parts) for a multi-part video.</summary>
    public async Task<List<VideoPage>> GetVideoPagesAsync()
    {
        if (videoView?.Data?.View?.Pages == null) return new List<VideoPage>();
        return videoView.Data.View.Pages.Select((p, i) => new VideoPage
        {
            Cid = p.Cid,
            Page = i + 1,
            Title = string.IsNullOrEmpty(p.Part) ? (videoView.Data.View.Title ?? string.Empty) : p.Part,
            Duration = p.Duration,
            IsSelected = false,
        }).ToList();
    }

    /// <summary>
    /// Returns video sections. Regular UGC videos have no sections — pages are wrapped
    /// in a single default section.
    /// </summary>
    public async Task<List<VideoSection>> GetVideoSectionsAsync()
    {
        var pages = await GetVideoPagesAsync();
        if (pages.Count == 0) return new List<VideoSection>();
        return new List<VideoSection>
        {
            new VideoSection { Id = 0, Title = "default", VideoPages = pages }
        };
    }

    /// <summary>Fetches the DASH/FLV play URL for the given cid at the requested quality.</summary>
    public async Task<PlayUrl?> GetPlayUrlAsync(long cid, int quality = 125)
    {
        if (videoView?.Data?.View == null) return null;
        return await VideoStreamApi.GetVideoPlayUrlAsync(
            videoView.Data.View.Aid,
            videoView.Data.View.Bvid,
            cid,
            quality);
    }
}
