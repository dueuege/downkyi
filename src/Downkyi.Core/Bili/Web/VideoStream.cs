using System.Text.Json;
using Downkyi.BiliSharp.Api.Sign;
using Downkyi.Core.Bili.Models.VideoStream;

namespace Downkyi.Core.Bili.Web;

public static class VideoStreamApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Fetches DASH/FLV play URL for a regular video.
    /// quality: 125=4K 120=4K 116=1080P60 112=1080P+ 80=1080P 64=720P 32=480P 16=360P
    /// fnval 4048 = DASH+HDR+4K+Dolby+HiRes
    /// </summary>
    public static async Task<PlayUrl?> GetVideoPlayUrlAsync(long avid, string? bvid, long cid, int quality = 125)
    {
        var parameters = new Dictionary<string, object>
        {
            { "fourk", 1 },
            { "fnver", 0 },
            { "fnval", 4048 },
            { "cid", cid },
            { "qn", quality },
        };
        if (bvid != null) parameters["bvid"] = bvid;
        else if (avid > -1) parameters["aid"] = avid;
        else return null;

        string query = WbiSign.ParametersToQuery(WbiSign.EncodeWbi(parameters));
        string url = $"https://api.bilibili.com/x/player/wbi/playurl?{query}";
        return await GetPlayUrlAsync(url);
    }

    /// <summary>Fetches play URL for a bangumi episode.</summary>
    public static async Task<PlayUrl?> GetBangumiPlayUrlAsync(long avid, string? bvid, long cid, int quality = 125)
    {
        string baseUrl = $"https://api.bilibili.com/pgc/player/web/playurl?cid={cid}&qn={quality}&fourk=1&fnver=0&fnval=4048";
        string url = bvid != null ? $"{baseUrl}&bvid={bvid}" : avid > -1 ? $"{baseUrl}&aid={avid}" : null!;
        if (url == null) return null;
        return await GetPlayUrlAsync(url);
    }

    /// <summary>Fetches play URL for a Cheese (paid course) episode.</summary>
    public static async Task<PlayUrl?> GetCheesePlayUrlAsync(long avid, string? bvid, long cid, long episodeId, int quality = 125)
    {
        string baseUrl = $"https://api.bilibili.com/pugv/player/web/playurl?cid={cid}&qn={quality}&fourk=1&fnver=0&fnval=4048";
        string url = bvid != null ? $"{baseUrl}&bvid={bvid}" : avid > -1 ? $"{baseUrl}&aid={avid}" : null!;
        if (url == null) return null;
        if (episodeId > 0) url += $"&ep_id={episodeId}";
        return await GetPlayUrlAsync(url);
    }

    private static async Task<PlayUrl?> GetPlayUrlAsync(string url)
    {
        string json = await BiliWebClient.GetAsync(url);
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var origin = JsonSerializer.Deserialize<PlayUrlOrigin>(json);
            return origin?.Data ?? origin?.Result;
        }
        catch (Exception e)
        {
            Log.Error(e, "GetPlayUrlAsync deserialize failed");
            return null;
        }
    }
}
