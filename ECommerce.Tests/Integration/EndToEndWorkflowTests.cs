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
using FluentAssertions;
using ECommerce.BLL.DTOs.Product;

namespace ECommerce.Tests.Integration
{
    public class EndToEndWorkflowTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public EndToEndWorkflowTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CompleteUserJourney_RegisterLoginCreateProductUpdateDelete_ShouldSucceed()
        {
            // 1. Register a new user
            var registerDto = new RegisterDto
            {
                UserName = $"journey_{Guid.NewGuid()}",
                Email = $"journey_{Guid.NewGuid()}@example.com",
                Password = "Journey123!@#",
                ConfirmPassword = "Journey123!@#"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            authResult.Success.Should().BeTrue();

            // 2. Login with the credentials
            var loginDto = new LoginDto
            {
                UserName = registerDto.UserName,
                Password = registerDto.Password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

            var token = loginResult.AccessToken;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3. Get user profile
            var profileResponse = await _client.GetAsync("/api/users/profile");
            profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 4. Create a product
            var productCode = $"P{new Random().Next(100, 999)}";
            var createProductContent = new MultipartFormDataContent();
            createProductContent.Add(new StringContent("Electronics"), "Category");
            createProductContent.Add(new StringContent(productCode), "ProductCode");
            createProductContent.Add(new StringContent("Journey Product"), "Name");
            createProductContent.Add(new StringContent("299.99"), "Price");
            createProductContent.Add(new StringContent("1"), "MinimumQuantity");
            createProductContent.Add(new StringContent("15"), "DiscountRate");

            var createProductResponse = await _client.PostAsync("/api/products", createProductContent);
            createProductResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdProduct = await createProductResponse.Content.ReadFromJsonAsync<ProductDto>();

            // 5. Get the created product
            var getProductResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
            getProductResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 6. Update the product
            var updateProductContent = new MultipartFormDataContent();
            updateProductContent.Add(new StringContent("Updated Journey Product"), "Name");
            updateProductContent.Add(new StringContent("349.99"), "Price");

            var updateProductResponse = await _client.PutAsync($"/api/products/{createdProduct.Id}", updateProductContent);
            updateProductResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 7. Delete the product
            var deleteProductResponse = await _client.DeleteAsync($"/api/products/{createdProduct.Id}");
            deleteProductResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 8. Verify product is deleted
            var verifyDeleteResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
            verifyDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // 9. Refresh the token
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = loginResult.RefreshToken
            };

            var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshTokenDto);
            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            refreshResult.Success.Should().BeTrue();
            refreshResult.AccessToken.Should().NotBeNullOrEmpty();
        }
    }
}
