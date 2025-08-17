using CommunityToolkit.Mvvm.ComponentModel;
using JoyConfig.Services;

namespace JoyConfig.ViewModels.Base;

public partial class EditorViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _title = "Tab";

    public LocalizationManager LocalizationManager => LocalizationManager.Instance;
}
