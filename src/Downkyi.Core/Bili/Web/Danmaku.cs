using System.Xml;
using Downkyi.Core.Bili.Models.Danmaku;

namespace Downkyi.Core.Bili.Web;

public static class DanmakuApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Fetches XML danmaku for a given cid and parses it into a list of DanmakuItem.
    /// Uses the legacy XML API which does not require authentication for most videos.
    /// </summary>
    public static async Task<List<DanmakuItem>> GetXmlDanmakuAsync(long cid)
    {
        string url = $"https://comment.bilibili.com/{cid}.xml";
        string xml = await BiliWebClient.GetAsync(url, "https://www.bilibili.com");
        if (string.IsNullOrEmpty(xml)) return new List<DanmakuItem>();

        var items = new List<DanmakuItem>();
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var nodes = doc.SelectNodes("//d");
            if (nodes == null) return items;

            foreach (XmlNode node in nodes)
            {
                string? p = node.Attributes?["p"]?.Value;
                string? content = node.InnerText;
                if (string.IsNullOrEmpty(p) || content == null) continue;

                string[] parts = p.Split(',');
                if (parts.Length < 8) continue;

                items.Add(new DanmakuItem
                {
                    Time = float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float t) ? t : 0,
                    Type = int.TryParse(parts[1], out int type) ? type : 1,
                    Size = int.TryParse(parts[2], out int size) ? size : 25,
                    Color = int.TryParse(parts[3], out int color) ? color : 16777215,
                    Timestamp = long.TryParse(parts[4], out long ts) ? ts : 0,
                    Pool = int.TryParse(parts[5], out int pool) ? pool : 0,
                    UserHash = parts[6],
                    DmId = parts[7],
                    Content = content
                });
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "GetXmlDanmakuAsync failed for cid {Cid}", cid);
        }

        return items;
    }
}
