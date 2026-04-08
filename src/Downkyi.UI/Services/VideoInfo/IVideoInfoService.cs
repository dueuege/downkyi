using Downkyi.Core.Bili.Models;
using Downkyi.UI.Models;

namespace Downkyi.UI.Services.VideoInfo;

public interface IVideoInfoService
{
    VideoInfoView? GetVideoView(string input);
    Task<List<VideoPage>> GetVideoPagesAsync(string input);
    Task<List<VideoSection>> GetVideoSectionsAsync(string input);
}
