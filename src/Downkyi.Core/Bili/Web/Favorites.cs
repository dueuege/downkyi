using System.Text.Json;
using Downkyi.Core.Bili.Models.Favorites;

namespace Downkyi.Core.Bili.Web;

public static class FavoritesApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public static async Task<List<FavoritesMetaInfo>> GetAllCreatedFavoritesAsync(long mid)
    {
        var result = new List<FavoritesMetaInfo>();
        int pn = 1;
        while (true)
        {
            string url = $"https://api.bilibili.com/x/v3/fav/folder/created/list?up_mid={mid}&pn={pn}&ps=50";
            string json = await BiliWebClient.GetAsync(url);
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<FavoritesListOrigin>(json);
                var list = origin?.Data?.List;
                if (list == null || list.Count == 0) break;
                result.AddRange(list);
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllCreatedFavoritesAsync page {Pn} failed", pn); break; }
        }
        return result;
    }

    public static async Task<List<FavoritesMetaInfo>> GetAllCollectedFavoritesAsync(long mid)
    {
        var result = new List<FavoritesMetaInfo>();
        int pn = 1;
        while (true)
        {
            string url = $"https://api.bilibili.com/x/v3/fav/folder/collected/list?up_mid={mid}&pn={pn}&ps=50";
            string json = await BiliWebClient.GetAsync(url);
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<FavoritesListOrigin>(json);
                var list = origin?.Data?.List;
                if (list == null || list.Count == 0) break;
                result.AddRange(list);
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllCollectedFavoritesAsync page {Pn} failed", pn); break; }
        }
        return result;
    }

    public static async Task<List<FavoritesMedia>> GetAllFavoritesMediaAsync(long mediaId)
    {
        var result = new List<FavoritesMedia>();
        int pn = 1;
        while (true)
        {
            string url = $"https://api.bilibili.com/x/v3/fav/resource/list?media_id={mediaId}&pn={pn}&ps=20&platform=web";
            string json = await BiliWebClient.GetAsync(url);
            if (string.IsNullOrEmpty(json)) break;
            try
            {
                var origin = JsonSerializer.Deserialize<FavoritesMediaResourceOrigin>(json);
                var medias = origin?.Data?.Medias;
                if (medias == null || medias.Count == 0) break;
                result.AddRange(medias);
                if (origin?.Data?.HasMore != true) break;
                pn++;
            }
            catch (Exception e) { Log.Error(e, "GetAllFavoritesMediaAsync page {Pn} failed", pn); break; }
        }
        return result;
    }
}
