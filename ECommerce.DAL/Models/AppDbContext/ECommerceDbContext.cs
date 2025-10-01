using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ECommerce.DAL.Models.AppDbContext
{
    public class ECommerceDbContext : DbContext
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                // Unique constraints
                entity.HasIndex(u => u.UserName)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_UserName");

                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                // Column configurations
                entity.Property(u => u.Password)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.RefreshToken)
                    .HasMaxLength(500);

                entity.Property(u => u.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(u => u.ModifiedDate)
                    .HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                // Unique constraints
                entity.HasIndex(p => p.ProductCode)
                    .IsUnique()
                    .HasDatabaseName("IX_Products_ProductCode");

                // Column configurations
                entity.Property(p => p.Price)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(p => p.DiscountRate)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(0);

                entity.Property(p => p.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(p => p.ModifiedDate)
                    .HasDefaultValueSql("GETDATE()");
            });

            SeedInitialData(modelBuilder);
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Seed sample products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Category = "Electronics",
                    ProductCode = "P001",
                    Name = "Smartphone",
                    Price = 699.99m,
                    MinimumQuantity = 1,
                    DiscountRate = 10m,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 2,
                    Category = "Electronics",
                    ProductCode = "P002",
                    Name = "Laptop",
                    Price = 1299.99m,
                    MinimumQuantity = 1,
                    DiscountRate = 15m,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 3,
                    Category = "Clothing",
                    ProductCode = "P003",
                    Name = "T-Shirt",
                    Price = 29.99m,
                    MinimumQuantity = 5,
                    DiscountRate = 5m,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 4,
                    Category = "Electronics",
                    ProductCode = "P004",
                    Name = "Wireless Headphones",
                    Price = 149.99m,
                    MinimumQuantity = 1,
                    DiscountRate = 20m,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 5,
                    Category = "Books",
                    ProductCode = "P005",
                    Name = "Programming Guide",
                    Price = 59.99m,
                    MinimumQuantity = 1,
                    DiscountRate = 0m,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsActive = true
                }
            );

        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedEntries)
            {
                if (entry.Entity is User user)
                {
                    user.ModifiedDate = DateTime.Now;
                }
                else if (entry.Entity is Product product)
                {
                    product.ModifiedDate = DateTime.Now;
                }
            }
        }
    }
}