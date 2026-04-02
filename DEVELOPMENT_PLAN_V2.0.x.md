# DownKyi v2.0.x — Development Plan & Feature Comparison

> Generated: 2026-04-02
> Current branch: `v2.0.x`
> Reference branch: `main` (v1.6.x, last stable release)

---

## Overview

`v2.0.x` is a **complete rewrite** of DownKyi from scratch. It replaces the WPF/.NET Framework 4.7.2 stack with **Avalonia UI + .NET 8**, introducing cross-platform support and a cleaner architecture. Both branches currently compile with 0 errors.

**Current completion estimate: ~35–40%**

The framework, navigation system, login flow, and settings UI are largely in place, but the core download workflow (video parsing → quality selection → download queue) is not yet functional.

---

## Architecture Comparison

| Area | main (v1.6.x) | v2.0.x |
|---|---|---|
| UI Framework | WPF | Avalonia UI |
| Target Framework | .NET Framework 4.7.2 | .NET 8 |
| Platform | Windows only | Windows / Linux / macOS |
| Project structure | 2 projects (DownKyi, DownKyi.Core) | 3 projects (DownKyi, Downkyi.UI, Downkyi.Core) |
| DI Container | Prism + DryIoc | Microsoft.Extensions.DependencyInjection |
| MVVM toolkit | Prism | CommunityToolkit.Mvvm |
| Navigation | Prism RegionManager | Custom NavigationService (forward/backward stack) |
| Settings storage | Custom settings | SettingsManager (singleton, same pattern) |
| Database | SQLite (unencrypted) | SQLite (encrypted via SQLCipher) |
| Downloader | Built-in + Aria2c | Built-in (partial port) |

---

## Current Status of v2.0.x Pages

| Page / Feature | UI Shell | ViewModel Logic | Core API | Status |
|---|:---:|:---:|:---:|---|
| App startup / splash | ✅ | ✅ | — | Done |
| Navigation system | ✅ | ✅ | — | Done |
| Login (QR code) | ✅ | ✅ | ✅ | Done |
| Login (cookies) | ✅ | ✅ | ✅ | Done |
| Index / home page | ✅ | ✅ | — | Done |
| Video Detail (URL input) | ✅ | ✅ | ✅ | Shell only — service stub |
| Video Detail (video info display) | ✅ | ⚠️ | ⚠️ | UI ready, data not wired |
| Video Detail (episode list) | ✅ | ❌ | ❌ | Not started |
| Video Detail (quality selection) | ✅ | ❌ | ❌ | Not started |
| Video Detail (download button) | ✅ | ❌ | ❌ | Not started |
| Download Manager (tabs) | ✅ | ✅ | — | Shell only |
| Downloading queue | ✅ | ❌ | ❌ | Empty stub |
| Finished downloads | ✅ | ❌ | ❌ | Empty stub |
| Settings (Basic) | ✅ | ✅ | — | Done |
| Settings (Video) | ✅ | ✅ | — | Done |
| Settings (Danmaku) | ✅ | ✅ | — | Done |
| Settings (Network) | ✅ | ⚠️ | — | Proxy apply TODO |
| Settings (About) | ✅ | ⚠️ | — | Update check TODO |
| Toolbox (BiliHelper) | ✅ | ✅ | ✅ | Done |
| Toolbox (Delogo) | ✅ | ✅ | — | Done |
| Toolbox (ExtractMedia) | ✅ | ✅ | — | Done |
| My Space | ✅ | ❌ | ❌ | Empty stub |
| User Space | ✅ | ❌ | ❌ | Empty stub |
| Bangumi (anime) info | — | ❌ | ❌ | Not started |
| Cheese (courses) info | — | ❌ | ❌ | Not started |

---

## Tasks Required for Feature Parity with main (v1.6.x)

Total estimated tasks: **42**

### Priority 1 — Core Download Flow (Blocker, ~12 tasks)

These are required before any download can occur. Nothing else matters without this working.

| # | Task | Description |
|---|---|---|
| P1-01 | Implement `VideoInfoService.GetVideoView()` | Map `DownKyi.Core.Bili.Web.Video.GetVideoInfo()` result to `VideoInfoView` model including title, cover, uploader, stats, publish date |
| P1-02 | Extend `VideoInfo` model with episode list | Add pages/parts list to `VideoInfo` in Core (`Pages`, `Sections`) |
| P1-03 | Implement `Video.GetVideoPages()` in Core | Call Bili API to get all parts/episodes for a video |
| P1-04 | Wire episode list into `VideoDetailViewModel` | Populate episode `ObservableCollection`, handle multi-part videos, enable selection |
| P1-05 | Implement play URL fetching in Core | `Video.GetPlayUrl()` — fetch stream URLs for selected quality/codec (DASH, FLV) |
| P1-06 | Implement quality/codec selection in `VideoDetailViewModel` | Populate quality list from available streams, allow user to select |
| P1-07 | Implement `BangumiInfoService.GetVideoView()` | Bangumi (anime/drama/movie) info and episode list |
| P1-08 | Implement `CheeseInfoService.GetVideoView()` | Cheese (paid courses) info and episode list |
| P1-09 | Implement download queue model | `DownloadingItem` with title, cover, progress, speed, status, cancellation token |
| P1-10 | Implement `DownloadingViewModel` | Populate downloading list, start/pause/cancel per item, pause-all/resume-all/delete-all |
| P1-11 | Implement `DownloadFinishedViewModel` | List of completed downloads, open file, clear history |
| P1-12 | Wire "Download" button in `VideoDetailViewModel` | Collect selected episodes + quality → push to download queue → navigate to Download Manager |

### Priority 2 — Download Engine (Core Infrastructure, ~7 tasks)

| # | Task | Description |
|---|---|---|
| P2-01 | Port multi-thread downloader to .NET 8 | `MultiThreadDownloader` / `PartialDownloader` — fix APIs deprecated in .NET 8 |
| P2-02 | Implement Aria2c download backend | Aria2c RPC client integration, same as v1 |
| P2-03 | Implement FFmpeg merge step | Merge video + audio streams post-download (DASH requires separate streams) |
| P2-04 | Implement download persistence | Save/restore download queue across app restarts (SQLite) |
| P2-05 | Implement download speed / progress tracking | Real-time speed calculation, ETA, total size |
| P2-06 | Implement download status state machine | NOT_STARTED → WAIT → DOWNLOADING → PAUSE → SUCCEED / FAILED |
| P2-07 | Implement post-download operations | None / Close App / Shutdown — triggered when queue empties |

### Priority 3 — User Space & Social (8 tasks)

| # | Task | Description |
|---|---|---|
| P3-01 | Implement `MySpaceViewModel` | Load logged-in user profile (avatar, username, background), show sub-pages, logout |
| P3-02 | Implement `UserSpaceViewModel` | Load other user profiles by mid |
| P3-03 | Implement Archive sub-page | List user's uploaded videos (paginated), select and download |
| P3-04 | Implement My Favorites | Favorite folders list, video list per folder, select and download |
| P3-05 | Implement My History | Watch history list (paginated), select and download |
| P3-06 | Implement My Bangumi Follow | Followed anime/series list, select and download |
| P3-07 | Implement My To-Watch List | Watch-later videos list, select and download |
| P3-08 | Implement Friends (Following / Followers) | List following and followers, navigate to user profiles |

### Priority 4 — Settings Completion (4 tasks)

| # | Task | Description |
|---|---|---|
| P4-01 | Implement proxy apply in `NetworkViewModel` | Apply HTTP proxy settings to HttpClient/WebClient globally at runtime |
| P4-02 | Implement "Check for Updates" in `AboutViewModel` | Fetch GitHub releases API, compare version, show dialog |
| P4-03 | Implement auto-parse behavior in `VideoDetailViewModel` | When `IsAutoParseVideo == YES` after URL input, auto-trigger parse and optionally auto-download |
| P4-04 | Implement `StringToResourceConverter.ConvertBack()` | Required for two-way bindings in settings |

### Priority 5 — Video Detail Completeness (5 tasks)

| # | Task | Description |
|---|---|---|
| P5-01 | Implement subtitle stream selection | Fetch available subtitle languages, allow user to select, download alongside video |
| P5-02 | Implement danmaku (bullet comment) download | Fetch danmaku XML/protobuf, convert to ASS format (Danmaku2Ass) |
| P5-03 | Implement cover image display | Load and cache cover image from URL in `VideoDetailView` |
| P5-04 | Implement `CopyCover()` command | Copy cover image to clipboard |
| P5-05 | Implement "Select All / Deselect All" episode selection | Multi-episode selection for batch download |

### Priority 6 — UX Polish (6 tasks)

| # | Task | Description |
|---|---|---|
| P6-01 | Implement clipboard monitoring | Watch clipboard for Bilibili URLs, auto-paste into search bar |
| P6-02 | Implement `MainSearchService` search logic | Handle search from index page, navigate to correct page based on URL type |
| P6-03 | Implement file naming configuration | Customizable filename parts (title, BV, date, UP name, etc.) |
| P6-04 | Implement alert/confirmation dialogs | Ported from Prism `IDialogService` to Avalonia equivalent |
| P6-05 | Implement download setter dialog | Pre-download quality override dialog per episode |
| P6-06 | Implement parsing selector dialog | Select scope (selected / section / all) before parsing |

---

## Advantages and New Features in v2.0.x

### 1. Cross-Platform Support
The single biggest improvement. v2.0.x runs on **Windows, Linux, and macOS** natively via Avalonia. The main branch is Windows-only due to WPF.

### 2. Modern .NET 8 Runtime
- Better performance (improved JIT, lower memory usage)
- Native AOT compilation possible in future
- Long-term support (LTS) until November 2026
- No dependency on .NET Framework installation

### 3. Encrypted Credential Storage
Login cookies and session data are stored in an **encrypted SQLite database** (SQLCipher), vs plain SQLite in v1. Credentials cannot be read by other apps or scripts.

### 4. Cleaner Architecture (3-layer separation)
```
DownKyi (Avalonia app)       — platform-specific entry point, views
Downkyi.UI (class library)   — ViewModels, services, business logic
Downkyi.Core (class library) — Bili API, downloader, settings (no UI dependency)
```
This means `Downkyi.Core` and `Downkyi.UI` can be reused for a CLI tool or alternative frontend without changes.

### 5. Improved Navigation System
Custom forward/backward navigation stack replaces Prism's region manager. Navigation history is preserved so users can go Back to the previous page, which was inconsistent in v1.

### 6. CommunityToolkit.Mvvm (Source Generators)
Less boilerplate: `[ObservableProperty]` and `[RelayCommand]` attributes generate property/command code at compile time. ViewModels are smaller and easier to read vs manual `INotifyPropertyChanged` implementations in v1.

### 7. Microsoft.Extensions.DependencyInjection
Standard .NET DI container replaces Prism + DryIoc. More familiar to .NET developers, smaller dependency footprint.

### 8. BiliSharp Integration (Updated Bili API)
v2.0.x uses `Downkyi.BiliSharp`, an updated Bilibili API library. This handles API signing/authentication changes that broke v1.6 after Bilibili's server-side updates (the main reason v2 was started).

### 9. Modern UI Theming
Avalonia supports proper dark/light theme switching and better HiDPI scaling compared to WPF on multi-monitor setups.

---

## Task Summary

| Priority | Area | Tasks | Effort |
|---|---|:---:|---|
| P1 | Core download flow | 12 | High — most critical path |
| P2 | Download engine | 7 | High — complex async/threading |
| P3 | User space & social | 8 | Medium — repetitive API pattern |
| P4 | Settings completion | 4 | Low |
| P5 | Video detail completeness | 5 | Medium |
| P6 | UX polish | 6 | Low–Medium |
| **Total** | | **42** | |

**Recommended order:** P1 → P2 → P4 → P5 → P3 → P6

P1 and P2 together deliver the minimum viable product (user can paste a URL and download a video). P3 adds all the collection/social browsing features. P5 and P6 bring it to full feature parity.
