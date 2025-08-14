using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Application.Services;

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
    public IRelayCommand<object> CloseCommand { get; set; } = new RelayCommand<object>(_ => { });
    
    public ILocalizationService LocalizationManager { get; }
    public IRelayCommand OkCommand => ConfirmCommand;
    
    public ConfirmationDialogViewModel(ILocalizationService? localizationService = null)
    {
        LocalizationManager = localizationService ?? new DefaultLocalizationService();
    }
    
    public ConfirmationDialogViewModel() : this(null) { }

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
