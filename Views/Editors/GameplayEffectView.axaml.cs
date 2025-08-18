using System;
using Avalonia.Controls;
using Avalonia.LogicalTree;
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
        
        // 调试：监听DataContext变化
        this.DataContextChanged += OnDataContextChanged;
        System.Diagnostics.Debug.WriteLine("[DEBUG] GameplayEffectView构造完成");
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] GameplayEffectView DataContext变化: {DataContext?.GetType().Name}");
        if (DataContext is GameplayEffectViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ViewModel Id: {vm.Id}, AttributeModifiers Count: {vm.AttributeModifiers.Count}");
            
            // 直接设置ListBox的ItemsSource，绕过XAML绑定
            var listBox = this.FindControl<Avalonia.Controls.ListBox>("ModifiersListBox");
            if (listBox != null)
            {
                listBox.ItemsSource = vm.AttributeModifiers;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] 直接设置ListBox ItemsSource，集合数量: {vm.AttributeModifiers.Count}");
            }
            
            // 属性类型绑定问题已通过XAML修复
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