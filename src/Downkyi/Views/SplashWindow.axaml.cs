using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Downkyi.Models;
using System;
using System.Threading.Tasks;

namespace Downkyi.Views;

public partial class SplashWindow : Window
{
    private readonly Action? _mainAction;

    public SplashWindow()
    {
    }

    public SplashWindow(Action mainAction)
    {
        InitializeComponent();

        nameVersion.Text = new AppInfo().VersionName;
        _mainAction = mainAction;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        DummyLoad();
    }

    private async void DummyLoad()
    {
        // Resolve BuiltinDownloadService on the UI thread so it captures the correct SynchronizationContext
        var downloadService = ServiceLocator.BuiltinDownloadService;

        // Restore persisted download state
        await ServiceLocator.DownloadingViewModel.LoadAsync();
        await ServiceLocator.DownloadFinishedViewModel.LoadAsync();

        // Start the download engine
        downloadService.Start();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _mainAction?.Invoke();
            Close();
        });
    }

}