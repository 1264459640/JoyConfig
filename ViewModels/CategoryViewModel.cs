using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using JoyConfig.Services;

namespace JoyConfig.ViewModels;

public partial class CategoryViewModel : EditorViewModelBase
{
    private readonly AttributeDatabaseViewModel _parentViewModel;

    [ObservableProperty]
    private string _categoryName;

    [ObservableProperty]
    private string? _errorMessage;

    public CategoryViewModel(AttributeDatabaseViewModel parentViewModel)
    {
        _parentViewModel = parentViewModel;
        _categoryName = "New.Category";
        Title = "New Category";
    }

    [RelayCommand]
    private void Save()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            ErrorMessage = "Category name cannot be empty.";
            return;
        }

        // In a real app, we would also check for duplicate names here.
        // This requires access to the list of categories in the parent view model.
        
        // For now, we just create it.
        _parentViewModel.AddNewCategory(CategoryName);
        
        // TODO: Close this editor tab.
    }
}
