using AttributeDatabaseEditor.Models;
using Microsoft.EntityFrameworkCore;

namespace AttributeDatabaseEditor.Data;

public class AttributeDbContext : DbContext
{
    public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }

    public AttributeDbContext(DbContextOptions<AttributeDbContext> options) : base(options)
    {
    }
}
