using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Attribute = JoyConfig.Core.Models.AttributeDatabase.Attribute;
// using JoyConfig.Application.ViewModels; // TODO: Fix this architectural violation

namespace JoyConfig.Core.Services;

public interface IDialogService
{
    // TODO: Move ViewModels to Core or use DTOs to remove Application dependency
    // Task<bool> ShowConfirmationDialogAsync(ConfirmationDialogViewModel viewModel);
    // Task<string?> ShowInputDialogAsync(InputDialogViewModel viewModel);
    // Task<Attribute?> ShowSelectAttributeDialogAsync(SelectAttributeViewModel viewModel);
    Task<string?> ShowSelectTemplateDialogAsync();
    Task<string?> ShowOpenFileDialogAsync(string title, FilePickerFileType filter);
    Task ShowMessageBoxAsync(string title, string message);
}
