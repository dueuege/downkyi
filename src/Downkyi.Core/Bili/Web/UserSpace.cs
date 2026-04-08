using System.Text.Json;
using Downkyi.BiliSharp.Api.Sign;
using Downkyi.Core.Bili.Models.Users;

namespace Downkyi.Core.Bili.Web;

public static class UserSpaceApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>Returns all uploaded videos for a user (paginated internally, 100/page).</summary>
    public static async Task<List<SpacePublicationVideo>> GetAllPublicationsAsync(long mid, int tid = 0, string keyword = "")
    {
        var result = new List<SpacePublicationVideo>();
        int pn = 1;
        while (true)
        {
            var parameters = new Dictionary<string, object>
            {
                { "mid", mid }, { "pn", pn }, { "ps", 100 }, { "tid", tid }, { "keyword", keyword }, { "order", "pubdate" }
            };
            string query = WbiSign.ParametersToQuery(WbiSign.EncodeWbi(parameters));
            string url = $"https://api.bilibili.com/x/space/wbi/arc/search?{query}";
            string json = await BiliWebClient.GetAsync(url, $"https://space.bilibili.com/{mid}/video");
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<SpacePublicationOrigin>(json);
                var vlist = origin?.Data?.List?.Vlist;
                if (vlist == null || vlist.Count == 0) break;
                result.AddRange(vlist);
                int total = origin?.Data?.Page?.Count ?? 0;
                if (result.Count >= total) break;
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllPublicationsAsync page {Pn} failed", pn); break; }
        }
        return result;
    }

    /// <summary>Returns all channels for a user.</summary>
    public static async Task<List<SpaceChannel>> GetChannelsAsync(long mid)
    {
        string url = $"https://api.bilibili.com/x/space/channel/list?mid={mid}&guest=1&jsonp=jsonp";
        string json = await BiliWebClient.GetAsync(url, $"https://space.bilibili.com/{mid}");
        if (string.IsNullOrEmpty(json)) return new List<SpaceChannel>();
        try
        {
            var origin = JsonSerializer.Deserialize<SpaceChannelListOrigin>(json);
            return origin?.Data?.List ?? new List<SpaceChannel>();
        }
        catch (Exception e) { Log.Error(e, "GetChannelsAsync failed for mid {Mid}", mid); return new List<SpaceChannel>(); }
    }

    /// <summary>Returns all channel videos for a specific channel (paginated internally).</summary>
    public static async Task<List<SpaceChannelArchive>> GetAllChannelVideosAsync(long mid, long cid)
    {
        var result = new List<SpaceChannelArchive>();
        int pn = 1;
        while (true)
        {
            string url = $"https://api.bilibili.com/x/space/channel/video?mid={mid}&cid={cid}&pn={pn}&ps=30&guest=1&jsonp=jsonp";
            string json = await BiliWebClient.GetAsync(url, $"https://space.bilibili.com/{mid}");
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<SpaceChannelVideoOrigin>(json);
                var archives = origin?.Data?.List?.Archives;
                if (archives == null || archives.Count == 0) break;
                result.AddRange(archives);
                int total = origin?.Data?.Page?.Total ?? 0;
                if (result.Count >= total) break;
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllChannelVideosAsync page {Pn} failed", pn); break; }
        }
        return result;
    }

    /// <summary>Returns seasons and series list for a user.</summary>
    public static async Task<SpaceSeasonsSeriesItems?> GetSeasonsSeriesAsync(long mid, int pn = 1, int ps = 20)
    {
        var parameters = new Dictionary<string, object> { { "mid", mid }, { "page_num", pn }, { "page_size", ps } };
        string query = WbiSign.ParametersToQuery(WbiSign.EncodeWbi(parameters));
        string url = $"https://api.bilibili.com/x/polymer/web-space/seasons_series_list?{query}";
        string json = await BiliWebClient.GetAsync(url, $"https://space.bilibili.com/{mid}");
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var origin = JsonSerializer.Deserialize<SpaceSeasonsSeriesOrigin>(json);
            return origin?.Data?.ItemsLists;
        }
        catch (Exception e) { Log.Error(e, "GetSeasonsSeriesAsync failed for mid {Mid}", mid); return null; }
    }
}
