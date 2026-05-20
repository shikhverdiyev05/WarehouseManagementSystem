using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WarehouseMS.Models;

namespace WarehouseMS.Data
{
    // IdentityDbContext<AppUser> istifadə edirik ki, sistem AppUser modelini tanısın
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
    {

        // Biznes Cədvəlləri
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Sale> Sales { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity-nin daxili konfiqurasiyalarını qoruyub saxlayırıq
            base.OnModelCreating(builder);

            // İlişkilərin (Relationship) və Məhdudiyyətlərin (Constraints) qurulması

            // 1. Decimal sütunlar üçün dəqiqlik (SQL-də xəta almamalıyıq)
            // Decimal sahələr üçün SQL dəqiqliyi
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // Cədvəl adlarını SQL-də yaratdıqlarımıza bağlayırıq
            builder.Entity<AppUser>().ToTable("AspNetUsers");

            // 2. Product - Category əlaqəsi (One-to-Many)
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Stock - Product əlaqəsi
            builder.Entity<Stock>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Warehouse - Shelf əlaqəsi
            builder.Entity<Shelf>()
                .HasOne(s => s.Warehouse)
                .WithMany(w => w.Shelves)
                .HasForeignKey(s => s.WarehouseId);

            // 5. Unikal SKU və Barcode
            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(p => p.Barcode)
                .IsUnique();

        }
    }
}