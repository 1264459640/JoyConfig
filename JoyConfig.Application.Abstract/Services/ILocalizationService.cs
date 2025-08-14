using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace JoyConfig.Application.Abstract.Services;

/// <summary>
/// 本地化服务接�?- 提供多语言支持和本地化资源访问
/// </summary>
public interface ILocalizationService : INotifyPropertyChanged
{
    /// <summary>
    /// 通过键获取本地化字符�?    /// </summary>
    /// <param name="key">本地化键</param>
    /// <returns>本地化后的字符串，如果找不到则返回null</returns>
    string? this[string key] { get; }

    /// <summary>
    /// 当前UI文化
    /// </summary>
    CultureInfo CurrentCulture { get; set; }

    /// <summary>
    /// 支持的语言列表
    /// </summary>
    IEnumerable<CultureInfo> SupportedLanguages { get; }

    /// <summary>
    /// 问候语
    /// </summary>
    string? Greeting { get; }

    /// <summary>
    /// 属性数据库
    /// </summary>
    string? AttributeDatabase { get; }

    /// <summary>
    /// 游戏效果数据�?    /// </summary>
    string? GameplayEffectDatabase { get; }

    /// <summary>
    /// 设置
    /// </summary>
    string? Settings { get; }
}
