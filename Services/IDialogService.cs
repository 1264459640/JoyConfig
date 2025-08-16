using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.ViewModels;

namespace JoyConfig.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmationDialogAsync(ConfirmationDialogViewModel viewModel);
    Task<string?> ShowInputDialogAsync(InputDialogViewModel viewModel);
    Task<Attribute?> ShowSelectAttributeDialogAsync(SelectAttributeViewModel viewModel);
    Task<string?> ShowOpenFileDialogAsync(string title, FilePickerFileType filter);
    Task ShowMessageBoxAsync(string title, string message);
    Task ShowErrorAsync(string title, string message);
}
