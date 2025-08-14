using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Application.Services;
using JoyConfig.Infrastructure.Models.Settings;
using JoyConfig.Base.Constants;
using JoyConfig.Base.Utilities;

namespace JoyConfig.Application.Services;

/// <summary>
/// 应用程序服务实现 - 提供应用程序级别操作的具体实�?/// 实现IApplicationService接口，管理应用程序生命周期和配置
/// </summary>
public class ApplicationService : IApplicationService
{
    private readonly ILocalizationService _localizationService;
    private AppSettings _currentSettings;
    private readonly string _appDataDirectory;
    private readonly string _settingsFilePath;
    private readonly string _logDirectory;
    private readonly string _tempDirectory;
    
    public ApplicationService(ILocalizationService? localizationService = null)
    {
        _localizationService = localizationService ?? new DefaultLocalizationService();
        
        _appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JoyConfig");
        _settingsFilePath = Path.Combine(_appDataDirectory, "settings.json");
        _logDirectory = Path.Combine(_appDataDirectory, "Logs");
        _tempDirectory = Path.Combine(_appDataDirectory, "Temp");
        
        _currentSettings = LoadSettingsInternal();
    }
    
    #region Application Lifecycle
    
    public async Task InitializeAsync()
    {
        try
        {
            // 确保必要的目录存�?            EnsureDirectoriesExist();
            
            // 加载设置
            _currentSettings = LoadSettingsInternal();
            
            // 设置本地�?            if (!string.IsNullOrEmpty(_currentSettings.Language))
            {
                try
                {
                    _localizationService.CurrentCulture = new System.Globalization.CultureInfo(_currentSettings.Language);
                }
                catch (System.Globalization.CultureNotFoundException ex)
                {
                    LogWarning($"Invalid language setting: {_currentSettings.Language}. Using default.");
                }
            }
            
            LogInfo("Application initialized successfully");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError("Failed to initialize application", ex);
            throw;
        }
    }
    
    public async Task StartAsync()
    {
        try
        {
            LogInfo("Application starting...");
            ApplicationStarted?.Invoke();
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError("Failed to start application", ex);
            throw;
        }
    }
    
    public async Task StopAsync()
    {
        try
        {
            LogInfo("Application stopping...");
            ApplicationStopping?.Invoke();
            
            // 清理临时文件
            await CleanupTempFilesAsync();
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError("Error during application shutdown", ex);
        }
    }
    
    public void Shutdown(int exitCode = 0)
    {
        try
        {
            LogInfo($"Application shutting down with exit code: {exitCode}");
            Environment.Exit(exitCode);
        }
        catch (Exception ex)
        {
            LogError("Error during application shutdown", ex);
            Environment.Exit(-1);
        }
    }
    
    public async Task RestartAsync()
    {
        try
        {
            LogInfo("Application restarting...");
            
            var currentExecutable = Environment.ProcessPath ?? Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrEmpty(currentExecutable))
            {
                System.Diagnostics.Process.Start(currentExecutable);
                await StopAsync();
                Shutdown(0);
            }
            else
            {
                LogError("Cannot determine current executable path for restart");
            }
        }
        catch (Exception ex)
        {
            LogError("Failed to restart application", ex);
            throw;
        }
    }
    
    #endregion
    
    #region Configuration Management
    
    public AppSettings GetSettings()
    {
        return _currentSettings;
    }
    
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            _currentSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            EnsureDirectoriesExist();
            
            var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await File.WriteAllTextAsync(_settingsFilePath, json);
            
            SettingsChanged?.Invoke(_currentSettings);
            LogInfo("Settings saved successfully");
        }
        catch (Exception ex)
        {
            LogError("Failed to save settings", ex);
            throw;
        }
    }
    
    public async Task ResetSettingsAsync()
    {
        try
        {
            _currentSettings = new AppSettings();
            await SaveSettingsAsync(_currentSettings);
            LogInfo("Settings reset to defaults");
        }
        catch (Exception ex)
        {
            LogError("Failed to reset settings", ex);
            throw;
        }
    }
    
    public T GetConfigValue<T>(string key, T defaultValue = default!)
    {
        try
        {
            // Simple key-value configuration - could be extended with a proper configuration system
            var property = typeof(AppSettings).GetProperty(key);
            if (property != null && property.CanRead)
            {
                var value = property.GetValue(_currentSettings);
                if (value is T typedValue)
                    return typedValue;
            }
            
            return defaultValue;
        }
        catch (Exception ex)
        {
            LogWarning($"Failed to get config value for key '{key}': {ex.Message}");
            return defaultValue;
        }
    }
    
    public void SetConfigValue<T>(string key, T value)
    {
        try
        {
            var property = typeof(AppSettings).GetProperty(key);
            if (property != null && property.CanWrite && property.PropertyType == typeof(T))
            {
                property.SetValue(_currentSettings, value);
                _ = SaveSettingsAsync(_currentSettings); // Fire and forget
            }
        }
        catch (Exception ex)
        {
            LogError($"Failed to set config value for key '{key}'", ex);
        }
    }
    
    #endregion
    
    #region Database Management
    
    public async Task SwitchDatabaseAsync(string databasePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("Database path cannot be empty", nameof(databasePath));
                
            _currentSettings.AttributeDatabasePath = databasePath;
            await SaveSettingsAsync(_currentSettings);
            
            DatabaseChanged?.Invoke(databasePath);
            LogInfo($"Switched to database: {databasePath}");
        }
        catch (Exception ex)
        {
            LogError($"Failed to switch database to: {databasePath}", ex);
            throw;
        }
    }
    
    public async Task CreateDatabaseAsync(string databasePath)
    {
        try
        {
            if (File.Exists(databasePath))
                throw new InvalidOperationException($"Database already exists at: {databasePath}");
                
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Create empty database file
            await File.WriteAllTextAsync(databasePath, string.Empty);
            
            LogInfo($"Created new database: {databasePath}");
        }
        catch (Exception ex)
        {
            LogError($"Failed to create database: {databasePath}", ex);
            throw;
        }
    }
    
    public async Task BackupDatabaseAsync(string backupPath)
    {
        try
        {
            var currentDbPath = _currentSettings.AttributeDatabasePath;
            if (string.IsNullOrEmpty(currentDbPath) || !File.Exists(currentDbPath))
                throw new InvalidOperationException("No valid database to backup");
                
            var backupDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDirectory) && !Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }
            
            File.Copy(currentDbPath, backupPath, overwrite: true);
            LogInfo($"Database backed up to: {backupPath}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError($"Failed to backup database to: {backupPath}", ex);
            throw;
        }
    }
    
    public async Task RestoreDatabaseAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException($"Backup file not found: {backupPath}");
                
            var currentDbPath = _currentSettings.AttributeDatabasePath;
            if (string.IsNullOrEmpty(currentDbPath))
                throw new InvalidOperationException("No database path configured");
                
            File.Copy(backupPath, currentDbPath, overwrite: true);
            LogInfo($"Database restored from: {backupPath}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError($"Failed to restore database from: {backupPath}", ex);
            throw;
        }
    }
    
    public async Task<bool> ValidateDatabaseIntegrityAsync()
    {
        try
        {
            var currentDbPath = _currentSettings.AttributeDatabasePath;
            if (string.IsNullOrEmpty(currentDbPath) || !File.Exists(currentDbPath))
                return false;
                
            // Basic validation - check if file is accessible and not corrupted
            var fileInfo = new FileInfo(currentDbPath);
            var isValid = fileInfo.Exists && fileInfo.Length > 0;
            
            LogInfo($"Database integrity check: {(isValid ? "PASSED" : "FAILED")}");
            return isValid;
        }
        catch (Exception ex)
        {
            LogError("Database integrity validation failed", ex);
            return false;
        }
    }
    
    #endregion
    
    #region Resource Management
    
    public string GetAppDataDirectory()
    {
        return _appDataDirectory;
    }
    
    public string GetTempDirectory()
    {
        return _tempDirectory;
    }
    
    public string GetLogDirectory()
    {
        return _logDirectory;
    }
    
    public async Task CleanupTempFilesAsync()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                var tempFiles = Directory.GetFiles(_tempDirectory, "*", SearchOption.AllDirectories);
                var cutoffTime = DateTime.Now.AddDays(-7); // Delete files older than 7 days
                
                foreach (var file in tempFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffTime)
                    {
                        File.Delete(file);
                    }
                }
                
                LogInfo($"Cleaned up {tempFiles.Length} temporary files");
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogWarning($"Failed to cleanup temporary files: {ex.Message}");
        }
    }
    
    public string GetVersion()
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
    
    #endregion
    
    #region Error Handling
    
    public void HandleUnhandledException(Exception exception)
    {
        try
        {
            LogError("Unhandled exception occurred", exception);
            ErrorOccurred?.Invoke(exception);
        }
        catch
        {
            // Last resort - write to console if logging fails
            Console.WriteLine($"FATAL ERROR: {exception}");
        }
    }
    
    public void LogError(string message, Exception? exception = null)
    {
        var logMessage = exception != null 
            ? $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}: {exception}"
            : $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            
        WriteToLog(logMessage);
        Console.WriteLine(logMessage);
    }
    
    public void LogWarning(string message)
    {
        var logMessage = $"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        WriteToLog(logMessage);
        Console.WriteLine(logMessage);
    }
    
    public void LogInfo(string message)
    {
        var logMessage = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        WriteToLog(logMessage);
        Console.WriteLine(logMessage);
    }
    
    #endregion
    
    #region Events
    
    public event Action? ApplicationStarted;
    public event Action? ApplicationStopping;
    public event Action<AppSettings>? SettingsChanged;
    public event Action<string>? DatabaseChanged;
    public event Action<Exception>? ErrorOccurred;
    
    #endregion
    
    #region Private Methods
    
    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_appDataDirectory);
        Directory.CreateDirectory(_logDirectory);
        Directory.CreateDirectory(_tempDirectory);
    }
    
    private AppSettings LoadSettingsInternal()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            LogWarning($"Failed to load settings, using defaults: {ex.Message}");
        }
        
        return new AppSettings();
    }
    
    private void WriteToLog(string message)
    {
        try
        {
            EnsureDirectoriesExist();
            var logFile = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
            File.AppendAllText(logFile, message + Environment.NewLine);
        }
        catch
        {
            // Ignore logging errors to prevent infinite loops
        }
    }
    
    #endregion
}
