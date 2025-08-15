using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Services;
using Avalonia.Controls;

namespace JoyConfig.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private readonly IViewModelFactory _viewModelFactory;

    [ObservableProperty]
    private object? _currentWorkspace;

    [ObservableProperty]
    private EditorViewModelBase? _currentEditor;

    [ObservableProperty]
    private SettingsViewModel? _settingsViewModel;
    
    [ObservableProperty]
    private TemplateManagerViewModel? _templateManagerViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainContentVisible))]
    private bool _isSettingsVisible;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainContentVisible))]
    private bool _isTemplateManagerVisible;

    [ObservableProperty]
    private string? _selectedWorkspace = "AttributeDatabase";
    
    [ObservableProperty]
    private string _statusMessage = "就绪";
    
    [ObservableProperty]
    private string _progressText = "";
    
    [ObservableProperty]
    private bool _isLoading;
    
    // IDE Layout Properties
    [ObservableProperty]
    private bool _isPrimarySidebarVisible = true;
    
    [ObservableProperty]
    private bool _isSecondarySidebarVisible = false;
    
    [ObservableProperty]
    private bool _isPanelVisible = false;
    
    [ObservableProperty]
    private GridLength _primarySidebarWidth = new GridLength(250);
    
    [ObservableProperty]
    private GridLength _secondarySidebarWidth = new GridLength(250);
    
    [ObservableProperty]
    private GridLength _panelHeight = new GridLength(200);
    
    [ObservableProperty]
    private object? _secondaryContent;
    
    [ObservableProperty]
    private object? _panelContent;
    
    public bool IsMainContentVisible => !IsSettingsVisible && !IsTemplateManagerVisible;

    public LocalizationManager LocalizationManager { get; }

    public MainViewModel(IDialogService dialogService, IViewModelFactory viewModelFactory)
    {
        _dialogService = dialogService;
        _viewModelFactory = viewModelFactory;
        LocalizationManager = LocalizationManager.Instance;
        
        // Set the default workspace
        CurrentWorkspace = _viewModelFactory.CreateAttributeDatabaseViewModel(this);
        
        // Set the default editor
        CurrentEditor = new WelcomeViewModel();
    }
    
    /// <summary>
    /// 更新状态栏信息
    /// </summary>
    public void UpdateStatus(string message, bool isLoading = false, string progressText = "")
    {
        StatusMessage = message;
        IsLoading = isLoading;
        ProgressText = progressText;
    }
    
    /// <summary>
    /// 设置加载状态
    /// </summary>
    public void SetLoading(bool isLoading, string progressText = "")
    {
        IsLoading = isLoading;
        ProgressText = progressText;
        if (isLoading)
        {
            StatusMessage = "正在处理...";
        }
        else
        {
            StatusMessage = "就绪";
        }
    }

    public void OpenEditor(EditorViewModelBase newEditor)
    {
        CurrentEditor = newEditor;
    }

    [RelayCommand]
    public void OpenAttributeDatabase()
    {
        CurrentWorkspace = _viewModelFactory.CreateAttributeDatabaseViewModel(this);
        SelectedWorkspace = "AttributeDatabase";
        IsSettingsVisible = false;
        IsTemplateManagerVisible = false;
    }

    [RelayCommand]
    private void OpenGameplayEffectDatabase()
    {
        // TODO: Replace with actual GameplayEffectDatabaseViewModel
        CurrentWorkspace = new WelcomeViewModel { Title = "Gameplay Effect Database" };
        SelectedWorkspace = "GameplayEffectDatabase";
        IsSettingsVisible = false;
        IsTemplateManagerVisible = false;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        SettingsViewModel = _viewModelFactory.CreateSettingsViewModel(this);
        IsSettingsVisible = true;
        IsTemplateManagerVisible = false;
        SelectedWorkspace = "Settings";
        CurrentWorkspace = null;
    }

    [RelayCommand]
    private void OpenTemplateManager()
    {
        TemplateManagerViewModel = _viewModelFactory.CreateTemplateManagerViewModel(this);
        IsTemplateManagerVisible = true;
        IsSettingsVisible = false;
        SelectedWorkspace = "TemplateManager";
        CurrentWorkspace = null;
    }

    public void LoadAttributeDatabase()
    {
        OpenAttributeDatabase();
    }
    
    // IDE Layout Commands
    [RelayCommand]
    private void TogglePrimarysidebar()
    {
        IsPrimarySidebarVisible = !IsPrimarySidebarVisible;
        if (!IsPrimarySidebarVisible)
        {
            PrimarySidebarWidth = new GridLength(0);
        }
        else
        {
            PrimarySidebarWidth = new GridLength(250);
        }
    }
    
    [RelayCommand]
    private void ToggleSecondarySidebar()
    {
        IsSecondarySidebarVisible = !IsSecondarySidebarVisible;
        if (!IsSecondarySidebarVisible)
        {
            SecondarySidebarWidth = new GridLength(0);
        }
        else
        {
            SecondarySidebarWidth = new GridLength(250);
        }
    }
    
    [RelayCommand]
    private void TogglePanel()
    {
        IsPanelVisible = !IsPanelVisible;
        if (!IsPanelVisible)
        {
            PanelHeight = new GridLength(0);
        }
        else
        {
            PanelHeight = new GridLength(200);
        }
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
