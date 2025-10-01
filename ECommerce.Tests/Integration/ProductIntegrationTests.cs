using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ECommerce.BLL.DTOs.Auth;
using ECommerce.BLL.DTOs.Product;
using FluentAssertions;

namespace ECommerce.Tests.Integration
{
    public class ProductIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ProductIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var registerDto = new RegisterDto
            {
                UserName = $"produser_{Guid.NewGuid()}",
                Email = $"prod_{Guid.NewGuid()}@example.com",
                Password = "Test123!@#",
                ConfirmPassword = "Test123!@#"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return result.AccessToken;
        }

        [Fact]
        public async Task GetProducts_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetProducts_WithAuth_ShouldReturnProducts()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/products");
            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            products.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateProduct_WithValidData_ShouldReturnCreatedProduct()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createDto = new CreateProductDto
            {
                Category = "Test Category",
                ProductCode = $"P{new Random().Next(100, 999)}",
                Name = "Test Product",
                Price = 99.99m,
                MinimumQuantity = 1,
                DiscountRate = 10m
            };

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(createDto.Category), "Category");
            content.Add(new StringContent(createDto.ProductCode), "ProductCode");
            content.Add(new StringContent(createDto.Name), "Name");
            content.Add(new StringContent(createDto.Price.ToString()), "Price");
            content.Add(new StringContent(createDto.MinimumQuantity.ToString()), "MinimumQuantity");
            content.Add(new StringContent(createDto.DiscountRate.ToString()), "DiscountRate");

            // Act
            var response = await _client.PostAsync("/api/products", content);
            var result = await response.Content.ReadFromJsonAsync<ProductDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Product");
        }

        [Fact]
        public async Task GetProductById_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // First create a product
            var productCode = $"P{new Random().Next(100, 999)}";
            var createDto = new CreateProductDto
            {
                Category = "Electronics",
                ProductCode = productCode,
                Name = "Specific Product",
                Price = 199.99m,
                MinimumQuantity = 1,
                DiscountRate = 5m
            };

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(createDto.Category), "Category");
            content.Add(new StringContent(createDto.ProductCode), "ProductCode");
            content.Add(new StringContent(createDto.Name), "Name");
            content.Add(new StringContent(createDto.Price.ToString()), "Price");
            content.Add(new StringContent(createDto.MinimumQuantity.ToString()), "MinimumQuantity");
            content.Add(new StringContent(createDto.DiscountRate.ToString()), "DiscountRate");

            var createResponse = await _client.PostAsync("/api/products", content);
            var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

            // Act
            var response = await _client.GetAsync($"/api/products/{createdProduct.Id}");
            var result = await response.Content.ReadFromJsonAsync<ProductDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Id.Should().Be(createdProduct.Id);
            result.Name.Should().Be("Specific Product");
        }

        [Fact]
        public async Task UpdateProduct_WithValidData_ShouldReturnUpdatedProduct()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // First create a product
            var productCode = $"P{new Random().Next(100, 999)}";
            var createDto = new CreateProductDto
            {
                Category = "Electronics",
                ProductCode = productCode,
                Name = "Original Name",
                Price = 99.99m,
                MinimumQuantity = 1,
                DiscountRate = 0m
            };

            var createContent = new MultipartFormDataContent();
            createContent.Add(new StringContent(createDto.Category), "Category");
            createContent.Add(new StringContent(createDto.ProductCode), "ProductCode");
            createContent.Add(new StringContent(createDto.Name), "Name");
            createContent.Add(new StringContent(createDto.Price.ToString()), "Price");
            createContent.Add(new StringContent(createDto.MinimumQuantity.ToString()), "MinimumQuantity");
            createContent.Add(new StringContent(createDto.DiscountRate.ToString()), "DiscountRate");

            var createResponse = await _client.PostAsync("/api/products", createContent);
            var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

            // Update the product
            var updateContent = new MultipartFormDataContent();
            updateContent.Add(new StringContent("Updated Name"), "Name");
            updateContent.Add(new StringContent("149.99"), "Price");

            // Act
            var response = await _client.PutAsync($"/api/products/{createdProduct.Id}", updateContent);
            var result = await response.Content.ReadFromJsonAsync<ProductDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.Price.Should().Be(149.99m);
        }

        [Fact]
        public async Task DeleteProduct_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // First create a product
            var productCode = $"P{new Random().Next(100, 999)}";
            var createDto = new CreateProductDto
            {
                Category = "Electronics",
                ProductCode = productCode,
                Name = "Product To Delete",
                Price = 99.99m,
                MinimumQuantity = 1,
                DiscountRate = 0m
            };

            var createContent = new MultipartFormDataContent();
            createContent.Add(new StringContent(createDto.Category), "Category");
            createContent.Add(new StringContent(createDto.ProductCode), "ProductCode");
            createContent.Add(new StringContent(createDto.Name), "Name");
            createContent.Add(new StringContent(createDto.Price.ToString()), "Price");
            createContent.Add(new StringContent(createDto.MinimumQuantity.ToString()), "MinimumQuantity");
            createContent.Add(new StringContent(createDto.DiscountRate.ToString()), "DiscountRate");

            var createResponse = await _client.PostAsync("/api/products", createContent);
            var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/products/{createdProduct.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the product is deleted
            var getResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCategories_WithAuth_ShouldReturnCategories()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/products/categories");
            var categories = await response.Content.ReadFromJsonAsync<List<string>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            categories.Should().NotBeNull();
        }
    }
}