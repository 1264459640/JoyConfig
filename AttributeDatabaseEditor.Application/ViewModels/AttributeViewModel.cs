using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using JoyConfig.Core.Services;
using JoyConfig.Core.Models.AttributeDatabase;

// 为AttributeDatabase中的Attribute类型创建别名，以避免与System.Attribute冲突
using Attribute = JoyConfig.Core.Models.AttributeDatabase.Attribute;
using AttributeSet = JoyConfig.Core.Models.AttributeDatabase.AttributeSet;
using AttributeValue = JoyConfig.Core.Models.AttributeDatabase.AttributeValue;

namespace JoyConfig.Application.ViewModels;

public partial class AttributeViewModel : EditorViewModelBase
{
    private readonly AttributeDatabaseViewModel _parentVm;
    private readonly IDialogService _dialogService;
    private readonly DbContextOptions<AttributeDatabaseContext> _dbContextOptions;

    [ObservableProperty]
    private Attribute _attribute;

    [ObservableProperty]
    private List<AttributeSet> _referencingAttributeSets = new();

    public AttributeViewModel(Attribute attribute, AttributeDatabaseViewModel parentVm, IDialogService dialogService, DbContextOptions<AttributeDatabaseContext> dbContextOptions)
    {
        _attribute = attribute;
        _parentVm = parentVm;
        _dialogService = dialogService;
        _dbContextOptions = dbContextOptions;
        Title = $"Attribute: {attribute.Id}";
        
        LoadReferencingAttributeSets();
    }

    private async void LoadReferencingAttributeSets()
    {
        await using var dbContext = new AttributeDatabaseContext(_dbContextOptions);
        ReferencingAttributeSets = await dbContext.AttributeValues
            .Where(v => v.AttributeId == Attribute.Id)
            .Select(v => v.AttributeSet)
            .Distinct()
            .ToListAsync();
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        await using var dbContext = new AttributeDatabaseContext(_dbContextOptions);
        dbContext.Attributes.Update(Attribute);
        await dbContext.SaveChangesAsync();
        await _parentVm.ReloadAndSelectAttributeAsync(Attribute.Id);
    }

    [RelayCommand]
    private async Task DeleteAttributeAsync()
    {
        var confirmVm = new ConfirmationDialogViewModel
        {
            Title = "Confirm Deletion",
            Message = $"Are you sure you want to delete the attribute '{Attribute.Id}'? This will remove it from all attribute sets. This action cannot be undone."
        };

        // var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);
        //
        // if (confirmed)
        // {
        //     await using var dbContext = new AttributeDatabaseContext(_dbContextOptions);
        //     var attribute = await dbContext.Attributes.FindAsync(Attribute.Id);
        //     if (attribute != null)
        //     {
        //         dbContext.Attributes.Remove(attribute);
        //         await dbContext.SaveChangesAsync();
        //         await _parentVm.ReloadAndCloseEditorAsync();
        //     }
        // }
        
        // For now, we'll just execute the deletion without confirmation
        await using var dbContext = new AttributeDatabaseContext(_dbContextOptions);
        var attribute = await dbContext.Attributes.FindAsync(Attribute.Id);
        if (attribute != null)
        {
            dbContext.Attributes.Remove(attribute);
            await dbContext.SaveChangesAsync();
            await _parentVm.ReloadAndCloseEditorAsync();
        }
    }
}
