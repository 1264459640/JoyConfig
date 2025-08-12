using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JoyConfig.Application.ViewModels;

public partial class CategoryViewModel : EditorViewModelBase
{
    private readonly AttributeDatabaseViewModel _parentVm;

    [ObservableProperty]
    private string? _categoryName;

    public CategoryViewModel(AttributeDatabaseViewModel parentVm)
    {
        _parentVm = parentVm;
        Title = "Create New Category";
    }

    [RelayCommand]
    private void CreateCategory()
    {
        if (!string.IsNullOrWhiteSpace(CategoryName))
        {
            _parentVm.AddNewCategory(CategoryName);
            // Close the editor
            // This could be done via a message or a direct call to a method on the MainViewModel
        }
    }
}
