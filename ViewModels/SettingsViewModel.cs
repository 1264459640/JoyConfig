using JoyConfig.Models.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using System.IO;
using System;

namespace JoyConfig.ViewModels
{
    public partial class SettingsViewModel : EditorViewModelBase
    {
        public event EventHandler? SaveAndClose;
        public event EventHandler? Cancel;

        private readonly string _settingsFilePath;
        private AppSettings _appSettings;

        [ObservableProperty]
        private string? _attributeDatabasePath;

        [ObservableProperty]
        private string? _gameplayEffectDatabasePath;

        [ObservableProperty]
        private string _theme;

        [ObservableProperty]
        private string _language;

        [ObservableProperty]
        private bool _isAutosaveEnabled;

        [ObservableProperty]
        private int _autosaveInterval;

        [ObservableProperty]
        private bool _loadLastDatabaseOnStartup;

        public SettingsViewModel()
        {
            Title = "Settings";
            // Define the path for the settings file
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolderPath = Path.Combine(appDataPath, "JoyConfig");
            Directory.CreateDirectory(appFolderPath); // Ensure the directory exists
            _settingsFilePath = Path.Combine(appFolderPath, "settings.json");
            _appSettings = new AppSettings();
            _theme = _appSettings.Theme;
            _language = _appSettings.Language;

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
            Language = _appSettings.Language;
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
            _appSettings.Language = Language;
            _appSettings.IsAutosaveEnabled = IsAutosaveEnabled;
            _appSettings.AutosaveInterval = AutosaveInterval;
            _appSettings.LoadLastDatabaseOnStartup = LoadLastDatabaseOnStartup;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_appSettings, options);
            File.WriteAllText(_settingsFilePath, json);
        }

        // In a real implementation, you would use a file dialog service
        [RelayCommand]
        private void BrowseForAttributeDatabase()
        {
            // Placeholder for file dialog logic
            // For now, we'll just simulate setting a path
            AttributeDatabasePath = "C:/path/to/your/AttributeDatabase.db";
        }

        [RelayCommand]
        private void BrowseForGameplayEffectDatabase()
        {
            // Placeholder for file dialog logic
            GameplayEffectDatabasePath = "C:/path/to/your/GameplayEffectDatabase.db";
        }

        [RelayCommand]
        private void TriggerSaveAndClose()
        {
            SaveAndClose?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void TriggerCancel()
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }
    }
}
