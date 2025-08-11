using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JoyConfig.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.Models.AttributeDatabase;

public partial class AttributeDatabaseContext : DbContext
{
    public static string? DbPath { get; set; }

    public AttributeDatabaseContext()
    {
    }

    public AttributeDatabaseContext(string dbPath)
    {
        DbPath = dbPath;
    }

    public AttributeDatabaseContext(DbContextOptions<AttributeDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attribute> Attributes { get; set; }

    public virtual DbSet<AttributeSet> AttributeSets { get; set; }

    public virtual DbSet<AttributeValue> AttributeValues { get; set; }

    public virtual DbSet<VBasicAttribute> VBasicAttributes { get; set; }

    public virtual DbSet<VBulletAttribute> VBulletAttributes { get; set; }

    public virtual DbSet<VShipAttribute> VShipAttributes { get; set; }

    public virtual DbSet<VWeaponDamage> VWeaponDamages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath ?? "Example/AttributeDatabase.db"}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttributeValue>(entity =>
        {
            entity.Property(e => e.AttributeId).HasColumnName("AttributeType");
            
            entity.HasIndex(e => new { e.AttributeSetId, e.AttributeId }, "IX_AttributeValues_AttributeSetId_AttributeType").IsUnique();

            entity.Property(e => e.MaxValue).HasDefaultValue(999999.0);
            entity.Property(e => e.MinValue).HasDefaultValue(-999999.0);

            entity.HasOne(d => d.AttributeSet).WithMany(p => p.AttributeValues).HasForeignKey(d => d.AttributeSetId);

            entity.HasOne(d => d.Attribute).WithMany(p => p.AttributeValues).HasForeignKey(d => d.AttributeId);
        });

        modelBuilder.Entity<VBasicAttribute>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_BasicAttributes");
        });

        modelBuilder.Entity<VBulletAttribute>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_BulletAttributes");
        });

        modelBuilder.Entity<VShipAttribute>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_ShipAttributes");
        });

        modelBuilder.Entity<VWeaponDamage>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_WeaponDamage");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public async Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(string attributeId)
    {
        return await GetReferencingAttributeSetsAsync(new List<string> { attributeId });
    }

    public async Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(List<string> attributeIds)
    {
        if (attributeIds == null || !attributeIds.Any())
        {
            return new List<AttributeSet>();
        }

        var query = from attrValue in AttributeValues
            join attrSet in AttributeSets on attrValue.AttributeSetId equals attrSet.Id
            where attributeIds.Contains(attrValue.AttributeId)
            select attrSet;

        return await query.Distinct().ToListAsync();
    }

    public async Task<AttributeChangePreview> PreviewAttributeChangeAsync(string oldId, string newId, string oldCategory, string newCategory)
    {
        Console.WriteLine("[DB] PreviewAttributeChangeAsync started.");
        var preview = new AttributeChangePreview
        {
            OldId = oldId,
            NewId = newId,
            OldCategory = oldCategory,
            NewCategory = newCategory
        };

        // Case 1: Category name has changed, affecting multiple attributes
        if (oldCategory != newCategory)
        {
            preview.AffectedAttributes = await Attributes.AsNoTracking()
                .Where(a => a.Category == oldCategory)
                .ToListAsync();
        }
        // Case 2: Only a single attribute ID is changing
        else if (oldId != newId)
        {
            var singleAttr = await Attributes.AsNoTracking().FirstOrDefaultAsync(a => a.Id == oldId);
            if (singleAttr != null)
            {
                preview.AffectedAttributes.Add(singleAttr);
            }
        }
        else
        {
            // No change
            return preview;
        }

        if (!preview.AffectedAttributes.Any())
        {
            return preview; // No attributes found to update
        }

        var affectedIds = preview.AffectedAttributes.Select(a => a.Id).ToList();
        Console.WriteLine($"[DB] Preview: Found {affectedIds.Count} affected attributes.");

        preview.AffectedAttributeSets = await GetReferencingAttributeSetsAsync(affectedIds);
        preview.AffectedValueCount = await AttributeValues.AsNoTracking().CountAsync(v => affectedIds.Contains(v.AttributeId));
        Console.WriteLine($"[DB] Preview: Found {preview.AffectedAttributeSets.Count} referencing sets and {preview.AffectedValueCount} values.");

        // Final validation: Check if any new IDs would conflict
        foreach (var attr in preview.AffectedAttributes)
        {
            var potentialNewId = attr.Id.Replace(oldCategory, newCategory);
            if (await Attributes.AsNoTracking().AnyAsync(a => a.Id == potentialNewId && a.Id != attr.Id))
            {
                preview.IsValid = false;
                preview.ErrorMessage = $"Conflict: An attribute with the new ID '{potentialNewId}' already exists.";
                return preview;
            }
        }
        
        preview.IsValid = true;

        return preview;
    }

    public async Task ExecuteAttributeChangeAsync(AttributeChangePreview preview)
    {
        if (!preview.IsValid || !preview.AffectedAttributes.Any())
        {
            return;
        }

        await using var transaction = await Database.BeginTransactionAsync();
        try
        {
            var idMap = preview.AffectedAttributes.ToDictionary(
                a => a.Id,
                a => a.Id.Replace(preview.OldCategory, preview.NewCategory)
            );
            
            // In case of a single ID change, the replace logic won't work, so we use the preview properties
            if (preview.OldCategory == preview.NewCategory && preview.OldId != preview.NewId)
            {
                idMap[preview.OldId] = preview.NewId;
            }

            // Step 1: Create new parent records (Attributes)
            foreach (var oldId in idMap.Keys)
            {
                var originalAttribute = preview.AffectedAttributes.First(a => a.Id == oldId);
                var newId = idMap[oldId];
                var newCategory = preview.NewCategory;
                var description = originalAttribute.Description;

                await Database.ExecuteSqlInterpolatedAsync(@$"
                    INSERT INTO Attributes (Id, Category, Description) 
                    VALUES ({newId}, {newCategory}, {description});");
            }

            // Step 2: Update foreign keys in child records (AttributeValues)
            foreach (var oldId in idMap.Keys)
            {
                var newId = idMap[oldId];
                // Use the correct column name 'AttributeType' in raw SQL
                await Database.ExecuteSqlInterpolatedAsync(@$"
                    UPDATE AttributeValues 
                    SET AttributeType = {newId} 
                    WHERE AttributeType = {oldId};");
                
                // Also update the denormalized AttributeCategory
                await Database.ExecuteSqlInterpolatedAsync(@$"
                    UPDATE AttributeValues
                    SET AttributeCategory = {preview.NewCategory}
                    WHERE AttributeType = {newId};");
            }

            // Step 3: Delete old parent records (Attributes)
            foreach (var oldId in idMap.Keys)
            {
                await Database.ExecuteSqlInterpolatedAsync(@$"
                    DELETE FROM Attributes 
                    WHERE Id = {oldId};");
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"A transactional error occurred during the attribute update. The operation was rolled back. Details: {ex.Message}", ex);
        }
    }

    public async Task DeleteCategoryAsync(string categoryName)
    {
        await using var transaction = await Database.BeginTransactionAsync();
        try
        {
            // Find all attributes in the category
            var attributesToDelete = await Attributes
                .Where(a => a.Category == categoryName)
                .ToListAsync();

            if (attributesToDelete.Any())
            {
                var attributeIds = attributesToDelete.Select(a => a.Id).ToList();

                // Find all referencing AttributeValues for these attributes
                var valuesToDelete = await AttributeValues
                    .Where(v => attributeIds.Contains(v.AttributeId))
                    .ToListAsync();
                
                // Remove children first
                if (valuesToDelete.Any())
                {
                    AttributeValues.RemoveRange(valuesToDelete);
                }
                
                // Then remove parents
                Attributes.RemoveRange(attributesToDelete);

                await SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"A transactional error occurred while deleting the category '{categoryName}'. The operation was rolled back. Details: {ex.Message}", ex);
        }
    }

    public async Task DeleteAttributeAsync(string attributeId)
    {
        await using var transaction = await Database.BeginTransactionAsync();
        try
        {
        // Step 1: Delete all referencing AttributeValues
            // Use the correct column name 'AttributeType' in raw SQL
            await Database.ExecuteSqlInterpolatedAsync(@$"
                DELETE FROM AttributeValues 
                WHERE AttributeType = {attributeId};");

            // Step 2: Delete the Attribute itself
            await Database.ExecuteSqlInterpolatedAsync(@$"
                DELETE FROM Attributes 
                WHERE Id = {attributeId};");

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"A transactional error occurred while deleting the attribute. The operation was rolled back. Details: {ex.Message}", ex);
        }
    }

    public async Task DeleteAttributeSetAsync(string attributeSetId)
    {
        await using var transaction = await Database.BeginTransactionAsync();
        try
        {
            // Step 1: Delete all AttributeValues associated with the AttributeSet
            await Database.ExecuteSqlInterpolatedAsync(@$"
                DELETE FROM AttributeValues 
                WHERE AttributeSetId = {attributeSetId};");

            // Step 2: Delete the AttributeSet itself
            await Database.ExecuteSqlInterpolatedAsync(@$"
                DELETE FROM AttributeSets 
                WHERE Id = {attributeSetId};");

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"A transactional error occurred while deleting the attribute set. The operation was rolled back. Details: {ex.Message}", ex);
        }
    }

    public static async Task<(bool IsValid, string? ErrorMessage)> ValidateDatabaseSchemaAsync(string? dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath) || !System.IO.File.Exists(dbPath))
        {
            return (false, "Database file path is not valid.");
        }

        var context = new AttributeDatabaseContext(dbPath);
        try
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            var requiredTables = new[] { "Attributes", "AttributeSets", "AttributeValues" };
            foreach (var table in requiredTables)
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}';";
                if (await command.ExecuteScalarAsync() == null)
                {
                    return (false, $"Missing required table: {table}");
                }
            }

            // Optional: Add column checks for critical tables
            // Example for Attributes table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info(Attributes);";
                using var reader = await command.ExecuteReaderAsync();
                var columns = new HashSet<string>();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString(1)); // Column name is at index 1
                }
                if (!columns.Contains("Id") || !columns.Contains("Category"))
                {
                    return (false, "Table 'Attributes' is missing required columns.");
                }
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred during database schema validation: {ex.Message}");
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}
