using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Services;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.ViewModels.AttributeDatabase;

public partial class SelectAttributeViewModel : ObservableObject
{
    private readonly IAttributeRepository _attributeRepository;
    public LocalizationManager LocalizationManager { get; }
    
    [ObservableProperty]
    private ObservableCollection<Attribute> _availableAttributes = new();

    [ObservableProperty]
    private Attribute? _selectedAttribute;

    public SelectAttributeViewModel(IAttributeRepository attributeRepository, IEnumerable<string> excludedAttributeIds)
    {
        _attributeRepository = attributeRepository;
        LocalizationManager = LocalizationManager.Instance;
        _ = LoadAttributesAsync(excludedAttributeIds);
    }

    private async Task LoadAttributesAsync(IEnumerable<string> excludedAttributeIds)
    {
        var attributes = await _attributeRepository.GetAllAttributesAsync();
        var filteredAttributes = attributes
            .Where(a => !excludedAttributeIds.Contains(a.Id))
            .ToList();
        AvailableAttributes = new ObservableCollection<Attribute>(filteredAttributes);
    }
}
