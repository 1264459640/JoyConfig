using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JoyConfig.Models.AttributeDatabase;
using CommunityToolkit.Mvvm.ComponentModel;
using Attribute = JoyConfig.Models.AttributeDatabase.Attribute;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using JoyConfig.Services;
using Avalonia.Threading;

namespace JoyConfig.ViewModels;

public partial class AttributeDatabaseViewModel : EditorViewModelBase
{
    private bool _isProgrammaticSelection;
    public MainViewModel MainViewModel { get; }
    private readonly IDialogService _dialogService;

    public AttributeDatabaseViewModel(MainViewModel mainViewModel, IDialogService dialogService)
    {
        MainViewModel = mainViewModel;
        _dialogService = dialogService;
        
        _ = LoadDataAsync();
    }
    [ObservableProperty]
    private ObservableCollection<AttributeSet> _attributeSets = new();

    [ObservableProperty]
    private ObservableCollection<AttributeCategoryViewModel> _attributeCategories = new();

    [ObservableProperty]
    private string? _errorMessage;

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

        using var context = new AttributeDatabaseContext();
        switch (item)
        {
            case Attribute attribute:
                MainViewModel.CurrentEditor = new AttributeViewModel(attribute, this, _dialogService);
                break;
            case AttributeSet attributeSet:
                MainViewModel.CurrentEditor = await AttributeSetViewModel.CreateAsync(attributeSet.Id, this, _dialogService);
                break;
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "Loading...");
        try
        {
            using var context = new AttributeDatabaseContext();
            if (!await context.Database.CanConnectAsync())
            {
                await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "Database connection failed.");
                return;
            }
            
            var sets = await context.AttributeSets.AsNoTracking().ToListAsync();
            var attributes = await context.Attributes.AsNoTracking().ToListAsync();
            
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
                ErrorMessage = null;
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = $"An error occurred: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CreateNewCategory()
    {
        var vm = new CategoryViewModel(this);
        MainViewModel.OpenEditor(vm);
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
        
        MainViewModel.CurrentEditor = new AttributeViewModel(newAttribute, this, _dialogService);
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

        var newId = await _dialogService.ShowInputDialogAsync(inputVm);

        if (string.IsNullOrWhiteSpace(newId))
        {
            ErrorMessage = "Creation cancelled.";
            return;
        }

        await using var dbContext = new AttributeDatabaseContext();
        
        // Check if the ID already exists
        if (await dbContext.AttributeSets.AnyAsync(s => s.Id == newId))
        {
            ErrorMessage = $"Error: Attribute Set with ID '{newId}' already exists.";
            return;
        }

        var newSet = new AttributeSet
        {
            Id = newId,
            Name = "New Attribute Set", // Default name, can be changed in the editor
            Description = "A new attribute set created from the editor."
        };
        
        dbContext.AttributeSets.Add(newSet);
        await dbContext.SaveChangesAsync();
        
        // Refresh the list in the UI
        AttributeSets.Add(newSet);

        // Select and open the new set for editing
        SelectedAttributeSet = newSet;
        MainViewModel.CurrentEditor = await AttributeSetViewModel.CreateAsync(newSet.Id, this, _dialogService);
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

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);

        if (confirmed)
        {
            try
            {
                await using var context = new AttributeDatabaseContext();
                await context.DeleteAttributeSetAsync(attributeSet.Id);

                // If the deleted set was being edited, close the editor
                if (MainViewModel.CurrentEditor is AttributeSetViewModel vm && vm.AttributeSet.Id == attributeSet.Id)
                {
                    MainViewModel.CurrentEditor = null;
                }
                
                await RefreshAttributeSetsAsync();
                ErrorMessage = $"Attribute set '{attributeSet.Name}' deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting attribute set: {ex.Message}";
            }
        }
        else
        {
            ErrorMessage = "Deletion cancelled.";
        }
    }

    [RelayCommand]
    private async Task RenameCategoryAsync(AttributeCategoryViewModel? categoryVm)
    {
        if (categoryVm?.CategoryName is null) return;
        Console.WriteLine("[ViewModel] RenameCategoryAsync started.");

        var inputVm = new InputDialogViewModel
        {
            Title = "Rename Category",
            Message = $"Enter a new name for the '{categoryVm.CategoryName}' category:",
            InputText = categoryVm.CategoryName
        };

        var newName = await _dialogService.ShowInputDialogAsync(inputVm);
        Console.WriteLine($"[ViewModel] Input dialog returned: '{newName}'");

        if (string.IsNullOrWhiteSpace(newName) || newName == categoryVm.CategoryName)
        {
            ErrorMessage = "Rename cancelled or name unchanged.";
            Console.WriteLine("[ViewModel] Rename cancelled or name unchanged.");
            return; // Cancelled or no change
        }
        
        await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "New name received. Previewing changes...");

        // We need a dummy old/new ID to pass to the preview service. 
        // The service primarily uses the category names for this type of change.
        var dummyOldId = $"{categoryVm.CategoryName}.Dummy";
        var dummyNewId = $"{newName}.Dummy";

        using var context = new AttributeDatabaseContext();
        Console.WriteLine("[ViewModel] Calling PreviewAttributeChangeAsync...");
        var preview = await context.PreviewAttributeChangeAsync(dummyOldId, dummyNewId, categoryVm.CategoryName, newName);
        Console.WriteLine($"[ViewModel] PreviewAttributeChangeAsync returned. IsValid: {preview.IsValid}, Affected Attributes: {preview.AffectedAttributes.Count}");

        if (!preview.IsValid)
        {
            ErrorMessage = preview.ErrorMessage;
            return;
        }
        
        ErrorMessage = "Preview successful. Awaiting confirmation...";

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

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);
        Console.WriteLine($"[ViewModel] Confirmation dialog returned: {confirmed}");

        if (confirmed)
        {
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "Confirmation received. Executing changes...");
            
            using var writeContext = new AttributeDatabaseContext();
            Console.WriteLine("[ViewModel] Calling ExecuteAttributeChangeAsync...");
            await writeContext.ExecuteAttributeChangeAsync(preview);
            Console.WriteLine("[ViewModel] ExecuteAttributeChangeAsync finished.");
            
            Console.WriteLine("[ViewModel] Reloading data...");
            await LoadDataAsync(); // Reload the entire tree
            Console.WriteLine("[ViewModel] Data reloaded.");
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "Rename successful.");
        }
        else
        {
            ErrorMessage = "Rename cancelled by user.";
        }
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
        var category = AttributeCategories.FirstOrDefault(c => attributeId.StartsWith(c.CategoryName + "."));
        if (category != null)
        {
            var attribute = category.Attributes.FirstOrDefault(a => a.Id == attributeId);
            if (attribute != null)
            {
                _isProgrammaticSelection = true;
                
                // Open a new editor for this attribute
                MainViewModel.CurrentEditor = new AttributeViewModel(attribute, this, _dialogService);
                
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
        MainViewModel.CurrentEditor = new WelcomeViewModel();
    }

    public async Task RefreshAttributeSetsAsync()
    {
        try
        {
            using var context = new AttributeDatabaseContext();
            var sets = await context.AttributeSets.AsNoTracking().ToListAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AttributeSets = new ObservableCollection<AttributeSet>(sets);
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = $"Failed to refresh sets: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(AttributeCategoryViewModel? categoryVm)
    {
        if (categoryVm?.CategoryName is null) return;

        ErrorMessage = null;
        
        // Safety Check
        var attributeIds = categoryVm.Attributes.Select(a => a.Id).ToList();
        List<AttributeSet> referencingSets;
        var affectedValuesCount = 0;
        
        using (var context = new AttributeDatabaseContext())
        {
            referencingSets = await context.GetReferencingAttributeSetsAsync(attributeIds);
            if (attributeIds.Any())
            {
                affectedValuesCount = await context.AttributeValues
                    .AsNoTracking()
                    .CountAsync(v => attributeIds.Contains(v.AttributeId));
            }
        }

        var dialogViewModel = new ConfirmationDialogViewModel
        {
            Title = "Confirm Category Deletion",
            Message = $"You are about to delete the entire '{categoryVm.CategoryName}' category. " +
                      $"This will permanently delete {categoryVm.Attributes.Count} attribute(s) and {affectedValuesCount} value(s) across {referencingSets.Count} attribute set(s). " +
                      "This action cannot be undone.",
            Details = referencingSets.Select(s => $"Set: {s.Id} ({s.Name})").ToList()
        };
        
        var confirmed = await _dialogService.ShowConfirmationDialogAsync(dialogViewModel);

        if (confirmed)
        {
            try
            {
                using var context = new AttributeDatabaseContext();
                await context.DeleteCategoryAsync(categoryVm.CategoryName);
                await ReloadAndCloseEditorAsync();
                ErrorMessage = $"Category '{categoryVm.CategoryName}' deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during deletion: {ex.Message}";
            }
        }
        else
        {
            ErrorMessage = "Deletion cancelled.";
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
