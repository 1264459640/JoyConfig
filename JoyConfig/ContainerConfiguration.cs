using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JoyConfig.Application.Services;
using JoyConfig.Infrastructure.Services;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Infrastructure.Data;
using JoyConfig.Application.ViewModels;

namespace JoyConfig;

/// <summary>
/// IoC容器配置类
/// 负责配置和管理所有依赖注入关系
/// </summary>
public static class ContainerConfiguration
{
    /// <summary>
    /// 配置所有服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="builder">Autofac容器构建器</param>
    public static void ConfigureServices(IServiceCollection services, ContainerBuilder builder)
    {
        ConfigureMicrosoftServices(services);
        ConfigureAutofacServices(builder);
    }
    
    /// <summary>
    /// 配置Microsoft DI服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private static void ConfigureMicrosoftServices(IServiceCollection services)
    {
        // 基础服务
        services.AddLogging();
        services.AddOptions();
        
        // 数据库上下文
        services.AddDbContext<AttributeDatabaseContext>();
        services.AddDbContext<GameplayEffectDatabaseContext>();
        
        // 配置日志
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }
    
    /// <summary>
    /// 配置Autofac服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    private static void ConfigureAutofacServices(ContainerBuilder builder)
    {
        // 注册各层服务 - 先注册依赖项，再注册使用者
        RegisterApplicationLayer(builder);
        RegisterInfrastructureLayer(builder);
        RegisterAbstractLayer(builder);
        RegisterUIAbstractLayer(builder);
    }
    
    /// <summary>
    /// 注册基础设施层服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    private static void RegisterInfrastructureLayer(ContainerBuilder builder)
    {
        // 数据访问服务
        builder.RegisterType<DataRepository>()
               .As<IDataRepository>()
               .InstancePerLifetimeScope();
        
        // 应用程序生命周期服务
        builder.RegisterType<JoyConfig.Application.Services.ApplicationService>()
               .As<IApplicationService>()
               .SingleInstance();
        
        // 模板管理服务
        builder.RegisterType<TemplateService>()
               .As<ITemplateService>()
               .InstancePerLifetimeScope();
    }
    
    /// <summary>
    /// 注册应用层服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    private static void RegisterApplicationLayer(ContainerBuilder builder)
    {
        // 对话框服务
        builder.RegisterType<DialogService>()
               .As<IDialogService>()
               .InstancePerLifetimeScope();
        
        // 本地化管理器
        builder.RegisterType<DefaultLocalizationService>()
               .As<ILocalizationService>()
               .SingleInstance();
        
        // ViewModels (按需注册)
        RegisterViewModels(builder);
    }
    
    /// <summary>
    /// 注册ViewModels
    /// </summary>
    /// <param name="builder">容器构建器</param>
    private static void RegisterViewModels(ContainerBuilder builder)
    {
        // 扫描并注册所有ViewModel
        builder.RegisterAssemblyTypes(typeof(JoyConfig.Application.ViewModels.MainViewModel).Assembly)
               .Where(t => t.Name.EndsWith("ViewModel"))
               .AsSelf()
               .InstancePerDependency();
    }
    
    /// <summary>
    /// 注册抽象层服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    private static void RegisterAbstractLayer(ContainerBuilder builder)
    {
        // UI服务接口实现
        builder.RegisterType<MainViewModel>()
                .As<IUIService>()
               .InstancePerLifetimeScope();
    }
    
    /// <summary>
    /// 注册UI抽象层服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    private static void RegisterUIAbstractLayer(ContainerBuilder builder)
    {
        // UI抽象服务的具体实现将在UI项目中注册
        // 这里可以注册一些默认实现或占位符
        
        // 窗口管理服务（待UI项目实现）
        // builder.RegisterType<WindowManager>()
        //        .As<IWindowManager>()
        //        .SingleInstance();
        
        // 导航服务（待UI项目实现）
        // builder.RegisterType<NavigationService>()
        //        .As<INavigationService>()
        //        .SingleInstance();
        
        // 主题管理服务（待UI项目实现）
        // builder.RegisterType<ThemeManager>()
        //        .As<IThemeManager>()
        //        .SingleInstance();
    }
    
    /// <summary>
    /// 验证容器配置
    /// </summary>
    /// <param name="container">容器实例</param>
    /// <returns>验证是否成功</returns>
    public static bool ValidateConfiguration(IContainer container)
    {
        try
        {
            // 验证关键服务是否可以解析
            var appService = container.Resolve<IApplicationService>();
            var dataRepository = container.Resolve<IDataRepository>();
            var dialogService = container.Resolve<IDialogService>();
            
            return appService != null && dataRepository != null && dialogService != null;
        }
        catch (Exception ex)
        {
            var logger = container.ResolveOptional<ILogger>();
            logger?.LogError(ex, "容器配置验证失败");
            return false;
        }
    }
}