using JoyConfig.Models.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using System.IO;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JoyConfig.ViewModels
{
    public class SettingsPage
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
    }

    public partial class SettingsViewModel : EditorViewModelBase
    {
        private readonly string _settingsFilePath;
        private AppSettings _appSettings;
        private readonly IDialogService _dialogService;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private string? _attributeDatabasePath;

        [ObservableProperty]
        private string? _gameplayEffectDatabasePath;

        [ObservableProperty]
        private string _theme;

        [ObservableProperty]
        private CultureInfo _selectedLanguage;

        [ObservableProperty]
        private bool _isAutosaveEnabled;

        [ObservableProperty]
        private int _autosaveInterval;

        [ObservableProperty]
        private bool _loadLastDatabaseOnStartup;

        [ObservableProperty]
        private ObservableCollection<SettingsPage> _settingPages;

        [ObservableProperty]
        private SettingsPage _selectedPage;

        public SettingsViewModel(IDialogService dialogService, MainViewModel mainViewModel)
        {
            Title = "Settings";
            _dialogService = dialogService;
            _mainViewModel = mainViewModel;
            
            // Define the path for the settings file
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolderPath = Path.Combine(appDataPath, "JoyConfig");
            Directory.CreateDirectory(appFolderPath); // Ensure the directory exists
            _settingsFilePath = Path.Combine(appFolderPath, "settings.json");
            
            _appSettings = new AppSettings();
            _theme = string.Empty;
            
            LoadSettings();
            
            _selectedLanguage = LocalizationManager.CurrentCulture;

            _settingPages = new ObservableCollection<SettingsPage>
            {
                new() { Key = "Database", DisplayName = LocalizationManager.Instance["Settings_Database"] ?? "Database" },
                new() { Key = "Appearance", DisplayName = LocalizationManager.Instance["Settings_Appearance"] ?? "Appearance" },
                new() { Key = "Editor Behavior", DisplayName = LocalizationManager.Instance["Settings_EditorBehavior"] ?? "Editor Behavior" },
                new() { Key = "About", DisplayName = LocalizationManager.Instance["Settings_About"] ?? "About" }
            };
            _selectedPage = _settingPages[0];

            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                _appSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            else
            {
                _appSettings = new AppSettings();
            }

            // Initialize properties from loaded settings
            AttributeDatabasePath = _appSettings.AttributeDatabasePath;
            GameplayEffectDatabasePath = _appSettings.GameplayEffectDatabasePath;
            Theme = _appSettings.Theme;
            LocalizationManager.Instance.CurrentCulture = new CultureInfo(_appSettings.Language);
            IsAutosaveEnabled = _appSettings.IsAutosaveEnabled;
            AutosaveInterval = _appSettings.AutosaveInterval;
            LoadLastDatabaseOnStartup = _appSettings.LoadLastDatabaseOnStartup;
        }

        [RelayCommand]
        public void SaveSettings()
        {
            // Update the settings object from view model properties
            _appSettings.AttributeDatabasePath = AttributeDatabasePath;
            _appSettings.GameplayEffectDatabasePath = GameplayEffectDatabasePath;
            _appSettings.Theme = Theme;
            _appSettings.Language = SelectedLanguage.Name;
            _appSettings.IsAutosaveEnabled = IsAutosaveEnabled;
            _appSettings.AutosaveInterval = AutosaveInterval;
            _appSettings.LoadLastDatabaseOnStartup = LoadLastDatabaseOnStartup;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_appSettings, options);
            File.WriteAllText(_settingsFilePath, json);
        }

        [RelayCommand]
        private async Task BrowseForAttributeDatabaseAsync()
        {
            var result = await _dialogService.ShowOpenFileDialogAsync("Open Attribute Database", new FilePickerFileType("Database Files") { Patterns = new[] { "*.db" } });
            if (result != null)
            {
                AttributeDatabasePath = result;
            }
        }

        [RelayCommand]
        private void BrowseForGameplayEffectDatabase()
        {
            // Placeholder for file dialog logic
            GameplayEffectDatabasePath = "C:/path/to/your/GameplayEffectDatabase.db";
        }

        [RelayCommand]
        private async Task ApplyAttributeDatabasePathAsync()
        {
            if (string.IsNullOrWhiteSpace(AttributeDatabasePath) || !File.Exists(AttributeDatabasePath))
            {
                await _dialogService.ShowMessageBoxAsync("Error", "Invalid database file path.");
                return;
            }

            var (isValid, errorMessage) = await AttributeDatabaseContext.ValidateDatabaseSchemaAsync(AttributeDatabasePath);

            if (!isValid)
            {
                await _dialogService.ShowMessageBoxAsync("Schema Validation Failed", errorMessage ?? "Unknown error");
                return;
            }

            _appSettings.AttributeDatabasePath = AttributeDatabasePath;
            SaveSettings();
            
            // Reload the main view
            _mainViewModel.LoadAttributeDatabase();
            await _dialogService.ShowMessageBoxAsync("Success", "Attribute database loaded successfully.");
        }
        
        partial void OnSelectedLanguageChanged(CultureInfo value)
        {
            if (value != null)
            {
                LocalizationManager.Instance.CurrentCulture = value;
                SaveSettings();
            }
        }
    }
}
