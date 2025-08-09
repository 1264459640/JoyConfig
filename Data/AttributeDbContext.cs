using JoyConfig.Models;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.Data;

public class AttributeDbContext : DbContext
{
    public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }

    public AttributeDbContext(DbContextOptions<AttributeDbContext> options) : base(options)
    {
    }
}
