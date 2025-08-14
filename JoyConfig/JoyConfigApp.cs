using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JoyConfig.Application.ViewModels;
using JoyConfig.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JoyConfig;

/// <summary>
/// JoyConfig Avalonia应用程序主类
/// 集成后端服务和前端UI的统一应用程序
/// </summary>
public partial class JoyConfigApp : Avalonia.Application
{
    /// <summary>
    /// 初始化应用程序
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// 框架初始化完成后的处理
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                // 配置UI容器
                AppContainer.Configure();
                
                // 分别创建ViewModels避免循环依赖
                var mainViewModel = AppContainer.Resolve<MainViewModel>();
                var attributeDatabaseViewModel = AppContainer.Resolve<AttributeDatabaseViewModel>();
                
                // 初始化默认工作区
                mainViewModel.InitializeDefaultWorkspace(attributeDatabaseViewModel);
                
                // 创建主窗口
                desktop.MainWindow = new JoyConfig.UI.MainWindow
                {
                    DataContext = mainViewModel
                };
                
                // 记录启动成功
                if (Program.GetLogger() != null)
                {
                    Program.GetLogger()!.LogInformation("JoyConfig UI界面启动成功");
                }
            }
            catch (Exception ex)
            {
                // 记录启动失败
                Console.WriteLine($"UI界面启动失败: {ex.Message}");
                if (Program.GetLogger() != null)
                {
                    Program.GetLogger()!.LogError(ex, "UI界面启动失败");
                }
                throw;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}