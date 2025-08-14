using System.Threading.Tasks;

namespace JoyConfig.UI.Abstract.Abstractions.WindowManagement;

/// <summary>
/// 窗口管理抽象接口
/// 遵循接口隔离原则，专注于窗口生命周期和状态管理
/// </summary>
public interface IWindowManager
{
    /// <summary>
    /// 显示主窗口
    /// </summary>
    /// <returns>显示任务</returns>
    Task ShowMainWindowAsync();
    
    /// <summary>
    /// 隐藏主窗口
    /// </summary>
    /// <returns>隐藏任务</returns>
    Task HideMainWindowAsync();
    
    /// <summary>
    /// 关闭主窗口
    /// </summary>
    /// <returns>关闭任务</returns>
    Task CloseMainWindowAsync();
    
    /// <summary>
    /// 设置窗口标题
    /// </summary>
    /// <param name="title">窗口标题</param>
    void SetWindowTitle(string title);
}

/// <summary>
/// 窗口状态管理接口
/// 专门负责窗口的显示状态控制
/// </summary>
public interface IWindowStateManager
{
    /// <summary>
    /// 最小化窗口
    /// </summary>
    void MinimizeWindow();
    
    /// <summary>
    /// 最大化窗口
    /// </summary>
    void MaximizeWindow();
    
    /// <summary>
    /// 还原窗口
    /// </summary>
    void RestoreWindow();
    
    /// <summary>
    /// 获取窗口状态
    /// </summary>
    /// <returns>窗口状态</returns>
    string GetWindowState();
    
    /// <summary>
    /// 设置窗口状态
    /// </summary>
    /// <param name="state">窗口状态</param>
    void SetWindowState(string state);
}