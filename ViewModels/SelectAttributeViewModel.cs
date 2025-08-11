using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Services;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.ViewModels;

public partial class SelectAttributeViewModel : ObservableObject
{
    public LocalizationManager LocalizationManager { get; }
    
    [ObservableProperty]
    private ObservableCollection<Attribute> _availableAttributes = new();

    [ObservableProperty]
    private Attribute? _selectedAttribute;

    public SelectAttributeViewModel(IEnumerable<string> excludedAttributeIds)
    {
        LocalizationManager = LocalizationManager.Instance;
        _ = LoadAttributesAsync(excludedAttributeIds);
    }

    private async Task LoadAttributesAsync(IEnumerable<string> excludedAttributeIds)
    {
        await using var dbContext = new AttributeDatabaseContext();
        var attributes = await dbContext.Attributes
            .AsNoTracking()
            .Where(a => !excludedAttributeIds.Contains(a.Id))
            .ToListAsync();
        AvailableAttributes = new ObservableCollection<Attribute>(attributes);
    }
}
