using System.Text.Json;
using Downkyi.Core.Bili.Models.Bangumi;

namespace Downkyi.Core.Bili.Web;

public static class BangumiApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Gets bangumi season info by seasonId or episodeId.
    /// Pass seasonId=-1 to use episodeId, and vice versa.
    /// </summary>
    public static async Task<BangumiSeason?> GetBangumiSeasonInfoAsync(long seasonId = -1, long episodeId = -1)
    {
        string baseUrl = "https://api.bilibili.com/pgc/view/web/season";
        string url;
        if (seasonId > -1) url = $"{baseUrl}?season_id={seasonId}";
        else if (episodeId > -1) url = $"{baseUrl}?ep_id={episodeId}";
        else return null;

        string json = await BiliWebClient.GetAsync(url);
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var origin = JsonSerializer.Deserialize<BangumiSeasonOrigin>(json);
            return origin?.Result;
        }
        catch (Exception e) { Log.Error(e, "GetBangumiSeasonInfoAsync failed"); return null; }
    }
}
