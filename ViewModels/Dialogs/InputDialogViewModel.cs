using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using JoyConfig.Services;

namespace JoyConfig.ViewModels.Dialogs;

public partial class InputDialogViewModel : ObservableObject
{
    public LocalizationManager LocalizationManager { get; }

    [ObservableProperty]
    private string _title = "Input";

    [ObservableProperty]
    private string _message = "Please enter a value:";

    [ObservableProperty]
    private string _inputText = "";

    public string? DialogResult { get; private set; }

    public ICommand? CloseCommand { get; set; }

    public InputDialogViewModel()
    {
        LocalizationManager = LocalizationManager.Instance;
    }

    [RelayCommand]
    private void Ok()
    {
        DialogResult = InputText;
        CloseCommand?.Execute("OK");
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = null;
        CloseCommand?.Execute(null);
    }
}
