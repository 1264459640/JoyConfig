using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JoyConfig.Application.Abstract.Services;

// 为AttributeDatabase中的Attribute类型创建别名，以避免与System.Attribute冲突
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;

namespace JoyConfig.Application.ViewModels;

public partial class SelectAttributeViewModel : ObservableObject
{
    private readonly List<Attribute> _allAttributes;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<Attribute> _filteredAttributes;

    [ObservableProperty]
    private Attribute? _selectedAttribute;

    public ILocalizationService LocalizationManager { get; }
    public System.Collections.ObjectModel.ObservableCollection<Attribute> AvailableAttributes => FilteredAttributes;

    public SelectAttributeViewModel(List<Attribute> allAttributes, ILocalizationService localization)
    {
        _allAttributes = allAttributes;
        _filteredAttributes = new System.Collections.ObjectModel.ObservableCollection<Attribute>(allAttributes);
        LocalizationManager = localization;
    }

    partial void OnSearchTextChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredAttributes = new System.Collections.ObjectModel.ObservableCollection<Attribute>(_allAttributes);
        }
        else
        {
            FilteredAttributes = new System.Collections.ObjectModel.ObservableCollection<Attribute>(
                _allAttributes.Where(a => a.Id.Contains(value, System.StringComparison.OrdinalIgnoreCase)));
        }
    }
}
