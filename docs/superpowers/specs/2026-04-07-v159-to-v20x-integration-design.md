# DownKyi v2.0.x — Integration Design Spec

> Generated: 2026-04-07
> Branch: `v2.0.x`
> Reference: `origin/v1.5.x` (v1.5.9 + v1.6.0 features)
> Supersedes: `DEVELOPMENT_PLAN_V2.0.x.md`

---

## 1. Overview

`v2.0.x` is a complete rewrite of DownKyi, replacing WPF/.NET Framework 4.7.2 with **Avalonia UI + .NET 8**. This spec defines the full integration of all v1.5.9/v1.6.x functionality into v2.0.x, plus improvements in architecture, cross-platform support, and security.

**Scope:** All features present in `origin/v1.5.x` (which includes both v1.5.9 and v1.6.0). Where v1.6.x improved on v1.5.9 (WBI signing, reduced API calls, title sanitization), v2.0.x adopts the improved version.

**Current completion: ~25–30%** (accounting for unported Core modules not visible in the original 42-task estimate).

**Revised total tasks: ~60** (was 42; gaps identified in analysis below).

---

## 2. Architecture & Porting Strategy

### Three-layer mapping

| v1.5.x location | v2.0.x location | Rule |
|---|---|---|
| `DownKyi.Core/BiliApi/**` | `DownKyi.Core/Bili/Web/**` | Use BiliSharp + new Web layer; port models and logic |
| `DownKyi.Core/Aria2cNet/**` | `DownKyi.Core/Aria2cNet/**` | Direct port; fix .NET 8 deprecated APIs |
| `DownKyi.Core/Danmaku2Ass/**` | `DownKyi.Core/Danmaku2Ass/**` | Direct port; replace BitmapSource with SkiaSharp if needed |
| `DownKyi.Core/Storage/**` | `DownKyi.Core/Storage/**` | Port DB layer; v2.0.x uses SQLCipher (encrypted) |
| `DownKyi.Core/FFmpeg/**` | `DownKyi.Core/FFmpeg/**` | Direct port; update process invocation for cross-platform |
| `DownKyi.Core/FileName/**` | `DownKyi.Core/FileName/**` | Port filename template/parts system |
| `DownKyi/Services/**` | `Downkyi.UI/Services/**` | Port removing Prism events; use DI + CommunityToolkit |
| `DownKyi/ViewModels/**` | `Downkyi.UI/ViewModels/**` | Replace `SetProperty` → `[ObservableProperty]`; replace `DelegateCommand` → `[RelayCommand]` |
| `DownKyi/Views/**` | `DownKyi/Views/**` | Avalonia AXAML; shells exist, wire bindings |

### Key porting rules

1. **Never copy WPF/Prism patterns.** Replace `IEventAggregator` with direct method calls or `WeakReferenceMessenger` (CommunityToolkit). Replace `IDialogService` with a new `IDialogService` abstraction backed by Avalonia dialogs.
2. **Logic from v1.5.x, architecture from v2.0.x.** Algorithm and API call code is reused; MVVM scaffolding is rewritten.
3. **v1.6.x improvements are adopted.** WBI signing, reduced parse API calls, title sanitization, re-download prompt.
4. **Cross-platform first.** Any Windows-only API (clipboard hooks, process launch) must have a cross-platform equivalent or conditional compile guard.

---

## 3. Delivery: 5 Vertical Feature Slices

Tasks are ordered so each slice produces a testable, runnable increment.

```
Slice 0 (P0) — Core API Modules          prerequisite for everything
Slice 1       — MVP Download              paste URL → download one video
Slice 2       — Full Download Engine      aria2, ffmpeg, persistence, post-op
Slice 3       — Full Video Detail         danmaku, subtitle, cover, batch
Slice 4       — User Space                space, favorites, history, friends
Slice 5       — Settings + Polish         proxy, update check, filename, UX
```

---

## 4. Slice 0 — Core API Modules (NEW — not in original plan)

These are pure `DownKyi.Core` tasks with no UI dependency. They unblock all later slices.

### P0-01 — Port VideoStream models and `GetPlayUrl()`
- **Reference:** `src/DownKyi.Core/BiliApi/VideoStream/Models/` (12 files: PlayUrl, PlayUrlDash, PlayUrlDashVideo, PlayUrlDashDolby, PlayUrlDashFlac, PlayUrlDurl, PlayUrlSupportFormat, PlayerV2, Subtitle, SubtitleInfo)
- **Target:** `DownKyi.Core/Bili/Models/` + extend `IVideo` / `Video.cs`
- **What:** Add `GetPlayUrl(long cid, int qn, bool dash)` to `IVideo`. Add all stream models. Include Dolby Atmos (id=30250) and Hi-Res FLAC (id=30251) support.

### P0-02 — Port Bangumi API (`BiliApi/Bangumi/`)
- **Reference:** 9 model files — BangumiInfo, BangumiType, BangumiSeason, BangumiEpisode, BangumiSeasonInfo, BangumiSection, BangumiStat, BangumiArea
- **Target:** `DownKyi.Core/Bili/Web/Bangumi.cs` + `DownKyi.Core/Bili/Models/Bangumi/`
- **What:** `GetBangumiInfo(string seasonId)`, `GetBangumiEpisodes(string seasonId)` — used by `BangumiInfoService` in UI layer.

### P0-03 — Port Cheese API (`BiliApi/Cheese/`)
- **Reference:** 8 model files — CheeseInfo, CheeseView, CheeseEpisode, CheeseEpisodeList, CheeseStat, CheeseUpInfo
- **Target:** `DownKyi.Core/Bili/Web/Cheese.cs` + `DownKyi.Core/Bili/Models/Cheese/`
- **What:** `GetCheeseInfo(string seasonId)`, `GetCheeseEpisodes(string seasonId)`.

### P0-04 — Port Favorites API (`BiliApi/Favorites/`)
- **Reference:** FavoritesInfo, FavoritesResource, FavoritesList, FavoritesMedia, FavoritesMetaInfo, MediaStatus, FavStatus, FavUpper
- **Target:** `DownKyi.Core/Bili/Web/Favorites.cs` + `DownKyi.Core/Bili/Models/Favorites/`
- **What:** `GetFavoritesList(long mid)`, `GetFavoritesMedia(long mediaId, int pn)` — paginated.

### P0-05 — Port History API (`BiliApi/History/`)
- **Reference:** History.cs, ToView.cs + HistoryList, HistoryData, HistoryCursor, ToViewList, ToViewData
- **Target:** `DownKyi.Core/Bili/Web/History.cs` + models
- **What:** `GetHistory(string cursor)` (paginated via cursor), `GetToViewList()`.

### P0-06 — Port Users API — Publications, Channels, Seasons (`BiliApi/Users/`)
- **Reference:** SpacePublication, SpaceChannel, SpaceSeasonsSeries, and 20+ support models
- **Target:** Extend `DownKyi.Core/Bili/Web/User.cs` + `DownKyi.Core/Bili/Models/Users/`
- **What:** `GetSpacePublication(long mid, int pn)`, `GetSpaceChannels(long mid)`, `GetSpaceSeasonsSeries(long mid)` — used by P4 UserSpace ViewModels.

### P0-07 — Port Users API — BangumiFollow, RelationFollow (`BiliApi/Users/`)
- **Reference:** BangumiFollow, RelationFollow, FollowingGroup models
- **Target:** Extend `DownKyi.Core/Bili/Web/User.cs`
- **What:** `GetBangumiFollow(long mid, int pn)`, `GetFollowing(long mid, int pn)`, `GetFollowers(long mid, int pn)`.

### P0-08 — Port Danmaku protobuf models (`BiliApi/protobuf/`)
- **Reference:** `BiliApi/protobuf/bilibili/community/service/dm/v1/` (protobuf definitions), `BiliApi/Danmaku/DanmakuProtobuf.cs`
- **Target:** `DownKyi.Core/Bili/Web/Danmaku.cs`
- **What:** `GetDanmaku(long cid)` — returns parsed DM list. Add Protobuf dependency if not present.

### P0-09 — Port Danmaku2Ass library (`Core/Danmaku2Ass/`)
- **Reference:** 11 files — Bilibili.cs, Collision.cs, Config.cs, Creater.cs, Danmaku.cs, Filter.cs, Producer.cs, Studio.cs + supporting classes
- **Target:** `DownKyi.Core/Danmaku2Ass/` (new directory)
- **What:** Full port of the danmaku-to-ASS converter. Replace any WPF `BitmapSource` usage with SkiaSharp or System.Drawing.Common for cross-platform text measurement.

### P0-10 — Port Storage/Database layer (`Core/Storage/`)
- **Reference:** DbHelper.cs, DownloadDb.cs, DownloadingDb.cs, DownloadedDb.cs, CoverDb.cs, HeaderDb.cs, StorageCover.cs, StorageHeader.cs
- **Target:** `DownKyi.Core/Storage/` (new directory, replaces the existing minimal Storage)
- **What:** Encrypted SQLite (SQLCipher) DB for download queue, download history, cover cache, user header cache. Schema: `downloading` table (id, bvid, cid, title, quality, status, progress, file_path), `downloaded` table (id, bvid, title, file_path, finish_time), `covers` table (url, local_path), `headers` table (mid, local_path).

### P0-11 — Port Aria2c RPC client (`Core/Aria2cNet/`)
- **Reference:** 35+ files — AriaManager.cs, AriaClient.cs, AriaServer.cs + 30 entity classes
- **Target:** `DownKyi.Core/Aria2cNet/` (already exists partially; verify completeness)
- **What:** JSON-RPC over WebSocket to Aria2c process. Verify all entity models are present. Update WebSocket API from `System.Net.WebSockets` if any .NET 4.x APIs used.

### P0-12 — Port FileName template/parts system (`Core/FileName/`)
- **Reference:** FileName.cs + model classes for filename parts (title, BV, cid, date, uploader, resolution, codec, etc.)
- **Target:** `DownKyi.Core/FileName/` (already exists; verify parts/template system is complete)
- **What:** `FilenameBuilder.Build(template, metadata)` — takes a list of `FileNamePart` enums and a metadata object, returns sanitized filename string. Include title sanitization fix from v1.6.x (crash when no valid chars).

---

## 5. Slice 1 — MVP Download

Goal: user can paste a Bilibili URL → see video info → pick quality → download a file.

### S1-01 — Complete `VideoInfo` Core model
- Extend `DownKyi.Core/Bili/Models/VideoInfo.cs`: add `Pages` (`List<VideoPage>`), `Sections` (`List<VideoSection>`), `CoverUrl`, `UpperMid`, `UpperName`, `TypeId`, stats (play, danmaku, like, coin, favorite, share, reply counts).
- Add `VideoPage` model: `Cid`, `Page`, `Title`, `Duration`.
- Add `VideoSection` model: `Id`, `Title`, `Pages`.

### S1-02 — Extend `IVideo` and `Video.cs` for pages
- Add `GetVideoPages(string bvid)` returning `List<VideoPage>`.
- Add `GetVideoSections(string bvid)` returning `List<VideoSection>` (for multi-part/UGC series).
- Reference: `src/DownKyi.Core/BiliApi/Video/` in v1.5.x.

### S1-03 — Complete `VideoInfoService.GetVideoView()`
- **Target:** `Downkyi.UI/Services/VideoInfo/VideoInfoService.cs`
- Map all `VideoInfo` fields to `VideoInfoView` model.
- Include zone/category lookup, play count formatting, publish date formatting.
- Reference: `src/DownKyi/Services/VideoInfoService.cs` in v1.5.x.

### S1-04 — Complete `VideoInfoService.GetVideoPages()` and `GetVideoSections()`
- Return `List<VideoPage>` and `List<VideoSection>` for the current input.
- Handle single-page videos (wrap in default section).

### S1-05 — Implement `GetVideoStream()` in `VideoInfoService`
- Calls `IVideo.GetPlayUrl()` (P0-01) for the given page's cid.
- Populates `VideoPage` with available qualities and stream URLs.

### S1-06 — Complete `BangumiInfoService`
- **Reference:** `src/DownKyi/Services/BangumiInfoService.cs`
- Implement `GetVideoView()`, `GetVideoPages()`, `GetVideoSections()`, `GetVideoStream()`.
- Uses P0-02 Bangumi Core API.

### S1-07 — Complete `CheeseInfoService`
- **Reference:** `src/DownKyi/Services/CheeseInfoService.cs`
- Implement `GetVideoView()`, `GetVideoPages()`, `GetVideoSections()`, `GetVideoStream()`.
- Uses P0-03 Cheese Core API.

### S1-08 — Add UI models: VideoPage, VideoSection, VideoQuality
- **Target:** `Downkyi.UI/Models/`
- Add observable `VideoPage`, `VideoSection`, `VideoQuality` models with `[ObservableProperty]` fields.
- Include `IsSelected` flag on each for batch selection.

### S1-09 — Wire episode list in `VideoDetailViewModel`
- Populate `ObservableCollection<VideoSection>` from `IVideoInfoService.GetVideoSections()`.
- Handle multi-part, bangumi, and cheese episode lists.
- **Reference:** `ViewVideoDetailViewModel.cs` lines ~200–400 in v1.5.x.

### S1-10 — Implement quality/codec selection in `VideoDetailViewModel`
- Populate quality list from `VideoPage.VideoQualityList`.
- Persist last-used quality in settings.
- **Reference:** `ViewVideoDetailViewModel.cs` quality-related methods in v1.5.x.

### S1-11 — Port `AddToDownloadService`
- **Target:** `Downkyi.UI/Services/Download/AddToDownloadService.cs` (new)
- **Reference:** `src/DownKyi/Services/Download/AddToDownloadService.cs` (835 lines in v1.5.x)
- Remove Prism `IEventAggregator` — use `WeakReferenceMessenger` or direct callback.
- Remove `IDialogService` dependency — inject `IDialogService` abstraction (defined in Slice 5).
- Creates `DownloadingItem` objects from selected pages and pushes to download queue.

### S1-12 — Implement `DownloadingItem` model
- **Target:** `Downkyi.UI/Models/DownloadingItem.cs`
- Fields: title, cover, bvid, cid, quality, status, progress (0–100), speed, file path, cancellation token.
- **Reference:** `src/DownKyi/ViewModels/DownloadManager/DownloadingItem.cs` in v1.5.x.

### S1-13 — Implement basic `DownloadingViewModel`
- Show downloading list, start/pause/cancel per item, pause-all/resume-all/clear-all.
- **Reference:** `src/DownKyi/ViewModels/DownloadManager/ViewDownloadingViewModel.cs` in v1.5.x.

### S1-14 — Implement `DownloadFinishedViewModel`
- List finished items (title, path, finish time), open-file command, clear-history command.
- **Reference:** `src/DownKyi/ViewModels/DownloadManager/ViewDownloadFinishedViewModel.cs`.

### S1-15 — Wire "Download" button in `VideoDetailViewModel`
- Collect selected episodes + quality → call `AddToDownloadService` → push to queue → navigate to Download Manager.
- Show parsing selector dialog (scope: selected / section / all).

---

## 6. Slice 2 — Full Download Engine

Goal: actual file download works reliably with both built-in and aria2c backends; state persists across restarts.

### S2-01 — Port `MultiThreadDownloader` and `PartialDownloader` to .NET 8
- **Target:** `DownKyi.Core/Downloader/` (already exists; audit for deprecated APIs)
- Fix: `HttpWebRequest` → `HttpClient`; `Thread` → `Task`; `ManualResetEvent` → `SemaphoreSlim`.
- Verify cross-platform (no Windows-only network APIs).

### S2-02 — Port abstract `DownloadService`
- **Target:** `Downkyi.UI/Services/Download/DownloadService.cs` (new)
- **Reference:** `src/DownKyi/Services/Download/DownloadService.cs` in v1.5.x
- Handles: DASH audio download, DASH video download, FLV/MP4 stream download, FFmpeg merge, subtitle download, danmaku download, cover download.
- Remove WPF-specific `TaskbarIcon`; replace with cross-platform notification abstraction.
- State machine: `NOT_STARTED → WAIT → DOWNLOADING → PAUSE → SUCCEED / FAILED / CANCELLED`.

### S2-03 — Port `BuiltinDownloadService`
- **Reference:** `src/DownKyi/Services/Download/BuiltinDownloadService.cs`
- Uses `MultiThreadDownloader` from P0/S2-01.
- Retry logic (5 retries per file).

### S2-04 — Port `AriaDownloadService` and `CustomAriaDownloadService`
- **Reference:** `src/DownKyi/Services/Download/AriaDownloadService.cs`, `CustomAriaDownloadService.cs`
- Uses P0-11 Aria2c RPC client.
- Starts `aria2c` process, registers RPC callbacks, maps download progress to `DownloadingItem`.

### S2-05 — Port FFmpeg merge step
- **Target:** `DownKyi.Core/FFmpeg/FFmpegHelper.cs` (extend existing)
- DASH video + audio merge command: `ffmpeg -i video.m4s -i audio.m4s -c copy output.mp4`
- Cross-platform process launch (use `Process.Start` with `UseShellExecute = false`).
- Post-merge cleanup of `.m4s` temp files.
- Handle FFmpeg binary not found: surface clear error to user.

### S2-06 — Implement download persistence (Storage/Database)
- Uses P0-10 Storage layer.
- On app start: restore `DOWNLOADING` / `PAUSED` items to `DownloadingViewModel`.
- On every status change: write to DB.
- On `SUCCEED`: move from downloading table to downloaded table.

### S2-07 — Implement download speed / progress tracking
- Real-time byte-count diff per 500ms interval → speed (MB/s), ETA.
- Bound to `DownloadingItem.SpeedDisplay` and `DownloadingItem.Progress`.

### S2-08 — Implement post-download operations
- Settings-driven: `None` / `CloseApp` / `Shutdown` when queue empties.
- **Reference:** v1.5.x `DownloadService` post-completion block.

---

## 7. Slice 3 — Full Video Detail

Goal: all per-video features work — danmaku, subtitle, cover, batch selection, clipboard.

### S3-01 — Implement danmaku download in `DownloadService`
- Uses P0-08 (Danmaku API) + P0-09 (Danmaku2Ass).
- Fetch danmaku XML/protobuf by cid → convert to ASS → save alongside video.
- Honour `Danmaku*` settings (font, size, opacity, filter keywords, etc.).

### S3-02 — Implement subtitle download and language selection
- Uses P0-01 VideoStream subtitle models (`Subtitle`, `SubtitleInfo`).
- Fetch available subtitle tracks from `GetPlayUrl()` response.
- Populate language selector in `VideoDetailViewModel`.
- Download selected subtitle JSON → convert to SRT/ASS.
- **Reference:** `src/DownKyi/Services/Download/DownloadService.cs` subtitle section.

### S3-03 — Implement cover image display and download
- Load cover from `VideoInfoView.CoverUrl` using `HttpClient`; cache via P0-10 `StorageCover`.
- Bind to cover `Image` control in `VideoDetailView`.
- Implement `CopyCoverUrl()` command (copy URL to clipboard).
- Implement `CopyCover()` command (copy image bytes to clipboard — cross-platform).

### S3-04 — Implement "Select All / Deselect All" episode selection
- Commands in `VideoDetailViewModel` that set `IsSelected` on all `VideoPage` items.
- Count selected → update download button label.

### S3-05 — Implement auto-parse behavior
- When `IsAutoParseVideo == YES`: on URL input, auto-trigger parse without clicking button.
- When `IsAutoDownloadAll == YES`: after parse, auto-add all episodes to queue.
- **Reference:** `src/DownKyi/ViewModels/ViewVideoDetailViewModel.cs` InputText setter.

### S3-06 — Implement parsing selector dialog
- **Reference:** `src/DownKyi/ViewModels/Dialogs/ViewParsingSelectorViewModel.cs`
- Avalonia dialog: choose scope — Selected / Current Section / All Sections.
- Returns `ParseScope` enum to caller.

### S3-07 — Implement download setter dialog
- **Reference:** `src/DownKyi/ViewModels/Dialogs/ViewDownloadSetterViewModel.cs`
- Per-download quality/codec override before queuing.

### S3-08 — Implement clipboard monitoring
- Avalonia: use `Application.Current.Clipboard` + timer polling (no clipboard hook available cross-platform).
- When clipboard text matches Bilibili URL pattern → populate `InputText` in `VideoDetailViewModel` or `IndexViewModel`.
- Controlled by `IsListenClipboard` setting.
- **Reference:** v1.5.x `MainWindowViewModel` clipboard hooker setup.

---

## 8. Slice 4 — User Space

Goal: all user-facing collection and profile browsing features work.

### S4-01 — Implement `MySpaceViewModel`
- Load logged-in user profile (avatar, username, level icon, background).
- Sub-page navigation: Publications, Favorites, History, BangumiFollow, ToView.
- Logout command.
- **Reference:** `src/DownKyi/ViewModels/ViewMySpaceViewModel.cs`.

### S4-02 — Implement `UserSpaceViewModel`
- Load any user's profile by mid (navigate from video uploader link).
- Sub-page navigation: Publications, Channels, Seasons/Series.
- **Reference:** `src/DownKyi/ViewModels/ViewPublicationViewModel.cs`, `ViewChannelViewModel.cs`, `ViewSeasonsSeriesViewModel.cs`.

### S4-03 — Implement Archive/Publications sub-page
- Paginated list of user's uploaded videos (uses P0-06).
- Select videos → add to download queue.
- **Reference:** `src/DownKyi/Views/UserSpace/ViewArchive.xaml`.

### S4-04 — Implement Channels sub-page
- List user's video channels (uses P0-06).
- Navigate into channel → list videos.
- **Reference:** `src/DownKyi/ViewModels/ViewChannelViewModel.cs`.

### S4-05 — Implement Seasons/Series sub-page
- List user's series and seasons (uses P0-06).
- **Reference:** `src/DownKyi/ViewModels/ViewSeasonsSeriesViewModel.cs`.

### S4-06 — Implement My Favorites
- List logged-in user's favorite folders (uses P0-04).
- Paginated video list per folder.
- Select → download.
- **Reference:** `src/DownKyi/ViewModels/ViewMyFavoritesViewModel.cs`.

### S4-07 — Implement Public Favorites
- Browse another user's public favorite folder by URL.
- **Reference:** `src/DownKyi/ViewModels/ViewPublicFavoritesViewModel.cs`.

### S4-08 — Implement My History
- Watch history list (paginated via cursor, uses P0-05).
- Select → download.
- **Reference:** `src/DownKyi/ViewModels/ViewMyHistoryViewModel.cs`.

### S4-09 — Implement My Bangumi Follow
- Followed anime/series list (uses P0-07).
- Select season → download episodes.
- **Reference:** `src/DownKyi/ViewModels/ViewMyBangumiFollowViewModel.cs`.

### S4-10 — Implement My To-Watch List
- Watch-later videos (uses P0-05 `GetToViewList()`).
- Select → download.
- **Reference:** `src/DownKyi/ViewModels/ViewMyToViewVideoViewModel.cs`.

### S4-11 — Implement Following / Followers (Friends)
- Two sub-pages: Following list, Followers list (uses P0-07).
- Navigate to user profile on tap.
- **Reference:** `src/DownKyi/ViewModels/Friends/ViewFollowingViewModel.cs`, `ViewFollowerViewModel.cs`.

---

## 9. Slice 5 — Settings + Polish

Goal: all settings apply correctly; UX matches v1.5.9 quality bar.

### S5-01 — Implement proxy apply in `NetworkViewModel`
- On settings save: call `HttpClient` factory / `WebProxy` update globally.
- Support HTTP and SOCKS5 proxy types.

### S5-02 — Implement "Check for Updates" in `AboutViewModel`
- Fetch GitHub releases API for latest tag.
- Compare semver against current version.
- Show update dialog with release notes link.

### S5-03 — Implement Avalonia dialog service abstraction
- Define `IDialogService` in `Downkyi.UI/Services/` with `ShowAlert()`, `ShowConfirm()`, `ShowCustom<T>()`.
- Implement using Avalonia `Window` / `DialogHost` pattern.
- Replaces Prism `IDialogService` used throughout v1.5.x.

### S5-04 — Implement alert / confirmation dialogs
- Port `ViewAlertDialogViewModel` → Avalonia equivalent.
- Port `ViewDownloadSetter` and `ViewParsingSelector` dialogs.

### S5-05 — Implement missing settings: auto-download, clipboard, sort, FLV→MP4
- Add settings fields: `IsListenClipboard`, `IsAutoDownloadAll`, `DownloadFinishedSort`, `IsTranscodingFlvToMp4`, `ParseScope`, `OrderFormat`, `HistoryVideoRootPaths`.
- Wire each to corresponding settings UI controls.

### S5-06 — Implement filename parts / template configuration
- Port `FileNameParts`, `FileNamePartTimeFormat` settings.
- Build settings UI for reordering/enabling filename parts (drag list or checkboxes).
- Uses P0-12 Core `FilenameBuilder`.

### S5-07 — Implement `MainSearchService` URL routing
- Parse input string → detect URL type (video BV, av, bangumi, cheese, user space, favorites, etc.).
- Navigate to correct page.
- **Reference:** `src/DownKyi/Services/SearchService.cs` in v1.5.x.

### S5-08 — Implement `StringToResourceConverter.ConvertBack()`
- Required for two-way bindings in settings dropdowns.

### S5-09 — Implement download list sorting
- `DownloadFinishedViewModel`: sort by finish time, title, file size.
- Persist sort preference in settings.

### S5-10 — Implement re-download confirmation (v1.6.x feature)
- When user tries to download an item already in the finished list: show confirm dialog.
- **Reference:** v1.6.x commit `如果存在下载完成列表，弹出选择框是否再次下载`.

### S5-11 — Taskbar / system tray notification (cross-platform)
- v1.5.x used `Hardcodet.Wpf.TaskbarNotification`.
- v2.0.x: use Avalonia tray icon (`TrayIcon` API, available in Avalonia 11+).
- Show notification on download complete.

---

## 10. Task Summary

| Slice | Area | Tasks | Depends On |
|---|---|:---:|---|
| P0 | Core API modules | 12 | — |
| S1 | MVP download | 15 | P0 (P0-01 to P0-03) |
| S2 | Full download engine | 8 | S1, P0-09 to P0-12 |
| S3 | Full video detail | 8 | S1, S2, P0-08, P0-09 |
| S4 | User space | 11 | P0-04 to P0-07 |
| S5 | Settings + polish | 11 | S1–S4 |
| **Total** | | **65** | |

**Recommended execution order:** P0 → S1 → S2 → S3 → S4 → S5

P0 + S1 together = minimum viable product (paste URL, download video).

---

## 11. v1.6.x Improvements to Adopt

These are v1.6.x changes not in v1.5.9 that v2.0.x should include:

| Feature | v1.6.x commit | Where to apply |
|---|---|---|
| WBI signing algorithm | `更新wbi签名算法` | `DownKyi.Core/Bili/Web/` — already in BiliSharp; verify |
| Reduced video parse API calls | `减少视频解析页面接口调用次数` | `VideoInfoService.GetVideoStream()` (S1-05) |
| Re-download confirmation dialog | `弹出选择框是否再次下载` | S5-10 |
| Title sanitization crash fix | `修复标题中无合法字符时崩溃的问题` | P0-12 `FilenameBuilder.Sanitize()` |
| Login QR code fix | Already done in v2.0.x | — |
| Download list popup optimization | `下载列表弹出框优化` | S1-13 `DownloadingViewModel` |

---

## 12. Risks & Constraints

| Risk | Mitigation |
|---|---|
| Danmaku2Ass uses WPF text measurement (`FormattedText`) | Replace with SkiaSharp `SKPaint.MeasureText()` |
| Aria2c binary ships as `aria2c.exe` (Windows only) | Bundle platform-specific binaries; add runtime detection |
| FFmpeg binary cross-platform | Bundle per-platform or detect system FFmpeg |
| Protobuf dependency for danmaku | Add `Google.Protobuf` NuGet package |
| Clipboard hook (Windows `WndProc`) used in v1.5.x | Use timer-based polling in Avalonia instead |
| `System.Runtime.Serialization.Formatters.Binary` (BinaryFormatter) removed in .NET 8 | Port any serialized types to `System.Text.Json` |
| v2.0.x uses encrypted SQLite (SQLCipher) vs plain SQLite in v1.5.x | Migration path not needed (fresh install); just use encrypted schema |
