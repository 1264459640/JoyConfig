using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoyConfig.UI.Abstract.Abstractions.Navigation;

/// <summary>
/// 视图导航抽象接口
/// 专注于页面导航功能
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// 导航到指定视图
    /// </summary>
    /// <param name="viewName">视图名称</param>
    /// <param name="parameters">导航参数</param>
    /// <returns>导航任务</returns>
    Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null);
    
    /// <summary>
    /// 导航到指定视图类型
    /// </summary>
    /// <typeparam name="T">视图类型</typeparam>
    /// <param name="parameters">导航参数</param>
    /// <returns>导航任务</returns>
    Task NavigateToAsync<T>(Dictionary<string, object>? parameters = null) where T : class;
}

/// <summary>
/// 导航历史管理接口
/// 专门负责导航历史的管理
/// </summary>
public interface INavigationHistoryManager
{
    /// <summary>
    /// 返回上一个视图
    /// </summary>
    /// <returns>导航任务</returns>
    Task GoBackAsync();
    
    /// <summary>
    /// 前进到下一个视图
    /// </summary>
    /// <returns>导航任务</returns>
    Task GoForwardAsync();
    
    /// <summary>
    /// 清除导航历史
    /// </summary>
    void ClearHistory();
    
    /// <summary>
    /// 检查是否可以返回
    /// </summary>
    /// <returns>是否可以返回</returns>
    bool CanGoBack();
    
    /// <summary>
    /// 检查是否可以前进
    /// </summary>
    /// <returns>是否可以前进</returns>
    bool CanGoForward();
}