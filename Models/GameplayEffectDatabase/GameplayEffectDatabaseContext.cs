using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.Models.GameplayEffectDatabase;

public partial class GameplayEffectDatabaseContext : DbContext
{
    public GameplayEffectDatabaseContext()
    {
    }

    public GameplayEffectDatabaseContext(DbContextOptions<GameplayEffectDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttributeEffect> AttributeEffects { get; set; }

    public virtual DbSet<AttributeModifier> AttributeModifiers { get; set; }

    public virtual DbSet<GameplayEffectTemplate> GameplayEffectTemplates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // 连接字符串将通过依赖注入配置，此处不需要硬编码
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=Example/GameplayEffectDatabase.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttributeEffect>(entity =>
        {
            entity.Property(e => e.IntervalSeconds).HasDefaultValue(1.0);
            entity.Property(e => e.IsInfinite).HasColumnType("BOOLEAN");
            entity.Property(e => e.IsPassive).HasColumnType("BOOLEAN");
            entity.Property(e => e.IsPeriodic)
                .HasDefaultValue(false)
                .HasColumnType("BOOLEAN");
        });

        modelBuilder.Entity<AttributeModifier>(entity =>
        {
            entity.Property(e => e.ExecutionOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Effect).WithMany(p => p.AttributeModifiers).HasForeignKey(d => d.EffectId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
