using System;
using System.Threading.Tasks;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Models.GameplayEffectDatabase;

namespace JoyConfig.Services;

/// <summary>
/// 数据库上下文工厂实现
/// </summary>
public class DbContextFactory : IDbContextFactory
{
    private string? _attributeDatabasePath;
    private string? _gameplayEffectDatabasePath;
    
    public DbContextFactory()
    {
        // 设置默认路径
        _attributeDatabasePath = "Example/AttributeDatabase.db";
        _gameplayEffectDatabasePath = "Example/GameplayEffectDatabase.db";
    }
    
    public AttributeDatabaseContext CreateAttributeDbContext()
    {
        // 在创建上下文之前设置数据库路径
        if (!string.IsNullOrEmpty(_attributeDatabasePath))
        {
            AttributeDatabaseContext.DbPath = _attributeDatabasePath;
        }
        var context = new AttributeDatabaseContext();
        return context;
    }
    
    public GameplayEffectDatabaseContext CreateGameplayEffectDbContext()
    {
        return new GameplayEffectDatabaseContext();
    }
    
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateAttributeDatabaseSchemaAsync()
    {
        return await AttributeDatabaseContext.ValidateDatabaseSchemaAsync(_attributeDatabasePath);
    }
    
    public void SetAttributeDatabasePath(string path)
    {
        _attributeDatabasePath = path;
        AttributeDatabaseContext.DbPath = path;
    }
    
    public void SetGameplayEffectDatabasePath(string path)
    {
        _gameplayEffectDatabasePath = path;
    }
    
    public string? GetAttributeDatabasePath()
    {
        return _attributeDatabasePath;
    }
    
    public string? GetGameplayEffectDatabasePath()
    {
        return _gameplayEffectDatabasePath;
    }
}