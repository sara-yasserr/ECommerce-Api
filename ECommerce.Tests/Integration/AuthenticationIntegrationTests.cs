using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ECommerce.BLL.DTOs.Auth;
using FluentAssertions;

namespace ECommerce.Tests.Integration
{
    public class AuthenticationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AuthenticationIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = $"testuser_{Guid.NewGuid()}",
                Email = $"test_{Guid.NewGuid()}@example.com",
                Password = "Test123!@#",
                ConfirmPassword = "Test123!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_ShouldReturnBadRequest()
        {
            // Arrange
            var username = $"duplicate_{Guid.NewGuid()}";
            var registerDto1 = new RegisterDto
            {
                UserName = username,
                Email = $"test1_{Guid.NewGuid()}@example.com",
                Password = "Test123!@#",
                ConfirmPassword = "Test123!@#"
            };

            var registerDto2 = new RegisterDto
            {
                UserName = username,
                Email = $"test2_{Guid.NewGuid()}@example.com",
                Password = "Test123!@#",
                ConfirmPassword = "Test123!@#"
            };

            // Act
            await _client.PostAsJsonAsync("/api/auth/register", registerDto1);
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto2);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange - First register a user
            var username = $"loginuser_{Guid.NewGuid()}";
            var password = "Test123!@#";
            var registerDto = new RegisterDto
            {
                UserName = username,
                Email = $"login_{Guid.NewGuid()}@example.com",
                Password = password,
                ConfirmPassword = password
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto
            {
                UserName = username,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AccessToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                UserName = "nonexistentuser",
                Password = "WrongPassword123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
        {
            // Arrange - Register and get tokens
            var username = $"refreshuser_{Guid.NewGuid()}";
            var registerDto = new RegisterDto
            {
                UserName = username,
                Email = $"refresh_{Guid.NewGuid()}@example.com",
                Password = "Test123!@#",
                ConfirmPassword = "Test123!@#"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = authResult.RefreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshTokenDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }
    }
}