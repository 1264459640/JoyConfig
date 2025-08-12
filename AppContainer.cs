using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using JoyConfig.Core.Services;
using JoyConfig.Infrastructure.Services;
using JoyConfig.Application.ViewModels;
using JoyConfig.Core.Models.AttributeDatabase;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace JoyConfig;

public static class AppContainer
{
    public static IContainer? Container { get; private set; }

    public static void Configure()
    {
        var builder = new ContainerBuilder();

        // Register services
        var services = new ServiceCollection();
        services.AddDbContext<AttributeDatabaseContext>(options =>
        {
            // This connection string will be managed by the application's configuration.
            // For now, we use the default path.
            options.UseSqlite("Data Source=Example/AttributeDatabase.db");
        });

        builder.Populate(services);

        // Register ViewModels using assembly scanning
        builder.RegisterAssemblyTypes(typeof(MainViewModel).Assembly)
               .Where(t => t.Name.EndsWith("ViewModel"))
               .AsSelf();
        
        builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();

        // Register other services
        builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
        builder.RegisterType<LocalizationManager>().SingleInstance();

        Container = builder.Build();
    }

    public static T Resolve<T>() where T : notnull
    {
        if (Container == null)
        {
            throw new System.InvalidOperationException("Autofac container has not been configured.");
        }
        return Container.Resolve<T>();
    }
}
