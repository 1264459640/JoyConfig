using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JoyConfig.Application.Services;
using JoyConfig.Infrastructure.Services;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Infrastructure.Data;
using JoyConfig.Application.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JoyConfig.Configuration;

namespace JoyConfig;

/// <summary>
/// JoyConfig主程序 - 集成后端服务和前端UI的统一应用程序
/// 采用Avalonia UI框架，集成IoC容器管理
/// </summary>
class Program
{
    private static IHost? _host;
    private static ILogger? _logger;
    
    /// <summary>
    /// 应用程序入口点
    /// </summary>
    /// <param name="args">命令行参数</param>
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // 初始化后端服务
            InitializeBackendServices();
            
            // 启动Avalonia UI应用程序
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"应用程序启动失败: {ex.Message}");
            Console.WriteLine($"详细错误: {ex}");
            Environment.Exit(1);
        }
        finally
        {
            // 清理后端服务
            _host?.Dispose();
        }
    }
    
    /// <summary>
     /// 构建Avalonia应用程序
     /// </summary>
     /// <returns>应用程序构建器</returns>
     public static AppBuilder BuildAvaloniaApp()
         => AppBuilder.Configure<JoyConfigApp>()
             .UsePlatformDetect()
             .WithInterFont()
             .LogToTrace();
     
     /// <summary>
     /// 初始化后端服务
     /// </summary>
     private static void InitializeBackendServices()
     {
         try
         {
             // 创建主机构建器
             var hostBuilder = CreateHostBuilder(Array.Empty<string>());
             
             // 构建主机
             _host = hostBuilder.Build();
             
             // 获取日志记录器
             _logger = _host.Services.GetRequiredService<ILogger<Program>>();
             _logger.LogInformation("JoyConfig后端服务初始化中...");
             
             // 获取应用程序服务并初始化
             var appService = _host.Services.GetRequiredService<IApplicationService>();
             Task.Run(async () =>
             {
                 try
                 {
                     await appService.InitializeAsync();
                     _logger.LogInformation("JoyConfig后端服务初始化成功");
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, "后端服务初始化失败");
                 }
             });
         }
         catch (Exception ex)
         {
             Console.WriteLine($"后端服务初始化失败: {ex.Message}");
             throw;
         }
     }
      
      /// <summary>
      /// 获取日志记录器
      /// </summary>
      /// <returns>日志记录器实例</returns>
      public static ILogger? GetLogger() => _logger;
      
      /// <summary>
      /// 创建主机构建器
      /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>主机构建器</returns>
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureServices((context, services) =>
            {
                // 配置Microsoft DI服务
                ConfigureMicrosoftServices(services);
            })
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                // 配置Autofac容器
                ConfigureAutofacContainer(builder);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
            });
    
    /// <summary>
    /// 配置Microsoft DI服务
    /// </summary>
    /// <param name="services">服务集合</param>
    static void ConfigureMicrosoftServices(IServiceCollection services)
    {
        // 添加基础服务
        services.AddLogging();
        services.AddOptions();
        
        // 添加数据库上下文
        services.AddDbContext<AttributeDatabaseContext>();
        services.AddDbContext<GameplayEffectDatabaseContext>();
    }
    
    /// <summary>
    /// 配置Autofac容器
    /// </summary>
    /// <param name="builder">容器构建器</param>
    static void ConfigureAutofacContainer(ContainerBuilder builder)
    {
        // 注册基础设施层服务
        RegisterInfrastructureServices(builder);
        
        // 注册应用层服务
        RegisterApplicationServices(builder);
        
        // 注册抽象接口服务
        RegisterAbstractServices(builder);
        
        // 注册UI抽象服务
        RegisterUIAbstractServices(builder);
    }
    
    /// <summary>
    /// 注册基础设施层服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    static void RegisterInfrastructureServices(ContainerBuilder builder)
    {
        // 注册数据仓储服务
        builder.RegisterType<DataRepository>()
               .As<IDataRepository>()
               .InstancePerLifetimeScope();
        
        // 注册应用程序服务
        builder.RegisterType<ApplicationService>()
               .As<IApplicationService>()
               .SingleInstance();
        
        // 注册模板服务
        builder.RegisterType<TemplateService>()
               .As<ITemplateService>()
               .InstancePerLifetimeScope();
    }
    
    /// <summary>
    /// 注册应用层服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    static void RegisterApplicationServices(ContainerBuilder builder)
    {
        // 注册对话框服务
        builder.RegisterType<DialogService>()
               .As<IDialogService>()
               .InstancePerLifetimeScope();
        
        // 注册本地化管理器
        builder.RegisterType<LocalizationManager>()
               .AsSelf()
               .SingleInstance();
    }
    
    /// <summary>
    /// 注册抽象接口服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    static void RegisterAbstractServices(ContainerBuilder builder)
    {
        // 注册UI服务接口
        builder.RegisterType<MainViewModel>()
                .As<IUIService>()
               .InstancePerLifetimeScope();
    }
    
    /// <summary>
    /// 注册UI抽象服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    static void RegisterUIAbstractServices(ContainerBuilder builder)
    {
        // UI抽象服务将在UI项目中实现并注册
        // JoyConfig作为IoC容器管理中心，不直接依赖UI层
        // UI服务的注册将通过UI项目的启动配置完成
    }
    

}