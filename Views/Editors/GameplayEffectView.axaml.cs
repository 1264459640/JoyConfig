using Avalonia.Controls;
using JoyConfig.ViewModels.GameplayEffectDatabase;
using JoyConfig.Views.Components;

namespace JoyConfig.Views.Editors;

public partial class GameplayEffectView : UserControl
{
    public GameplayEffectView()
    {
        InitializeComponent();
        
        // 订阅TagEditor的TagsChanged事件
        var tagEditor = this.FindControl<TagEditor>("TagEditor");
        if (tagEditor != null)
        {
            tagEditor.TagsChanged += OnTagsChanged;
        }
    }
    
    private void OnTagsChanged(object? sender, string tagsText)
    {
        if (DataContext is GameplayEffectViewModel viewModel)
        {
            viewModel.Tags = tagsText;
        }
    }
}