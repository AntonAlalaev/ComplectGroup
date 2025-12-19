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


    public DbSet<WarehouseItem> WarehouseItems { get; set; }
    public DbSet<ReceiptTransaction> ReceiptTransactions { get; set; }
    public DbSet<ShippingTransaction> ShippingTransactions { get; set; }
    public DbSet<PositionShipment> PositionShipments { get; set; }



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
            
            // Foreign Key для Complectation
            entity.HasOne<Complectation>()
                .WithMany(c => c.Positions)
                .HasForeignKey(p => p.ComplectationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            // Связь с PositionShipment (один к одному)
            entity.HasOne(e => e.Shipment)
                .WithOne(ps => ps.Position)
                .HasForeignKey<PositionShipment>(ps => ps.PositionId)
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
                .HasPrecision(10, 2);
            
            entity.Property(e => e.TotalVolume)
                .HasPrecision(10, 2);

            // ← Статус отгрузки комплектации
            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(ComplectationStatus.Draft)
                .HasConversion<int>(); // Сохраняется как int в БД

            // ← Дата фактической полной отгрузки
            entity.Property(e => e.FullyShippedDate);            
            
            // Связь с Positions (один ко многим)
            entity.HasMany(e => e.Positions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.ToTable("Complectations");
        });

        // ===== Конфигурация WarehouseItem =====
        modelBuilder.Entity<WarehouseItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AvailableQuantity)
                .IsRequired();
            
            entity.Property(e => e.ReservedQuantity)
                .IsRequired();
            
            entity.Property(e => e.LastModifiedDate)
                .IsRequired();
            
            // Foreign Key для Part
            entity.HasOne(e => e.Part)
                .WithMany()
                .HasForeignKey(e => e.PartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.ToTable("WarehouseItems");
        });

        // ===== Конфигурация ReceiptTransaction =====
        modelBuilder.Entity<ReceiptTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Quantity)
                .IsRequired();
            
            entity.Property(e => e.ReceiptDate)
                .IsRequired();
            
            entity.Property(e => e.Notes)
                .HasMaxLength(500);
            
            // Foreign Key для Part
            entity.HasOne(e => e.Part)
                .WithMany()
                .HasForeignKey(e => e.PartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.ToTable("ReceiptTransactions");
        });

        // ===== Конфигурация ShippingTransaction =====
        modelBuilder.Entity<ShippingTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Quantity)
                .IsRequired();
            
            entity.Property(e => e.ShippingDate)
                .IsRequired();
            
            entity.Property(e => e.Notes)
                .HasMaxLength(500);
            
            // Foreign Key для Part
            entity.HasOne(e => e.Part)
                .WithMany()
                .HasForeignKey(e => e.PartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            // Foreign Key для Position
            entity.HasOne(e => e.Position)
                .WithMany()
                .HasForeignKey(e => e.PositionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.ToTable("ShippingTransactions");
        });

        // ===== Конфигурация PositionShipment =====
        modelBuilder.Entity<PositionShipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ShippedQuantity)
                .IsRequired();
            
            entity.Property(e => e.FirstShippedDate);
            
            entity.Property(e => e.LastShippedDate);
            
            // Foreign Key для Position (один к одному)
            entity.HasOne(e => e.Position)
                .WithOne(p => p.Shipment)
                .HasForeignKey<PositionShipment>(ps => ps.PositionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.ToTable("PositionShipments");
        });
    }
}