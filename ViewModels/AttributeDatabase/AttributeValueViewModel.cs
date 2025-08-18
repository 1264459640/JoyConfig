using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Services;

namespace JoyConfig.ViewModels.AttributeDatabase;

public partial class AttributeValueViewModel : ObservableObject
{
    public LocalizationManager LocalizationManager { get; }

    [ObservableProperty]
    private AttributeValue _attributeValue;

    [ObservableProperty]
    private string _attributeName = string.Empty;

    [ObservableProperty]
    private string _attributeCategory = string.Empty;

    public ICommand? RemoveCommand { get; }

    public AttributeValueViewModel(AttributeValue attributeValue, ICommand? removeCommand)
    {
        _attributeValue = attributeValue;
        RemoveCommand = removeCommand;
        LocalizationManager = LocalizationManager.Instance;
    }
}
