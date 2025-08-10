using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using JoyConfig.ViewModels;

namespace JoyConfig.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmationDialogAsync(ConfirmationDialogViewModel viewModel);
    Task<string?> ShowInputDialogAsync(InputDialogViewModel viewModel);
    Task<string?> ShowOpenFileDialogAsync(string title, FilePickerFileType filter);
    Task ShowMessageBoxAsync(string title, string message);
}
