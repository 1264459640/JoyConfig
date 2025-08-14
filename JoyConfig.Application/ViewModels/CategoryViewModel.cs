using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Application.Services;

namespace JoyConfig.Application.ViewModels;

public partial class CategoryViewModel : EditorViewModelBase
{
    private readonly AttributeDatabaseViewModel _parentVm;

    [ObservableProperty]
    private string? _categoryName;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    public ILocalizationService LocalizationManager { get; }
    public IRelayCommand SaveCommand => CreateCategoryCommand;

    public CategoryViewModel(AttributeDatabaseViewModel parentVm, ILocalizationService? localizationService = null)
    {
        _parentVm = parentVm;
        LocalizationManager = localizationService ?? new DefaultLocalizationService();
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
