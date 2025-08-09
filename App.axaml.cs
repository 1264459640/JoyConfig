using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JoyConfig.ViewModels;
using System;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;

namespace JoyConfig;

public partial class App : Application
{
    public override void Initialize()
    {
        Console.WriteLine("App.Initialize() started.");
        AvaloniaXamlLoader.Load(this);
        Console.WriteLine("App.Initialize() finished.");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Console.WriteLine("App.OnFrameworkInitializationCompleted() started.");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Console.WriteLine("ApplicationLifetime is IClassicDesktopStyleApplicationLifetime.");
            var mainViewModel = new MainViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            Console.WriteLine("MainWindow created.");
        }

        base.OnFrameworkInitializationCompleted();
        Console.WriteLine("App.OnFrameworkInitializationCompleted() finished.");
    }
}
