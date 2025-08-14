using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Infrastructure.Models.AttributeDatabase;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;
using AttributeSet = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeSet;
// using JoyConfig.Infrastructure.Services;

namespace JoyConfig.Application.ViewModels;

public partial class MainViewModel : ObservableObject, IUIService
{
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localization;

    // 供XAML绑定访问的属�?
    public ILocalizationService LocalizationManager => _localization;

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

    public MainViewModel(
        IDialogService dialogService,
        ILocalizationService localization,
        SettingsViewModel settingsViewModel)
    {
        _dialogService = dialogService;
        _localization = localization;
        
        SettingsViewModel = settingsViewModel;
        SettingsViewModel.DatabaseChanged += LoadAttributeDatabase;
        
        // Set the default editor
        CurrentEditor = new WelcomeViewModel();
    }
    
    /// <summary>
    /// 初始化默认工作区（避免构造函数循环依赖）
    /// </summary>
    public void InitializeDefaultWorkspace(AttributeDatabaseViewModel attributeDatabaseViewModel)
    {
        if (CurrentWorkspace == null)
        {
            CurrentWorkspace = attributeDatabaseViewModel;
        }
    }

    #region IUIService Implementation
    
    public void OpenEditor(string editorType, object? context = null)
    {
        EditorViewModelBase? newEditor = editorType.ToLower() switch
        {
            "attribute" when context is Attribute attr => new AttributeViewModel(attr, (AttributeDatabaseViewModel)CurrentWorkspace!, _dialogService, null!),
            "attributeset" when context is AttributeSet attrSet => AttributeSetViewModel.CreateAsync(attrSet.Id, (AttributeDatabaseViewModel)CurrentWorkspace!, _dialogService).Result,
            "template" => new TemplateManagerViewModel(_dialogService, _localization),
            "welcome" => new WelcomeViewModel(),
            _ => null
        };
        
        if (newEditor != null)
        {
            CurrentEditor = newEditor;
            EditorChanged?.Invoke(editorType, context);
        }
    }
    
    public void OpenAttributeEditor(Attribute attribute)
    {
        OpenEditor("attribute", attribute);
    }
    
    public void OpenAttributeSetEditor(AttributeSet attributeSet)
    {
        OpenEditor("attributeset", attributeSet);
    }
    
    public void OpenTemplateManager()
    {
        OpenEditor("template");
    }
    
    public void CloseCurrentEditor()
    {
        CurrentEditor = new WelcomeViewModel();
        EditorChanged?.Invoke("welcome", null);
    }
    
    public void SwitchWorkspace(string workspaceType)
    {
        switch (workspaceType.ToLower())
        {
            case "attributedatabase":
                OpenAttributeDatabaseWorkspace();
                break;
            case "gameplayeffectdatabase":
                OpenGameplayEffectDatabaseWorkspace();
                break;
            case "settings":
                OpenSettings();
                break;
        }
    }
    
    public void OpenAttributeDatabaseWorkspace()
    {
        SelectedWorkspace = "AttributeDatabase";
        IsSettingsVisible = false;
        WorkspaceChanged?.Invoke("AttributeDatabase");
    }
    
    public void OpenGameplayEffectDatabaseWorkspace()
    {
        // TODO: Implement when GameplayEffectDatabase workspace is available
        SelectedWorkspace = "GameplayEffectDatabase";
        IsSettingsVisible = false;
        WorkspaceChanged?.Invoke("GameplayEffectDatabase");
    }
    
    public void OpenSettings()
    {
        IsSettingsVisible = true;
        SelectedWorkspace = "Settings";
        CurrentWorkspace = null;
        WorkspaceChanged?.Invoke("Settings");
    }
    
    public void CloseSettings()
    {
        IsSettingsVisible = false;
        OpenAttributeDatabaseWorkspace();
    }
    
    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
        StatusMessageChanged?.Invoke(message);
    }
    
    public void ClearStatusMessage()
    {
        SetStatusMessage(string.Empty);
    }
    
    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        // For now, just set as status message
        // TODO: Implement proper notification system
        SetStatusMessage($"[{type}] {message}");
    }
    
    public void NavigateTo(string targetType, string targetId)
    {
        switch (targetType.ToLower())
        {
            case "attribute":
                NavigateToAttribute(targetId);
                break;
            case "attributeset":
                NavigateToAttributeSet(targetId);
                break;
        }
    }
    
    public void NavigateToAttribute(string attributeId)
    {
        // TODO: Implement navigation to specific attribute
        SetStatusMessage($"Navigating to attribute: {attributeId}");
    }
    
    public void NavigateToAttributeSet(string attributeSetId)
    {
        // TODO: Implement navigation to specific attribute set
        SetStatusMessage($"Navigating to attribute set: {attributeSetId}");
    }
    
    public void GoBack()
    {
        // TODO: Implement navigation history
        SetStatusMessage("Go back - not implemented yet");
    }
    
    public void GoForward()
    {
        // TODO: Implement navigation history
        SetStatusMessage("Go forward - not implemented yet");
    }
    
    public event Action<string, object?>? EditorChanged;
    public event Action<string>? WorkspaceChanged;
    public event Action<string>? StatusMessageChanged;
    
    #endregion
    
    // Legacy methods for backward compatibility
    public void OpenEditor(EditorViewModelBase newEditor)
    {
        CurrentEditor = newEditor;
        EditorChanged?.Invoke("custom", newEditor);
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
