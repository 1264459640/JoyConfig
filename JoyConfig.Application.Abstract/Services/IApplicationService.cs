using System;
using System.Threading.Tasks;
using JoyConfig.Infrastructure.Models.Settings;

namespace JoyConfig.Application.Abstract.Services;

/// <summary>
/// 应用程序服务接口 - 提供应用程序级别操作的抽象契�?/// 遵循依赖倒置原则，管理应用程序的生命周期和全局状�?/// </summary>
public interface IApplicationService
{
    #region Application Lifecycle
    
    /// <summary>
    /// 初始化应用程�?    /// </summary>
    /// <returns>初始化任�?/returns>
    Task InitializeAsync();
    
    /// <summary>
    /// 启动应用程序
    /// </summary>
    /// <returns>启动任务</returns>
    Task StartAsync();
    
    /// <summary>
    /// 停止应用程序
    /// </summary>
    /// <returns>停止任务</returns>
    Task StopAsync();
    
    /// <summary>
    /// 关闭应用程序
    /// </summary>
    /// <param name="exitCode">退出代�?/param>
    void Shutdown(int exitCode = 0);
    
    /// <summary>
    /// 重启应用程序
    /// </summary>
    /// <returns>重启任务</returns>
    Task RestartAsync();
    
    #endregion
    
    #region Configuration Management
    
    /// <summary>
    /// 获取应用程序设置
    /// </summary>
    /// <returns>应用程序设置</returns>
    AppSettings GetSettings();
    
    /// <summary>
    /// 保存应用程序设置
    /// </summary>
    /// <param name="settings">应用程序设置</param>
    /// <returns>保存任务</returns>
    Task SaveSettingsAsync(AppSettings settings);
    
    /// <summary>
    /// 重置设置为默认�?    /// </summary>
    /// <returns>重置任务</returns>
    Task ResetSettingsAsync();
    
    /// <summary>
    /// 获取配置�?    /// </summary>
    /// <typeparam name="T">配置值类�?/typeparam>
    /// <param name="key">配置�?/param>
    /// <param name="defaultValue">默认�?/param>
    /// <returns>配置�?/returns>
    T GetConfigValue<T>(string key, T defaultValue = default!);
    
    /// <summary>
    /// 设置配置�?    /// </summary>
    /// <typeparam name="T">配置值类�?/typeparam>
    /// <param name="key">配置�?/param>
    /// <param name="value">配置�?/param>
    void SetConfigValue<T>(string key, T value);
    
    #endregion
    
    #region Database Management
    
    /// <summary>
    /// 切换数据�?    /// </summary>
    /// <param name="databasePath">数据库路�?/param>
    /// <returns>切换任务</returns>
    Task SwitchDatabaseAsync(string databasePath);
    
    /// <summary>
    /// 创建新数据库
    /// </summary>
    /// <param name="databasePath">数据库路�?/param>
    /// <returns>创建任务</returns>
    Task CreateDatabaseAsync(string databasePath);
    
    /// <summary>
    /// 备份数据�?    /// </summary>
    /// <param name="backupPath">备份路径</param>
    /// <returns>备份任务</returns>
    Task BackupDatabaseAsync(string backupPath);
    
    /// <summary>
    /// 恢复数据�?    /// </summary>
    /// <param name="backupPath">备份路径</param>
    /// <returns>恢复任务</returns>
    Task RestoreDatabaseAsync(string backupPath);
    
    /// <summary>
    /// 验证数据库完整�?    /// </summary>
    /// <returns>验证结果</returns>
    Task<bool> ValidateDatabaseIntegrityAsync();
    
    #endregion
    
    #region Resource Management
    
    /// <summary>
    /// 获取应用程序数据目录
    /// </summary>
    /// <returns>数据目录路径</returns>
    string GetAppDataDirectory();
    
    /// <summary>
    /// 获取临时目录
    /// </summary>
    /// <returns>临时目录路径</returns>
    string GetTempDirectory();
    
    /// <summary>
    /// 获取日志目录
    /// </summary>
    /// <returns>日志目录路径</returns>
    string GetLogDirectory();
    
    /// <summary>
    /// 清理临时文件
    /// </summary>
    /// <returns>清理任务</returns>
    Task CleanupTempFilesAsync();
    
    /// <summary>
    /// 获取应用程序版本
    /// </summary>
    /// <returns>版本信息</returns>
    string GetVersion();
    
    #endregion
    
    #region Error Handling
    
    /// <summary>
    /// 处理未捕获异�?    /// </summary>
    /// <param name="exception">异常信息</param>
    void HandleUnhandledException(Exception exception);
    
    /// <summary>
    /// 记录错误
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="exception">异常信息</param>
    void LogError(string message, Exception? exception = null);
    
    /// <summary>
    /// 记录警告
    /// </summary>
    /// <param name="message">警告消息</param>
    void LogWarning(string message);
    
    /// <summary>
    /// 记录信息
    /// </summary>
    /// <param name="message">信息消息</param>
    void LogInfo(string message);
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// 应用程序启动事件
    /// </summary>
    event Action? ApplicationStarted;
    
    /// <summary>
    /// 应用程序停止事件
    /// </summary>
    event Action? ApplicationStopping;
    
    /// <summary>
    /// 设置更改事件
    /// </summary>
    event Action<AppSettings>? SettingsChanged;
    
    /// <summary>
    /// 数据库更改事�?    /// </summary>
    event Action<string>? DatabaseChanged;
    
    /// <summary>
    /// 错误发生事件
    /// </summary>
    event Action<Exception>? ErrorOccurred;
    
    #endregion
}
