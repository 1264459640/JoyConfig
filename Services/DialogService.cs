using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.ViewModels.AttributeDatabase;
using JoyConfig.ViewModels.Dialogs;
using JoyConfig.Views;

namespace JoyConfig.Services;

public class DialogService : IDialogService
{
    public async Task<bool> ShowConfirmationDialogAsync(ConfirmationDialogViewModel viewModel)
    {
        Console.WriteLine("[DialogService] ShowConfirmationDialogAsync called.");
        var dialog = new Window
        {
            Title = viewModel.Title,
            Content = new ConfirmationDialogView
            {
                DataContext = viewModel
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SizeToContent = SizeToContent.WidthAndHeight
        };

        viewModel.CloseCommand = new RelayCommand<object?>(_ =>
        {
            dialog.Close();
        });

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
        {
            Console.WriteLine("[DialogService] Showing confirmation dialog.");
            await dialog.ShowDialog(desktop.MainWindow);
            Console.WriteLine($"[DialogService] Confirmation dialog closed. Result: {viewModel.DialogResult}");
            return viewModel.DialogResult;
        }

        return false;
    }

    public async Task<string?> ShowInputDialogAsync(InputDialogViewModel viewModel)
    {
        Console.WriteLine("[DialogService] ShowInputDialogAsync called.");
        var dialog = new Window
        {
            Title = viewModel.Title,
            Content = new InputDialogView
            {
                DataContext = viewModel
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SizeToContent = SizeToContent.WidthAndHeight
        };

        // The VM's Ok/Cancel commands will set its DialogResult property.
        // We just need a way to close the window.
        viewModel.CloseCommand = new RelayCommand<object?>(_ =>
        {
            dialog.Close();
        });

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
        {
            Console.WriteLine("[DialogService] Showing input dialog.");
            await dialog.ShowDialog(desktop.MainWindow);
            // After the dialog is closed, we return the result that the VM has stored.
            Console.WriteLine($"[DialogService] Input dialog closed. Result: '{viewModel.DialogResult}'");
            return viewModel.DialogResult;
        }

        return null;
    }

    public async Task<JoyConfig.Models.AttributeDatabase.Attribute?> ShowSelectAttributeDialogAsync(SelectAttributeViewModel viewModel)
    {
        var dialog = new Window
        {
            Title = "Select Attribute",
            Content = new SelectAttributeView
            {
                DataContext = viewModel
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Width = 400,
            Height = 500
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
        {
            var result = await dialog.ShowDialog<bool>(desktop.MainWindow);
            if (result)
            {
                return viewModel.SelectedAttribute;
            }
        }

        return null;
    }

    public async Task<string?> ShowOpenFileDialogAsync(string title, FilePickerFileType filter)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
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

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
        {
            await dialog.ShowDialog(desktop.MainWindow);
        }
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        await ShowMessageBoxAsync(title, message);
    }

    // 便捷方法实现
    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var viewModel = new ConfirmationDialogViewModel
        {
            Title = title,
            Message = message
        };
        return await ShowConfirmationDialogAsync(viewModel);
    }

    public async Task<string?> ShowInputAsync(string title, string prompt, string defaultValue = "")
    {
        var viewModel = new InputDialogViewModel
        {
            Title = title,
            Message = prompt,
            InputText = defaultValue
        };
        return await ShowInputDialogAsync(viewModel);
    }

    public async Task ShowInfoAsync(string title, string message)
    {
        await ShowMessageBoxAsync(title, message);
    }

    public async Task ShowWarningAsync(string title, string message)
    {
        await ShowMessageBoxAsync(title, message);
    }
}
