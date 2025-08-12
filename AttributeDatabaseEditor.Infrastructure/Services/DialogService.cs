using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
// using JoyConfig.Application.ViewModels;
// using JoyConfig.Views;
using JoyConfig.Core.Services;
using JoyConfig.Core.Models.AttributeDatabase;

namespace JoyConfig.Infrastructure.Services;

public class DialogService : IDialogService
{
    // public async Task<bool> ShowConfirmationDialogAsync(ConfirmationDialogViewModel viewModel)
    // {
    //     var dialog = new Window
    //     {
    //         Title = viewModel.Title,
    //         Content = new ConfirmationDialogView { DataContext = viewModel },
    //         WindowStartupLocation = WindowStartupLocation.CenterOwner,
    //         CanResize = false,
    //         SizeToContent = SizeToContent.WidthAndHeight
    //     };
    //     viewModel.CloseCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<object?>(_ => dialog.Close());
    //     if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
    //     {
    //         await dialog.ShowDialog(desktop.MainWindow);
    //         return viewModel.DialogResult;
    //     }
    //     return false;
    // }

    // public async Task<string?> ShowInputDialogAsync(InputDialogViewModel viewModel)
    // {
    //     var dialog = new Window
    //     {
    //         Title = viewModel.Title,
    //         Content = new InputDialogView { DataContext = viewModel },
    //         WindowStartupLocation = WindowStartupLocation.CenterOwner,
    //         CanResize = false,
    //         SizeToContent = SizeToContent.WidthAndHeight
    //     };
    //     viewModel.CloseCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<object?>(_ => dialog.Close());
    //     if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
    //     {
    //         await dialog.ShowDialog(desktop.MainWindow);
    //         return viewModel.DialogResult;
    //     }
    //     return null;
    // }

    // public async Task<Attribute?> ShowSelectAttributeDialogAsync(SelectAttributeViewModel viewModel)
    // {
    //     var dialog = new Window
    //     {
    //         Title = "Select Attribute",
    //         Content = new SelectAttributeView { DataContext = viewModel },
    //         WindowStartupLocation = WindowStartupLocation.CenterOwner,
    //         Width = 400,
    //         Height = 500
    //     };
    //     if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
    //     {
    //         var result = await dialog.ShowDialog<bool>(desktop.MainWindow);
    //         if (result)
    //         {
    //             return viewModel.SelectedAttribute;
    //         }
    //     }
    //     return null;
    // }

    public async Task<string?> ShowSelectTemplateDialogAsync()
    {
        // TODO: Implement this method properly
        // Currently returning null as a placeholder
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
}
