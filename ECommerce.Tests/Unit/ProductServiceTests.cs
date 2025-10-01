using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.DTOs.Product;
using ECommerce.BLL.Interfaces;
using ECommerce.BLL.Services;
using ECommerce.DAL.Interfaces;
using ECommerce.DAL.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Tests.Unit
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IFileService> _mockFileService;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockFileService = new Mock<IFileService>();
            _productService = new ProductService(_mockProductRepository.Object, _mockFileService.Object);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                ProductCode = "P001",
                Name = "Test Product",
                Price = 99.99m
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(product);
            _mockFileService.Setup(x => x.GetFileUrl(It.IsAny<string>()))
                .Returns("http://example.com/image.jpg");

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.ProductCode.Should().Be("P001");
        }

        [Fact]
        public async Task CreateProductAsync_WithValidData_ShouldReturnCreatedProduct()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Category = "Electronics",
                ProductCode = "P001",
                Name = "New Product",
                Price = 199.99m,
                MinimumQuantity = 1,
                DiscountRate = 10m
            };

            _mockProductRepository.Setup(x => x.ProductCodeExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
            _mockProductRepository.Setup(x => x.CreateAsync(It.IsAny<Product>()))
                .ReturnsAsync((Product p) => { p.ProductId = 1; return p; });
            _mockFileService.Setup(x => x.GetFileUrl(It.IsAny<string>()))
                .Returns("http://example.com/image.jpg");

            // Act
            var result = await _productService.CreateProductAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.ProductCode.Should().Be("P001");
            result.Price.Should().Be(199.99m);
        }

        [Fact]
        public async Task CreateProductAsync_WithDuplicateProductCode_ShouldThrowException()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                ProductCode = "P001",
                Name = "Product",
                Price = 99.99m
            };

            _mockProductRepository.Setup(x => x.ProductCodeExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _productService.CreateProductAsync(createDto)
            );
        }

        [Fact]
        public async Task GetProductsAsync_WithFilters_ShouldReturnFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductCode = "P001", Category = "Electronics" },
                new Product { ProductId = 2, ProductCode = "P002", Category = "Electronics" }
            };

            _mockProductRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(products);
            _mockFileService.Setup(x => x.GetFileUrl(It.IsAny<string>()))
                .Returns("http://example.com/image.jpg");

            // Act
            var result = await _productService.GetProductsAsync(1, 10, "Electronics", null);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(p => p.Category == "Electronics").Should().BeTrue();
        }

        [Fact]
        public async Task DeleteProductAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                ImagePath = "products/image.jpg"
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(product);
            _mockProductRepository.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockFileService.Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _productService.DeleteProductAsync(1);

            // Assert
            result.Should().BeTrue();
            _mockFileService.Verify(x => x.DeleteFileAsync(It.IsAny<string>()), Times.Once);
        }
    }
}