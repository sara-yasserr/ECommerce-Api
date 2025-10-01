using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.DTOs.Product;
using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Interfaces;
using ECommerce.DAL.Models;

namespace ECommerce.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileService _fileService;

        public ProductService(IProductRepository productRepository, IFileService fileService)
        {
            _productRepository = productRepository;
            _fileService = fileService;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? MapToProductDto(product) : null;
        }

        public async Task<ProductDto?> GetProductByCodeAsync(string productCode)
        {
            var product = await _productRepository.GetByCodeAsync(productCode);
            return product != null ? MapToProductDto(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync(int page = 1, int pageSize = 10, string? category = null, string? searchTerm = null)
        {
            var products = await _productRepository.GetAllAsync(page, pageSize, category, searchTerm);
            return products.Select(MapToProductDto);
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _productRepository.GetCategoriesAsync();
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            // Check if product code already exists
            if (await _productRepository.ProductCodeExistsAsync(createProductDto.ProductCode))
            {
                throw new InvalidOperationException("Product code already exists");
            }

            // Handle image upload
            string? imagePath = null;
            if (createProductDto.Image != null)
            {
                imagePath = await _fileService.SaveFileAsync(createProductDto.Image, "products");
            }

            var product = new Product
            {
                Category = createProductDto.Category,
                ProductCode = createProductDto.ProductCode,
                Name = createProductDto.Name,
                ImagePath = imagePath,
                Price = createProductDto.Price,
                MinimumQuantity = createProductDto.MinimumQuantity,
                DiscountRate = createProductDto.DiscountRate,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsActive = true
            };

            var createdProduct = await _productRepository.CreateAsync(product);
            return MapToProductDto(createdProduct);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            // Check if new product code already exists (excluding current product)
            if (!string.IsNullOrEmpty(updateProductDto.ProductCode) &&
                updateProductDto.ProductCode != product.ProductCode &&
                await _productRepository.ProductCodeExistsAsync(updateProductDto.ProductCode, id))
            {
                throw new InvalidOperationException("Product code already exists");
            }

            // Handle image upload
            if (updateProductDto.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    await _fileService.DeleteFileAsync(product.ImagePath);
                }

                product.ImagePath = await _fileService.SaveFileAsync(updateProductDto.Image, "products");
            }

            // Update product properties
            if (!string.IsNullOrEmpty(updateProductDto.Category))
                product.Category = updateProductDto.Category;

            if (!string.IsNullOrEmpty(updateProductDto.ProductCode))
                product.ProductCode = updateProductDto.ProductCode;

            if (!string.IsNullOrEmpty(updateProductDto.Name))
                product.Name = updateProductDto.Name;

            if (updateProductDto.Price.HasValue)
                product.Price = updateProductDto.Price.Value;

            if (updateProductDto.MinimumQuantity.HasValue)
                product.MinimumQuantity = updateProductDto.MinimumQuantity.Value;

            if (updateProductDto.DiscountRate.HasValue)
                product.DiscountRate = updateProductDto.DiscountRate.Value;

            product.ModifiedDate = DateTime.Now;

            var updatedProduct = await _productRepository.UpdateAsync(product);
            return MapToProductDto(updatedProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null && !string.IsNullOrEmpty(product.ImagePath))
            {
                // Delete associated image file
                await _fileService.DeleteFileAsync(product.ImagePath);
            }

            return await _productRepository.DeleteAsync(id);
        }

        private ProductDto MapToProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.ProductId,
                Category = product.Category,
                ProductCode = product.ProductCode,
                Name = product.Name,
                ImageUrl = !string.IsNullOrEmpty(product.ImagePath) ? _fileService.GetFileUrl(product.ImagePath) : null,
                Price = product.Price,
                MinimumQuantity = product.MinimumQuantity,
                DiscountRate = product.DiscountRate,
                CreatedAt = product.CreatedDate,
                UpdatedAt = product.ModifiedDate,
                IsActive = product.IsActive
            };
        }
    }
}
