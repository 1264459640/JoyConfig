using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AttributeDatabaseEditor.Models.AttributeDatabase;

public partial class AttributeDatabaseContext : DbContext
{
    public AttributeDatabaseContext()
    {
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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=Example/AttributeDatabase.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttributeValue>(entity =>
        {
            entity.HasIndex(e => new { e.AttributeSetId, e.AttributeType }, "IX_AttributeValues_AttributeSetId_AttributeType").IsUnique();

            entity.Property(e => e.MaxValue).HasDefaultValue(999999.0);
            entity.Property(e => e.MinValue).HasDefaultValue(-999999.0);

            entity.HasOne(d => d.AttributeSet).WithMany(p => p.AttributeValues).HasForeignKey(d => d.AttributeSetId);

            entity.HasOne(d => d.AttributeTypeNavigation).WithMany(p => p.AttributeValues).HasForeignKey(d => d.AttributeType);
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
}
