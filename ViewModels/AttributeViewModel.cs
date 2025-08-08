using AttributeDatabaseEditor.Models.AttributeDatabase;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AttributeDatabaseEditor.ViewModels;

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
