using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Core.Services;
using JoyConfig.Infrastructure.Services;

namespace JoyConfig.Application.ViewModels;

public interface IUIService
{
    void OpenEditor(EditorViewModelBase newEditor);
    void SetStatusMessage(string message);
}

public partial class MainViewModel : ObservableObject, IUIService
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

    [ObservableProperty]
    private string? _statusMessage;

    public LocalizationManager LocalizationManager { get; }

    public MainViewModel(
        IDialogService dialogService, 
        SettingsViewModel settingsViewModel,
        AttributeDatabaseViewModel attributeDatabaseViewModel)
    {
        _dialogService = dialogService;
        LocalizationManager = LocalizationManager.Instance;
        
        // Set the default workspace
        CurrentWorkspace = attributeDatabaseViewModel;
        SettingsViewModel = settingsViewModel;
        SettingsViewModel.DatabaseChanged += LoadAttributeDatabase;
        
        // Set the default editor
        CurrentEditor = new WelcomeViewModel();
    }

    public void OpenEditor(EditorViewModelBase newEditor)
    {
        CurrentEditor = newEditor;
    }

    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
    }

    [RelayCommand]
    private void OpenAttributeDatabase(AttributeDatabaseViewModel attributeDatabaseViewModel)
    {
        CurrentWorkspace = attributeDatabaseViewModel;
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
    private void OpenSettings(SettingsViewModel settingsViewModel)
    {
        SettingsViewModel = settingsViewModel;
        IsSettingsVisible = true;
        SelectedWorkspace = "Settings";
        CurrentWorkspace = null;
    }

    [RelayCommand]
    private void OpenTemplateManager(TemplateManagerViewModel templateManagerViewModel)
    {
        CurrentEditor = templateManagerViewModel;
    }

    public void LoadAttributeDatabase()
    {
        // This will be handled by the DI container now
        // OpenAttributeDatabase();
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
