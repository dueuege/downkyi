using Downkyi.Core.Bili;
using Downkyi.Core.Bili.Models;
using Downkyi.Core.Bili.Zone;
using Downkyi.Core.Utils;
using Downkyi.UI.Models;

namespace Downkyi.UI.Services.VideoInfo;

public class VideoInfoService : IVideoInfoService
{
    public VideoInfoView? GetVideoView(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        var video = BiliLocator.Video(input);
        var info = video.GetVideoInfo();
        if (info == null) return null;

        var view = new VideoInfoView
        {
            CoverUrl = info.CoverUrl,
            UpperMid = info.UpperMid,
            TypeId = info.TypeId,
        };

        view.Title = info.Title;
        view.Description = info.Description;
        view.VideoZone = VideoZone.GetZoneName(info.TypeId, info.Title);
        view.UpName = info.UpperName;
        view.CreateTime = info.PublishTime; // already formatted "yyyy-MM-dd HH:mm:ss"

        view.PlayNumber = Format.FormatNumber(info.PlayCount);
        view.DanmakuNumber = Format.FormatNumber(info.DanmakuCount);
        view.LikeNumber = Format.FormatNumber(info.LikeCount);
        view.CoinNumber = Format.FormatNumber(info.CoinCount);
        view.FavoriteNumber = Format.FormatNumber(info.FavoriteCount);
        view.ShareNumber = Format.FormatNumber(info.ShareCount);
        view.ReplyNumber = Format.FormatNumber(info.ReplyCount);

        return view;
    }

    public async Task<List<VideoPage>> GetVideoPagesAsync(string input)
    {
        var video = BiliLocator.Video(input);
        return await video.GetVideoPagesAsync();
    }

    public async Task<List<VideoSection>> GetVideoSectionsAsync(string input)
    {
        var video = BiliLocator.Video(input);
        return await video.GetVideoSectionsAsync();
    }
}
