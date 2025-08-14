using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JoyConfig.Application.ViewModels;
using JoyConfig.Configuration;

namespace JoyConfig.UI;

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
            // 简化启动逻辑，但保持IoC容器支持
            try
            {
                desktop.MainWindow = new JoyConfig.UI.MainWindow
                {
                    DataContext = JoyConfig.Configuration.AppContainer.Resolve<MainViewModel>()
                };
            }
            catch
            {
                // 如果IoC容器失败，使用简单的启动方式
                desktop.MainWindow = new JoyConfig.UI.MainWindow();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
