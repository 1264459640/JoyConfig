using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JoyConfig.Models.AttributeDatabase;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.ViewModels;

public enum DisplayMode
{
    List,
    Card
}

public partial class AttributeSetViewModel : EditorViewModelBase
{
    [ObservableProperty]
    private AttributeSet? _attributeSet;

    [ObservableProperty]
    private ObservableCollection<AttributeValue> _attributeValues = new();
    
    private DisplayMode _currentDisplayMode = DisplayMode.Card;

    public DisplayMode CurrentDisplayMode
    {
        get => _currentDisplayMode;
        set
        {
            if (SetProperty(ref _currentDisplayMode, value))
            {
                OnPropertyChanged(nameof(IsCardViewVisible));
                OnPropertyChanged(nameof(IsListViewVisible));
            }
        }
    }

    public bool IsCardViewVisible => CurrentDisplayMode == DisplayMode.Card;
    public bool IsListViewVisible => CurrentDisplayMode == DisplayMode.List;

    private readonly string _attributeSetId;

    private AttributeSetViewModel(string attributeSetId)
    {
        _attributeSetId = attributeSetId;
    }

    public static async Task<AttributeSetViewModel> CreateAsync(string attributeSetId)
    {
        var viewModel = new AttributeSetViewModel(attributeSetId);
        await viewModel.InitializeAsync();
        return viewModel;
    }

    private async Task InitializeAsync()
    {
        if (string.IsNullOrEmpty(Models.AttributeDatabase.AttributeDatabaseContext.DbPath))
        {
            // Handle the case where the database path is not set
            Title = "Error: Database path not set";
            return;
        }

        await using var dbContext = new Models.AttributeDatabase.AttributeDatabaseContext();

        AttributeSet = await dbContext.AttributeSets.FindAsync(_attributeSetId);
        if (AttributeSet is null)
        {
            // Handle the case where the attribute set is not found
            Title = "Error: Set not found";
            return;
        }
        
        Title = $"Set: {AttributeSet.Name}";

        var values = await dbContext.AttributeValues
            .Where(v => v.AttributeSetId == _attributeSetId)
            .Include(v => v.AttributeTypeNavigation) // Eagerly load related attribute
            .ToListAsync();
        AttributeValues = new ObservableCollection<AttributeValue>(values);
    }
}
