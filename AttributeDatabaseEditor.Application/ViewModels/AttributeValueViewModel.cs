using CommunityToolkit.Mvvm.ComponentModel;
using JoyConfig.Core.Models.AttributeDatabase;

// 为AttributeDatabase中的AttributeValue类型创建别名，以避免可能的命名冲突
using AttributeValue = JoyConfig.Core.Models.AttributeDatabase.AttributeValue;

namespace JoyConfig.Application.ViewModels;

public partial class AttributeValueViewModel : ObservableObject
{
    [ObservableProperty]
    private AttributeValue _value;

    public AttributeValueViewModel(AttributeValue value)
    {
        _value = value;
    }
}
