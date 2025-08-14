using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Infrastructure.Models.Settings;
using JoyConfig.Application.Abstract.Services;
// using JoyConfig.Infrastructure.Services;

namespace JoyConfig.Application.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localization;
    private AppSettings _appSettings;

    public event Action? DatabaseChanged;

    [ObservableProperty]
    private string? _databasePath;

    [ObservableProperty]
    private CultureInfo? _selectedLanguage;
    
    [ObservableProperty]
    private string? _attributeDatabasePath;
    
    [ObservableProperty]
    private string? _gameplayEffectDatabasePath;
    
    [ObservableProperty]
    private string? _theme;
    
    [ObservableProperty]
    private bool _isAutosaveEnabled;
    
    [ObservableProperty]
    private int _autosaveInterval;
    
    [ObservableProperty]
    private bool _loadLastDatabaseOnStartup;
    
    [ObservableProperty]
    private string? _selectedPage;
    
    [ObservableProperty]
    private List<string> _settingPages = new() { "Database", "General", "Advanced", "About" };

    public IEnumerable<CultureInfo> SupportedLanguages => _localization.SupportedLanguages;

    // 供XAML访问
    public ILocalizationService LocalizationManager => _localization;
    
    [RelayCommand]
    private async Task BrowseForAttributeDatabase()
    {
        // TODO: Implement file browser
    }
    
    [RelayCommand]
    private async Task BrowseForGameplayEffectDatabase()
    {
        // TODO: Implement file browser
    }
    
    [RelayCommand]
    private async Task ApplyAttributeDatabasePath()
    {
        // TODO: Implement path application
    }

    public SettingsViewModel(IDialogService dialogService, ILocalizationService localization)
    {
        _dialogService = dialogService;
        _localization = localization;
        _appSettings = LoadAppSettings();
        DatabasePath = _appSettings.AttributeDatabasePath;
        SelectedLanguage = _localization.CurrentCulture;
    }

    private AppSettings LoadAppSettings()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception)
        {
            // Handle exceptions
        }
        return new AppSettings();
    }

    private async Task SaveAppSettingsAsync()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var json = JsonSerializer.Serialize(_appSettings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configPath, json);
        }
        catch (Exception)
        {
            // Handle exceptions
        }
    }

    partial void OnSelectedLanguageChanged(CultureInfo? value)
    {
        if (value != null)
        {
            _localization.CurrentCulture = value;
            _appSettings.Language = value.Name;
            _ = SaveAppSettingsAsync();
        }
    }

    [RelayCommand]
    private async Task BrowseDatabasePathAsync()
    {
        var path = await _dialogService.ShowOpenFileDialogAsync("Select Database File", FilePickerFileTypes.All);
        if (!string.IsNullOrWhiteSpace(path))
        {
            DatabasePath = path;
        _appSettings.AttributeDatabasePath = path;
            await SaveAppSettingsAsync();
            DatabaseChanged?.Invoke();
        }
    }
}
