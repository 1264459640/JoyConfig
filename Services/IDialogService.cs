using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.ViewModels.AttributeDatabase;
using JoyConfig.ViewModels.Dialogs;

namespace JoyConfig.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmationDialogAsync(ConfirmationDialogViewModel viewModel);
    Task<string?> ShowInputDialogAsync(InputDialogViewModel viewModel);
    Task<Attribute?> ShowSelectAttributeDialogAsync(SelectAttributeViewModel viewModel);
    Task<string?> ShowOpenFileDialogAsync(string title, FilePickerFileType filter);
    Task ShowMessageBoxAsync(string title, string message);
    Task ShowErrorAsync(string title, string message);
    
    // 便捷方法
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task<string?> ShowInputAsync(string title, string prompt, string defaultValue = "");
    Task ShowInfoAsync(string title, string message);
}
