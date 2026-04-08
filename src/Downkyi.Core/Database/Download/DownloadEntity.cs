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
