using CommunityToolkit.Mvvm.ComponentModel;

namespace AttributeDatabaseEditor.ViewModels;

public partial class EditorViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _title = "Tab";
}
