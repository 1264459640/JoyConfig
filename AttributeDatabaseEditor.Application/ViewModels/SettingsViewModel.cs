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
using JoyConfig.Core.Models.Settings;
using JoyConfig.Core.Services;
using JoyConfig.Infrastructure.Services;

namespace JoyConfig.Application.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private AppSettings _appSettings;

    public event Action? DatabaseChanged;

    [ObservableProperty]
    private string? _databasePath;

    [ObservableProperty]
    private CultureInfo? _selectedLanguage;

    public IEnumerable<CultureInfo> SupportedLanguages => LocalizationManager.Instance.SupportedLanguages;

    public SettingsViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        _appSettings = LoadAppSettings();
        DatabasePath = _appSettings.AttributeDatabasePath;
        SelectedLanguage = LocalizationManager.Instance.CurrentCulture;
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
            LocalizationManager.Instance.CurrentCulture = value;
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
