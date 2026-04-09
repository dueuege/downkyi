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
