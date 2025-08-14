namespace JoyConfig.UI.Abstract.Abstractions.UserInterface;

/// <summary>
/// 状态栏管理抽象接口
/// 专注于状态栏的文本和状态显示
/// </summary>
public interface IStatusBarManager
{
    /// <summary>
    /// 设置状态文本
    /// </summary>
    /// <param name="text">状态文本</param>
    void SetStatusText(string text);
    
    /// <summary>
    /// 添加状态栏项
    /// </summary>
    /// <param name="key">项键</param>
    /// <param name="content">项内容</param>
    void AddStatusItem(string key, object content);
    
    /// <summary>
    /// 移除状态栏项
    /// </summary>
    /// <param name="key">项键</param>
    void RemoveStatusItem(string key);
}

/// <summary>
/// 进度指示器管理接口
/// 专门负责进度条和忙碌指示器的管理
/// </summary>
public interface IProgressIndicatorManager
{
    /// <summary>
    /// 显示进度条
    /// </summary>
    /// <param name="value">进度值（0-100）</param>
    /// <param name="text">进度文本</param>
    void ShowProgress(int value, string? text = null);
    
    /// <summary>
    /// 隐藏进度条
    /// </summary>
    void HideProgress();
    
    /// <summary>
    /// 显示忙碌指示器
    /// </summary>
    /// <param name="text">忙碌文本</param>
    void ShowBusy(string? text = null);
    
    /// <summary>
    /// 隐藏忙碌指示器
    /// </summary>
    void HideBusy();
}