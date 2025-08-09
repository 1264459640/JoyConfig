using CommunityToolkit.Mvvm.ComponentModel;

namespace JoyConfig.ViewModels;

public partial class EditorViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _title = "Tab";
}
