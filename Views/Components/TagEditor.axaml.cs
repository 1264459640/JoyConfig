using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JoyConfig.Models.GameplayEffectDatabase;

namespace JoyConfig.Views.Components;

public partial class TagEditor : UserControl
{
    private readonly List<string> _tags = new();
    private readonly string[] _presetTags = TagTypes.All;

    public event EventHandler<string>? TagsChanged;

    /// <summary>
    /// TagsText依赖属性
    /// </summary>
    public static readonly StyledProperty<string> TagsTextProperty =
        AvaloniaProperty.Register<TagEditor, string>(nameof(TagsText), string.Empty);

    /// <summary>
    /// 获取或设置标签文本
    /// </summary>
    public string TagsText
    {
        get => GetValue(TagsTextProperty);
        set => SetValue(TagsTextProperty, value);
    }

    public TagEditor()
    {
        InitializeComponent();
        InitializePresetTags();
    }

    /// <summary>
    /// 属性变更处理
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TagsTextProperty)
        {
            var newValue = change.NewValue?.ToString() ?? string.Empty;
            UpdateTagsFromText(newValue);
        }
    }

    /// <summary>
    /// 从文本更新标签列表
    /// </summary>
    private void UpdateTagsFromText(string tagsText)
    {
        _tags.Clear();
        if (!string.IsNullOrWhiteSpace(tagsText))
        {
            var tags = tagsText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(t => t.Trim())
                              .Where(t => !string.IsNullOrEmpty(t))
                              .Distinct();
            _tags.AddRange(tags);
        }
        RefreshTagsDisplay();
    }

    /// <summary>
    /// 初始化预设标签
    /// </summary>
    private void InitializePresetTags()
    {
        var presetsPanel = this.FindControl<WrapPanel>("PresetsWrapPanel");
        if (presetsPanel == null) return;

        foreach (var presetTag in _presetTags)
        {
            var button = new Button
            {
                Content = presetTag,
                Classes = { "preset-tag" },
                Margin = new Avalonia.Thickness(4, 2),
                Padding = new Avalonia.Thickness(8, 4)
            };

            button.Click += (s, e) => AddTag(presetTag);
            presetsPanel.Children.Add(button);
        }
    }

    /// <summary>
    /// 刷新标签显示
    /// </summary>
    private void RefreshTagsDisplay()
    {
        var tagsPanel = this.FindControl<WrapPanel>("TagsWrapPanel");
        var emptyStateText = this.FindControl<TextBlock>("EmptyStateText");

        if (tagsPanel == null || emptyStateText == null) return;

        tagsPanel.Children.Clear();

        if (_tags.Count == 0)
        {
            emptyStateText.IsVisible = true;
            return;
        }

        emptyStateText.IsVisible = false;

        foreach (var tag in _tags)
        {
            var tagBorder = new Border
            {
                Classes = { "tag" },
                Child = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = tag,
                            Foreground = Avalonia.Media.Brushes.White,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                        },
                        new Button
                        {
                            Content = "×",
                            Classes = { "tag-remove" },
                            CommandParameter = tag
                        }
                    }
                }
            };

            // 为删除按钮添加点击事件
            var removeButton = (Button)((StackPanel)tagBorder.Child).Children[1];
            removeButton.Click += (s, e) => RemoveTag(tag);

            tagsPanel.Children.Add(tagBorder);
        }
    }

    /// <summary>
    /// 添加标签
    /// </summary>
    private void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || _tags.Contains(tag))
            return;

        _tags.Add(tag.Trim());
        var newTagsText = string.Join(",", _tags);
        SetValue(TagsTextProperty, newTagsText);
        RefreshTagsDisplay();
        TagsChanged?.Invoke(this, newTagsText);
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    private void RemoveTag(string tag)
    {
        if (_tags.Remove(tag))
        {
            var newTagsText = string.Join(",", _tags);
            SetValue(TagsTextProperty, newTagsText);
            RefreshTagsDisplay();
            TagsChanged?.Invoke(this, newTagsText);
        }
    }

    /// <summary>
    /// 添加标签按钮点击事件
    /// </summary>
    private void OnAddTagClick(object? sender, RoutedEventArgs e)
    {
        var textBox = this.FindControl<TextBox>("NewTagTextBox");
        if (textBox?.Text != null)
        {
            AddTag(textBox.Text);
            textBox.Text = string.Empty;
        }
    }

    /// <summary>
    /// 新标签文本框按键事件
    /// </summary>
    private void OnNewTagKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnAddTagClick(sender, e);
        }
    }

    /// <summary>
    /// 显示/隐藏预设标签面板
    /// </summary>
    private void OnShowPresetsClick(object? sender, RoutedEventArgs e)
    {
        var presetsPanel = this.FindControl<Border>("PresetsPanel");
        if (presetsPanel != null)
        {
            presetsPanel.IsVisible = !presetsPanel.IsVisible;
        }
    }
}