using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using JoyConfig.Application.Abstract.Services;

namespace JoyConfig.Application.Services;

public class LocalizationManager : ObservableObject, ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    // 移除静态单例，改为通过DI提供
    // private static LocalizationManager? _instance;

    public LocalizationManager()
    {
        _resourceManager = new ResourceManager("JoyConfig.Resources.Strings", Assembly.GetEntryAssembly()!);
    }

    // public static LocalizationManager Instance => _instance ??= new LocalizationManager();

    public string? this[string key] => _resourceManager.GetString(key, CultureInfo.CurrentUICulture);

    public CultureInfo CurrentCulture
    {
        get => CultureInfo.CurrentUICulture;
        set
        {
            if (Equals(CultureInfo.CurrentUICulture, value)) return;
            CultureInfo.CurrentUICulture = value;
            OnPropertyChanged(nameof(CurrentCulture));
            OnPropertyChanged(string.Empty); // Update all bindings
        }
    }

    public string? Greeting => this["Greeting"];
    public string? AttributeDatabase => this["AttributeDatabase"];
    public string? GameplayEffectDatabase => this["GameplayEffectDatabase"];
    public string? Settings => this["Settings"];

    public IEnumerable<CultureInfo> SupportedLanguages { get; } = new List<CultureInfo>
    {
        new("en-US"),
        new("zh-CN")
    };
}
