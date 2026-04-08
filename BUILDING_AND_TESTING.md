# DownKyi v2.0.0-beta — Build & Testing Guide

> Branch: `v2.0.x` · Version: `2.0.0-beta` · Framework: Avalonia UI / .NET 8
>
> This branch is a full rewrite of DownKyi (WPF → Avalonia, .NET Framework → .NET 8).
> It is a **work-in-progress beta**. See [What Works](#what-works) before testing.

---

## Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0.x | Required to build and run |
| **ffmpeg** | any recent | Place `ffmpeg.exe` (Windows) or `ffmpeg` (Linux/macOS) in the app output directory **or** on your system `PATH`. Required for video+audio merge. |
| **aria2c** *(optional)* | 1.36+ | Place `aria2c.exe` (Windows) or `aria2c` (Linux/macOS) in the app output directory. The Aria2c download backend is not yet active in this beta — built-in downloader is used instead. |
| Git | any | To clone the repo |
| A Bilibili account | — | Required for most video downloads (login via QR code or cookies) |

---

## Clone & Checkout

```bash
git clone https://github.com/dueuege/downkyi.git
cd downkyi
git checkout v2.0.x
```

---

## Build

All commands run from the **`src/`** directory.

### Quick run (Debug)

```bash
cd src
dotnet run --project DownKyi/Downkyi.csproj
```

The app will open. Log files are written to `./Logs/` (relative to the working directory).

### Build only

```bash
cd src
dotnet build DownKyi/Downkyi.csproj
# Output: src/DownKyi/bin/Debug/net8.0/
```

### Release build

```bash
cd src
dotnet build DownKyi/Downkyi.csproj -c Release
# Output: src/DownKyi/bin/Release/net8.0/
```

### Self-contained publish (single folder, no SDK needed to run)

```bash
cd src

# Windows x64
dotnet publish DownKyi/Downkyi.csproj -c Release -r win-x64 --self-contained -o ./publish/win-x64

# Linux x64
dotnet publish DownKyi/Downkyi.csproj -c Release -r linux-x64 --self-contained -o ./publish/linux-x64

# macOS arm64 (Apple Silicon)
dotnet publish DownKyi/Downkyi.csproj -c Release -r osx-arm64 --self-contained -o ./publish/osx-arm64
```

After publishing, copy `ffmpeg.exe` / `ffmpeg` into the output folder before running.

---

## ffmpeg Setup

Download a static ffmpeg build for your platform:

- **Windows:** https://www.gyan.dev/ffmpeg/builds/ → `ffmpeg-release-essentials.zip` → extract `ffmpeg.exe`
- **Linux:** `sudo apt install ffmpeg` or download from https://johnvansickle.com/ffmpeg/
- **macOS:** `brew install ffmpeg`

Place `ffmpeg.exe` / `ffmpeg` in the same folder as `Downkyi.exe` / `Downkyi`, **or** ensure `ffmpeg` is on your `PATH`.

---

## What Works

These features are implemented and ready to test as of `v2.0.0-beta`:

| Feature | Status | Notes |
|---|:---:|---|
| App startup & splash screen | ✅ | |
| Navigation (forward / back stack) | ✅ | |
| Login — QR code | ✅ | Scan with Bilibili mobile app |
| Login — Cookies | ✅ | Paste browser cookies string |
| Login — logout | ✅ | |
| Settings persistence | ✅ | Saved to `./Config/Settings/` |
| Settings — download path | ✅ | Set before downloading |
| Settings — video quality default | ✅ | 4K / 1080P60 / 1080P / etc. |
| Settings — video codec preference | ✅ | AVC (H.264) or HEVC (H.265) |
| Settings — filename template | ✅ | Configurable parts |
| **Video URL parsing** | ✅ | BV / AV / ep / ss / md links |
| **Video info display** | ✅ | Title, uploader, stats, zone |
| **Episode list (multi-page videos)** | ✅ | Select individual pages |
| **Bangumi (anime) episode list** | ✅ | Season and extra sections |
| **Cheese (paid course) episode list** | ✅ | |
| **Add selected episodes to queue** | ✅ | Respects IsSelected per page |
| **Add all episodes to queue** | ✅ | "Add All" variant |
| **Download queue UI** | ✅ | Shows items, progress bar, status |
| **Download resume after restart** | ✅ | Waiting items are restored from DB |
| **DASH stream download (built-in)** | ✅ | Requires ffmpeg — see above |
| **Download history (completed list)** | ✅ | Open file / folder after completion |
| **Delete from download history** | ✅ | |
| Copy cover URL to clipboard | ✅ | Right-click menu on video info |
| Navigate to uploader space | ✅ | Click uploader name |

---

## What Does NOT Work Yet

These are planned for upcoming slices and are **not** functional in this beta:

| Feature | Planned Slice |
|---|---|
| Danmaku (弹幕) download alongside video | Slice 3 |
| Subtitle download | Slice 3 |
| Cover image display in video detail | Slice 3 |
| Select All / Deselect All episodes | Slice 3 |
| Auto-parse URL on paste | Slice 3 |
| User space browsing (videos, favorites) | Slice 4 |
| Favorites list download | Slice 4 |
| Watch history download | Slice 4 |
| Aria2c download backend | Slice 2 (pending) |
| Download pause / resume | Slice 2 (pending) |
| Download speed display | Slice 2 (pending) |
| Quality selector dropdown in UI | Slice 3 |
| Proxy settings | Slice 5 |
| App update check | Slice 5 |

---

## End-to-End Test Walkthrough

This is the primary test flow for `v2.0.0-beta`:

1. **Build and run** the app (Debug or Release).
2. **Login**: click the user icon → choose QR code or cookies → complete login. Confirm your username appears.
3. **Paste a URL** on the home page search bar. Supported formats:
   - Regular video: `https://www.bilibili.com/video/BVxxxxxxxxxx`
   - Anime: `https://www.bilibili.com/bangumi/play/ss12345` or `ep123456`
   - Paid course: `https://www.bilibili.com/cheese/play/ss12345`
4. Press **Enter** or click the search button. The video info panel should populate.
5. In the episode list, **select one or more episodes** (first episode is pre-selected).
6. Click **Add to Download** (or Add All). The app should navigate to the download manager.
7. The item(s) should appear in the **Downloading** tab with status `Waiting`, then transition to `Downloading` and show progress.
8. On completion, items move to the **Completed** tab. Click the folder icon to open the output directory.

> **If download fails:** check that `ffmpeg` is reachable (run `ffmpeg -version` in a terminal). Also verify the download path exists and is writable (Settings → Basic → Download path).

---

## Known Issues / Beta Limitations

- **No quality selector in the UI yet** — the quality used for download is the one set in Settings → Video → Default Quality. The UI dropdown is scaffolded but not wired.
- **No pause/resume** — cancelling and re-queuing works via the Cancel button, but paused items are not resumable yet.
- **No download speed display** — progress bar works; speed text is a placeholder.
- **Danmaku/subtitle/cover** are not downloaded even if the settings checkboxes are enabled.
- **Aria2c backend** is disabled — all downloads use the built-in HttpClient downloader regardless of the setting.
- **First-run database migration**: if you previously ran an older `v2.0.x` build with an existing `Storage/Download.db`, the `content_type` column will be added automatically on first run (sqlite-net handles this). No manual action needed.

---

## Project Structure (brief)

```
src/
  DownKyi/              — Avalonia app shell (views, startup, DI registration)
  Downkyi.UI/           — ViewModels, Services, UI models (platform-agnostic)
  Downkyi.Core/         — Core APIs: Bili HTTP, Settings, Database, Downloader, FFmpeg
```

---

## Logs

Log files are written to `./Logs/` relative to the working directory (or the publish output folder). Check `Logs/downkyi-YYYY-MM-DD.log` if something unexpected happens.

---

*Last updated: 2026-04-08 — reflects commits through `039509d` (Slice 2 complete)*
