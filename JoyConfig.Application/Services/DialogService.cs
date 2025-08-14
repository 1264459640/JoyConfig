using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Layout;
using Avalonia.Media;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Infrastructure.Models.DTOs;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;

namespace JoyConfig.Application.Services;

public class DialogService : IDialogService
{
    public async Task<ConfirmationDialogResult> ShowConfirmationDialogAsync(ConfirmationDialogDto dialogData)
    {
        var result = new ConfirmationDialogResult();
        
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return result;
        }

        // 创建简单的确认对话框
        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };
        
        stackPanel.Children.Add(new TextBlock
        {
            Text = dialogData.Message,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400
        });
        
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };
        
        var confirmButton = new Button
        {
            Content = dialogData.ConfirmButtonText,
            IsDefault = true,
            MinWidth = 80
        };
        
        var cancelButton = new Button
        {
            Content = dialogData.CancelButtonText,
            IsCancel = true,
            MinWidth = 80
        };
        
        buttonPanel.Children.Add(confirmButton);
        if (dialogData.ShowCancelButton)
        {
            buttonPanel.Children.Add(cancelButton);
        }
        
        stackPanel.Children.Add(buttonPanel);
        
        var dialog = new Window
        {
            Title = dialogData.Title,
            Content = stackPanel,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInTaskbar = false
        };
        
        confirmButton.Click += (_, _) =>
        {
            result.IsConfirmed = true;
            dialog.Close();
        };
        
        cancelButton.Click += (_, _) =>
        {
            result.IsConfirmed = false;
            dialog.Close();
        };
        
        await dialog.ShowDialog(desktop.MainWindow);
        return result;
    }

    public async Task<InputDialogResult> ShowInputDialogAsync(InputDialogDto dialogData)
    {
        var result = new InputDialogResult();
        
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return result;
        }

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };
        
        if (!string.IsNullOrEmpty(dialogData.Prompt))
        {
            stackPanel.Children.Add(new TextBlock
            {
                Text = dialogData.Prompt,
                TextWrapping = TextWrapping.Wrap
            });
        }
        
        var textBox = new TextBox
        {
            Text = dialogData.DefaultValue,
            Watermark = dialogData.Placeholder,
            MaxLength = dialogData.MaxLength,
            PasswordChar = dialogData.IsPassword ? '●' : '\0',
            MinWidth = 300
        };
        
        stackPanel.Children.Add(textBox);
        
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };
        
        var okButton = new Button
        {
            Content = "确定",
            IsDefault = true,
            MinWidth = 80
        };
        
        var cancelButton = new Button
        {
            Content = "取消",
            IsCancel = true,
            MinWidth = 80
        };
        
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        stackPanel.Children.Add(buttonPanel);
        
        var dialog = new Window
        {
            Title = dialogData.Title,
            Content = stackPanel,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInTaskbar = false
        };
        
        okButton.Click += (_, _) =>
        {
            result.IsConfirmed = true;
            result.InputText = textBox.Text ?? string.Empty;
            dialog.Close();
        };
        
        cancelButton.Click += (_, _) =>
        {
            result.IsConfirmed = false;
            dialog.Close();
        };
        
        // 设置焦点到文本框
        dialog.Opened += (_, _) => textBox.Focus();
        
        await dialog.ShowDialog(desktop.MainWindow);
        return result;
    }

    public async Task<SelectAttributeDialogResult> ShowSelectAttributeDialogAsync(SelectAttributeDialogDto dialogData)
    {
        var result = new SelectAttributeDialogResult();
        
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return result;
        }

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };
        
        // 搜索框
        var searchBox = new TextBox
        {
            Watermark = "搜索属性...",
            Text = dialogData.FilterText
        };
        stackPanel.Children.Add(searchBox);
        
        // 属性列表
        var listBox = new ListBox
        {
            MinHeight = 300,
            MinWidth = 400,
            SelectionMode = dialogData.AllowMultipleSelection ? SelectionMode.Multiple : SelectionMode.Single
        };
        
        // 填充属性列表
        foreach (var attr in dialogData.AvailableAttributes)
        {
            var item = new ListBoxItem
            {
                Content = $"{attr.Id} - {attr.Category}",
                Tag = attr
            };
            
            if (dialogData.PreselectedAttributeIds.Contains(attr.Id))
            {
                item.IsSelected = true;
            }
            
            listBox.Items.Add(item);
        }
        
        stackPanel.Children.Add(listBox);
        
        // 搜索功能
        searchBox.TextChanged += (_, _) =>
        {
            var filterText = searchBox.Text?.ToLower() ?? string.Empty;
            foreach (ListBoxItem item in listBox.Items)
            {
                if (item.Tag is Attribute attr)
                {
                    item.IsVisible = string.IsNullOrEmpty(filterText) ||
                                   attr.Id.ToLower().Contains(filterText) ||
                                   attr.Category.ToLower().Contains(filterText) ||
                                   (attr.Description?.ToLower().Contains(filterText) ?? false);
                }
            }
        };
        
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };
        
        var okButton = new Button
        {
            Content = "确定",
            IsDefault = true,
            MinWidth = 80
        };
        
        var cancelButton = new Button
        {
            Content = "取消",
            IsCancel = true,
            MinWidth = 80
        };
        
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        stackPanel.Children.Add(buttonPanel);
        
        var dialog = new Window
        {
            Title = dialogData.Title,
            Content = stackPanel,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = true,
            Width = 500,
            Height = 450,
            ShowInTaskbar = false
        };
        
        okButton.Click += (_, _) =>
        {
            result.IsConfirmed = true;
            result.SelectedAttributes = listBox.SelectedItems
                .Cast<ListBoxItem>()
                .Where(item => item.Tag is Attribute)
                .Select(item => (Attribute)item.Tag!)
                .ToList();
            dialog.Close();
        };
        
        cancelButton.Click += (_, _) =>
        {
            result.IsConfirmed = false;
            dialog.Close();
        };
        
        await dialog.ShowDialog(desktop.MainWindow);
        return result;
    }

    public async Task<string?> ShowSelectTemplateDialogAsync()
    {
        // TODO: Implement this method properly
        // Currently returning null as a placeholder
        return null;
    }

    public async Task<string?> ShowOpenFileDialogAsync(string title, FilePickerFileType filter)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return null;
        }

        var result = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] { filter }
        });

        return result?.FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task ShowMessageBoxAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Content = new TextBlock { Text = message, Margin = new Thickness(20) },
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SizeToContent = SizeToContent.WidthAndHeight
        };

        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
        {
            await dialog.ShowDialog(desktop.MainWindow);
        }
    }
}
