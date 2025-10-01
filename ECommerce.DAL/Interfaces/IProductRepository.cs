using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.DAL.Models;

namespace ECommerce.DAL.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetByCodeAsync(string productCode);
        Task<IEnumerable<Product>> GetAllAsync(int page = 1, int pageSize = 10, string? category = null, string? searchTerm = null);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ProductCodeExistsAsync(string productCode, int? excludeId = null);
        Task<int> GetTotalCountAsync(string? category = null, string? searchTerm = null);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<Product>> GetDiscountedProductsAsync(decimal minDiscountRate);
        Task<string> GenerateNextProductCodeAsync();
        Task UpdateProductStockAsync(int productId, int quantity);
    }
}
