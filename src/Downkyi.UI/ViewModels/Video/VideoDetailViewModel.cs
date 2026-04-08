using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downkyi.Core.Log;
using Downkyi.Core.Settings;
using Downkyi.Core.Settings.Enum;
using Downkyi.Core.Settings.Models;
using Downkyi.UI.Models;
using Downkyi.UI.Mvvm;
using Downkyi.UI.Services.Download;
using Downkyi.UI.Services.VideoInfo;
using Downkyi.UI.ViewModels.DownloadManager;
using Downkyi.UI.ViewModels.User;

namespace Downkyi.UI.ViewModels.Video;

public partial class VideoDetailViewModel : ViewModelBase
{
    public const string Key = "VideoDetail";

    private readonly IVideoInfoServiceFactory _videoInfoServiceFactory;
    private readonly DownloadingViewModel _downloadingViewModel;
    private readonly DownloadFinishedViewModel _downloadFinishedViewModel;

    // 保存输入字符串，避免被用户修改
    private string? _input = null;

    #region 页面属性申明

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _loadingVisibility;

    [ObservableProperty]
    private bool _contentVisibility;

    [ObservableProperty]
    private VideoInfoView _videoInfoView = new();

    [ObservableProperty]
    private ObservableCollection<VideoSectionItem> _videoSections = new();

    [ObservableProperty]
    private VideoSectionItem? _selectedSection;

    [ObservableProperty]
    private ObservableCollection<VideoQualityItem> _videoQualities = new();

    [ObservableProperty]
    private VideoQualityItem? _selectedQuality;

    #endregion

    public VideoDetailViewModel(BaseServices baseServices,
        IVideoInfoServiceFactory videoInfoServiceFactory,
        DownloadingViewModel downloadingViewModel,
        DownloadFinishedViewModel downloadFinishedViewModel) : base(baseServices)
    {
        _videoInfoServiceFactory = videoInfoServiceFactory;
        _downloadingViewModel = downloadingViewModel;
        _downloadFinishedViewModel = downloadFinishedViewModel;
        ContentVisibility = true;
    }

    #region 命令申明

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task BackwardAsync()
    {
        Dictionary<string, object> parameter = new()
        {
            { "key", Key },
        };

        await NavigationService.BackwardAsync(parameter);
    }

    [RelayCommand]
    private async Task InputAsync()
    {
        if (string.IsNullOrEmpty(InputText)) return;

        Log.Logger.Debug($"InputText: {InputText}");
        _input = Regex.Replace(InputText, @"[【]*[^【]*[^】]*[】 ]", "");

        LoadingVisibility = true;
        ContentVisibility = false;

        VideoInfoView? fetchedView = null;
        List<Downkyi.Core.Bili.Models.VideoSection>? fetchedSections = null;

        try
        {
            IVideoInfoService service = _videoInfoServiceFactory.Create(_input);

            await Task.Run(async () =>
            {
                fetchedView = service.GetVideoView(_input);
                fetchedSections = await service.GetVideoSectionsAsync(_input);
            });

            // Update UI on calling (UI) thread
            if (fetchedView != null)
            {
                VideoInfoView.CoverUrl = fetchedView.CoverUrl;
                VideoInfoView.UpperMid = fetchedView.UpperMid;
                VideoInfoView.TypeId = fetchedView.TypeId;
                VideoInfoView.Title = fetchedView.Title;
                VideoInfoView.Description = fetchedView.Description;
                VideoInfoView.VideoZone = fetchedView.VideoZone;
                VideoInfoView.UpName = fetchedView.UpName;
                VideoInfoView.CreateTime = fetchedView.CreateTime;
                VideoInfoView.PlayNumber = fetchedView.PlayNumber;
                VideoInfoView.DanmakuNumber = fetchedView.DanmakuNumber;
                VideoInfoView.LikeNumber = fetchedView.LikeNumber;
                VideoInfoView.CoinNumber = fetchedView.CoinNumber;
                VideoInfoView.FavoriteNumber = fetchedView.FavoriteNumber;
                VideoInfoView.ShareNumber = fetchedView.ShareNumber;
                VideoInfoView.ReplyNumber = fetchedView.ReplyNumber;
            }

            if (fetchedSections != null)
            {
                VideoSections.Clear();
                foreach (var sec in fetchedSections)
                {
                    var sectionItem = new VideoSectionItem
                    {
                        Id = sec.Id,
                        Title = sec.Title,
                        IsSelected = VideoSections.Count == 0,
                    };
                    foreach (var page in sec.VideoPages)
                    {
                        sectionItem.VideoPages.Add(new VideoPageItem
                        {
                            Cid = page.Cid,
                            Page = page.Page,
                            Title = page.Title,
                            Duration = page.Duration,
                            IsSelected = page.IsSelected,
                            Avid = page.Avid != 0 ? page.Avid : (fetchedView?.Aid ?? 0),
                            Bvid = !string.IsNullOrEmpty(page.Bvid) ? page.Bvid : (fetchedView?.Bvid ?? string.Empty),
                            Epid = page.Epid,
                        });
                    }
                    VideoSections.Add(sectionItem);
                }
                SelectedSection = VideoSections.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "VideoDetailViewModel.InputAsync failed");
        }
        finally
        {
            LoadingVisibility = false;
            ContentVisibility = true;
        }
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task DownloadManager()
    {
        await NavigationService.ForwardAsync(DownloadManagerViewModel.Key);
    }

    [RelayCommand]
    private void CopyCover() { }

    [RelayCommand]
    private async Task CopyCoverUrl()
    {
        await ClipboardService.SetTextAsync(VideoInfoView.CoverUrl);
        Log.Logger.Info("复制封面url到剪贴板");
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task Upper()
    {
        await NavigateToViewUserSpace(VideoInfoView.UpperMid);
    }

    /// <summary>Adds selected pages to the download queue.</summary>
    [RelayCommand]
    private async Task AddToDownloadAsync(bool downloadAll = false)
    {
        string directory;

        // Use default save path if configured
        if (SettingsManager.Instance.IsUseSaveVideoRootPath() == AllowStatus.YES)
        {
            directory = SettingsManager.Instance.GetSaveVideoRootPath();
        }
        else
        {
            // Fall back to default until folder picker dialog is wired (Slice 5)
            directory = SettingsManager.Instance.GetSaveVideoRootPath();
        }

        if (string.IsNullOrEmpty(directory)) return;

        var service = new AddToDownloadService(_downloadingViewModel, _downloadFinishedViewModel);
        int added = await service.AddToDownloadAsync(VideoInfoView, VideoSections, directory, downloadAll);

        Log.Logger.Info($"Added {added} item(s) to download queue");

        if (added > 0)
        {
            await NavigationService.ForwardAsync(DownloadManagerViewModel.Key);
        }
    }

    #endregion

    private async Task NavigateToViewUserSpace(long mid)
    {
        Dictionary<string, object> parameter = new()
        {
            { "key", Key },
            { "value", mid },
        };

        UserInfoSettings userInfo = SettingsManager.Instance.GetUserInfo();
        if (userInfo != null && userInfo.Mid == mid)
        {
            await NavigationService.ForwardAsync(MySpaceViewModel.Key, parameter);
        }
        else
        {
            await NavigationService.ForwardAsync(UserSpaceViewModel.Key, parameter);
        }
    }

    public override async void OnNavigatedTo(Dictionary<string, object>? parameter)
    {
        base.OnNavigatedTo(parameter);

        if (parameter!.TryGetValue("value", out object? value))
        {
            if (!LoadingVisibility)
            {
                _input = (string)value;
                InputText = _input;
                await InputAsync();
            }
        }
    }
}
