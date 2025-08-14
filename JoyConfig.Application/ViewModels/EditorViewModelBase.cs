using CommunityToolkit.Mvvm.ComponentModel;

namespace JoyConfig.Application.ViewModels;

public partial class EditorViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string? _title;
}
