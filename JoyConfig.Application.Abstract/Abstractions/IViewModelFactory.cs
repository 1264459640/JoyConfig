using System;
using System.Threading.Tasks;

namespace JoyConfig.Application.Abstract.Abstractions;

/// <summary>
/// ViewModel工厂抽象接口 - 提供ViewModel创建和管理的抽象契约
/// </summary>
public interface IViewModelFactory
{
    /// <summary>
    /// 创建指定类型的ViewModel
    /// </summary>
    /// <typeparam name="T">ViewModel类型</typeparam>
    /// <returns>ViewModel实例</returns>
    T CreateViewModel<T>() where T : class;
    
    /// <summary>
    /// 异步创建指定类型的ViewModel
    /// </summary>
    /// <typeparam name="T">ViewModel类型</typeparam>
    /// <returns>ViewModel实例</returns>
    Task<T> CreateViewModelAsync<T>() where T : class;
    
    /// <summary>
    /// 根据类型创建ViewModel
    /// </summary>
    /// <param name="viewModelType">ViewModel类型</param>
    /// <returns>ViewModel实例</returns>
    object CreateViewModel(Type viewModelType);
    
    /// <summary>
    /// 释放ViewModel资源
    /// </summary>
    /// <param name="viewModel">要释放的ViewModel</param>
    void ReleaseViewModel(object viewModel);
}

/// <summary>
/// 编辑器ViewModel抽象接口
/// </summary>
public interface IEditorViewModel
{
    /// <summary>
    /// 编辑器标�?    /// </summary>
    string Title { get; set; }
    
    /// <summary>
    /// 是否有未保存的更�?    /// </summary>
    bool HasUnsavedChanges { get; }
    
    /// <summary>
    /// 保存更改
    /// </summary>
    /// <returns>保存任务</returns>
    Task<bool> SaveAsync();
    
    /// <summary>
    /// 取消更改
    /// </summary>
    /// <returns>取消任务</returns>
    Task<bool> CancelAsync();
    
    /// <summary>
    /// 关闭编辑�?    /// </summary>
    /// <returns>关闭任务</returns>
    Task<bool> CloseAsync();
}

/// <summary>
/// 对话框ViewModel抽象接口
/// </summary>
public interface IDialogViewModel
{
    /// <summary>
    /// 对话框标�?    /// </summary>
    string Title { get; set; }
    
    /// <summary>
    /// 对话框结�?    /// </summary>
    bool? DialogResult { get; set; }
    
    /// <summary>
    /// 确认命令
    /// </summary>
    void Confirm();
    
    /// <summary>
    /// 取消命令
    /// </summary>
    void Cancel();
}

/// <summary>
/// 转换器抽象接�?/// </summary>
public interface IValueConverter
{
    /// <summary>
    /// 转换�?    /// </summary>
    /// <param name="value">源�?/param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">文化信息</param>
    /// <returns>转换后的�?/returns>
    object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture);
    
    /// <summary>
    /// 反向转换�?    /// </summary>
    /// <param name="value">目标�?/param>
    /// <param name="targetType">源类�?/param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">文化信息</param>
    /// <returns>反向转换后的�?/returns>
    object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture);
}
