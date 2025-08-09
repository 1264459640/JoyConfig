using JoyConfig.Models.AttributeDatabase;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JoyConfig.ViewModels;

public partial class AttributeViewModel : EditorViewModelBase
{
    [ObservableProperty]
    private Attribute _attribute;

    public AttributeViewModel(Attribute attribute)
    {
        _attribute = attribute;
        Title = $"Attribute: {attribute.Id}";
    }
}
