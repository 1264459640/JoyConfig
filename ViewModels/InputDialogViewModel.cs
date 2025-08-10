using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace JoyConfig.ViewModels;

public partial class InputDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Input";

    [ObservableProperty]
    private string _message = "Please enter a value:";

    [ObservableProperty]
    private string _inputText = "";

    public string? DialogResult { get; private set; }

    public ICommand? CloseCommand { get; set; }
    
    [RelayCommand]
    private void Ok()
    {
        DialogResult = InputText;
        // The CloseCommand will be set by the DialogService to close the window.
        // We pass a dummy parameter to indicate success.
        CloseCommand?.Execute("OK");
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = null;
        CloseCommand?.Execute(null);
    }
}
