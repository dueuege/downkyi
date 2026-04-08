namespace Downkyi.Core.Bili.Models;

public class VideoInfo
{
    public long Aid { get; set; }
    public string Bvid { get; set; } = string.Empty;
    public long Cid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PublishTime { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public long UpperMid { get; set; }
    public string UpperName { get; set; } = string.Empty;
    public int TypeId { get; set; }
    // Stats
    public long PlayCount { get; set; }
    public long DanmakuCount { get; set; }
    public long LikeCount { get; set; }
    public long CoinCount { get; set; }
    public long FavoriteCount { get; set; }
    public long ShareCount { get; set; }
    public long ReplyCount { get; set; }
    // Episode data (populated by GetVideoPages / GetVideoSections)
    public List<VideoPage> Pages { get; set; } = new();
    public List<VideoSection> Sections { get; set; } = new();
}

public class VideoPage
{
    public long Cid { get; set; }
    public int Page { get; set; }
    public string Title { get; set; } = string.Empty;
    public long Duration { get; set; }
    public bool IsSelected { get; set; }
    // Ids needed to fetch play URL (Avid/Bvid = same as VideoInfo for regular videos; per-episode for bangumi/cheese)
    public long Avid { get; set; }
    public string Bvid { get; set; } = string.Empty;
    public long Epid { get; set; }
}

public class VideoSection
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<VideoPage> VideoPages { get; set; } = new();
}
