using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Services;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.ViewModels;

public partial class AttributeSetViewModel : EditorViewModelBase
{
    private readonly AttributeDatabaseViewModel _parentViewModel;
    private readonly IDialogService _dialogService;
    private readonly string _attributeSetId;

    [ObservableProperty]
    private AttributeSet? _attributeSet;

    [ObservableProperty]
    private ObservableCollection<AttributeValueViewModel> _attributeValues = new();

    private AttributeSetViewModel(string attributeSetId, AttributeDatabaseViewModel parentViewModel, IDialogService dialogService)
    {
        _attributeSetId = attributeSetId;
        _parentViewModel = parentViewModel;
        _dialogService = dialogService;
    }

    public static async Task<AttributeSetViewModel> CreateAsync(string attributeSetId, AttributeDatabaseViewModel parentViewModel, IDialogService dialogService)
    {
        var viewModel = new AttributeSetViewModel(attributeSetId, parentViewModel, dialogService);
        await viewModel.InitializeAsync();
        return viewModel;
    }

    private async Task InitializeAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Models.AttributeDatabase.AttributeDatabaseContext.DbPath))
            {
                Title = "Error: Database path not set";
                return;
            }

            await using var dbContext = new Models.AttributeDatabase.AttributeDatabaseContext();

            AttributeSet = await dbContext.AttributeSets.FindAsync(_attributeSetId);
            if (AttributeSet is null)
            {
                Title = "Error: Set not found";
                return;
            }
            
            Title = $"Set: {AttributeSet.Name}";

            // Use a single, efficient query to get values and their related attributes.
            var query = from value in dbContext.AttributeValues
                        join attr in dbContext.Attributes on value.AttributeId equals attr.Id
                        where value.AttributeSetId == _attributeSetId
                        select new { Value = value, Attr = attr };
            
            var results = await query.ToListAsync();
            
            // The join ensures that Value.Attribute is correctly populated.
            // EF Core's relationship fix-up will automatically link the entities.
            foreach (var result in results)
            {
                result.Value.Attribute = result.Attr;
            }

            var viewModels = results.Select(r => new AttributeValueViewModel(r.Value, RemoveAttributeValueCommand));
            AttributeValues = new ObservableCollection<AttributeValueViewModel>(viewModels);
        }
        catch (Exception)
        {
            Title = "Error loading data";
            // Optionally, log the exception to a file or analytics service
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (AttributeSet == null)
        {
            await _dialogService.ShowMessageBoxAsync("Error", "AttributeSet is not loaded.");
            return;
        }

        try
        {
            await using var dbContext = new Models.AttributeDatabase.AttributeDatabaseContext();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            // 1. Update the AttributeSet's name and description
            await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE AttributeSets 
                SET Name = {AttributeSet.Name}, Description = {AttributeSet.Description} 
                WHERE Id = {AttributeSet.Id}");

            // 2. Update each AttributeValue individually
            foreach (var vm in AttributeValues)
            {
                var value = vm.AttributeValue;
                // Use the correct column name 'AttributeTypeComment' in raw SQL
                await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE AttributeValues 
                    SET BaseValue = {value.BaseValue}, MinValue = {value.MinValue}, MaxValue = {value.MaxValue}, AttributeTypeComment = {value.Comment} 
                    WHERE Id = {value.Id}");
            }

            await transaction.CommitAsync();
            await _dialogService.ShowMessageBoxAsync("Success", "Successfully saved changes.");
            
            // Refresh the parent list
            await _parentViewModel.RefreshAttributeSetsAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageBoxAsync("Error", $"An error occurred while saving: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddAttributeValue()
    {
        if (AttributeSet == null) return;

        var excludedIds = AttributeValues.Select(v => v.AttributeValue.AttributeId).ToList();
        var dialogViewModel = new SelectAttributeViewModel(excludedIds);

        var selectedAttribute = await _dialogService.ShowSelectAttributeDialogAsync(dialogViewModel);

        if (selectedAttribute != null)
        {
            try
            {
                await using var dbContext = new Models.AttributeDatabase.AttributeDatabaseContext();
                var newValue = new AttributeValue
                {
                    AttributeSetId = AttributeSet.Id,
                    AttributeId = selectedAttribute.Id,
                    AttributeCategory = selectedAttribute.Category // Ensure the NOT NULL field is populated
                };
                dbContext.AttributeValues.Add(newValue);
                await dbContext.SaveChangesAsync();
                
                // Refresh the list
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                // Improved error logging to show inner exception details
                var errorMessage = $"An error occurred while adding the attribute: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nInner Exception:\n{ex.InnerException.Message}";
                }
                await _dialogService.ShowMessageBoxAsync("Error", errorMessage);
            }
        }
    }

    [RelayCommand]
    private async Task RemoveAttributeValue(AttributeValueViewModel? attributeValueVm)
    {
        if (attributeValueVm == null) return;

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(new ConfirmationDialogViewModel
        {
            Title = "Confirm Deletion",
            Message = $"Are you sure you want to remove the attribute '{attributeValueVm.AttributeValue.AttributeId}' from this set?"
        });

        if (confirmed)
        {
            try
            {
                await using var dbContext = new Models.AttributeDatabase.AttributeDatabaseContext();
                // Use ExecuteSqlInterpolatedAsync for direct deletion
                await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM AttributeValues 
                    WHERE Id = {attributeValueVm.AttributeValue.Id}");
                
                // Refresh the list by removing the item directly from the collection
                // This is more efficient than reloading everything from the DB
                AttributeValues.Remove(attributeValueVm);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageBoxAsync("Error", $"An error occurred while removing the attribute: {ex.Message}");
            }
        }
    }
}
