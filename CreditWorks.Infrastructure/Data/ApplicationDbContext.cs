using System;
using CreditWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Manufacturer> Manufacturers
        => Set<Manufacturer>();

    public DbSet<VehicleCategory> VehicleCategories
        => Set<VehicleCategory>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Owner)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(v => v.Weight)
                .HasPrecision(18, 2);

            entity.HasOne(v => v.Manufacturer)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(m => m.Name).IsUnique();
        });

        modelBuilder.Entity<VehicleCategory>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.MinWeight)
                .HasPrecision(18, 2);

            entity.Property(c => c.MaxWeight)
                .HasPrecision(18, 2);

            entity.Property(c => c.Icon)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Manufacturer>().HasData(
    new Manufacturer { Id = 1, Name = "Mazda" },
    new Manufacturer { Id = 2, Name = "Mercedes" },
    new Manufacturer { Id = 3, Name = "Honda" },
    new Manufacturer { Id = 4, Name = "Ferrari" },
    new Manufacturer { Id = 5, Name = "Toyota" }
);

        modelBuilder.Entity<VehicleCategory>().HasData(
            new VehicleCategory
            {
                Id = 1,
                Name = "Light",
                MinWeight = 0m,
                MaxWeight = 500m,
                Icon = "car"
            },
            new VehicleCategory
            {
                Id = 2,
                Name = "Medium",
                MinWeight = 500m,
                MaxWeight = 2500m,
                Icon = "truck"
            },
            new VehicleCategory
            {
                Id = 3,
                Name = "Heavy",
                MinWeight = 2500m,
                MaxWeight = null,
                Icon = "truck-heavy"
            }
        );
    }

}