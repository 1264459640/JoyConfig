using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JoyConfig.ViewModels;

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

    [ObservableProperty]
    private string? _selectedWorkspace = "AttributeDatabase";

    public MainViewModel()
    {
        // Set the default workspace
        CurrentWorkspace = new AttributeDatabaseViewModel(this);
        
        // Set the default editor
        CurrentEditor = new WelcomeViewModel();
    }

    [RelayCommand]
    private void OpenAttributeDatabase()
    {
        CurrentWorkspace = new AttributeDatabaseViewModel(this);
        SelectedWorkspace = "AttributeDatabase";
        IsSettingsVisible = false;
    }

    [RelayCommand]
    private void OpenGameplayEffectDatabase()
    {
        // TODO: Replace with actual GameplayEffectDatabaseViewModel
        CurrentWorkspace = new WelcomeViewModel { Title = "Gameplay Effect Database" };
        SelectedWorkspace = "GameplayEffectDatabase";
        IsSettingsVisible = false;
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
        SelectedWorkspace = "Settings";
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
