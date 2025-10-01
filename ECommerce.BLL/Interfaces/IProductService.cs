using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.DTOs.Product;

namespace ECommerce.BLL.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto?> GetProductByCodeAsync(string productCode);
        Task<IEnumerable<ProductDto>> GetProductsAsync(int page = 1, int pageSize = 10, string? category = null, string? searchTerm = null);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
    }
}
