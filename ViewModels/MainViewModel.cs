using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AttributeDatabaseEditor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentWorkspace;

    [ObservableProperty]
    private EditorViewModelBase? _currentEditor;

    [ObservableProperty]
    private SettingsViewModel? _settingsViewModel;

    [ObservableProperty]
    private bool _isSettingsVisible;

    public MainViewModel()
    {
        // Set the default workspace
        CurrentWorkspace = new AttributeDatabaseViewModel(this);
        
        // Set the default editor
        CurrentEditor = new WelcomeViewModel();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        SettingsViewModel = new SettingsViewModel();
        SettingsViewModel.SaveAndClose += (s, e) => 
        {
            SettingsViewModel.SaveSettingsCommand.Execute(null);
            IsSettingsVisible = false;
        };
        SettingsViewModel.Cancel += (s, e) => IsSettingsVisible = false;
        IsSettingsVisible = true;
    }
}

// A simple ViewModel for the Welcome tab
public class WelcomeViewModel : EditorViewModelBase
{
    public WelcomeViewModel()
    {
        Title = "Welcome";
    }
}
