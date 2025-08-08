using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AttributeDatabaseEditor.Models.AttributeDatabase;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace AttributeDatabaseEditor.ViewModels;

public partial class AttributeDatabaseViewModel : ObservableObject
{
    public MainViewModel MainViewModel { get; }

    public AttributeDatabaseViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;
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
        if (value is not null)
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
            case Models.AttributeDatabase.Attribute attribute:
                MainViewModel.CurrentEditor = new AttributeViewModel(attribute);
                break;
            case AttributeSet attributeSet:
                MainViewModel.CurrentEditor = await AttributeSetViewModel.CreateAsync(attributeSet.Id);
                break;
        }
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        ErrorMessage = null;
        try
        {
            var baseDirectory = AppContext.BaseDirectory;
            var databasePath = Path.Combine(baseDirectory, "Example", "AttributeDatabase.db");

            if (!File.Exists(databasePath))
            {
                ErrorMessage = $"Database not found at: {databasePath}";
                return;
            }

            var options = new DbContextOptionsBuilder<Models.AttributeDatabase.AttributeDatabaseContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;

            await using var dbContext = new Models.AttributeDatabase.AttributeDatabaseContext(options);
            
            // Load Attribute Sets
            var sets = await dbContext.AttributeSets.ToListAsync();
            AttributeSets = new ObservableCollection<AttributeSet>(sets);

            // Load and group Attributes by Category
            var attributes = await dbContext.Attributes.ToListAsync();
            var grouped = attributes
                .GroupBy(a => a.Category)
                .Select(g => new AttributeCategoryViewModel 
                { 
                    CategoryName = g.Key, 
                    Attributes = new ObservableCollection<Models.AttributeDatabase.Attribute>(g) 
                });
            AttributeCategories = new ObservableCollection<AttributeCategoryViewModel>(grouped);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }
}

public partial class AttributeCategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _categoryName;

    [ObservableProperty]
    private ObservableCollection<Models.AttributeDatabase.Attribute> _attributes = new();

    [ObservableProperty]
    private bool _isExpanded = true; // Default to expanded
}
