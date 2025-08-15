using Avalonia;
using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using JoyConfig.Models.Settings;

namespace JoyConfig;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        LoadAndSetLanguage();
        Console.WriteLine("Starting application...");
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error starting application:");
            Console.WriteLine(e.ToString());
        }
        Console.WriteLine("Application stopped.");
    }

    private static void LoadAndSetLanguage()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolderPath = Path.Combine(appDataPath, "JoyConfig");
        var settingsFilePath = Path.Combine(appFolderPath, "settings.json");

        if (!File.Exists(settingsFilePath)) return;

        var json = File.ReadAllText(settingsFilePath);
        var appSettings = JsonSerializer.Deserialize<AppSettings>(json);
        if (appSettings != null && !string.IsNullOrEmpty(appSettings.Language))
        {
            try
            {
                var culture = new CultureInfo(appSettings.Language);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                // Handle invalid language string in settings
                Console.WriteLine($"Invalid language code in settings: {appSettings.Language}");
            }
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
