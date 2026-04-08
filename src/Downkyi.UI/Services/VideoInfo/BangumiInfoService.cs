using System.Text.RegularExpressions;
using Downkyi.Core.Bili.Models;
using Downkyi.Core.Bili.Models.Bangumi;
using Downkyi.Core.Bili.Utils;
using Downkyi.Core.Bili.Web;
using Downkyi.Core.Utils;
using Downkyi.UI.Models;

namespace Downkyi.UI.Services.VideoInfo;

public class BangumiInfoService : IVideoInfoService
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public VideoInfoView? GetVideoView(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        var season = GetSeasonAsync(input).GetAwaiter().GetResult();
        if (season == null) return null;

        var view = new VideoInfoView
        {
            CoverUrl = season.Cover,
            UpperMid = season.UpInfo?.Mid ?? -1,
            TypeId = 13,
        };
        view.Title = season.Title;
        view.Description = season.Evaluate;
        view.VideoZone = string.IsNullOrEmpty(season.TypeName) ? "番剧" : season.TypeName;
        view.UpName = season.UpInfo?.Uname ?? string.Empty;

        if (season.Stat != null)
        {
            view.PlayNumber = Format.FormatNumber(season.Stat.Views);
            view.DanmakuNumber = Format.FormatNumber(season.Stat.Danmakus);
            view.LikeNumber = Format.FormatNumber(season.Stat.Likes);
            view.CoinNumber = Format.FormatNumber(season.Stat.Coins);
            view.FavoriteNumber = Format.FormatNumber(season.Stat.Favorites);
            view.ReplyNumber = Format.FormatNumber(season.Stat.Reply);
            view.ShareNumber = Format.FormatNumber(season.Stat.Share);
        }

        return view;
    }

    public async Task<List<VideoPage>> GetVideoPagesAsync(string input)
    {
        var season = await GetSeasonAsync(input);
        if (season?.Episodes == null) return new List<VideoPage>();

        var pages = new List<VideoPage>();
        int order = 0;
        foreach (var ep in season.Episodes)
        {
            order++;
            string name = BuildEpisodeName(ep, order);
            pages.Add(new VideoPage
            {
                Cid = ep.Cid,
                Page = order,
                Title = name,
                Duration = ep.Duration,
                IsSelected = order == 1,
            });
        }
        return pages;
    }

    public async Task<List<VideoSection>> GetVideoSectionsAsync(string input)
    {
        var season = await GetSeasonAsync(input);
        if (season == null) return new List<VideoSection>();

        var mainPages = await GetVideoPagesAsync(input);
        var sections = new List<VideoSection>
        {
            new VideoSection { Id = 0, Title = season.Title ?? "正片", VideoPages = mainPages }
        };

        if (season.Section != null)
        {
            foreach (var sec in season.Section)
            {
                if (sec.Episodes == null || sec.Episodes.Count == 0) continue;
                var secPages = new List<VideoPage>();
                int order = 0;
                foreach (var ep in sec.Episodes)
                {
                    order++;
                    secPages.Add(new VideoPage
                    {
                        Cid = ep.Cid,
                        Page = order,
                        Title = BuildEpisodeName(ep, order),
                        Duration = ep.Duration,
                    });
                }
                sections.Add(new VideoSection
                {
                    Id = sec.Id,
                    Title = sec.Title ?? string.Empty,
                    VideoPages = secPages
                });
            }
        }

        return sections;
    }

    private static string BuildEpisodeName(BangumiEpisode ep, int order)
    {
        if (!string.IsNullOrEmpty(ep.ShareCopy))
        {
            string name = Regex.Replace(ep.ShareCopy, @"^《.*?》", "").Trim();
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        if (!string.IsNullOrEmpty(ep.LongTitle)) return ep.LongTitle;
        if (!string.IsNullOrEmpty(ep.Title)) return ep.Title;
        return $"EP{order}";
    }

    private static async Task<BangumiSeason?> GetSeasonAsync(string input)
    {
        try
        {
            if (ParseEntrance.IsBangumiSeasonUrl(input))
                return await BangumiApi.GetBangumiSeasonInfoAsync(seasonId: ParseEntrance.GetBangumiSeasonId(input));
            if (ParseEntrance.IsBangumiEpisodeUrl(input))
                return await BangumiApi.GetBangumiSeasonInfoAsync(episodeId: ParseEntrance.GetBangumiEpisodeId(input));
            if (ParseEntrance.IsBangumiMediaUrl(input))
            {
                var media = await BangumiApi.GetBangumiMediaInfoAsync(ParseEntrance.GetBangumiMediaId(input));
                if (media?.SeasonId > 0)
                    return await BangumiApi.GetBangumiSeasonInfoAsync(seasonId: media.SeasonId);
            }
        }
        catch (Exception e) { Log.Error(e, "BangumiInfoService.GetSeasonAsync failed for {Input}", input); }
        return null;
    }
}
