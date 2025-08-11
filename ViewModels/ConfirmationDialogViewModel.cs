using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows.Input;
using JoyConfig.Services;

namespace JoyConfig.ViewModels;

public partial class ConfirmationDialogViewModel : ObservableObject
{
    public LocalizationManager LocalizationManager { get; }

    [ObservableProperty]
    private string _title = "Confirm Action";

    [ObservableProperty]
    private string _message = "";

    [ObservableProperty]
    private List<string> _details = new();

    public bool DialogResult { get; private set; }

    public ICommand? CloseCommand { get; set; }

    public ConfirmationDialogViewModel()
    {
        LocalizationManager = LocalizationManager.Instance;
    }

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
