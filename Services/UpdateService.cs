using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Reflection;

namespace JoyConfig.Services;

public interface IUpdateService
{
    Task CheckForUpdatesAsync();
    Task<bool> IsUpdateAvailableAsync();
    Task InstallUpdatesAndRestartAsync();
}

public class UpdateService : IUpdateService
{
    private readonly string _repoOwner = "1264459640";
    private readonly string _repoName = "JoyConfig";
    private readonly HttpClient _httpClient;

    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "JoyConfig-UpdateChecker");
    }

    public async Task CheckForUpdatesAsync()
    {
        try
        {
            var hasUpdate = await IsUpdateAvailableAsync();
            
            if (hasUpdate)
            {
                Debug.WriteLine("Update available");
            }
            else
            {
                Debug.WriteLine("No updates available");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking for updates: {ex.Message}");
        }
    }

    public async Task<bool> IsUpdateAvailableAsync()
    {
        try
        {
            var url = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";
            var response = await _httpClient.GetStringAsync(url);
            
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;
            
            if (root.TryGetProperty("tag_name", out var tagElement))
            {
                var latestVersion = tagElement.GetString();
                var currentVersion = GetCurrentVersion();
                
                Debug.WriteLine($"Current version: {currentVersion}, Latest version: {latestVersion}");
                
                // Simple version comparison (you might want to use a more sophisticated method)
                return !string.Equals(currentVersion, latestVersion, StringComparison.OrdinalIgnoreCase);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking for updates: {ex.Message}");
            return false;
        }
    }

    public async Task InstallUpdatesAndRestartAsync()
    {
        try
        {
            // For now, just open the releases page in the browser
            var url = $"https://github.com/{_repoOwner}/{_repoName}/releases/latest";
            
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            
            Process.Start(psi);
            
            await Task.Delay(1000); // Give time for browser to open
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening releases page: {ex.Message}");
            throw;
        }
    }
    
    private string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}