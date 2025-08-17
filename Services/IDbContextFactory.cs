using System;
using System.Threading.Tasks;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Models.GameplayEffectDatabase;

namespace JoyConfig.Services;

/// <summary>
/// 数据库上下文工厂接口
/// </summary>
public interface IDbContextFactory
{
    /// <summary>
    /// 创建属性数据库上下文
    /// </summary>
    AttributeDatabaseContext CreateAttributeDbContext();
    
    /// <summary>
    /// 创建游戏效果数据库上下文
    /// </summary>
    GameplayEffectDatabaseContext CreateGameplayEffectDatabaseContext();
    
    /// <summary>
    /// 验证属性数据库架构
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateAttributeDatabaseSchemaAsync();
    
    /// <summary>
    /// 设置属性数据库路径
    /// </summary>
    void SetAttributeDatabasePath(string path);
    
    /// <summary>
    /// 设置游戏效果数据库路径
    /// </summary>
    void SetGameplayEffectDatabasePath(string path);
    
    /// <summary>
    /// 获取当前属性数据库路径
    /// </summary>
    string? GetAttributeDatabasePath();
    
    /// <summary>
    /// 获取当前游戏效果数据库路径
    /// </summary>
    string? GetGameplayEffectDatabasePath();
}