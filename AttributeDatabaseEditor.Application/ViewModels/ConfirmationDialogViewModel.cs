using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JoyConfig.Application.ViewModels;

public partial class ConfirmationDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private List<string> _details = new();

    public bool DialogResult { get; private set; }
    public IRelayCommand<object> CloseCommand { get; set; }

    [RelayCommand]
    private void Confirm()
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
