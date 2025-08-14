using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JoyConfig.Infrastructure.Models.AttributeDatabase;
using JoyConfig.Infrastructure.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Text.Json;
using JoyConfig.Infrastructure.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Application.Services;
using Avalonia.Threading;

namespace JoyConfig.Application.ViewModels;

public partial class AttributeDatabaseViewModel : EditorViewModelBase
{
    private bool _isProgrammaticSelection;
    // public MainViewModel MainViewModel { get; } // Will be replaced by IUIService
    private readonly IDialogService _dialogService;
    private readonly IDataRepository _dataRepository;
    private readonly IUIService _uiService;
    
    public ILocalizationService LocalizationManager { get; }

    public AttributeDatabaseViewModel(IDialogService dialogService, IDataRepository dataRepository, IUIService uiService, ILocalizationService? localizationService = null)
    {
        _dialogService = dialogService;
        _dataRepository = dataRepository;
        _uiService = uiService;
        LocalizationManager = localizationService ?? new DefaultLocalizationService();
        
        _ = LoadDataAsync();
    }
    [ObservableProperty]
    private ObservableCollection<AttributeSet> _attributeSets = new();

    [ObservableProperty]
    private ObservableCollection<AttributeCategoryViewModel> _attributeCategories = new();

    [ObservableProperty]
    private AttributeSet? _selectedAttributeSet;

    [ObservableProperty]
    private object? _selectedAttribute; // Can be AttributeCategoryViewModel or Attribute

    partial void OnSelectedAttributeSetChanged(AttributeSet? value)
    {
        if (value is not null)
        {
            SelectedAttribute = null; // Clear other selection
            OpenEditorCommand.Execute(value);
        }
    }

    partial void OnSelectedAttributeChanged(object? value)
    {
        if (value is not null && !_isProgrammaticSelection)
        {
            SelectedAttributeSet = null; // Clear other selection
            OpenEditorCommand.Execute(value);
        }
    }

    [RelayCommand]
    public async Task OpenEditorAsync(object? item)
    {
        if (item is null) return;

        switch (item)
        {
            case Attribute attribute:
                // MainViewModel.CurrentEditor = new AttributeViewModel(attribute, this, _dialogService, _dbContext);
                break;
            case AttributeSet attributeSet:
                // MainViewModel.CurrentEditor = await AttributeSetViewModel.CreateAsync(attributeSet.Id, this, _dialogService);
                break;
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        // await Dispatcher.UIThread.InvokeAsync(() => MainViewModel.StatusMessage = "Loading...");
        try
        {
            var sets = await _dataRepository.GetAllAttributeSetsAsync();
            var attributes = await _dataRepository.GetAllAttributesAsync();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AttributeSets = new ObservableCollection<AttributeSet>(sets);
                var grouped = attributes
                    .GroupBy(a => a.Category)
                    .Select(g => new AttributeCategoryViewModel 
                    { 
                        CategoryName = g.Key, 
                        Attributes = new ObservableCollection<Attribute>(g) 
                    });
                AttributeCategories = new ObservableCollection<AttributeCategoryViewModel>(grouped);
                // MainViewModel.StatusMessage = "Ready";
            });
        }
        catch (Exception ex)
        {
            // await Dispatcher.UIThread.InvokeAsync(() => MainViewModel.StatusMessage = $"An error occurred: {ex.Message}");
            _uiService.SetStatusMessage($"An error occurred: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CreateNewCategory()
    {
        var vm = new CategoryViewModel(this);
        // MainViewModel.OpenEditor(vm);
    }

    public void AddNewCategory(string name)
    {
        if (AttributeCategories.Any(c => c.CategoryName == name))
        {
            // In a real app, the CategoryViewModel would handle this and show an error.
            return;
        }
        
        var newCategory = new AttributeCategoryViewModel
        {
            CategoryName = name,
            Attributes = new ObservableCollection<Attribute>()
        };
        AttributeCategories.Add(newCategory);
    }

    [RelayCommand]
    private void CreateNewAttributeInCategory(string? category)
    {
        category ??= "Uncategorized";

        var newAttribute = new Attribute
        {
            Id = "New.Attribute.Id", 
            Category = category,
            Description = "A new attribute definition."
        };
        
        // MainViewModel.CurrentEditor = new AttributeViewModel(newAttribute, this, _dialogService, _dbContext);
    }

    [RelayCommand]
    private async Task CreateFromTemplate()
    {
        var chosenTemplateName = await _dialogService.ShowSelectTemplateDialogAsync();

        if (string.IsNullOrWhiteSpace(chosenTemplateName))
        {
            // MainViewModel.StatusMessage = "Template selection cancelled.";
            return;
        }

        // var idVm = new InputDialogViewModel
        // {
        //     Title = "Enter New ID",
        //     Message = "Enter a unique ID for the new attribute set:",
        //     InputText = $"New.{chosenTemplateName}.Set"
        // };
        // var newId = await _dialogService.ShowInputDialogAsync(idVm);
        //
        // if (string.IsNullOrWhiteSpace(newId))
        // {
        //     // MainViewModel.StatusMessage = "Creation cancelled.";
        //     return;
        // }
        
        // For now, we'll just use a default ID
        var newId = $"New.{chosenTemplateName}.Set";

        try
        {
            if (await _dataRepository.AttributeSetExistsAsync(newId))
            {
                // MainViewModel.StatusMessage = $"Error: Attribute Set with ID '{newId}' already exists.";
                return;
            }

            var templatesPath = Path.Combine(AppContext.BaseDirectory, "Templates");
            var templateFilePath = Path.Combine(templatesPath, $"{chosenTemplateName}.json");
            var json = await File.ReadAllTextAsync(templateFilePath);
            var template = JsonSerializer.Deserialize<AttributeSetTemplate>(json);

            if (template == null)
            {
                // MainViewModel.StatusMessage = "Failed to read the template file.";
                return;
            }

            var newSet = new AttributeSet
            {
                Id = newId,
                Name = template.Name,
                Description = template.Description
            };

            foreach (var valTemplate in template.AttributeValues)
            {
                newSet.AttributeValues.Add(new AttributeValue
                {
                    AttributeId = valTemplate.AttributeId,
                    AttributeCategory = valTemplate.AttributeCategory,
                    BaseValue = valTemplate.BaseValue,
                    MinValue = valTemplate.MinValue,
                    MaxValue = valTemplate.MaxValue,
                    Comment = valTemplate.Comment
                });
            }

            await _dataRepository.AddAttributeSetAsync(newSet);

            await RefreshAttributeSetsAsync();
            SelectedAttributeSet = newSet;
            // MainViewModel.CurrentEditor = await AttributeSetViewModel.CreateAsync(newSet.Id, this, _dialogService);
        }
        catch (Exception ex)
        {
            // MainViewModel.StatusMessage = $"Failed to create from template: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateNewAttributeSet()
    {
        var inputVm = new InputDialogViewModel
        {
            Title = "Create New Attribute Set",
            Message = "Enter a unique ID for the new attribute set:",
            InputText = "New.AttributeSet.Id"
        };

        // var newId = await _dialogService.ShowInputDialogAsync(inputVm);
        //
        // if (string.IsNullOrWhiteSpace(newId))
        // {
        //     // MainViewModel.StatusMessage = "Creation cancelled.";
        //     return;
        // }
        
        // For now, we'll just use a default ID
        var newId = "NewAttributeSet";

        // Check if the ID already exists
        if (await _dataRepository.AttributeSetExistsAsync(newId))
        {
            // MainViewModel.StatusMessage = $"Error: Attribute Set with ID '{newId}' already exists.";
            return;
        }

        var newSet = new AttributeSet
        {
            Id = newId,
            Name = "New Attribute Set", // Default name, can be changed in the editor
            Description = "A new attribute set created from the editor."
        };
        
        await _dataRepository.AddAttributeSetAsync(newSet);
        
        // Refresh the list in the UI
        AttributeSets.Add(newSet);

        // Select and open the new set for editing
        SelectedAttributeSet = newSet;
        // MainViewModel.CurrentEditor = await AttributeSetViewModel.CreateAsync(newSet.Id, this, _dialogService);
    }

    [RelayCommand]
    private async Task DeleteAttributeSetAsync(AttributeSet? attributeSet)
    {
        if (attributeSet is null) return;

        var confirmVm = new ConfirmationDialogViewModel
        {
            Title = "Confirm Deletion",
            Message = $"Are you sure you want to delete the attribute set '{attributeSet.Name}' ({attributeSet.Id})? This will also remove all associated attribute values. This action cannot be undone."
        };

        // var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);
        //
        // if (confirmed)
        // {
        //     try
        //     {
        //         await _dbContext.DeleteAttributeSetAsync(attributeSet.Id);
        //
        //         // If the deleted set was being edited, close the editor
        //         // if (MainViewModel.CurrentEditor is AttributeSetViewModel vm && vm.AttributeSet?.Id == attributeSet.Id)
        //         // {
        //         //     MainViewModel.CurrentEditor = null;
        //         // }
        //         
        //         await RefreshAttributeSetsAsync();
        //         // MainViewModel.StatusMessage = $"Attribute set '{attributeSet.Name}' deleted successfully.";
        //     }
        //     catch (Exception ex)
        //     {
        //         // MainViewModel.StatusMessage = $"Error deleting attribute set: {ex.Message}";
        //     }
        // }
        // else
        // {
        //     // MainViewModel.StatusMessage = "Deletion cancelled.";
        // }
        
        // For now, we'll just execute the deletion without confirmation
        try
        {
            await _dataRepository.DeleteAttributeSetAsync(attributeSet.Id);
            await RefreshAttributeSetsAsync();
        }
        catch (Exception ex)
        {
            // MainViewModel.StatusMessage = $"Error deleting attribute set: {ex.Message}";
            _uiService.SetStatusMessage($"Error deleting attribute set: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RenameCategoryAsync(AttributeCategoryViewModel? categoryVm)
    {
        if (categoryVm?.CategoryName is null) return;
        Console.WriteLine("[ViewModel] RenameCategoryAsync started.");

        // var inputVm = new InputDialogViewModel
        // {
        //     Title = "Rename Category",
        //     Message = $"Enter a new name for the '{categoryVm.CategoryName}' category:",
        //     InputText = categoryVm.CategoryName
        // };

        // var newName = await _dialogService.ShowInputDialogAsync(inputVm);
        // Console.WriteLine($"[ViewModel] Input dialog returned: '{newName}'");
        
        // For now, we'll just use a default name
        var newName = "RenamedCategory";
        Console.WriteLine($"[ViewModel] Using default name: '{newName}'");

        // if (string.IsNullOrWhiteSpace(newName) || newName == categoryVm.CategoryName)
        // {
        //     // MainViewModel.StatusMessage = "Rename cancelled or name unchanged.";
        //     Console.WriteLine("[ViewModel] Rename cancelled or name unchanged.");
        //     return; // Cancelled or no change
        // }
        
        // await Dispatcher.UIThread.InvokeAsync(() => MainViewModel.StatusMessage = "New name received. Previewing changes...");

        // We need a dummy old/new ID to pass to the preview service. 
        // The service primarily uses the category names for this type of change.
        var dummyOldId = $"{categoryVm.CategoryName}.Dummy";
        var dummyNewId = $"{newName}.Dummy";

        Console.WriteLine("[ViewModel] Calling PreviewAttributeChangeAsync...");
        if (categoryVm.CategoryName is null)
        {
            return;
        }
        var preview = await _dataRepository.PreviewAttributeChangeAsync(dummyOldId, dummyNewId, categoryVm.CategoryName, newName);
        Console.WriteLine($"[ViewModel] PreviewAttributeChangeAsync returned. IsValid: {preview.IsValid}, Affected Attributes: {preview.AffectedAttributes.Count}");

        if (!preview.IsValid)
        {
            // MainViewModel.StatusMessage = preview.ErrorMessage;
            return;
        }
        
        // MainViewModel.StatusMessage = "Preview successful. Awaiting confirmation...";

        if (!preview.AffectedAttributes.Any())
        {
            // This case is for renaming an empty category.
            // We can just update the view model directly.
            categoryVm.CategoryName = newName;
            return;
        }

        var confirmVm = new ConfirmationDialogViewModel
        {
            Title = "Confirm Category Rename",
            Message = $"Renaming '{categoryVm.CategoryName}' to '{newName}' will update {preview.AffectedAttributes.Count} attribute(s) and {preview.AffectedValueCount} value(s) across {preview.AffectedAttributeSets.Count} attribute set(s). This action cannot be undone.",
            Details = preview.AffectedAttributeSets.Select(s => $"Set: {s.Id} ({s.Name})").ToList()
        };

        // var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);
        // Console.WriteLine($"[ViewModel] Confirmation dialog returned: {confirmed}");
        //
        // if (confirmed)
        // {
        //     // await Dispatcher.UIThread.InvokeAsync(() => MainViewModel.StatusMessage = "Confirmation received. Executing changes...");
        //     
        //     Console.WriteLine("[ViewModel] Calling ExecuteAttributeChangeAsync...");
        //     await _dbContext.ExecuteAttributeChangeAsync(preview);
        //     Console.WriteLine("[ViewModel] ExecuteAttributeChangeAsync finished.");
        //     
        //     Console.WriteLine("[ViewModel] Reloading data...");
        //     await LoadDataAsync(); // Reload the entire tree
        //     Console.WriteLine("[ViewModel] Data reloaded.");
        //     // await Dispatcher.UIThread.InvokeAsync(() => MainViewModel.StatusMessage = "Rename successful.");
        // }
        // else
        // {
        //     // MainViewModel.StatusMessage = "Rename cancelled by user.";
        // }
        
        // For now, we'll just execute the changes without confirmation
        Console.WriteLine("[ViewModel] Calling ExecuteAttributeChangeAsync...");
        await _dataRepository.ExecuteAttributeChangeAsync(preview);
        Console.WriteLine("[ViewModel] ExecuteAttributeChangeAsync finished.");
        
        Console.WriteLine("[ViewModel] Reloading data...");
        await LoadDataAsync(); // Reload the entire tree
        Console.WriteLine("[ViewModel] Data reloaded.");
    }

    public async Task RefreshAttributeListAsync(Attribute attribute)
    {
        var categoryVm = AttributeCategories.FirstOrDefault(c => c.CategoryName == attribute.Category);
        if (categoryVm != null)
        {
            // Category exists, check if the attribute is already in the list
            var attrInList = categoryVm.Attributes.FirstOrDefault(a => a.Id == attribute.Id);
            if (attrInList == null)
            {
                // Add new attribute to existing category
                categoryVm.Attributes.Add(attribute);
            }
            // If it exists, it's an update, and the data binding will handle UI changes.
        }
        else
        {
            // Category does not exist, create a new one
            categoryVm = new AttributeCategoryViewModel
            {
                CategoryName = attribute.Category,
                Attributes = new ObservableCollection<Attribute> { attribute }
            };
            AttributeCategories.Add(categoryVm);
        }

        // Ensure the category is expanded and the item is selected
        categoryVm.IsExpanded = true;
        await Task.Delay(100); // Give UI time to render the new item
        SelectedAttribute = attribute;
    }
    
    public async Task ReloadAndSelectAttributeAsync(string attributeId)
    {
        await LoadDataAsync();
        
        // Find the attribute in the reloaded data
        var category = AttributeCategories.FirstOrDefault(c => c.CategoryName != null && attributeId.StartsWith(c.CategoryName + "."));
        if (category != null)
        {
            var attribute = category.Attributes.FirstOrDefault(a => a.Id == attributeId);
            if (attribute != null)
            {
                _isProgrammaticSelection = true;
                
                // Open a new editor for this attribute
                // if (MainViewModel != null)
                // {
                //     MainViewModel.CurrentEditor = new AttributeViewModel(attribute, this, _dialogService, _dbContext);
                // }
                
                // Also update the selection in the list
                SelectedAttribute = attribute;
                category.IsExpanded = true;
                
                _isProgrammaticSelection = false;
            }
        }
    }

    public async Task ReloadAndCloseEditorAsync()
    {
        await LoadDataAsync();
        // MainViewModel.CurrentEditor = new WelcomeViewModel();
    }

    public async Task RefreshAttributeSetsAsync()
    {
        try
        {
            var sets = await _dataRepository.GetAllAttributeSetsAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AttributeSets = new ObservableCollection<AttributeSet>(sets);
            });
        }
        catch (Exception ex)
        {
            // await Dispatcher.UIThread.InvokeAsync(() => MainViewModel.StatusMessage = $"Failed to refresh sets: {ex.Message}");
            _uiService.SetStatusMessage($"Failed to refresh sets: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(AttributeCategoryViewModel? categoryVm)
    {
        if (categoryVm?.CategoryName is null) return;

        // MainViewModel.StatusMessage = null;
        
        // Safety Check
        var attributeIds = categoryVm.Attributes.Select(a => a.Id).ToList();
        List<AttributeSet> referencingSets;
        var affectedValuesCount = 0;
        
        referencingSets = await _dataRepository.GetReferencingAttributeSetsAsync(attributeIds);
        if (attributeIds.Any())
        {
            affectedValuesCount = await _dataRepository.GetAttributeValuesCountAsync(attributeIds);
        }

        var dialogViewModel = new ConfirmationDialogViewModel
        {
            Title = "Confirm Category Deletion",
            Message = $"You are about to delete the entire '{categoryVm.CategoryName}' category. " +
                      $"This will permanently delete {categoryVm.Attributes.Count} attribute(s) and {affectedValuesCount} value(s) across {referencingSets.Count} attribute set(s). " +
                      "This action cannot be undone.",
            Details = referencingSets.Select(s => $"Set: {s.Id} ({s.Name})").ToList()
        };
        
        // var confirmed = await _dialogService.ShowConfirmationDialogAsync(dialogViewModel);
        //
        // if (confirmed)
        // {
        //     try
        //     {
        //         await _dbContext.DeleteCategoryAsync(categoryVm.CategoryName);
        //         await ReloadAndCloseEditorAsync();
        //         // MainViewModel.StatusMessage = $"Category '{categoryVm.CategoryName}' deleted successfully.";
        //     }
        //     catch (Exception ex)
        //     {
        //         // MainViewModel.StatusMessage = $"An error occurred during deletion: {ex.Message}";
        //     }
        // }
        // else
        // {
        //     // MainViewModel.StatusMessage = "Deletion cancelled.";
        // }
        
        // For now, we'll just execute the deletion without confirmation
        try
        {
            await _dataRepository.DeleteCategoryAsync(categoryVm.CategoryName);
            await ReloadAndCloseEditorAsync();
            // MainViewModel.StatusMessage = $"Category '{categoryVm.CategoryName}' deleted successfully.";
        }
        catch (Exception ex)
        {
            // MainViewModel.StatusMessage = $"An error occurred during deletion: {ex.Message}";
            _uiService.SetStatusMessage($"An error occurred during deletion: {ex.Message}");
        }
    }
}

public partial class AttributeCategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _categoryName;

    [ObservableProperty]
    private ObservableCollection<Attribute> _attributes = new();

    [ObservableProperty]
    private bool _isExpanded = false; // Default to collapsed
}
