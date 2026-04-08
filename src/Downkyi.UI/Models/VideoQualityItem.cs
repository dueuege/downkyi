using CommunityToolkit.Mvvm.ComponentModel;

namespace Downkyi.UI.Models;

/// <summary>Represents one available video quality option in the quality picker.</summary>
public partial class VideoQualityItem : ObservableObject
{
    public int QualityId { get; set; }

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isSelected;
}
