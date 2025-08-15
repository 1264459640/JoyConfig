using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JoyConfig.Services;
using JoyConfig.ViewModels;
using JoyConfig.Infrastructure;
using System;
using System.Globalization;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using Autofac;

namespace JoyConfig;

public partial class App : Application
{
    private IContainer? _container;
    
    public override void Initialize()
    {
        Console.WriteLine("App.Initialize() started.");
        AvaloniaXamlLoader.Load(this);
        
        // 配置依赖注入容器
        ConfigureContainer();
        
        Console.WriteLine("App.Initialize() finished.");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Console.WriteLine("App.OnFrameworkInitializationCompleted() started.");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Console.WriteLine("ApplicationLifetime is IClassicDesktopStyleApplicationLifetime.");
            
            // 确保容器已配置
            if (_container == null)
            {
                throw new InvalidOperationException("依赖注入容器未正确配置");
            }
            
            // 设置本地化
            var localizationManager = _container.Resolve<LocalizationManager>();
            localizationManager.CurrentCulture = CultureInfo.CurrentUICulture;

            // 设置默认数据库路径
            var dbContextFactory = _container.Resolve<IDbContextFactory>();
            dbContextFactory.SetAttributeDatabasePath("Example/AttributeDatabase.db");

            // 创建主ViewModel
            var viewModelFactory = _container.Resolve<IViewModelFactory>();
            var mainViewModel = viewModelFactory.CreateMainViewModel();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            Console.WriteLine("MainWindow created.");
        }

        base.OnFrameworkInitializationCompleted();
        Console.WriteLine("App.OnFrameworkInitializationCompleted() finished.");
    }
    
    /// <summary>
    /// 配置AutoFac依赖注入容器
    /// </summary>
    private void ConfigureContainer()
    {
        var builder = new ContainerBuilder();
        
        // 注册AutoFac模块
        builder.RegisterModule<AutofacModule>();
        
        // 构建容器
        _container = builder.Build();
        
        Console.WriteLine("依赖注入容器配置完成");
    }
    
    /// <summary>
    /// 获取依赖注入容器
    /// </summary>
    public IContainer? Container => _container;
}
