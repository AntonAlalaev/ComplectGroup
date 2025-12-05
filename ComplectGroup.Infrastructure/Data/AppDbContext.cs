using ComplectGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplectGroup.Infrastructure.Data;

/// <summary>
/// DbContext для приложения ComplectGroup
/// Управляет взаимодействием с БД через Entity Framework Core
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Таблица "Главы" (разделы комплектации)
    /// </summary>
    public DbSet<Chapter> Chapters { get; set; } = null!;

    /// <summary>
    /// Таблица "Детали" 
    /// </summary>
    public DbSet<Part> Parts { get; set; } = null!;

    /// <summary>
    /// Таблица "Позиции" (элементы комплектации)
    /// </summary>
    public DbSet<Position> Positions { get; set; } = null!;

    /// <summary>
    /// Таблица "Комплектации"
    /// </summary>
    public DbSet<Complectation> Complectations { get; set; } = null!;

    /// <summary>
    /// Конфигурация моделей при создании БД
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Конфигурация Chapter =====
        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.ToTable("Chapters");
        });

        // ===== Конфигурация Part =====
        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            // Foreign Key для Chapter
            entity.HasOne(e => e.Chapter)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable("Parts");
        });

        // ===== Конфигурация Position =====
        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Quantity)
                .IsRequired();

            // Foreign Key для Part
            entity.HasOne(e => e.Part)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign Key для Complectation (нужно добавить в Entity)
            entity.HasOne<Complectation>()
                .WithMany(c => c.Positions)
                .HasForeignKey(p => p.ComplectationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable("Positions");
        });

        // ===== Конфигурация Complectation =====
        modelBuilder.Entity<Complectation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Manager)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Customer)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ShippingDate)
                .IsRequired();

            entity.Property(e => e.CreatedDate);

            entity.Property(e => e.ShippingTerms)
                .HasMaxLength(500);

            entity.Property(e => e.TotalWeight)
                .HasPrecision(10, 2); // 10 цифр, 2 после запятой

            entity.Property(e => e.TotalVolume)
                .HasPrecision(10, 2);

            // Связь с Positions (один ко многим)
            entity.HasMany(e => e.Positions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable("Complectations");
        });
    }
}