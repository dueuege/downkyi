using System.Text.Json;
using Downkyi.Core.Bili.Models.History;

namespace Downkyi.Core.Bili.Web;

public static class HistoryApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Fetches one page of watch history. Use cursor.Max and cursor.ViewAt from the result
    /// as startId/startTime for the next page. Pass startId=0, startTime=0 for the first page.
    /// </summary>
    public static async Task<HistoryData?> GetHistoryAsync(long startId = 0, long startTime = 0, int ps = 30)
    {
        string url = $"https://api.bilibili.com/x/web-interface/history/cursor?max={startId}&view_at={startTime}&ps={ps}&business=archive";
        string json = await BiliWebClient.GetAsync(url);
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var origin = JsonSerializer.Deserialize<HistoryOrigin>(json);
            return origin?.Data;
        }
        catch (Exception e) { Log.Error(e, "GetHistoryAsync failed"); return null; }
    }

    /// <summary>Fetches all videos from the watch-later (To-View) list.</summary>
    public static async Task<List<ToViewList>> GetToViewAsync()
    {
        string json = await BiliWebClient.GetAsync("https://api.bilibili.com/x/v2/history/toview");
        if (string.IsNullOrEmpty(json)) return new List<ToViewList>();
        try
        {
            var origin = JsonSerializer.Deserialize<ToViewOrigin>(json);
            return origin?.Data?.List ?? new List<ToViewList>();
        }
        catch (Exception e) { Log.Error(e, "GetToViewAsync failed"); return new List<ToViewList>(); }
    }
}
