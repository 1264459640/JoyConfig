using Autofac;
using JoyConfig.Services;
using JoyConfig.ViewModels;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Models.GameplayEffectDatabase;
using System;

namespace JoyConfig.Infrastructure;

/// <summary>
/// AutoFac依赖注入配置模块
/// </summary>
public class AutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // 注册服务层
        RegisterServices(builder);
        
        // 注册数据访问层
        RegisterRepositories(builder);
        
        // 注册ViewModel工厂
        RegisterViewModelFactory(builder);
        
        // 注册ViewModels
        RegisterViewModels(builder);
    }
    
    /// <summary>
    /// 注册服务层组件
    /// </summary>
    private void RegisterServices(ContainerBuilder builder)
    {
        // 对话框服务 - 单例
        builder.RegisterType<DialogService>()
               .As<IDialogService>()
               .SingleInstance();
        
        // 本地化管理器 - 单例
        builder.Register(c => LocalizationManager.Instance)
               .As<LocalizationManager>()
               .SingleInstance();
        
        // 数据库上下文工厂 - 单例
        builder.RegisterType<DbContextFactory>()
               .As<IDbContextFactory>()
               .SingleInstance();
        
        // 更新服务 - 单例
        builder.RegisterType<UpdateService>()
               .As<IUpdateService>()
               .SingleInstance();
    }
    
    /// <summary>
    /// 注册数据访问层组件
    /// </summary>
    private void RegisterRepositories(ContainerBuilder builder)
    {
        // 属性仓储 - 每次请求创建新实例
        builder.RegisterType<AttributeRepository>()
               .As<IAttributeRepository>()
               .InstancePerDependency();
        
        // 属性集仓储 - 每次请求创建新实例
        builder.RegisterType<AttributeSetRepository>()
               .As<IAttributeSetRepository>()
               .InstancePerDependency();
    }
    
    /// <summary>
    /// 注册ViewModel工厂
    /// </summary>
    private void RegisterViewModelFactory(ContainerBuilder builder)
    {
        builder.RegisterType<ViewModelFactory>()
               .As<IViewModelFactory>()
               .SingleInstance();
    }
    
    /// <summary>
    /// 注册ViewModels
    /// </summary>
    private void RegisterViewModels(ContainerBuilder builder)
    {
        // 主ViewModel - 单例
        builder.RegisterType<MainViewModel>()
               .AsSelf()
               .SingleInstance();
        
        // 其他ViewModels - 每次请求创建新实例
        builder.RegisterType<AttributeDatabaseViewModel>()
               .AsSelf()
               .InstancePerDependency();
        
        builder.RegisterType<AttributeViewModel>()
               .AsSelf()
               .InstancePerDependency();
        
        builder.RegisterType<AttributeSetViewModel>()
               .AsSelf()
               .InstancePerDependency();
        
        builder.RegisterType<SettingsViewModel>()
               .AsSelf()
               .InstancePerDependency();
        
        builder.RegisterType<CategoryViewModel>()
               .AsSelf()
               .InstancePerDependency();
        
        builder.RegisterType<SelectAttributeViewModel>()
               .AsSelf()
               .InstancePerDependency();
        
        builder.RegisterType<ConfirmationDialogViewModel>()
               .AsSelf()
               .InstancePerDependency();
        
        builder.RegisterType<InputDialogViewModel>()
               .AsSelf()
               .InstancePerDependency();
    }
}