using Downkyi.Core.Bili.Models;
using Downkyi.Core.Bili.Models.Cheese;
using Downkyi.Core.Bili.Utils;
using Downkyi.Core.Bili.Web;
using Downkyi.Core.Utils;
using Downkyi.UI.Models;

namespace Downkyi.UI.Services.VideoInfo;

public class CheeseInfoService : IVideoInfoService
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public VideoInfoView? GetVideoView(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        var cheeseView = GetCheeseViewAsync(input).GetAwaiter().GetResult();
        if (cheeseView == null) return null;

        var view = new VideoInfoView
        {
            CoverUrl = cheeseView.Cover,
            UpperMid = cheeseView.UpInfo?.Mid ?? -1,
            TypeId = 0,
        };
        view.Title = cheeseView.Title;
        view.Description = cheeseView.Subtitle;
        view.VideoZone = "课程";
        view.UpName = cheeseView.UpInfo?.Uname ?? string.Empty;

        if (cheeseView.Stat != null)
            view.PlayNumber = Format.FormatNumber(cheeseView.Stat.Play);

        return view;
    }

    public async Task<List<VideoPage>> GetVideoPagesAsync(string input)
    {
        var cheeseView = await GetCheeseViewAsync(input);
        if (cheeseView == null) return new List<VideoPage>();

        // Use paginated API for full episode list if needed
        var episodes = await CheeseApi.GetAllCheeseEpisodesAsync(cheeseView.SeasonId);
        if (episodes.Count == 0 && cheeseView.Episodes != null)
            episodes = cheeseView.Episodes;

        var pages = new List<VideoPage>();
        int order = 0;
        foreach (var ep in episodes)
        {
            order++;
            pages.Add(new VideoPage
            {
                Cid = ep.Cid,
                Page = order,
                Title = string.IsNullOrEmpty(ep.Title) ? $"第{order}节" : ep.Title,
                Duration = ep.Duration,
                IsSelected = order == 1,
            });
        }
        return pages;
    }

    public async Task<List<VideoSection>> GetVideoSectionsAsync(string input)
    {
        var cheeseView = await GetCheeseViewAsync(input);
        var pages = await GetVideoPagesAsync(input);

        return new List<VideoSection>
        {
            new VideoSection
            {
                Id = 0,
                Title = cheeseView?.Title ?? "课程",
                VideoPages = pages
            }
        };
    }

    private static async Task<CheeseView?> GetCheeseViewAsync(string input)
    {
        try
        {
            if (ParseEntrance.IsCheeseSeasonUrl(input))
                return await CheeseApi.GetCheeseViewInfoAsync(seasonId: ParseEntrance.GetCheeseSeasonId(input));
            if (ParseEntrance.IsCheeseEpisodeUrl(input))
                return await CheeseApi.GetCheeseViewInfoAsync(episodeId: ParseEntrance.GetCheeseEpisodeId(input));
        }
        catch (Exception e) { NLog.LogManager.GetCurrentClassLogger().Error(e, "CheeseInfoService.GetCheeseViewAsync failed for {Input}", input); }
        return null;
    }
}
