using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Application.Services;
using JoyConfig.Infrastructure.Models.AttributeDatabase;

// 为AttributeDatabase中的AttributeValue类型创建别名，以避免可能的命名冲突
using AttributeValue = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeValue;

namespace JoyConfig.Application.ViewModels;

public partial class AttributeValueViewModel : ObservableObject
{
    [ObservableProperty]
    private AttributeValue _value;
    
    public ILocalizationService LocalizationManager { get; }
    public AttributeValue AttributeValue => Value;
    
    [RelayCommand]
    private void Remove()
    {
        // TODO: Implement remove logic
    }

    public AttributeValueViewModel(AttributeValue value, ILocalizationService? localizationService = null)
    {
        _value = value;
        LocalizationManager = localizationService ?? new DefaultLocalizationService();
    }
}
