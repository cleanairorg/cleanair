using System;
using System.Collections.Generic;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Scaffolding;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DeviceThreshold> DeviceThresholds { get; set; }

    public virtual DbSet<Devicelog> Devicelogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceThreshold>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("device_threshold_pkey");

            entity.ToTable("device_threshold", "weatherstation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Deviceid).HasColumnName("deviceid");
            entity.Property(e => e.GoodMax).HasColumnName("good_max");
            entity.Property(e => e.GoodMin).HasColumnName("good_min");
            entity.Property(e => e.Metric).HasColumnName("metric");
            entity.Property(e => e.WarnMax).HasColumnName("warn_max");
            entity.Property(e => e.WarnMin).HasColumnName("warn_min");
        });

        modelBuilder.Entity<Devicelog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("devicelog_pkey");

            entity.ToTable("devicelog", "weatherstation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Airquality).HasColumnName("airquality");
            entity.Property(e => e.Deviceid).HasColumnName("deviceid");
            entity.Property(e => e.Humidity)
                .HasPrecision(4, 2)
                .HasColumnName("humidity");
            entity.Property(e => e.Pressure)
                .HasPrecision(6, 2)
                .HasColumnName("pressure");
            entity.Property(e => e.Temperature)
                .HasPrecision(4, 2)
                .HasColumnName("temperature");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.Unit).HasColumnName("unit");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user", "weatherstation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Salt).HasColumnName("salt");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
