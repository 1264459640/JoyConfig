using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using JoyConfig.Application.Abstract.Services;

namespace JoyConfig.Application.Services;

public class DefaultLocalizationService : ILocalizationService
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public CultureInfo CurrentCulture { get; set; } = CultureInfo.CurrentCulture;
    
    public string? this[string key] => key; // 返回key本身作为默认值
    
    public IEnumerable<CultureInfo> SupportedLanguages => new[] { CultureInfo.CurrentCulture };
    
    public string? Greeting => "Hello";
    
    public string? AttributeDatabase => "Attribute Database";
    
    public string? GameplayEffectDatabase => "Gameplay Effect Database";
    
    public string? Settings => "Settings";
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}