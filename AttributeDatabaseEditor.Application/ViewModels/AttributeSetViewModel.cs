using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using JoyConfig.Core.Services;
using JoyConfig.Core.Models.AttributeDatabase;

// 为AttributeDatabase中的类型创建别名，以避免与System命名空间的类型冲突
using Attribute = JoyConfig.Core.Models.AttributeDatabase.Attribute;
using AttributeSet = JoyConfig.Core.Models.AttributeDatabase.AttributeSet;
using AttributeValue = JoyConfig.Core.Models.AttributeDatabase.AttributeValue;
using AttributeValueViewModel = JoyConfig.Application.ViewModels.AttributeValueViewModel;

namespace JoyConfig.Application.ViewModels;

public partial class AttributeSetViewModel : EditorViewModelBase
{
    private readonly AttributeDatabaseViewModel _parentVm;
    private readonly IDialogService _dialogService;
    private readonly DbContextOptions<AttributeDatabaseContext> _dbContextOptions;

    [ObservableProperty]
    private AttributeSet? _attributeSet;

    [ObservableProperty]
    private ObservableCollection<AttributeValueViewModel> _attributeValues = new();

    [ObservableProperty]
    private string? _searchText;

    public AttributeSetViewModel(AttributeSet attributeSet, AttributeDatabaseViewModel parentVm, IDialogService dialogService, DbContextOptions<AttributeDatabaseContext> dbContextOptions)
    {
        _attributeSet = attributeSet;
        _parentVm = parentVm;
        _dialogService = dialogService;
        _dbContextOptions = dbContextOptions;
        Title = attributeSet.Name;
        
        LoadAttributeValues();
    }

    public static async Task<AttributeSetViewModel> CreateAsync(string attributeSetId, AttributeDatabaseViewModel parentVm, IDialogService dialogService)
    {
        var dbContextOptions = new DbContextOptionsBuilder<AttributeDatabaseContext>()
            .UseSqlite("Data Source=Example/AttributeDatabase.db")
            .Options;
        
        await using var dbContext = new AttributeDatabaseContext(dbContextOptions);
        var attributeSet = await dbContext.AttributeSets
            .Include(s => s.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .FirstOrDefaultAsync(s => s.Id == attributeSetId);

        if (attributeSet == null)
        {
            throw new ArgumentException("AttributeSet not found");
        }

        return new AttributeSetViewModel(attributeSet, parentVm, dialogService, dbContextOptions);
    }

    private void LoadAttributeValues()
    {
        if (AttributeSet == null) return;

        var filteredValues = string.IsNullOrWhiteSpace(SearchText)
            ? AttributeSet.AttributeValues
            : AttributeSet.AttributeValues.Where(v =>
                v.AttributeId.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (v.Comment?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        AttributeValues = new ObservableCollection<AttributeValueViewModel>(
            filteredValues.Select(v => new AttributeValueViewModel(v)));
    }

    partial void OnSearchTextChanged(string? value)
    {
        LoadAttributeValues();
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (AttributeSet == null) return;

        await using var dbContext = new AttributeDatabaseContext(_dbContextOptions);
        
        // Update existing values
        foreach (var vm in AttributeValues)
        {
            var entity = await dbContext.AttributeValues.FindAsync(vm.Value.Id);
            if (entity != null)
            {
                entity.BaseValue = vm.Value.BaseValue;
                entity.MinValue = vm.Value.MinValue;
                entity.MaxValue = vm.Value.MaxValue;
                entity.Comment = vm.Value.Comment;
            }
        }
        
        await dbContext.SaveChangesAsync();
        // Optionally, show a status message
    }

    [RelayCommand]
    private async Task AddAttributeValueAsync()
    {
        await using var dbContext = new AttributeDatabaseContext(_dbContextOptions);
        var allAttributes = await dbContext.Attributes.ToListAsync();
        
        // var selectVm = new SelectAttributeViewModel(allAttributes);
        // var selectedAttribute = await _dialogService.ShowSelectAttributeDialogAsync(selectVm);
        //
        // if (selectedAttribute != null && AttributeSet != null)
        // {
        //     if (AttributeSet.AttributeValues.Any(v => v.AttributeId == selectedAttribute.Id))
        //     {
        //         // Attribute already exists in this set
        //         return;
        //     }
        //
        //     var newValue = new AttributeValue
        //     {
        //         AttributeSetId = AttributeSet.Id,
        //         AttributeId = selectedAttribute.Id,
        //         AttributeCategory = selectedAttribute.Category,
        //         // BaseValue, MinValue, MaxValue will use defaults
        //     };
        
        // For now, we'll just add a default attribute
        if (AttributeSet != null && allAttributes.Any())
        {
            var selectedAttribute = allAttributes.First();
            if (AttributeSet.AttributeValues.Any(v => v.AttributeId == selectedAttribute.Id))
            {
                // Attribute already exists in this set
                return;
            }

            var newValue = new AttributeValue
            {
                AttributeSetId = AttributeSet.Id,
                AttributeId = selectedAttribute.Id,
                AttributeCategory = selectedAttribute.Category,
                // BaseValue, MinValue, MaxValue will use defaults
            };

            dbContext.AttributeValues.Add(newValue);
            await dbContext.SaveChangesAsync();

            // Refresh UI
            AttributeSet.AttributeValues.Add(newValue);
            LoadAttributeValues();
        }
    }

    [RelayCommand]
    private async Task RemoveAttributeValueAsync(AttributeValueViewModel? valueVm)
    {
        if (valueVm == null || AttributeSet == null) return;

        await using var dbContext = new AttributeDatabaseContext(_dbContextOptions);
        var entity = await dbContext.AttributeValues.FindAsync(valueVm.Value.Id);
        if (entity != null)
        {
            dbContext.AttributeValues.Remove(entity);
            await dbContext.SaveChangesAsync();

            // Refresh UI
            var toRemove = AttributeSet.AttributeValues.FirstOrDefault(v => v.Id == valueVm.Value.Id);
            if (toRemove != null)
            {
                AttributeSet.AttributeValues.Remove(toRemove);
                LoadAttributeValues();
            }
        }
    }
}
