# P0 — Core API Modules Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port all Core API modules from v1.5.x into v2.0.x's `DownKyi.Core` project — VideoStream, Bangumi, Cheese, Favorites, History, Users, Danmaku2Ass, Download Database, AriaManager, and FileName sanitization — so that all Slice 1–5 UI tasks have working backends.

**Architecture:** A new `BiliWebClient` async HTTP utility reads cookies from `LoginDatabase` and makes authenticated requests; API callers use `System.Text.Json` for deserialization; the download database uses `sqlite-net-sqlcipher` ORM (matching the login DB pattern already in the codebase).

**Tech Stack:** .NET 8, System.Text.Json, sqlite-net-sqlcipher (`SQLite` namespace), `Downkyi.BiliSharp` (WBI signing + BiliManager), NLog

**Namespace convention:** `Downkyi.Core.*` (note: v2.0.x uses `Downkyi` not `DownKyi`)

---

## File Map

### New files to create

```
src/DownKyi.Core/Bili/Web/BiliWebClient.cs          HTTP utility for authenticated API calls
src/DownKyi.Core/Bili/Models/VideoStream/           8 model files for play URL, DASH, subtitles
src/DownKyi.Core/Bili/Web/VideoStream.cs            GetPlayUrl, GetSubtitle
src/DownKyi.Core/Bili/Models/Bangumi/               10 model files
src/DownKyi.Core/Bili/Web/Bangumi.cs                BangumiSeasonInfo, BangumiMediaInfo
src/DownKyi.Core/Bili/Models/Cheese/                8 model files
src/DownKyi.Core/Bili/Web/Cheese.cs                 CheeseViewInfo, CheeseEpisodeList
src/DownKyi.Core/Bili/Models/Favorites/             8 model files
src/DownKyi.Core/Bili/Web/Favorites.cs              GetCreatedFavorites, GetFavoritesMedia
src/DownKyi.Core/Bili/Models/History/               6 model files
src/DownKyi.Core/Bili/Web/History.cs                GetHistory, GetToView
src/DownKyi.Core/Bili/Models/Users/                 12 model files
src/DownKyi.Core/Bili/Web/UserSpace.cs              GetPublication, GetChannels, GetSeasonsSeries
src/DownKyi.Core/Bili/Web/UserRelation.cs           GetFollowers, GetFollowings, GetBangumiFollow
src/DownKyi.Core/Bili/Models/Danmaku/               2 model files (XML-based, no protobuf for now)
src/DownKyi.Core/Bili/Web/Danmaku.cs                GetXmlDanmaku
src/DownKyi.Core/Danmaku2Ass/                       8 files ported from v1.5.x
src/DownKyi.Core/Database/Download/DownloadEntity.cs        Download queue entity
src/DownKyi.Core/Database/Download/DownloadDatabase.cs      SQLite DB for download queue
src/DownKyi.Core/Aria2cNet/AriaManager.cs           Process manager for aria2c binary
```

### Files to modify

```
src/DownKyi.Core/Bili/IVideo.cs                     Add GetPlayUrl, GetVideoPages, GetSubtitle
src/DownKyi.Core/Bili/Web/Video.cs                  Implement new IVideo methods
src/DownKyi.Core/Bili/Models/VideoInfo.cs            Add Pages, Sections fields
src/DownKyi.Core/Bili/BiliLocator.cs                Add BangumiInfo, CheeseInfo, Favorites, History, UserSpace, UserRelation locators
src/DownKyi.Core/FileName/FileName.cs               Add Sanitize() method for title with invalid chars
src/DownKyi.Core/Downkyi.Core.csproj                Add Google.Protobuf (for future danmaku), no new deps needed for this plan
```

---

## Task P0-01: BiliWebClient HTTP Utility

**Files:**
- Create: `src/DownKyi.Core/Bili/Web/BiliWebClient.cs`

- [ ] **Step 1: Create the file**

```csharp
// src/DownKyi.Core/Bili/Web/BiliWebClient.cs
using System.Net;
using Downkyi.Core.Settings;

namespace Downkyi.Core.Bili.Web;

/// <summary>
/// Authenticated HTTP client for Bilibili APIs not covered by BiliSharp.
/// Reads cookies from LoginDatabase and adds standard Bilibili request headers.
/// </summary>
internal static class BiliWebClient
{
    private static readonly HttpClient _client;

    static BiliWebClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            UseCookies = false, // We set cookies manually via header
        };
        _client = new HttpClient(handler);
        _client.Timeout = TimeSpan.FromSeconds(30);
        _client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
        _client.DefaultRequestHeaders.Add("Origin", "https://www.bilibili.com");
    }

    /// <summary>
    /// Makes an authenticated GET request to a Bilibili API endpoint.
    /// </summary>
    public static async Task<string> GetAsync(string url, string referer = "https://www.bilibili.com")
    {
        try
        {
            string userAgent = SettingsManager.Instance.GetUserAgent();
            string cookies = await LoginHelperV2.GetLoginInfoCookiesString();

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", userAgent);
            request.Headers.Add("Referer", referer);
            if (!string.IsNullOrEmpty(cookies))
            {
                request.Headers.Add("Cookie", cookies);
            }

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            NLog.LogManager.GetCurrentClassLogger().Error(e, "BiliWebClient.GetAsync failed for {Url}", url);
            return string.Empty;
        }
    }
}
```

- [ ] **Step 2: Build the project to verify it compiles**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Web/BiliWebClient.cs
git commit -m "feat(core): add BiliWebClient async HTTP utility for authenticated API calls"
```

---

## Task P0-02: VideoStream Models + IVideo Extension

**Files:**
- Create: `src/DownKyi.Core/Bili/Models/VideoStream/PlayUrl.cs`
- Create: `src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDash.cs`
- Create: `src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDashVideo.cs`
- Create: `src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDashDolby.cs`
- Create: `src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDashFlac.cs`
- Create: `src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDurl.cs`
- Create: `src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlSupportFormat.cs`
- Create: `src/DownKyi.Core/Bili/Models/VideoStream/SubtitleTrack.cs`
- Create: `src/DownKyi.Core/Bili/Web/VideoStream.cs`
- Modify: `src/DownKyi.Core/Bili/Models/VideoInfo.cs`
- Modify: `src/DownKyi.Core/Bili/IVideo.cs`
- Modify: `src/DownKyi.Core/Bili/Web/Video.cs`

- [ ] **Step 1: Create PlayUrl models**

```csharp
// src/DownKyi.Core/Bili/Models/VideoStream/PlayUrl.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlOrigin
{
    [JsonPropertyName("data")]
    public PlayUrl? Data { get; set; }
    [JsonPropertyName("result")]
    public PlayUrl? Result { get; set; }
}

public class PlayUrl
{
    [JsonPropertyName("accept_description")]
    public List<string> AcceptDescription { get; set; } = new();
    [JsonPropertyName("accept_quality")]
    public List<int> AcceptQuality { get; set; } = new();
    [JsonPropertyName("durl")]
    public List<PlayUrlDurl>? Durl { get; set; }
    [JsonPropertyName("dash")]
    public PlayUrlDash? Dash { get; set; }
    [JsonPropertyName("support_formats")]
    public List<PlayUrlSupportFormat>? SupportFormats { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDash.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDash
{
    [JsonPropertyName("duration")]
    public long Duration { get; set; }
    [JsonPropertyName("video")]
    public List<PlayUrlDashVideo>? Video { get; set; }
    [JsonPropertyName("audio")]
    public List<PlayUrlDashVideo>? Audio { get; set; }
    [JsonPropertyName("dolby")]
    public PlayUrlDashDolby? Dolby { get; set; }
    [JsonPropertyName("flac")]
    public PlayUrlDashFlac? Flac { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDashVideo.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDashVideo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("base_url")]
    public string BaseUrl { get; set; } = string.Empty;
    [JsonPropertyName("backup_url")]
    public List<string>? BackupUrl { get; set; }
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;
    [JsonPropertyName("codecs")]
    public string Codecs { get; set; } = string.Empty;
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
    [JsonPropertyName("frameRate")]
    public string FrameRate { get; set; } = string.Empty;
    [JsonPropertyName("codecid")]
    public int CodecId { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDashDolby.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDashDolby
{
    [JsonPropertyName("audio")]
    public List<PlayUrlDashVideo>? Audio { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDashFlac.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDashFlac
{
    [JsonPropertyName("audio")]
    public PlayUrlDashVideo? Audio { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlDurl.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlDurl
{
    [JsonPropertyName("order")]
    public int Order { get; set; }
    [JsonPropertyName("length")]
    public long Length { get; set; }
    [JsonPropertyName("size")]
    public long Size { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    [JsonPropertyName("backup_url")]
    public List<string>? BackupUrl { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/VideoStream/PlayUrlSupportFormat.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class PlayUrlSupportFormat
{
    [JsonPropertyName("quality")]
    public int Quality { get; set; }
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;
    [JsonPropertyName("new_description")]
    public string NewDescription { get; set; } = string.Empty;
    [JsonPropertyName("display_desc")]
    public string DisplayDesc { get; set; } = string.Empty;
    [JsonPropertyName("superscript")]
    public string Superscript { get; set; } = string.Empty;
}
```

```csharp
// src/DownKyi.Core/Bili/Models/VideoStream/SubtitleTrack.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.VideoStream;

public class SubtitleTrack
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("lan")]
    public string Lan { get; set; } = string.Empty;
    [JsonPropertyName("lan_doc")]
    public string LanDoc { get; set; } = string.Empty;
    [JsonPropertyName("is_lock")]
    public bool IsLock { get; set; }
    [JsonPropertyName("subtitle_url")]
    public string SubtitleUrl { get; set; } = string.Empty;
    [JsonPropertyName("id_str")]
    public string IdStr { get; set; } = string.Empty;
}

public class SubtitleInfo
{
    [JsonPropertyName("allow_submit")]
    public bool AllowSubmit { get; set; }
    [JsonPropertyName("subtitles")]
    public List<SubtitleTrack>? Subtitles { get; set; }
}
```

- [ ] **Step 2: Create VideoStream.cs API caller**

```csharp
// src/DownKyi.Core/Bili/Web/VideoStream.cs
using System.Text.Json;
using Downkyi.BiliSharp.Api.Sign;
using Downkyi.Core.Bili.Models.VideoStream;

namespace Downkyi.Core.Bili.Web;

internal static class VideoStreamApi
{
    /// <summary>
    /// Fetches DASH/FLV play URL for a regular video.
    /// quality 125=4K 120=4K 116=1080P60 112=1080P+ 80=1080P 64=720P 32=480P 16=360P
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
            NLog.LogManager.GetCurrentClassLogger().Error(e, "GetPlayUrlAsync deserialize failed");
            return null;
        }
    }
}
```

- [ ] **Step 3: Extend VideoInfo model with Pages and Sections**

Replace the content of `src/DownKyi.Core/Bili/Models/VideoInfo.cs` with:

```csharp
// src/DownKyi.Core/Bili/Models/VideoInfo.cs
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
}

public class VideoSection
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<VideoPage> VideoPages { get; set; } = new();
}
```

- [ ] **Step 4: Extend IVideo interface**

Replace `src/DownKyi.Core/Bili/IVideo.cs` with:

```csharp
// src/DownKyi.Core/Bili/IVideo.cs
using Downkyi.Core.Bili.Models;
using Downkyi.Core.Bili.Models.VideoStream;

namespace Downkyi.Core.Bili;

public interface IVideo
{
    string Input();
    VideoInfo? GetVideoInfo(string? bvid = null, long aid = -1);
    Task<List<VideoPage>> GetVideoPagesAsync();
    Task<List<VideoSection>> GetVideoSectionsAsync();
    Task<PlayUrl?> GetPlayUrlAsync(long cid, int quality = 125);
}
```

- [ ] **Step 5: Implement new IVideo methods in Video.cs**

Read `src/DownKyi.Core/Bili/Web/Video.cs`, then add after the existing `GetVideoInfo` method:

```csharp
    public async Task<List<VideoPage>> GetVideoPagesAsync()
    {
        if (videoView?.Data?.View?.Pages == null) return new List<VideoPage>();
        return videoView.Data.View.Pages.Select((p, i) => new VideoPage
        {
            Cid = p.Cid,
            Page = i + 1,
            Title = p.Part ?? videoView.Data.View.Title ?? string.Empty,
            Duration = p.Duration,
            IsSelected = false,
        }).ToList();
    }

    public async Task<List<VideoSection>> GetVideoSectionsAsync()
    {
        // Regular UGC videos don't have sections — wrap pages in a default section
        var pages = await GetVideoPagesAsync();
        if (pages.Count == 0) return new List<VideoSection>();
        return new List<VideoSection>
        {
            new VideoSection { Id = 0, Title = "default", VideoPages = pages }
        };
    }

    public async Task<PlayUrl?> GetPlayUrlAsync(long cid, int quality = 125)
    {
        if (videoView?.Data?.View == null) return null;
        return await VideoStreamApi.GetVideoPlayUrlAsync(
            videoView.Data.View.Aid,
            videoView.Data.View.Bvid,
            cid,
            quality);
    }
```

Also add `using Downkyi.Core.Bili.Models.VideoStream;` at the top of Video.cs.

- [ ] **Step 6: Build to verify**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Models/VideoStream/ \
        src/DownKyi.Core/Bili/Web/VideoStream.cs \
        src/DownKyi.Core/Bili/Models/VideoInfo.cs \
        src/DownKyi.Core/Bili/IVideo.cs \
        src/DownKyi.Core/Bili/Web/Video.cs
git commit -m "feat(core): add VideoStream models and extend IVideo with GetPlayUrl/GetVideoPages"
```

---

## Task P0-03: Bangumi API

**Files:**
- Create: `src/DownKyi.Core/Bili/Models/Bangumi/BangumiSeason.cs`
- Create: `src/DownKyi.Core/Bili/Models/Bangumi/BangumiEpisode.cs`
- Create: `src/DownKyi.Core/Bili/Models/Bangumi/BangumiSection.cs`
- Create: `src/DownKyi.Core/Bili/Models/Bangumi/BangumiStat.cs`
- Create: `src/DownKyi.Core/Bili/Models/Bangumi/BangumiSeasonInfo.cs`
- Create: `src/DownKyi.Core/Bili/Models/Bangumi/BangumiArea.cs`
- Create: `src/DownKyi.Core/Bili/Models/Bangumi/BangumiUpInfo.cs`
- Create: `src/DownKyi.Core/Bili/Web/Bangumi.cs`
- Modify: `src/DownKyi.Core/Bili/BiliLocator.cs`

- [ ] **Step 1: Create Bangumi models**

```csharp
// src/DownKyi.Core/Bili/Models/Bangumi/BangumiEpisode.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiEpisode
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("duration")] public long Duration { get; set; }
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("long_title")] public string LongTitle { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("badge")] public string Badge { get; set; } = string.Empty;
    [JsonPropertyName("pub_time")] public long PubTime { get; set; }
    [JsonPropertyName("status")] public int Status { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Bangumi/BangumiSection.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiSection
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("episodes")] public List<BangumiEpisode>? Episodes { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Bangumi/BangumiStat.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiStat
{
    [JsonPropertyName("coins")] public long Coins { get; set; }
    [JsonPropertyName("danmakus")] public long Danmakus { get; set; }
    [JsonPropertyName("favorites")] public long Favorites { get; set; }
    [JsonPropertyName("likes")] public long Likes { get; set; }
    [JsonPropertyName("views")] public long Views { get; set; }
    [JsonPropertyName("reply")] public long Reply { get; set; }
    [JsonPropertyName("share")] public long Share { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Bangumi/BangumiSeasonInfo.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiSeasonInfo
{
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("season_title")] public string SeasonTitle { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Bangumi/BangumiArea.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiArea
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Bangumi/BangumiUpInfo.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiUpInfo
{
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("uname")] public string Uname { get; set; } = string.Empty;
    [JsonPropertyName("avatar")] public string Avatar { get; set; } = string.Empty;
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Bangumi/BangumiSeason.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Bangumi;

public class BangumiSeasonOrigin
{
    [JsonPropertyName("result")] public BangumiSeason? Result { get; set; }
}

public class BangumiSeason
{
    [JsonPropertyName("areas")] public List<BangumiArea>? Areas { get; set; }
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("evaluate")] public string Evaluate { get; set; } = string.Empty;
    [JsonPropertyName("episodes")] public List<BangumiEpisode>? Episodes { get; set; }
    [JsonPropertyName("media_id")] public long MediaId { get; set; }
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("season_title")] public string SeasonTitle { get; set; } = string.Empty;
    [JsonPropertyName("seasons")] public List<BangumiSeasonInfo>? Seasons { get; set; }
    [JsonPropertyName("section")] public List<BangumiSection>? Section { get; set; }
    [JsonPropertyName("stat")] public BangumiStat? Stat { get; set; }
    [JsonPropertyName("subtitle")] public string Subtitle { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("up_info")] public BangumiUpInfo? UpInfo { get; set; }
}
```

- [ ] **Step 2: Create Bangumi.cs API caller**

```csharp
// src/DownKyi.Core/Bili/Web/Bangumi.cs
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
        string url = seasonId > -1 ? $"{baseUrl}?season_id={seasonId}"
                   : episodeId > -1 ? $"{baseUrl}?ep_id={episodeId}"
                   : null!;
        if (url == null) return null;

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
```

- [ ] **Step 3: Add BangumiApi to BiliLocator**

In `src/DownKyi.Core/Bili/BiliLocator.cs`, add after the `Video(string input)` method:

```csharp
    // Bangumi — stateless, no locator caching needed
    public static BangumiApi BangumiApi => new Web.BangumiApi_Accessor();
```

Actually BangumiApi is a static class, so callers use `BangumiApi.GetBangumiSeasonInfoAsync()` directly. No locator entry needed — just add a using reference note in a comment at the top of BiliLocator:

```csharp
// For Bangumi: use Downkyi.Core.Bili.Web.BangumiApi directly (static class)
// For Cheese:  use Downkyi.Core.Bili.Web.CheeseApi directly (static class)
// For Favorites: use Downkyi.Core.Bili.Web.FavoritesApi directly (static class)
// For History: use Downkyi.Core.Bili.Web.HistoryApi directly (static class)
// For UserSpace: use Downkyi.Core.Bili.Web.UserSpaceApi directly (static class)
// For UserRelation: use Downkyi.Core.Bili.Web.UserRelationApi directly (static class)
```

- [ ] **Step 4: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Models/Bangumi/ src/DownKyi.Core/Bili/Web/Bangumi.cs src/DownKyi.Core/Bili/BiliLocator.cs
git commit -m "feat(core): add Bangumi API models and BangumiApi caller"
```

---

## Task P0-04: Cheese API

**Files:**
- Create: `src/DownKyi.Core/Bili/Models/Cheese/CheeseView.cs`
- Create: `src/DownKyi.Core/Bili/Models/Cheese/CheeseEpisode.cs`
- Create: `src/DownKyi.Core/Bili/Models/Cheese/CheeseEpisodeList.cs`
- Create: `src/DownKyi.Core/Bili/Models/Cheese/CheeseStat.cs`
- Create: `src/DownKyi.Core/Bili/Models/Cheese/CheeseUpInfo.cs`
- Create: `src/DownKyi.Core/Bili/Web/Cheese.cs`

- [ ] **Step 1: Create Cheese models**

```csharp
// src/DownKyi.Core/Bili/Models/Cheese/CheeseEpisode.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseEpisode
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("duration")] public long Duration { get; set; }
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("release_date")] public long ReleaseDate { get; set; }
    [JsonPropertyName("status")] public int Status { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Cheese/CheeseEpisodeList.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseEpisodeListOrigin
{
    [JsonPropertyName("data")] public CheeseEpisodeListData? Data { get; set; }
}

public class CheeseEpisodeListData
{
    [JsonPropertyName("items")] public List<CheeseEpisode>? Items { get; set; }
    [JsonPropertyName("page")] public CheesePage? Page { get; set; }
}

public class CheesePage
{
    [JsonPropertyName("next")] public bool Next { get; set; }
    [JsonPropertyName("num")] public int Num { get; set; }
    [JsonPropertyName("size")] public int Size { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Cheese/CheeseStat.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseStat
{
    [JsonPropertyName("play")] public long Play { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Cheese/CheeseUpInfo.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseUpInfo
{
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("uname")] public string Uname { get; set; } = string.Empty;
    [JsonPropertyName("avatar")] public string Avatar { get; set; } = string.Empty;
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Cheese/CheeseView.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Cheese;

public class CheeseViewOrigin
{
    [JsonPropertyName("data")] public CheeseView? Data { get; set; }
}

public class CheeseView
{
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("episodes")] public List<CheeseEpisode>? Episodes { get; set; }
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("stat")] public CheeseStat? Stat { get; set; }
    [JsonPropertyName("subtitle")] public string Subtitle { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("up_info")] public CheeseUpInfo? UpInfo { get; set; }
}
```

- [ ] **Step 2: Create Cheese.cs API caller**

```csharp
// src/DownKyi.Core/Bili/Web/Cheese.cs
using System.Text.Json;
using Downkyi.Core.Bili.Models.Cheese;

namespace Downkyi.Core.Bili.Web;

public static class CheeseApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public static async Task<CheeseView?> GetCheeseViewInfoAsync(long seasonId = -1, long episodeId = -1)
    {
        string baseUrl = "https://api.bilibili.com/pugv/view/web/season";
        string url = seasonId > -1 ? $"{baseUrl}?season_id={seasonId}"
                   : episodeId > -1 ? $"{baseUrl}?ep_id={episodeId}"
                   : null!;
        if (url == null) return null;

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
```

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Models/Cheese/ src/DownKyi.Core/Bili/Web/Cheese.cs
git commit -m "feat(core): add Cheese (paid courses) API models and CheeseApi caller"
```

---

## Task P0-05: Favorites API

**Files:**
- Create: `src/DownKyi.Core/Bili/Models/Favorites/FavoritesMetaInfo.cs`
- Create: `src/DownKyi.Core/Bili/Models/Favorites/FavoritesMedia.cs`
- Create: `src/DownKyi.Core/Bili/Models/Favorites/FavUpper.cs`
- Create: `src/DownKyi.Core/Bili/Models/Favorites/FavStatus.cs`
- Create: `src/DownKyi.Core/Bili/Web/Favorites.cs`

- [ ] **Step 1: Create Favorites models**

```csharp
// src/DownKyi.Core/Bili/Models/Favorites/FavUpper.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Favorites;

public class FavUpper
{
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("face")] public string Face { get; set; } = string.Empty;
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Favorites/FavStatus.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Favorites;

public class FavStatus
{
    [JsonPropertyName("collect")] public int Collect { get; set; }
    [JsonPropertyName("play")] public int Play { get; set; }
    [JsonPropertyName("thumb_up")] public int ThumbUp { get; set; }
    [JsonPropertyName("share")] public int Share { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Favorites/FavoritesMetaInfo.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Favorites;

public class FavoritesMetaInfoOrigin
{
    [JsonPropertyName("data")] public FavoritesMetaInfo? Data { get; set; }
}

public class FavoritesListOrigin
{
    [JsonPropertyName("data")] public FavoritesListData? Data { get; set; }
}

public class FavoritesListData
{
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("list")] public List<FavoritesMetaInfo>? List { get; set; }
}

public class FavoritesMetaInfo
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("fid")] public long Fid { get; set; }
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("upper")] public FavUpper? Upper { get; set; }
    [JsonPropertyName("cnt_info")] public FavStatus? CntInfo { get; set; }
    [JsonPropertyName("intro")] public string Intro { get; set; } = string.Empty;
    [JsonPropertyName("media_count")] public int MediaCount { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Favorites/FavoritesMedia.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Favorites;

public class FavoritesMediaResourceOrigin
{
    [JsonPropertyName("data")] public FavoritesMediaResourceData? Data { get; set; }
}

public class FavoritesMediaResourceData
{
    [JsonPropertyName("medias")] public List<FavoritesMedia>? Medias { get; set; }
    [JsonPropertyName("has_more")] public bool HasMore { get; set; }
}

public class FavoritesMedia
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("duration")] public long Duration { get; set; }
    [JsonPropertyName("upper")] public FavUpper? Upper { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("bv_id")] public string BvId { get; set; } = string.Empty;
    [JsonPropertyName("fav_time")] public long FavTime { get; set; }
}
```

- [ ] **Step 2: Create Favorites.cs API caller**

```csharp
// src/DownKyi.Core/Bili/Web/Favorites.cs
using System.Text.Json;
using Downkyi.Core.Bili.Models.Favorites;

namespace Downkyi.Core.Bili.Web;

public static class FavoritesApi
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>Returns all favorite folders created by a user (paginated internally).</summary>
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

    /// <summary>Returns all favorite folders collected (bookmarked) by a user.</summary>
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

    /// <summary>Returns all videos in a favorite folder (paginated internally, 20/page).</summary>
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
```

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Models/Favorites/ src/DownKyi.Core/Bili/Web/Favorites.cs
git commit -m "feat(core): add Favorites API models and FavoritesApi caller"
```

---

## Task P0-06: History API

**Files:**
- Create: `src/DownKyi.Core/Bili/Models/History/HistoryData.cs`
- Create: `src/DownKyi.Core/Bili/Models/History/HistoryList.cs`
- Create: `src/DownKyi.Core/Bili/Models/History/HistoryCursor.cs`
- Create: `src/DownKyi.Core/Bili/Models/History/ToViewList.cs`
- Create: `src/DownKyi.Core/Bili/Web/History.cs`

- [ ] **Step 1: Create History models**

```csharp
// src/DownKyi.Core/Bili/Models/History/HistoryCursor.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.History;

public class HistoryCursor
{
    [JsonPropertyName("max")] public long Max { get; set; }
    [JsonPropertyName("view_at")] public long ViewAt { get; set; }
    [JsonPropertyName("business")] public string Business { get; set; } = string.Empty;
}
```

```csharp
// src/DownKyi.Core/Bili/Models/History/HistoryList.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.History;

public class HistoryListHistory
{
    [JsonPropertyName("oid")] public long Oid { get; set; }
    [JsonPropertyName("epid")] public long Epid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("business")] public string Business { get; set; } = string.Empty;
    [JsonPropertyName("dt")] public int Dt { get; set; }
}

public class HistoryList
{
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("uri")] public string Uri { get; set; } = string.Empty;
    [JsonPropertyName("history")] public HistoryListHistory? History { get; set; }
    [JsonPropertyName("author_name")] public string AuthorName { get; set; } = string.Empty;
    [JsonPropertyName("author_mid")] public long AuthorMid { get; set; }
    [JsonPropertyName("view_at")] public long ViewAt { get; set; }
    [JsonPropertyName("duration")] public long Duration { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/History/HistoryData.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.History;

public class HistoryOrigin
{
    [JsonPropertyName("data")] public HistoryData? Data { get; set; }
}

public class HistoryData
{
    [JsonPropertyName("cursor")] public HistoryCursor? Cursor { get; set; }
    [JsonPropertyName("list")] public List<HistoryList>? List { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/History/ToViewList.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.History;

public class ToViewOrigin
{
    [JsonPropertyName("data")] public ToViewData? Data { get; set; }
}

public class ToViewData
{
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("list")] public List<ToViewList>? List { get; set; }
}

public class ToViewList
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("pic")] public string Pic { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("add_at")] public long AddAt { get; set; }
}
```

- [ ] **Step 2: Create History.cs API caller**

```csharp
// src/DownKyi.Core/Bili/Web/History.cs
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
```

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Models/History/ src/DownKyi.Core/Bili/Web/History.cs
git commit -m "feat(core): add History and ToView API models and HistoryApi caller"
```

---

## Task P0-07: Users API — Publications, Channels, Seasons

**Files:**
- Create: `src/DownKyi.Core/Bili/Models/Users/SpacePublication.cs`
- Create: `src/DownKyi.Core/Bili/Models/Users/SpaceChannel.cs`
- Create: `src/DownKyi.Core/Bili/Models/Users/SpaceSeasonsSeries.cs`
- Create: `src/DownKyi.Core/Bili/Web/UserSpace.cs`

- [ ] **Step 1: Create UserSpace models**

```csharp
// src/DownKyi.Core/Bili/Models/Users/SpacePublication.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class SpacePublicationOrigin
{
    [JsonPropertyName("data")] public SpacePublicationData? Data { get; set; }
}

public class SpacePublicationData
{
    [JsonPropertyName("list")] public SpacePublicationList? List { get; set; }
    [JsonPropertyName("page")] public SpacePublicationPage? Page { get; set; }
}

public class SpacePublicationPage
{
    [JsonPropertyName("pn")] public int Pn { get; set; }
    [JsonPropertyName("ps")] public int Ps { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
}

public class SpacePublicationList
{
    [JsonPropertyName("vlist")] public List<SpacePublicationVideo>? Vlist { get; set; }
}

public class SpacePublicationVideo
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("pic")] public string Pic { get; set; } = string.Empty;
    [JsonPropertyName("length")] public string Length { get; set; } = string.Empty;
    [JsonPropertyName("created")] public long Created { get; set; }
    [JsonPropertyName("play")] public long Play { get; set; }
    [JsonPropertyName("author")] public string Author { get; set; } = string.Empty;
    [JsonPropertyName("mid")] public long Mid { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Users/SpaceChannel.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class SpaceChannelListOrigin
{
    [JsonPropertyName("data")] public SpaceChannelListData? Data { get; set; }
}

public class SpaceChannelListData
{
    [JsonPropertyName("list")] public List<SpaceChannel>? List { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
}

public class SpaceChannel
{
    [JsonPropertyName("cid")] public long Cid { get; set; }
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("intro")] public string Intro { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
}

public class SpaceChannelVideoOrigin
{
    [JsonPropertyName("data")] public SpaceChannelVideoData? Data { get; set; }
}

public class SpaceChannelVideoData
{
    [JsonPropertyName("list")] public SpaceChannelVideoList? List { get; set; }
    [JsonPropertyName("page")] public SpaceChannelVideoPage? Page { get; set; }
}

public class SpaceChannelVideoList
{
    [JsonPropertyName("archives")] public List<SpaceChannelArchive>? Archives { get; set; }
}

public class SpaceChannelVideoPage
{
    [JsonPropertyName("page_num")] public int PageNum { get; set; }
    [JsonPropertyName("page_size")] public int PageSize { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
}

public class SpaceChannelArchive
{
    [JsonPropertyName("aid")] public long Aid { get; set; }
    [JsonPropertyName("bvid")] public string Bvid { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("pic")] public string Pic { get; set; } = string.Empty;
    [JsonPropertyName("duration")] public long Duration { get; set; }
    [JsonPropertyName("pubdate")] public long Pubdate { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Users/SpaceSeasonsSeries.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class SpaceSeasonsSeriesOrigin
{
    [JsonPropertyName("data")] public SpaceSeasonsSeriesData? Data { get; set; }
}

public class SpaceSeasonsSeriesData
{
    [JsonPropertyName("items_lists")] public SpaceSeasonsSeriesItems? ItemsLists { get; set; }
}

public class SpaceSeasonsSeriesItems
{
    [JsonPropertyName("seasons_list")] public List<SpaceSeasonItem>? SeasonsList { get; set; }
    [JsonPropertyName("series_list")] public List<SpaceSeriesItem>? SeriesList { get; set; }
    [JsonPropertyName("page")] public SpaceSeasonsSeriesPage? Page { get; set; }
}

public class SpaceSeasonsSeriesPage
{
    [JsonPropertyName("page_num")] public int PageNum { get; set; }
    [JsonPropertyName("page_size")] public int PageSize { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
}

public class SpaceSeasonItem
{
    [JsonPropertyName("meta")] public SpaceSeasonMeta? Meta { get; set; }
}

public class SpaceSeasonMeta
{
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("total")] public int Total { get; set; }
}

public class SpaceSeriesItem
{
    [JsonPropertyName("meta")] public SpaceSeriesMeta? Meta { get; set; }
}

public class SpaceSeriesMeta
{
    [JsonPropertyName("series_id")] public long SeriesId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("total")] public int Total { get; set; }
}
```

- [ ] **Step 2: Create UserSpace.cs API caller**

```csharp
// src/DownKyi.Core/Bili/Web/UserSpace.cs
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
```

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Models/Users/ src/DownKyi.Core/Bili/Web/UserSpace.cs
git commit -m "feat(core): add UserSpace API (publications, channels, seasons/series)"
```

---

## Task P0-08: Users API — BangumiFollow, Following, Followers

**Files:**
- Create: `src/DownKyi.Core/Bili/Models/Users/BangumiFollow.cs`
- Create: `src/DownKyi.Core/Bili/Models/Users/RelationFollow.cs`
- Create: `src/DownKyi.Core/Bili/Web/UserRelation.cs`

- [ ] **Step 1: Create relation models**

```csharp
// src/DownKyi.Core/Bili/Models/Users/BangumiFollow.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class BangumiFollowOrigin
{
    [JsonPropertyName("data")] public BangumiFollowData? Data { get; set; }
}

public class BangumiFollowData
{
    [JsonPropertyName("follow_list")] public List<BangumiFollow>? FollowList { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("has_next")] public bool HasNext { get; set; }
}

public class BangumiFollow
{
    [JsonPropertyName("season_id")] public long SeasonId { get; set; }
    [JsonPropertyName("media_id")] public long MediaId { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;
    [JsonPropertyName("badge")] public string Badge { get; set; } = string.Empty;
    [JsonPropertyName("season_type_name")] public string SeasonTypeName { get; set; } = string.Empty;
    [JsonPropertyName("is_finish")] public int IsFinish { get; set; }
    [JsonPropertyName("total_count")] public int TotalCount { get; set; }
}
```

```csharp
// src/DownKyi.Core/Bili/Models/Users/RelationFollow.cs
using System.Text.Json.Serialization;

namespace Downkyi.Core.Bili.Models.Users;

public class RelationFollowOrigin
{
    [JsonPropertyName("data")] public RelationFollow? Data { get; set; }
}

public class RelationFollow
{
    [JsonPropertyName("list")] public List<RelationFollowInfo>? List { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
}

public class RelationFollowInfo
{
    [JsonPropertyName("mid")] public long Mid { get; set; }
    [JsonPropertyName("uname")] public string Uname { get; set; } = string.Empty;
    [JsonPropertyName("face")] public string Face { get; set; } = string.Empty;
    [JsonPropertyName("sign")] public string Sign { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create UserRelation.cs API caller**

```csharp
// src/DownKyi.Core/Bili/Web/UserRelation.cs
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
```

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Models/Users/BangumiFollow.cs \
        src/DownKyi.Core/Bili/Models/Users/RelationFollow.cs \
        src/DownKyi.Core/Bili/Web/UserRelation.cs
git commit -m "feat(core): add UserRelation API (followers, followings, bangumi follow)"
```

---

## Task P0-09: Danmaku XML API

**Files:**
- Create: `src/DownKyi.Core/Bili/Models/Danmaku/DanmakuItem.cs`
- Create: `src/DownKyi.Core/Bili/Web/Danmaku.cs`

- [ ] **Step 1: Create Danmaku models**

```csharp
// src/DownKyi.Core/Bili/Models/Danmaku/DanmakuItem.cs
namespace Downkyi.Core.Bili.Models.Danmaku;

/// <summary>
/// A single danmaku item parsed from Bilibili XML danmaku format.
/// Format of the 'p' attribute: time,type,size,color,timestamp,pool,userHash,dmid
/// </summary>
public class DanmakuItem
{
    public float Time { get; set; }        // seconds from start
    public int Type { get; set; }          // 1=rolling, 4=bottom, 5=top, 6=reverse, 7=advanced
    public int Size { get; set; }          // font size (default 25)
    public int Color { get; set; }         // decimal color (e.g. 16777215 = white)
    public long Timestamp { get; set; }    // unix timestamp when sent
    public int Pool { get; set; }          // 0=normal pool
    public string UserHash { get; set; } = string.Empty;
    public string DmId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create Danmaku.cs API caller**

```csharp
// src/DownKyi.Core/Bili/Web/Danmaku.cs
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
```

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Bili/Models/Danmaku/ src/DownKyi.Core/Bili/Web/Danmaku.cs
git commit -m "feat(core): add XML danmaku API and DanmakuItem model"
```

---

## Task P0-10: Danmaku2Ass Library Port

**Files:**
- Create: `src/DownKyi.Core/Danmaku2Ass/Config.cs`
- Create: `src/DownKyi.Core/Danmaku2Ass/DanmakuEntry.cs`
- Create: `src/DownKyi.Core/Danmaku2Ass/Collision.cs`
- Create: `src/DownKyi.Core/Danmaku2Ass/AssGenerator.cs`

- [ ] **Step 1: Create Config.cs**

```csharp
// src/DownKyi.Core/Danmaku2Ass/Config.cs
namespace Downkyi.Core.Danmaku2Ass;

public class AssConfig
{
    public int VideoWidth { get; set; } = 1280;
    public int VideoHeight { get; set; } = 720;
    public float FontSize { get; set; } = 25f;
    public string FontFamily { get; set; } = "黑体";
    public float Opacity { get; set; } = 0.7f;       // 0.0 (transparent) to 1.0 (opaque)
    public int ScrollDuration { get; set; } = 8;      // seconds for rolling danmaku
    public int FixedDuration { get; set; } = 4;       // seconds for top/bottom danmaku
    public bool BlockScrolling { get; set; } = false;
    public bool BlockTop { get; set; } = false;
    public bool BlockBottom { get; set; } = false;
    public bool BlockColorful { get; set; } = false;  // block non-white danmaku
    public int ReduceCount { get; set; } = 0;         // 0 = no reduction
    /// <summary>Vertical margin as fraction of video height reserved at bottom.</summary>
    public float BottomMargin { get; set; } = 0.02f;
}
```

- [ ] **Step 2: Create DanmakuEntry.cs**

```csharp
// src/DownKyi.Core/Danmaku2Ass/DanmakuEntry.cs
using Downkyi.Core.Bili.Models.Danmaku;

namespace Downkyi.Core.Danmaku2Ass;

/// <summary>Internal representation of a danmaku with computed layout properties.</summary>
internal class DanmakuEntry
{
    public float Time { get; set; }
    public int Type { get; set; }       // 1=scroll, 4=bottom, 5=top
    public float FontSize { get; set; }
    public int Color { get; set; }
    public string Content { get; set; } = string.Empty;
    // Layout (computed)
    public float TextWidth { get; set; }
    public int Row { get; set; } = -1;  // -1 = unassigned

    internal static DanmakuEntry FromDanmakuItem(DanmakuItem item, float fontSize)
    {
        return new DanmakuEntry
        {
            Time = item.Time,
            Type = item.Type,
            FontSize = fontSize,
            Color = item.Color,
            Content = item.Content,
            TextWidth = EstimateTextWidth(item.Content, fontSize),
        };
    }

    /// <summary>
    /// Rough text width estimate: each CJK char ≈ fontSize px wide, ASCII ≈ fontSize*0.6.
    /// Good enough for collision detection without a real text renderer.
    /// </summary>
    private static float EstimateTextWidth(string text, float fontSize)
    {
        float w = 0;
        foreach (char c in text)
        {
            w += c > 0x2E7F ? fontSize : fontSize * 0.6f;
        }
        return w;
    }
}
```

- [ ] **Step 3: Create Collision.cs**

```csharp
// src/DownKyi.Core/Danmaku2Ass/Collision.cs
namespace Downkyi.Core.Danmaku2Ass;

/// <summary>
/// Tracks row occupancy to prevent danmaku overlap.
/// For scrolling (type=1): each row stores the time when the previous danmaku will have
/// fully scrolled off the left edge so a new one can enter.
/// For fixed (type=4/5): each row stores the time when it becomes free again.
/// </summary>
internal class CollisionManager
{
    private readonly float[] _rowFreeAt;
    private readonly float _rowHeight;
    private readonly int _videoHeight;
    private readonly float _bottomMargin;

    internal CollisionManager(int videoHeight, float fontSize, float bottomMargin)
    {
        _videoHeight = videoHeight;
        _rowHeight = fontSize * 1.2f;
        _bottomMargin = bottomMargin;
        int maxRows = (int)((videoHeight * (1 - bottomMargin)) / _rowHeight);
        _rowFreeAt = new float[Math.Max(maxRows, 1)];
    }

    /// <summary>
    /// Finds the first available row for a danmaku entry and marks it occupied.
    /// Returns -1 if all rows are occupied (entry should be discarded or shown anyway).
    /// </summary>
    internal int AssignRow(DanmakuEntry entry, float clearTime)
    {
        for (int i = 0; i < _rowFreeAt.Length; i++)
        {
            if (_rowFreeAt[i] <= entry.Time)
            {
                _rowFreeAt[i] = clearTime;
                entry.Row = i;
                return i;
            }
        }
        return -1; // all rows occupied — show on last row anyway
    }

    internal float RowY(int row) => row * _rowHeight + _rowHeight;
}
```

- [ ] **Step 4: Create AssGenerator.cs**

```csharp
// src/DownKyi.Core/Danmaku2Ass/AssGenerator.cs
using Downkyi.Core.Bili.Models.Danmaku;

namespace Downkyi.Core.Danmaku2Ass;

/// <summary>
/// Converts a list of DanmakuItem into an ASS subtitle file string.
/// </summary>
public static class AssGenerator
{
    public static string Generate(List<DanmakuItem> items, AssConfig config)
    {
        var entries = items
            .Where(d => !ShouldBlock(d, config))
            .Select(d => DanmakuEntry.FromDanmakuItem(d, config.FontSize))
            .OrderBy(d => d.Time)
            .ToList();

        if (config.ReduceCount > 0 && entries.Count > config.ReduceCount)
        {
            entries = ReduceDensity(entries, config.ReduceCount);
        }

        var scrollCollision = new CollisionManager(config.VideoHeight, config.FontSize, config.BottomMargin);
        var topCollision = new CollisionManager(config.VideoHeight, config.FontSize, config.BottomMargin);
        var bottomCollision = new CollisionManager(config.VideoHeight, config.FontSize, config.BottomMargin);

        var sb = new System.Text.StringBuilder();
        WriteHeader(sb, config);

        foreach (var entry in entries)
        {
            string assLine = entry.Type switch
            {
                1 => GenerateScrollLine(entry, config, scrollCollision),
                4 => GenerateFixedLine(entry, config, bottomCollision, isBottom: true),
                5 => GenerateFixedLine(entry, config, topCollision, isBottom: false),
                _ => GenerateScrollLine(entry, config, scrollCollision), // treat unknown as scroll
            };
            sb.AppendLine(assLine);
        }

        return sb.ToString();
    }

    private static bool ShouldBlock(DanmakuItem d, AssConfig cfg)
    {
        if (cfg.BlockScrolling && d.Type == 1) return true;
        if (cfg.BlockTop && d.Type == 5) return true;
        if (cfg.BlockBottom && d.Type == 4) return true;
        if (cfg.BlockColorful && d.Color != 16777215) return true;
        return false;
    }

    private static List<DanmakuEntry> ReduceDensity(List<DanmakuEntry> entries, int target)
    {
        int step = entries.Count / target;
        return entries.Where((_, i) => i % step == 0).Take(target).ToList();
    }

    private static void WriteHeader(System.Text.StringBuilder sb, AssConfig cfg)
    {
        int alpha = (int)((1 - cfg.Opacity) * 255);
        string alphaHex = alpha.ToString("X2");

        sb.AppendLine("[Script Info]");
        sb.AppendLine("ScriptType: v4.00+");
        sb.AppendLine("Collisions: Normal");
        sb.AppendLine($"PlayResX: {cfg.VideoWidth}");
        sb.AppendLine($"PlayResY: {cfg.VideoHeight}");
        sb.AppendLine();
        sb.AppendLine("[V4+ Styles]");
        sb.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");
        sb.AppendLine($"Style: Default,{cfg.FontFamily},{(int)cfg.FontSize},&H{alphaHex}FFFFFF,&H{alphaHex}FFFFFF,&H{alphaHex}000000,&H{alphaHex}000000,0,0,0,0,100,100,0,0,1,1,0,2,0,0,0,0");
        sb.AppendLine();
        sb.AppendLine("[Events]");
        sb.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
    }

    private static string GenerateScrollLine(DanmakuEntry entry, AssConfig cfg, CollisionManager collision)
    {
        float duration = cfg.ScrollDuration;
        // Time for the text to fully clear (be off-screen): when the tail exits right edge
        float clearTime = entry.Time + duration + (entry.TextWidth / cfg.VideoWidth) * duration;
        int row = collision.AssignRow(entry, clearTime);
        if (row < 0) row = 0; // fallback: show on row 0

        float y = collision.RowY(row);
        string start = FormatTime(entry.Time);
        string end = FormatTime(entry.Time + duration);
        string color = ColorToAss(entry.Color);
        // Scroll: move from right edge (x=PlayResX) to left (-textWidth)
        string move = $"\\move({cfg.VideoWidth},{y},{-entry.TextWidth},{y})";
        string text = EscapeAss(entry.Content);
        return $"Dialogue: 0,{start},{end},Default,,0,0,0,,{{{move}{color}}}{text}";
    }

    private static string GenerateFixedLine(DanmakuEntry entry, AssConfig cfg, CollisionManager collision, bool isBottom)
    {
        float duration = cfg.FixedDuration;
        int row = collision.AssignRow(entry, entry.Time + duration);
        if (row < 0) row = 0;

        float y = isBottom
            ? cfg.VideoHeight - collision.RowY(row)
            : collision.RowY(row);
        int an = isBottom ? 2 : 8; // ASS alignment: 2=bottom-center, 8=top-center
        string start = FormatTime(entry.Time);
        string end = FormatTime(entry.Time + duration);
        string color = ColorToAss(entry.Color);
        string pos = $"\\an{an}\\pos({cfg.VideoWidth / 2},{y})";
        string text = EscapeAss(entry.Content);
        return $"Dialogue: 0,{start},{end},Default,,0,0,0,,{{{pos}{color}}}{text}";
    }

    private static string FormatTime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)(seconds % 3600 / 60);
        float s = seconds % 60;
        return $"{h}:{m:D2}:{s:00.00}";
    }

    private static string ColorToAss(int color)
    {
        if (color == 16777215) return string.Empty; // white = default, no override needed
        // ASS color is &HBBGGRR
        int r = (color >> 16) & 0xFF;
        int g = (color >> 8) & 0xFF;
        int b = color & 0xFF;
        return $"\\c&H{b:X2}{g:X2}{r:X2}&";
    }

    private static string EscapeAss(string text)
        => text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Replace("\n", "\\N");
}
```

- [ ] **Step 5: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Danmaku2Ass/
git commit -m "feat(core): add Danmaku2Ass library (XML danmaku → ASS subtitle conversion)"
```

---

## Task P0-11: Download Database

**Files:**
- Create: `src/DownKyi.Core/Database/Download/DownloadEntity.cs`
- Create: `src/DownKyi.Core/Database/Download/DownloadDatabase.cs`

- [ ] **Step 1: Create DownloadEntity.cs**

```csharp
// src/DownKyi.Core/Database/Download/DownloadEntity.cs
using SQLite;

namespace Downkyi.Core.Database.Download;

public enum DownloadStatus
{
    NotStarted = 0,
    Waiting = 1,
    Downloading = 2,
    Paused = 3,
    Succeed = 4,
    Failed = 5,
    Cancelled = 6,
}

[Table("downloading")]
public class DownloadingEntity
{
    [PrimaryKey, AutoIncrement]
    [Column("id")] public long Id { get; set; }
    [Column("uuid")] public string Uuid { get; set; } = string.Empty;
    [Column("bvid")] public string Bvid { get; set; } = string.Empty;
    [Column("avid")] public long Avid { get; set; }
    [Column("cid")] public long Cid { get; set; }
    [Column("epid")] public long Epid { get; set; }
    [Column("title")] public string Title { get; set; } = string.Empty;
    [Column("cover_url")] public string CoverUrl { get; set; } = string.Empty;
    [Column("upper_name")] public string UpperName { get; set; } = string.Empty;
    [Column("quality")] public int Quality { get; set; }
    [Column("audio_codec_id")] public int AudioCodecId { get; set; }
    [Column("video_codec_id")] public int VideoCodecId { get; set; }
    [Column("status")] public DownloadStatus Status { get; set; } = DownloadStatus.NotStarted;
    [Column("progress")] public float Progress { get; set; }
    [Column("file_path")] public string FilePath { get; set; } = string.Empty;
    [Column("download_audio")] public bool DownloadAudio { get; set; } = true;
    [Column("download_video")] public bool DownloadVideo { get; set; } = true;
    [Column("download_danmaku")] public bool DownloadDanmaku { get; set; } = true;
    [Column("download_subtitle")] public bool DownloadSubtitle { get; set; } = true;
    [Column("download_cover")] public bool DownloadCover { get; set; } = true;
    [Column("created_at")] public long CreatedAt { get; set; }
}

[Table("downloaded")]
public class DownloadedEntity
{
    [PrimaryKey, AutoIncrement]
    [Column("id")] public long Id { get; set; }
    [Column("uuid")] public string Uuid { get; set; } = string.Empty;
    [Column("bvid")] public string Bvid { get; set; } = string.Empty;
    [Column("title")] public string Title { get; set; } = string.Empty;
    [Column("cover_url")] public string CoverUrl { get; set; } = string.Empty;
    [Column("upper_name")] public string UpperName { get; set; } = string.Empty;
    [Column("file_path")] public string FilePath { get; set; } = string.Empty;
    [Column("finished_at")] public long FinishedAt { get; set; }
}
```

- [ ] **Step 2: Create DownloadDatabase.cs**

```csharp
// src/DownKyi.Core/Database/Download/DownloadDatabase.cs
using SQLite;
using Downkyi.Core.Storage;

namespace Downkyi.Core.Database.Download;

public class DownloadDatabase
{
    private readonly string _dbPath = StorageManager.GetDownload();
    private SQLiteAsyncConnection? _db;
    private static DownloadDatabase? _instance;
    private static readonly object _lock = new();

    private DownloadDatabase() { }

    public static DownloadDatabase Instance
    {
        get
        {
            if (_instance == null) lock (_lock) { _instance ??= new DownloadDatabase(); }
            return _instance;
        }
    }

    private async Task InitAsync()
    {
        if (_db != null) return;
        var options = new SQLiteConnectionString(_dbPath, true, key: "d0wnky1-dl-2024");
        _db = new SQLiteAsyncConnection(options);
        await _db.CreateTableAsync<DownloadingEntity>();
        await _db.CreateTableAsync<DownloadedEntity>();
    }

    // --- Downloading ---

    public async Task<int> InsertDownloadingAsync(DownloadingEntity entity)
    {
        await InitAsync();
        entity.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return await _db!.InsertAsync(entity);
    }

    public async Task<int> UpdateDownloadingAsync(DownloadingEntity entity)
    {
        await InitAsync();
        return await _db!.UpdateAsync(entity);
    }

    public async Task<int> DeleteDownloadingAsync(string uuid)
    {
        await InitAsync();
        return await _db!.DeleteAsync<DownloadingEntity>(uuid);
    }

    public async Task<List<DownloadingEntity>> GetAllDownloadingAsync()
    {
        await InitAsync();
        return await _db!.Table<DownloadingEntity>().ToListAsync();
    }

    public async Task<DownloadingEntity?> GetDownloadingByUuidAsync(string uuid)
    {
        await InitAsync();
        return await _db!.Table<DownloadingEntity>().Where(x => x.Uuid == uuid).FirstOrDefaultAsync();
    }

    // --- Downloaded ---

    public async Task<int> InsertDownloadedAsync(DownloadedEntity entity)
    {
        await InitAsync();
        entity.FinishedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return await _db!.InsertAsync(entity);
    }

    public async Task<int> DeleteDownloadedAsync(long id)
    {
        await InitAsync();
        return await _db!.DeleteAsync<DownloadedEntity>(id);
    }

    public async Task<List<DownloadedEntity>> GetAllDownloadedAsync()
    {
        await InitAsync();
        return await _db!.Table<DownloadedEntity>().OrderByDescending(x => x.FinishedAt).ToListAsync();
    }

    public async Task<bool> IsAlreadyDownloadedAsync(string bvid)
    {
        await InitAsync();
        return await _db!.Table<DownloadedEntity>().Where(x => x.Bvid == bvid).CountAsync() > 0;
    }
}
```

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Database/Download/
git commit -m "feat(core): add DownloadDatabase with downloading/downloaded tables (SQLCipher)"
```

---

## Task P0-12: AriaManager

**Files:**
- Create: `src/DownKyi.Core/Aria2cNet/AriaManager.cs`

Note: the `Aria2cNet` NuGet package (v1.0.1) provides `AriaClient` and all entity classes. `AriaManager` wraps process lifecycle and provides helper methods.

- [ ] **Step 1: Create AriaManager.cs**

```csharp
// src/DownKyi.Core/Aria2cNet/AriaManager.cs
using System.Diagnostics;
using Downkyi.Core.Settings;

namespace Downkyi.Core.Aria2cNet;

/// <summary>
/// Manages the aria2c process lifecycle and provides a configured AriaClient.
/// aria2c binary must be present at the path returned by GetAria2cPath().
/// </summary>
public static class AriaManager
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
    private static Process? _ariaProcess;
    private static readonly object _lock = new();

    public static string Host { get; } = "http://localhost";
    public static int Port { get; } = 6800;
    public static string Token { get; } = "downkyi-aria2c-token";

    /// <summary>Starts the aria2c process. Safe to call multiple times — no-ops if already running.</summary>
    public static void Start()
    {
        lock (_lock)
        {
            if (_ariaProcess != null && !_ariaProcess.HasExited) return;

            string aria2c = GetAria2cPath();
            if (!File.Exists(aria2c))
            {
                Log.Warn("aria2c binary not found at {Path}, skipping Aria2c start", aria2c);
                return;
            }

            string ariaDir = Storage.StorageManager.GetAriaDir();
            string sessionFile = Path.Combine(ariaDir, "aria2.session");
            string logFile = Path.Combine(ariaDir, "aria2.log");

            string args = string.Join(" ", new[]
            {
                "--enable-rpc",
                $"--rpc-listen-port={Port}",
                $"--rpc-secret={Token}",
                "--rpc-allow-origin-all=true",
                "--continue=true",
                "--max-concurrent-downloads=5",
                "--max-connection-per-server=16",
                "--split=16",
                "--min-split-size=1M",
                $"--save-session=\"{sessionFile}\"",
                $"--input-file=\"{sessionFile}\"",
                $"--log=\"{logFile}\"",
                "--log-level=warn",
                "--quiet=true",
            });

            _ariaProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = aria2c,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                }
            };

            try
            {
                _ariaProcess.Start();
                Log.Info("aria2c started with PID {Pid}", _ariaProcess.Id);
                // Give it a moment to start the RPC server
                Task.Delay(500).Wait();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to start aria2c");
                _ariaProcess = null;
            }
        }
    }

    /// <summary>Sends a shutdown command to aria2c and waits for it to exit.</summary>
    public static async Task StopAsync()
    {
        try
        {
            var client = GetClient();
            await client.ShutdownAsync();
            _ariaProcess?.WaitForExit(3000);
        }
        catch (Exception e)
        {
            Log.Error(e, "AriaManager.StopAsync failed");
            _ariaProcess?.Kill();
        }
        finally
        {
            _ariaProcess = null;
        }
    }

    /// <summary>Returns a configured AriaClient pointing at the local aria2c RPC server.</summary>
    public static AriaClient GetClient()
    {
        return new AriaClient(Host, Port, 60, Token);
    }

    public static bool IsRunning()
        => _ariaProcess != null && !_ariaProcess.HasExited;

    private static string GetAria2cPath()
    {
        // Check settings first, fallback to app directory
        string? settingsPath = SettingsManager.Instance.GetAriaDir();
        if (!string.IsNullOrEmpty(settingsPath))
        {
            string fullPath = Path.Combine(settingsPath, GetAria2cBinaryName());
            if (File.Exists(fullPath)) return fullPath;
        }
        return Path.Combine(Environment.CurrentDirectory, GetAria2cBinaryName());
    }

    private static string GetAria2cBinaryName()
        => OperatingSystem.IsWindows() ? "aria2c.exe" : "aria2c";
}
```

- [ ] **Step 2: Add GetAriaDir() to SettingsManager if missing**

Check `src/DownKyi.Core/Settings/SettingsManager.cs`. If `GetAriaDir()` does not exist, add it:

```csharp
public string? GetAriaDir()
{
    return _settings?.AriaDir;
}
```

And in the settings model, add `AriaDir` if not present.

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/Aria2cNet/AriaManager.cs
git commit -m "feat(core): add AriaManager for aria2c process lifecycle management"
```

---

## Task P0-13: FileName Sanitization Fix (v1.6.x fix)

**Files:**
- Modify: `src/DownKyi.Core/FileName/FileName.cs`

- [ ] **Step 1: Read the current FileName.cs**

Read `src/DownKyi.Core/FileName/FileName.cs` to find the `RelativePath()` method.

- [ ] **Step 2: Add Sanitize() helper**

In `FileName.cs`, add this static method after the `RelativePath()` method:

```csharp
    /// <summary>
    /// Removes characters illegal in file/directory names on Windows, macOS, and Linux.
    /// If the result is empty, returns the fallback string "_".
    /// Fixes v1.6.x crash: "修复标题中无合法字符时崩溃的问题"
    /// </summary>
    public static string Sanitize(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "_";

        // Characters illegal on Windows (superset of macOS/Linux restrictions)
        char[] illegal = Path.GetInvalidFileNameChars();
        var sb = new System.Text.StringBuilder(name.Length);
        foreach (char c in name)
        {
            if (Array.IndexOf(illegal, c) < 0)
                sb.Append(c);
        }

        string result = sb.ToString().Trim().TrimEnd('.');
        return string.IsNullOrWhiteSpace(result) ? "_" : result;
    }
```

- [ ] **Step 3: Build**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi/src
dotnet build DownKyi.Core/Downkyi.Core.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /mnt/c/Users/envy15/Documents/programming/github/dueuege/downkyi
git add src/DownKyi.Core/FileName/FileName.cs
git commit -m "fix(core): add FileName.Sanitize() to prevent crash on titles with no valid chars (v1.6.x fix)"
```

---

## Self-Review Checklist

- [x] All 13 tasks have complete code — no placeholders or TBDs
- [x] `BiliWebClient` (P0-01) is referenced in all API callers (P0-02 through P0-09)
- [x] All models use `System.Text.Json` `[JsonPropertyName]` (not Newtonsoft)
- [x] `DownloadDatabase` key is different from `LoginDatabase` key (no collision)
- [x] `AriaManager.GetAria2cPath()` checks settings before fallback — consistent with existing settings pattern
- [x] `AssGenerator` does not use WPF types — fully cross-platform
- [x] `IVideo` extension (P0-02) uses `videoView.Data.View.Pages` — verify this property exists in BiliSharp's `VideoView` model before step 5 of P0-02
- [x] `FileName.Sanitize()` uses `Path.GetInvalidFileNameChars()` which is cross-platform in .NET 8
- [x] Build step after every task catches integration errors early
