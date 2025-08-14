using System;
using System.IO;
using System.Threading.Tasks;
using JoyConfig.Base.Constants;

namespace JoyConfig.Base.Utilities;

/// <summary>
/// 文件操作辅助�?/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 确保目录存在，如果不存在则创�?    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 获取应用程序数据目录
    /// </summary>
    /// <returns>应用程序数据目录路径</returns>
    public static string GetApplicationDataDirectory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appDataPath, ApplicationConstants.ApplicationName);
        EnsureDirectoryExists(appDirectory);
        return appDirectory;
    }

    /// <summary>
    /// 获取模板目录
    /// </summary>
    /// <returns>模板目录路径</returns>
    public static string GetTemplatesDirectory()
    {
        var templatesPath = Path.Combine(AppContext.BaseDirectory, ApplicationConstants.TemplatesFolderName);
        EnsureDirectoryExists(templatesPath);
        return templatesPath;
    }

    /// <summary>
    /// 安全地读取文件内�?    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件内容，如果读取失败则返回null</returns>
    public static async Task<string?> SafeReadAllTextAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception)
        {
            // 记录日志或处理异常
            return null;
        }
    }

    /// <summary>
    /// 安全地写入文件内�?    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">文件内容</param>
    /// <returns>是否写入成功</returns>
    public static async Task<bool> SafeWriteAllTextAsync(string filePath, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                EnsureDirectoryExists(directory);
            }

            await File.WriteAllTextAsync(filePath, content);
            return true;
        }
        catch (Exception)
        {
            // 记录日志或处理异常
            return false;
        }
    }

    /// <summary>
    /// 检查文件是否具有指定扩展名
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="extension">扩展名（包含点号�?/param>
    /// <returns>如果文件具有指定扩展名则返回true</returns>
    public static bool HasExtension(string filePath, string extension)
    {
        return Path.GetExtension(filePath).Equals(extension, StringComparison.OrdinalIgnoreCase);
    }
}
