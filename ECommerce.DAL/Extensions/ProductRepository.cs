using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.DAL.Interfaces;
using ECommerce.DAL.Models;
using ECommerce.DAL.Models.AppDbContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Extensions
{
    public class ProductRepository : IProductRepository
    {
        private readonly ECommerceDbContext _context;

        public ProductRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);
        }

        public async Task<Product?> GetByCodeAsync(string productCode)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductCode == productCode && p.IsActive);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(int page = 1, int pageSize = 10, string? category = null, string? searchTerm = null)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive);

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
            }

            // Filter by search term (name or product code)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.ProductCode.Contains(searchTerm));
            }

            return await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false; // Soft delete
                product.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.ProductId == id && p.IsActive);
        }

        public async Task<bool> ProductCodeExistsAsync(string productCode, int? excludeId = null)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.ProductCode == productCode && p.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.ProductId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetTotalCountAsync(string? category = null, string? searchTerm = null)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.ProductCode.Contains(searchTerm));
            }

            return await query.CountAsync();
        }

       //stored procedures methods

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            var categoryParam = new SqlParameter("@Category", category);

            return await _context.Products
                .FromSqlRaw("EXEC sp_GetProductsByCategory @Category", categoryParam)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetDiscountedProductsAsync(decimal minDiscountRate)
        {
            var discountParam = new SqlParameter("@MinDiscountRate", minDiscountRate);

            return await _context.Products
                .FromSqlRaw("EXEC sp_GetDiscountedProducts @MinDiscountRate", discountParam)
                .ToListAsync();
        }

        public async Task<string> GenerateNextProductCodeAsync()
        {
            var codeParam = new SqlParameter
            {
                ParameterName = "@NextProductCode",
                SqlDbType = SqlDbType.VarChar,
                Size = 20,
                Direction = ParameterDirection.Output
            };

            await _context.Database
                .ExecuteSqlRawAsync("EXEC sp_GenerateNextProductCode @NextProductCode OUTPUT", codeParam);

            return codeParam.Value?.ToString() ?? "P001";
        }

        public async Task UpdateProductStockAsync(int productId, int quantity)
        {
            var productIdParam = new SqlParameter("@ProductId", productId);
            var quantityParam = new SqlParameter("@Quantity", quantity);

            await _context.Database
                .ExecuteSqlRawAsync("EXEC sp_UpdateProductStock @ProductId, @Quantity",
                    productIdParam, quantityParam);
        }
    }
}

