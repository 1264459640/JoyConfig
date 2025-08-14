namespace JoyConfig.Base.Constants;

/// <summary>
/// 应用程序常量定义
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// 应用程序名称
    /// </summary>
    public const string ApplicationName = "JoyConfig";

    /// <summary>
    /// 应用程序版本
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// 配置文件名称
    /// </summary>
    public const string ConfigFileName = "appsettings.json";

    /// <summary>
    /// 默认数据库文件名
    /// </summary>
    public const string DefaultDatabaseFileName = "AttributeDatabase.db";

    /// <summary>
    /// 模板文件夹名�?    /// </summary>
    public const string TemplatesFolderName = "Templates";

    /// <summary>
    /// 支持的文件扩展名
    /// </summary>
    public static class FileExtensions
    {
        public const string Database = ".db";
        public const string Json = ".json";
        public const string Template = ".json";
    }

    /// <summary>
    /// 默认设置�?    /// </summary>
    public static class DefaultSettings
    {
        public const string Language = "en-US";
        public const string Theme = "Dark";
        public const bool AutosaveEnabled = true;
        public const int AutosaveInterval = 300; // 5分钟
        public const bool LoadLastDatabaseOnStartup = true;
    }
}
