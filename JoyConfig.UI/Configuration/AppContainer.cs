using Autofac;
using Autofac.Extensions.DependencyInjection;
using JoyConfig.Application.ViewModels;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Infrastructure.Services;
using JoyConfig.Application.Services;
using JoyConfig.Infrastructure.Data;
using JoyConfig.Base.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System;

namespace JoyConfig.Configuration;

/// <summary>
/// Autofac 依赖注入容器配置 - 管理应用程序依赖关系和对象生命周�?
/// 采用四层架构�?_Base -> 1_1_Frontend/1_2_Backend -> 2_Main，依赖方向统一向下
/// </summary>
public static class AppContainer
{
    public static IContainer? Container { get; private set; }

    /// <summary>
    /// 配置并构建依赖注入容�?
    /// </summary>
    public static void Configure()
    {
        var builder = new ContainerBuilder();

        // 1. 注册基础层服�?(0_Base) - 通用工具和基础设施
        RegisterBaseServices(builder);

        // 2. 注册核心层服�?(0_Base) - 接口定义和数据模�?
        RegisterCoreServices(builder);

        // 3. 注册基础设施层服�?(0_Base) - 数据访问、外部服务等
        RegisterInfrastructureServices(builder);

        // 4. 注册前端层服�?(1_1_Frontend) - ViewModels和UI逻辑
        RegisterFrontendServices(builder);

        // 5. 注册后端层服�?(1_2_Backend) - 业务逻辑协调
        RegisterBackendServices(builder);

        Container = builder.Build();
    }

    /// <summary>
    /// 注册基础层服�?- 通用工具、常量、扩展方法等
    /// </summary>
    private static void RegisterBaseServices(ContainerBuilder builder)
    {
        // 基础层通常不需要注册服务，主要提供静态工具类和常�?
        // 如果有需要注册的基础服务，可以在这里添加
    }

    /// <summary>
    /// 注册核心层服�?- 接口定义和数据模�?
    /// </summary>
    private static void RegisterCoreServices(ContainerBuilder builder)
    {
        // Microsoft.Extensions.DependencyInjection 服务注册
        var services = new ServiceCollection();
        
        // 数据库上下文配置
        services.AddDbContext<AttributeDatabaseContext>(options =>
        {
            // 使用基础层常量定义默认数据库路径
            var dbPath = $"Data Source=Example/{ApplicationConstants.DefaultDatabaseFileName}";
            options.UseSqlite(dbPath);
        });

        // �?Microsoft DI 服务注册�?Autofac
        builder.Populate(services);
    }

    /// <summary>
    /// 注册基础设施层服�?- 具体实现�?
    /// </summary>
    private static void RegisterInfrastructureServices(ContainerBuilder builder)
    {
        // 对话框服�?
        builder.RegisterType<DialogService>()
               .As<IDialogService>()
               .SingleInstance();

        // 本地化服�?
        builder.RegisterType<LocalizationManager>()
               .As<ILocalizationService>()
               .SingleInstance();
               
        // 数据访问服务
        builder.RegisterType<DataRepository>()
               .As<IDataRepository>()
               .InstancePerLifetimeScope();
               
        // 模板服务
        builder.RegisterType<TemplateService>()
               .As<ITemplateService>()
               .SingleInstance();
               
        // 应用程序服务
        builder.RegisterType<ApplicationService>()
               .As<IApplicationService>()
               .SingleInstance();
    }

    /// <summary>
    /// 注册前端层服�?- ViewModels和UI相关逻辑
    /// </summary>
    private static void RegisterFrontendServices(ContainerBuilder builder)
    {
        // 使用程序集扫描注册所有ViewModel
        builder.RegisterAssemblyTypes(typeof(MainViewModel).Assembly)
               .Where(t => t.Name.EndsWith("ViewModel"))
               .AsSelf()
               .InstancePerDependency(); // 每次依赖时创建新实例

        // 主ViewModel特殊处理 - 单例模式
        builder.RegisterType<MainViewModel>()
               .AsSelf()
               .As<IUIService>()
               .SingleInstance();
    }

    /// <summary>
    /// 注册后端层服�?- 业务逻辑协调、命令处理等
    /// </summary>
    private static void RegisterBackendServices(ContainerBuilder builder)
    {
        // 这里可以添加业务服务，如�?
        // - 业务逻辑协调�?
        // - 命令处理�?
        // - 业务规则验证�?
        // 
        // 示例�?
        // builder.RegisterType<AttributeBusinessService>()
        //        .As<IAttributeBusinessService>()
        //        .InstancePerDependency();
    }

    /// <summary>
    /// 解析服务实例
    /// </summary>
    /// <typeparam name="T">要解析的服务类型</typeparam>
    /// <returns>服务实例</returns>
    /// <exception cref="InvalidOperationException">当容器未配置时抛�?/exception>
    public static T Resolve<T>() where T : notnull
    {
        if (Container == null)
        {
            throw new InvalidOperationException("Autofac container has not been configured. Please call AppContainer.Configure() first.");
        }
        
        return Container.Resolve<T>();
    }

    /// <summary>
    /// 释放容器资源
    /// </summary>
    public static void Dispose()
    {
        Container?.Dispose();
        Container = null;
    }
}
