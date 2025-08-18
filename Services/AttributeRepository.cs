using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Models.DTOs;

namespace JoyConfig.Services;

/// <summary>
/// 属性数据访问实现
/// </summary>
public class AttributeRepository : IAttributeRepository
{
    private readonly IDbContextFactory _dbContextFactory;

    public AttributeRepository(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<Attribute>> GetAllAttributesAsync()
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.Attributes.AsNoTracking().ToListAsync();
    }

    public async Task<Attribute?> GetAttributeByIdAsync(string id)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.Attributes.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Attribute>> GetAttributesByCategoryAsync(string category)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.Attributes.AsNoTracking()
                          .Where(a => a.Category == category)
                          .ToListAsync();
    }

    public async Task<Attribute> CreateAttributeAsync(Attribute attribute)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        context.Attributes.Add(attribute);
        await context.SaveChangesAsync();
        return attribute;
    }

    public async Task UpdateAttributeAsync(Attribute attribute)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        context.Attributes.Update(attribute);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAttributeAsync(string attributeId)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        await context.DeleteAttributeAsync(attributeId);
    }

    public async Task<AttributeChangePreview> PreviewAttributeChangeAsync(string oldId, string newId, string oldCategory, string newCategory)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.PreviewAttributeChangeAsync(oldId, newId, oldCategory, newCategory);
    }

    public async Task ExecuteAttributeChangeAsync(AttributeChangePreview preview)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        await context.ExecuteAttributeChangeAsync(preview);
    }

    public async Task DeleteCategoryAsync(string categoryName)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        await context.DeleteCategoryAsync(categoryName);
    }

    public async Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(string attributeId)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.GetReferencingAttributeSetsAsync(attributeId);
    }

    public async Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(List<string> attributeIds)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.GetReferencingAttributeSetsAsync(attributeIds);
    }

    public async Task<int> GetAttributeValueCountAsync(List<string> attributeIds)
    {
        await using var context = _dbContextFactory.CreateAttributeDbContext();
        return await context.AttributeValues
            .AsNoTracking()
            .CountAsync(v => attributeIds.Contains(v.AttributeId));
    }
}