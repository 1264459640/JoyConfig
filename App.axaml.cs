using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JoyConfig.Application.ViewModels;
using JoyConfig.Views;

namespace JoyConfig;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = JoyConfig.AppContainer.Resolve<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
