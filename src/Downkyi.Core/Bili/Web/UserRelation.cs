using System.Text.Json;
using Downkyi.Core.Bili.Models.Users;

namespace Downkyi.Core.Bili.Web;

public static class UserRelationApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>Returns all followers of a user (paginated internally, 50/page).</summary>
    public static async Task<List<RelationFollowInfo>> GetAllFollowersAsync(long mid)
    {
        var result = new List<RelationFollowInfo>();
        int pn = 1;
        while (true)
        {
            string url = $"https://api.bilibili.com/x/relation/followers?vmid={mid}&pn={pn}&ps=50";
            string json = await BiliWebClient.GetAsync(url);
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<RelationFollowOrigin>(json);
                var list = origin?.Data?.List;
                if (list == null || list.Count == 0) break;
                result.AddRange(list);
                if (result.Count >= (origin?.Data?.Total ?? 0)) break;
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllFollowersAsync page {Pn} failed", pn); break; }
        }
        return result;
    }

    /// <summary>Returns all users followed by mid (paginated internally, 50/page).</summary>
    public static async Task<List<RelationFollowInfo>> GetAllFollowingsAsync(long mid)
    {
        var result = new List<RelationFollowInfo>();
        int pn = 1;
        while (true)
        {
            string url = $"https://api.bilibili.com/x/relation/followings?vmid={mid}&pn={pn}&ps=50&order=desc";
            string json = await BiliWebClient.GetAsync(url);
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<RelationFollowOrigin>(json);
                var list = origin?.Data?.List;
                if (list == null || list.Count == 0) break;
                result.AddRange(list);
                if (result.Count >= (origin?.Data?.Total ?? 0)) break;
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllFollowingsAsync page {Pn} failed", pn); break; }
        }
        return result;
    }

    /// <summary>Returns all bangumi/anime followed by a user (paginated internally, 30/page).</summary>
    public static async Task<List<BangumiFollow>> GetAllBangumiFollowAsync(long mid, int type = 1)
    {
        // type: 1=anime 2=cinema
        var result = new List<BangumiFollow>();
        int pn = 1;
        while (true)
        {
            string url = $"https://api.bilibili.com/x/space/bangumi/follow/list?vmid={mid}&follow_status=0&pn={pn}&ps=30&type={type}";
            string json = await BiliWebClient.GetAsync(url, $"https://space.bilibili.com/{mid}");
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<BangumiFollowOrigin>(json);
                var list = origin?.Data?.FollowList;
                if (list == null || list.Count == 0) break;
                result.AddRange(list);
                if (origin?.Data?.HasNext != true) break;
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllBangumiFollowAsync page {Pn} failed", pn); break; }
        }
        return result;
    }
}
