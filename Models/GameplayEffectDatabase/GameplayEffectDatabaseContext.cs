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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=Example/GameplayEffectDatabase.db");

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
