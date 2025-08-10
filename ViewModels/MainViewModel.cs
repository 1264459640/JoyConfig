using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Services;

namespace JoyConfig.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;

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

    public MainViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        
        // Set database path
        Models.AttributeDatabase.AttributeDatabaseContext.DbPath = "Example/AttributeDatabase.db";
        
        // Set the default workspace
        CurrentWorkspace = new AttributeDatabaseViewModel(this, _dialogService);
        
        // Set the default editor
        CurrentEditor = new WelcomeViewModel();
    }

    public void OpenEditor(EditorViewModelBase newEditor)
    {
        CurrentEditor = newEditor;
    }

    [RelayCommand]
    private void OpenAttributeDatabase()
    {
        CurrentWorkspace = new AttributeDatabaseViewModel(this, _dialogService);
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
        SettingsViewModel = new SettingsViewModel(_dialogService, this);
        IsSettingsVisible = true;
        SelectedWorkspace = "Settings";
    }

    public void LoadAttributeDatabase()
    {
        OpenAttributeDatabase();
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
