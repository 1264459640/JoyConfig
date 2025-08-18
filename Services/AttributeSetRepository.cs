using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.ViewModels.AttributeDatabase;

namespace JoyConfig.Services;

/// <summary>
/// 属性集数据访问实现
/// </summary>
public class AttributeSetRepository : IAttributeSetRepository
{
    private readonly IDbContextFactory _dbContextFactory;

    public AttributeSetRepository(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<AttributeSet>> GetAllAttributeSetsAsync()
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.AttributeSets.AsNoTracking().ToListAsync();
    }

    public async Task<AttributeSet?> GetAttributeSetByIdAsync(string id)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.AttributeSets.FindAsync(id);
    }

    public async Task<List<AttributeValueViewModel>> GetAttributeValueViewModelsAsync(string attributeSetId, ICommand? removeCommand = null)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();

        var query = from value in context.AttributeValues
                    join attr in context.Attributes on value.AttributeId equals attr.Id
                    where value.AttributeSetId == attributeSetId
                    select new { value, attr };

        var results = await query.ToListAsync();

        return results.Select(r => new AttributeValueViewModel(r.value, removeCommand)
        {
            AttributeName = r.attr.Id,
            AttributeCategory = r.attr.Category
        }).ToList();
    }

    public async Task<AttributeSet> CreateAttributeSetAsync(AttributeSet attributeSet)
    {
        Console.WriteLine($"[CreateAttributeSetAsync] 开始创建属性集：{attributeSet.Id} - {attributeSet.Name}");

        try
        {
            await using var context = _dbContextFactory.CreateAttributeDbContext();
            Console.WriteLine($"[CreateAttributeSetAsync] 数据库上下文创建成功");

            context.AttributeSets.Add(attributeSet);
            Console.WriteLine($"[CreateAttributeSetAsync] 属性集已添加到上下文");

            await context.SaveChangesAsync();
            Console.WriteLine($"[CreateAttributeSetAsync] 属性集保存成功：{attributeSet.Id}");

            return attributeSet;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateAttributeSetAsync] 创建属性集时发生异常：{ex.Message}");
            Console.WriteLine($"[CreateAttributeSetAsync] 异常类型：{ex.GetType().Name}");
            Console.WriteLine($"[CreateAttributeSetAsync] 异常堆栈：{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"[CreateAttributeSetAsync] 内部异常：{ex.InnerException.Message}");
                Console.WriteLine($"[CreateAttributeSetAsync] 内部异常堆栈：{ex.InnerException.StackTrace}");
            }

            throw;
        }
    }

    public async Task UpdateAttributeSetAsync(AttributeSet attributeSet)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        context.AttributeSets.Update(attributeSet);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAttributeSetAsync(string attributeSetId)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        await context.DeleteAttributeSetAsync(attributeSetId);
    }

    public async Task SaveAttributeSetChangesAsync(string attributeSetId, string name, string description)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 更新属性集基本信息
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE AttributeSets 
                SET Name = {name}, Description = {description}
                WHERE Id = {attributeSetId}");

            // 注意：AttributeValues表中没有AttributeSetName列，所以不需要更新
            // 如果将来需要在AttributeValues中存储属性集名称，需要先添加该列到数据库

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task AddAttributeValueAsync(string attributeSetId, string attributeId)
    {
        Console.WriteLine($"[AddAttributeValueAsync] 开始添加属性值：AttributeSetId={attributeSetId}, AttributeId={attributeId}");

        try
        {
            await using var context = _dbContextFactory.CreateAttributeDbContext();
            Console.WriteLine($"[AddAttributeValueAsync] 数据库上下文创建成功");

            // 获取属性信息以获取其分类
            Console.WriteLine($"[AddAttributeValueAsync] 查找属性：{attributeId}");
            var attribute = await context.Attributes.FirstOrDefaultAsync(a => a.Id == attributeId);
            if (attribute == null)
            {
                Console.WriteLine($"[AddAttributeValueAsync] 属性未找到：{attributeId}");
                throw new InvalidOperationException($"Attribute with ID '{attributeId}' not found.");
            }
            Console.WriteLine($"[AddAttributeValueAsync] 找到属性：{attribute.Id} - {attribute.Category}");

            var newValue = new AttributeValue
            {
                AttributeSetId = attributeSetId,
                AttributeId = attributeId,
                AttributeCategory = attribute.Category, // 设置AttributeCategory字段
                BaseValue = 0,
                MinValue = -999999,
                MaxValue = 999999
            };
            Console.WriteLine($"[AddAttributeValueAsync] 创建属性值对象：{newValue.AttributeSetId} - {newValue.AttributeId} - {newValue.AttributeCategory}");

            context.AttributeValues.Add(newValue);
            Console.WriteLine($"[AddAttributeValueAsync] 属性值已添加到上下文");

            await context.SaveChangesAsync();
            Console.WriteLine($"[AddAttributeValueAsync] 属性值保存成功：{attributeSetId} - {attributeId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AddAttributeValueAsync] 添加属性值时发生异常：{ex.Message}");
            Console.WriteLine($"[AddAttributeValueAsync] 异常类型：{ex.GetType().Name}");
            Console.WriteLine($"[AddAttributeValueAsync] 异常堆栈：{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"[AddAttributeValueAsync] 内部异常：{ex.InnerException.Message}");
                Console.WriteLine($"[AddAttributeValueAsync] 内部异常堆栈：{ex.InnerException.StackTrace}");
            }

            throw;
        }
    }

    public async Task RemoveAttributeValueAsync(string attributeSetId, string attributeId)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM AttributeValues 
            WHERE AttributeSetId = {attributeSetId} AND AttributeType = {attributeId}");
    }

    public async Task<bool> AttributeSetExistsAsync(string id)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.AttributeSets.AnyAsync(s => s.Id == id);
    }

    public async Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(List<string> attributeIds)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.GetReferencingAttributeSetsAsync(attributeIds);
    }

    public async Task UpdateAttributeValueAsync(AttributeValue attributeValue)
    {
        Console.WriteLine($"[UpdateAttributeValueAsync] 开始更新属性值：AttributeSetId={attributeValue.AttributeSetId}, AttributeId={attributeValue.AttributeId}");

        try
        {
            await using var context = _dbContextFactory.CreateAttributeDbContext();
            Console.WriteLine($"[UpdateAttributeValueAsync] 数据库上下文创建成功");

            await using var transaction = await context.Database.BeginTransactionAsync();
            Console.WriteLine($"[UpdateAttributeValueAsync] 事务开始");

            try
            {
                await context.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE AttributeValues 
                    SET BaseValue = {attributeValue.BaseValue}, 
                        MinValue = {attributeValue.MinValue}, 
                        MaxValue = {attributeValue.MaxValue}
                    WHERE AttributeSetId = {attributeValue.AttributeSetId} 
                      AND AttributeType = {attributeValue.AttributeId}");
                Console.WriteLine($"[UpdateAttributeValueAsync] SQL更新执行成功");

                await transaction.CommitAsync();
                Console.WriteLine($"[UpdateAttributeValueAsync] 事务提交成功：{attributeValue.AttributeSetId} - {attributeValue.AttributeId}");
            }
            catch
            {
                Console.WriteLine($"[UpdateAttributeValueAsync] 事务回滚");
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateAttributeValueAsync] 更新属性值时发生异常：{ex.Message}");
            Console.WriteLine($"[UpdateAttributeValueAsync] 异常类型：{ex.GetType().Name}");
            Console.WriteLine($"[UpdateAttributeValueAsync] 异常堆栈：{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"[UpdateAttributeValueAsync] 内部异常：{ex.InnerException.Message}");
                Console.WriteLine($"[UpdateAttributeValueAsync] 内部异常堆栈：{ex.InnerException.StackTrace}");
            }

            throw;
        }
    }

    public async Task UpdateAttributeSetIdAsync(string oldId, string newId)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 首先检查新ID是否已存在
            var existingSet = await context.AttributeSets.FindAsync(newId);
            if (existingSet != null)
            {
                throw new InvalidOperationException($"属性集ID '{newId}' 已存在");
            }

            // 获取原始属性集信息
            var originalSet = await context.AttributeSets.FindAsync(oldId);
            if (originalSet == null)
            {
                throw new InvalidOperationException($"属性集ID '{oldId}' 不存在");
            }

            // 步骤1: 创建新的属性集记录
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO AttributeSets (Id, Name, Description) 
                VALUES ({newId}, {originalSet.Name}, {originalSet.Description})");

            // 步骤2: 更新AttributeValues表中的外键引用
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE AttributeValues 
                SET AttributeSetId = {newId}
                WHERE AttributeSetId = {oldId}");

            // 步骤3: 删除旧的属性集记录
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM AttributeSets 
                WHERE Id = {oldId}");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}