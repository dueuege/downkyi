using System.Text.Json;
using Downkyi.Core.Bili.Models.Cheese;

namespace Downkyi.Core.Bili.Web;

public static class CheeseApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public static async Task<CheeseView?> GetCheeseViewInfoAsync(long seasonId = -1, long episodeId = -1)
    {
        string baseUrl = "https://api.bilibili.com/pugv/view/web/season";
        string url;
        if (seasonId > -1) url = $"{baseUrl}?season_id={seasonId}";
        else if (episodeId > -1) url = $"{baseUrl}?ep_id={episodeId}";
        else return null;

        string json = await BiliWebClient.GetAsync(url);
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var origin = JsonSerializer.Deserialize<CheeseViewOrigin>(json);
            return origin?.Data;
        }
        catch (Exception e) { Log.Error(e, "GetCheeseViewInfoAsync failed"); return null; }
    }

    public static async Task<List<CheeseEpisode>> GetAllCheeseEpisodesAsync(long seasonId, int ps = 50)
    {
        var result = new List<CheeseEpisode>();
        int pn = 1;
        while (true)
        {
            string url = $"https://api.bilibili.com/pugv/view/web/ep/list?season_id={seasonId}&pn={pn}&ps={ps}";
            string json = await BiliWebClient.GetAsync(url);
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<CheeseEpisodeListOrigin>(json);
                var items = origin?.Data?.Items;
                if (items == null || items.Count == 0) break;
                result.AddRange(items);
                if (origin?.Data?.Page?.Next != true) break;
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllCheeseEpisodesAsync page {Pn} failed", pn); break; }
        }
        return result;
    }
}
