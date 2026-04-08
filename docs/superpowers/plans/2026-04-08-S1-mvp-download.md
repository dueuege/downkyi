# Slice 1 — MVP Download Implementation Plan

> **Goal:** User can paste a Bilibili URL → see video info → pick quality → add to download queue.
> **Branch:** `v2.0.x`
> **Prerequisite:** All P0 tasks complete (commit `8d862f1`)

---

## Status: ✅ Already done (from P0)
- S1-01: `VideoInfo` Core model — `VideoInfo.cs` has Pages, Sections, CoverUrl, stats ✅
- S1-02: `IVideo.GetVideoPages/GetVideoSections` — implemented in `Video.cs` ✅

---

## Task S1-03: Port VideoZone lookup to Core

**Files:**
- Create: `src/DownKyi.Core/Bili/Zone/VideoZone.cs`

Zone lookup is needed by VideoInfoService to display `动画>MAD·AMV` style strings from `TypeId`.

- [ ] **Step 1: Create VideoZone.cs**

```csharp
// src/DownKyi.Core/Bili/Zone/VideoZone.cs
namespace Downkyi.Core.Bili.Zone;

public class ZoneAttr
{
    public int Id { get; }
    public string EnName { get; }
    public string Name { get; }
    public int ParentId { get; }
    public ZoneAttr(int id, string enName, string name, int parentId = 0)
    { Id = id; EnName = enName; Name = name; ParentId = parentId; }
}

public static class VideoZone
{
    private static readonly List<ZoneAttr> _zones = BuildZones();

    public static List<ZoneAttr> GetZones() => _zones;

    public static string GetZoneName(int tid, string fallback = "")
    {
        var zone = _zones.Find(z => z.Id == tid);
        if (zone == null) return fallback;
        var parent = _zones.Find(z => z.Id == zone.ParentId);
        return parent != null ? $"{parent.Name}>{zone.Name}" : zone.Name;
    }

    private static List<ZoneAttr> BuildZones() => new()
    {
        // 动画
        new(1, "douga", "动画"),
        new(24, "mad", "MAD·AMV", 1),
        new(25, "mmd", "MMD·3D", 1),
        new(47, "voice", "短片·手书·配音", 1),
        new(210, "garage_kit", "手办·模玩", 1),
        new(86, "tokusatsu", "特摄", 1),
        new(253, "acgntalks", "动漫杂谈", 1),
        new(27, "other", "综合", 1),
        // 番剧
        new(13, "anime", "番剧"),
        new(33, "serial", "连载动画", 13),
        new(32, "finish", "完结动画", 13),
        new(51, "information", "资讯", 13),
        new(152, "offical", "官方延伸", 13),
        // 国创
        new(167, "guochuang", "国创"),
        new(153, "chinese", "国产动画", 167),
        new(168, "original", "国产原创相关", 167),
        new(169, "puppetry", "布袋戏", 167),
        new(195, "motioncomic", "动态漫·广播剧", 167),
        new(170, "information", "资讯", 167),
        // 音乐
        new(3, "music", "音乐"),
        new(28, "original", "原创音乐", 3),
        new(31, "cover", "翻唱", 3),
        new(59, "perform", "演奏", 3),
        new(29, "mv", "MV", 3),
        new(54, "live", "音乐现场", 3),
        new(130, "vocaloid", "VOCALOID·UTAU", 3),
        new(243, "electronic", "电音", 3),
        new(30, "other", "音乐综合", 3),
        // 舞蹈
        new(129, "dance", "舞蹈"),
        new(20, "otaku", "宅舞", 129),
        new(154, "three_d", "舞蹈综合", 129),
        new(156, "demo", "舞蹈教程", 129),
        new(198, "hiphop", "街舞", 129),
        new(199, "star", "明星舞蹈", 129),
        new(200, "china", "中国舞", 129),
        // 游戏
        new(4, "game", "游戏"),
        new(17, "stand_alone", "单机游戏", 4),
        new(171, "esports", "电子竞技", 4),
        new(172, "mobile", "手机游戏", 4),
        new(65, "online", "网络游戏", 4),
        new(173, "board", "桌游棋牌", 4),
        new(121, "gmv", "GMV", 4),
        new(136, "music", "音游", 4),
        new(19, "mugen", "Mugen", 4),
        // 知识
        new(36, "knowledge", "知识"),
        new(201, "science", "科学科普", 36),
        new(124, "social_science", "社科·法律·心理", 36),
        new(228, "humanity_history", "人文历史", 36),
        new(207, "finance", "财经商业", 36),
        new(208, "campus", "校园学习", 36),
        new(209, "career", "职业职场", 36),
        new(229, "design", "设计·创意", 36),
        new(122, "skill", "野生技术协会", 36),
        // 科技
        new(188, "tech", "科技"),
        new(95, "digital", "数码", 188),
        new(230, "application", "软件应用", 188),
        new(231, "computer_tech", "计算机技术", 188),
        new(232, "industry", "科工机械", 188),
        new(233, "wireless", "极客DIY", 188),
        // 运动
        new(234, "sports", "运动"),
        new(235, "basketball", "篮球", 234),
        new(249, "football", "足球", 234),
        new(164, "aerobics", "健身", 234),
        new(236, "athletic", "竞技体育", 234),
        new(237, "culture", "运动文化", 234),
        new(238, "综合", "运动综合", 234),
        // 汽车
        new(223, "car", "汽车"),
        new(245, "racing", "赛车", 223),
        new(246, "modifiedvehicle", "改装玩车", 223),
        new(247, "tips", "新能源车", 223),
        new(248, "cartourist", "房车", 223),
        new(240, "drafts", "汽车生活", 223),
        new(244, "culture", "汽车文化", 223),
        new(176, "record", "购车攻略", 223),
        // 生活
        new(160, "life", "生活"),
        new(138, "funny", "搞笑", 160),
        new(250, "travel", "出行", 160),
        new(251, "rurallife", "三农", 160),
        new(239, "home", "家居房产", 160),
        new(161, "handmake", "手工", 160),
        new(162, "painting", "绘画", 160),
        new(21, "daily", "日常", 160),
        // 美食
        new(211, "food", "美食"),
        new(76, "make", "美食制作", 211),
        new(212, "detective", "美食侦探", 211),
        new(213, "measurement", "美食测评", 211),
        new(214, "rural", "田园美食", 211),
        new(215, "record", "美食记录", 211),
        // 动物圈
        new(217, "animal", "动物圈"),
        new(218, "cat", "喵星人", 217),
        new(219, "dog", "汪星人", 217),
        new(220, "panda", "大熊猫", 217),
        new(221, "wild_animal", "野生动物", 217),
        new(222, "reptiles", "爬宠", 217),
        new(75, "other_animal", "动物综合", 217),
        // 鬼畜
        new(119, "kichiku", "鬼畜"),
        new(22, "guide", "鬼畜调教", 119),
        new(26, "mad", "音MAD", 119),
        new(126, "raps", "人力VOCALOID", 119),
        new(216, "movie_cut", "鬼畜剧场", 119),
        new(127, "other", "教程演示", 119),
        // 时尚
        new(155, "fashion", "时尚"),
        new(157, "makeup", "美妆护肤", 155),
        new(252, "cos", "仿妆cos", 155),
        new(158, "clothing", "穿搭", 155),
        new(159, "catwalk", "时尚潮流", 155),
        // 资讯
        new(202, "information", "资讯"),
        new(203, "hot", "热点", 202),
        new(204, "global", "环球", 202),
        new(205, "social", "社会", 202),
        new(206, "multiple", "综合", 202),
        // 娱乐
        new(5, "ent", "娱乐"),
        new(71, "variety", "综艺", 5),
        new(241, "fans", "粉丝创作", 5),
        new(242, "celebrity", "明星综合", 5),
        // 影视
        new(181, "cinephile", "影视"),
        new(182, "cinecism", "影视杂谈", 181),
        new(183, "montage", "影视剪辑", 181),
        new(85, "shortfilm", "小剧场", 181),
        new(184, "trailer_info", "预告·资讯", 181),
        // 纪录片
        new(177, "documentary", "纪录片"),
        new(37, "history", "历史", 177),
        new(178, "science", "科学", 177),
        new(179, "military", "军事", 177),
        new(180, "travel", "探险", 177),
        new(185, "animal", "自然", 177),
        new(186, "humanity_history", "人文", 177),
        // 电影
        new(23, "movie", "电影"),
        new(147, "chinese", "华语电影", 23),
        new(145, "west", "欧美电影", 23),
        new(146, "japan", "日本电影", 23),
        new(83, "documentary", "纪录片", 23),
        new(187, "other", "其他国家", 23),
        // 电视剧
        new(11, "teleplay", "电视剧"),
        new(185, "mainland", "国产剧", 11),
        new(187, "overseas", "海外剧", 11),
    };
}
```

- [ ] **Step 2: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
"/mnt/c/Program Files/dotnet/dotnet.exe" build DownKyi.Core/Downkyi.Core.csproj
```

- [ ] **Step 3: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/Downkyi.Core/Bili/Zone/
git commit -m "feat(core): add VideoZone lookup table (zone id → display name)"
```

---

## Task S1-04: Extend IVideoInfoService with GetVideoPages/GetVideoSections/GetVideoStream

**Files:**
- Modify: `src/Downkyi.UI/Services/VideoInfo/IVideoInfoService.cs`

```csharp
using Downkyi.Core.Bili.Models;
using Downkyi.UI.Models;

namespace Downkyi.UI.Services.VideoInfo;

public interface IVideoInfoService
{
    VideoInfoView? GetVideoView(string input);
    Task<List<VideoPage>> GetVideoPagesAsync(string input);
    Task<List<VideoSection>> GetVideoSectionsAsync(string input);
}
```

No build step needed (interface change will cause compile errors in stubs which we fix in S1-05–07).

---

## Task S1-05: Implement VideoInfoService

**Files:**
- Modify: `src/Downkyi.UI/Services/VideoInfo/VideoInfoService.cs`

```csharp
using Downkyi.Core.Bili;
using Downkyi.Core.Bili.Models;
using Downkyi.Core.Bili.Zone;
using Downkyi.Core.Utils;
using Downkyi.UI.Models;

namespace Downkyi.UI.Services.VideoInfo;

public class VideoInfoService : IVideoInfoService
{
    public VideoInfoView? GetVideoView(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        var video = BiliLocator.Video(input);
        var info = video.GetVideoInfo();
        if (info == null) return null;

        var view = new VideoInfoView
        {
            CoverUrl = info.CoverUrl,
            UpperMid = info.UpperMid,
            TypeId = info.TypeId,
        };

        view.Title = info.Title;
        view.Description = info.Description;
        view.VideoZone = VideoZone.GetZoneName(info.TypeId, info.Title);
        view.UpName = info.UpperName;

        // Publish time (unix timestamp → string)
        if (long.TryParse(info.PublishTime, out long pubUnix))
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(pubUnix).ToLocalTime();
            view.CreateTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            view.CreateTime = info.PublishTime;
        }

        view.PlayNumber = Format.FormatNumber(info.PlayCount);
        view.DanmakuNumber = Format.FormatNumber(info.DanmakuCount);
        view.LikeNumber = Format.FormatNumber(info.LikeCount);
        view.CoinNumber = Format.FormatNumber(info.CoinCount);
        view.FavoriteNumber = Format.FormatNumber(info.FavoriteCount);
        view.ShareNumber = Format.FormatNumber(info.ShareCount);
        view.ReplyNumber = Format.FormatNumber(info.ReplyCount);

        return view;
    }

    public async Task<List<VideoPage>> GetVideoPagesAsync(string input)
    {
        var video = BiliLocator.Video(input);
        return await video.GetVideoPagesAsync();
    }

    public async Task<List<VideoSection>> GetVideoSectionsAsync(string input)
    {
        var video = BiliLocator.Video(input);
        return await video.GetVideoSectionsAsync();
    }
}
```

Note: `info.PublishTime` in the Core model is stored as a string from the BiliSharp Pubdate (long). Check `Video.cs` — if Pubdate is mapped as a long timestamp, we need to confirm the format. Will adjust in implementation if needed.

- [ ] **Build + Commit**
```bash
git add src/Downkyi.UI/Services/VideoInfo/IVideoInfoService.cs src/Downkyi.UI/Services/VideoInfo/VideoInfoService.cs
git commit -m "feat(ui): implement VideoInfoService.GetVideoView/Pages/Sections"
```

---

## Task S1-06: Implement BangumiInfoService

**Files:**
- Modify: `src/Downkyi.UI/Services/VideoInfo/BangumiInfoService.cs`

```csharp
using Downkyi.Core.Bili.Models;
using Downkyi.Core.Bili.Utils;
using Downkyi.Core.Bili.Web;
using Downkyi.Core.Utils;
using Downkyi.UI.Models;
using System.Text.RegularExpressions;

namespace Downkyi.UI.Services.VideoInfo;

public class BangumiInfoService : IVideoInfoService
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public VideoInfoView? GetVideoView(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        var season = GetSeasonAsync(input).GetAwaiter().GetResult();
        if (season == null) return null;

        var view = new VideoInfoView
        {
            CoverUrl = season.Cover ?? string.Empty,
            UpperMid = season.UpInfo?.Mid ?? -1,
            TypeId = 13, // 番剧
        };
        view.Title = season.Title ?? string.Empty;
        view.Description = season.Evaluate ?? string.Empty;
        view.VideoZone = season.TypeName ?? "番剧";
        view.UpName = season.UpInfo?.Name ?? string.Empty;
        view.PlayNumber = season.Stat != null ? Format.FormatNumber(season.Stat.Views) : "0";
        view.DanmakuNumber = season.Stat != null ? Format.FormatNumber(season.Stat.Danmakus) : "0";
        view.LikeNumber = season.Stat != null ? Format.FormatNumber(season.Stat.Likes) : "0";
        view.CoinNumber = season.Stat != null ? Format.FormatNumber(season.Stat.Coins) : "0";
        view.FavoriteNumber = season.Stat != null ? Format.FormatNumber(season.Stat.Favorites) : "0";

        return view;
    }

    public async Task<List<VideoPage>> GetVideoPagesAsync(string input)
    {
        var season = await GetSeasonAsync(input);
        if (season?.Episodes == null) return new List<VideoPage>();

        var pages = new List<VideoPage>();
        int order = 0;
        foreach (var ep in season.Episodes)
        {
            order++;
            string name = string.IsNullOrEmpty(ep.ShareCopy) ? ep.LongTitle ?? ep.Title ?? $"EP{order}"
                : Regex.Replace(ep.ShareCopy, @"^《.*?》", "").Trim();
            if (string.IsNullOrWhiteSpace(name)) name = $"EP{order}";

            pages.Add(new VideoPage
            {
                Cid = ep.Cid,
                Page = order,
                Title = name,
                Duration = ep.Duration,
                IsSelected = order == 1,
            });
        }
        return pages;
    }

    public async Task<List<VideoSection>> GetVideoSectionsAsync(string input)
    {
        var season = await GetSeasonAsync(input);
        if (season == null) return new List<VideoSection>();

        // Main episodes section
        var mainPages = await GetVideoPagesAsync(input);
        var sections = new List<VideoSection>
        {
            new VideoSection { Id = 0, Title = season.Title ?? "正片", VideoPages = mainPages }
        };

        // Extra sections (PV, special, etc.)
        if (season.Section != null)
        {
            foreach (var sec in season.Section)
            {
                if (sec.Episodes == null || sec.Episodes.Count == 0) continue;
                var secPages = new List<VideoPage>();
                int order = 0;
                foreach (var ep in sec.Episodes)
                {
                    order++;
                    secPages.Add(new VideoPage
                    {
                        Cid = ep.Cid,
                        Page = order,
                        Title = ep.LongTitle ?? ep.Title ?? $"EP{order}",
                        Duration = ep.Duration,
                    });
                }
                sections.Add(new VideoSection { Id = sec.Id, Title = sec.Title ?? string.Empty, VideoPages = secPages });
            }
        }

        return sections;
    }

    private static async Task<Downkyi.Core.Bili.Models.Bangumi.BangumiSeason?> GetSeasonAsync(string input)
    {
        try
        {
            if (ParseEntrance.IsBangumiSeasonUrl(input))
                return await BangumiApi.GetBangumiSeasonInfoAsync(seasonId: ParseEntrance.GetBangumiSeasonId(input));
            if (ParseEntrance.IsBangumiEpisodeUrl(input))
                return await BangumiApi.GetBangumiSeasonInfoAsync(episodeId: ParseEntrance.GetBangumiEpisodeId(input));
            if (ParseEntrance.IsBangumiMediaUrl(input))
            {
                var media = await BangumiApi.GetBangumiMediaInfoAsync(ParseEntrance.GetBangumiMediaId(input));
                if (media?.SeasonId > 0)
                    return await BangumiApi.GetBangumiSeasonInfoAsync(seasonId: media.SeasonId);
            }
        }
        catch (Exception e) { NLog.LogManager.GetCurrentClassLogger().Error(e, "BangumiInfoService.GetSeasonAsync failed"); }
        return null;
    }
}
```

- [ ] **Build + Commit**

---

## Task S1-07: Implement CheeseInfoService

**Files:**
- Modify: `src/Downkyi.UI/Services/VideoInfo/CheeseInfoService.cs`

Similar pattern to BangumiInfoService but using CheeseApi.

---

## Task S1-08: Add UI Models (VideoPage, VideoSection, VideoQuality)

**Files:**
- Create: `src/Downkyi.UI/Models/VideoPageItem.cs` (observable UI wrapper)
- Create: `src/Downkyi.UI/Models/VideoSectionItem.cs`
- Create: `src/Downkyi.UI/Models/VideoQualityItem.cs`

These are observable wrappers of Core models for use in the ViewModel.

---

## Task S1-09/S1-10: Wire VideoDetailViewModel

After services are complete, update `VideoDetailViewModel.InputAsync()` to:
1. Call `service.GetVideoView()` → update `VideoInfoView`
2. Call `service.GetVideoSectionsAsync()` → populate `ObservableCollection<VideoSectionItem>`
3. Show quality list

---

## Task S1-11: Port AddToDownloadService + S1-12: DownloadingItem

Port `AddToDownloadService` from v1.5.x, removing Prism. Create `DownloadingItem` observable model.

---

## Task S1-13/S1-14/S1-15: Wire Download ViewModels + Button

Wire DownloadingViewModel, DownloadFinishedViewModel to DownloadDatabase. Wire Download button in VideoDetailViewModel.

---

## Implementation Notes

- `ParseEntrance` is in `Downkyi.Core.Bili.Utils`
- `BangumiApi` needs `GetBangumiMediaInfoAsync(long mediaId)` — check if this exists, add if not
- `VideoInfoService` PublishTime — `Video.cs` maps `videoView.Data.View.Pubdate` (unix long) directly; stored as long in VideoInfo but the model has `string PublishTime` — need to reconcile
- Fix: `VideoInfo.PublishTime` should be `long` not `string`, OR we format in Video.cs
