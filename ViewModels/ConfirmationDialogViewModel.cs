using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows.Input;

namespace JoyConfig.ViewModels;

public partial class ConfirmationDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Confirm Action";

    [ObservableProperty]
    private string _message = "";

    [ObservableProperty]
    private List<string> _details = new();

    public bool DialogResult { get; private set; }

    public ICommand? CloseCommand { get; set; }

    [RelayCommand]
    private void Ok()
    {
        DialogResult = true;
        CloseCommand?.Execute(null);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        CloseCommand?.Execute(null);
    }
}
