using JoyConfig.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.Infrastructure.Data;

public class AttributeDbContext : DbContext
{
    public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }

    public AttributeDbContext(DbContextOptions<AttributeDbContext> options) : base(options)
    {
    }
}
