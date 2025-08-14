using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Application.Services;

namespace JoyConfig.Application.ViewModels;

public partial class InputDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string? _inputText;

    public string? DialogResult { get; private set; }
    public IRelayCommand<object> CloseCommand { get; set; } = new RelayCommand<object>(_ => { });
    
    public ILocalizationService LocalizationManager { get; }
    
    public InputDialogViewModel(ILocalizationService? localizationService = null)
    {
        LocalizationManager = localizationService ?? new DefaultLocalizationService();
    }
    
    public InputDialogViewModel() : this(null) { }

    [RelayCommand]
    private void Ok()
    {
        DialogResult = InputText;
        CloseCommand?.Execute(null);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = null;
        CloseCommand?.Execute(null);
    }
}
