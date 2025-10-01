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
using ECommerce.BLL.DTOs.User;
using FluentAssertions;

namespace ECommerce.Tests.Integration
{
    public class UserIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public UserIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<(string token, int userId)> RegisterAndGetTokenAsync()
        {
            var registerDto = new RegisterDto
            {
                UserName = $"testuser_{Guid.NewGuid()}",
                Email = $"test_{Guid.NewGuid()}@example.com",
                Password = "Test123!@#",
                ConfirmPassword = "Test123!@#"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return (result.AccessToken, result.User.Id);
        }

        [Fact]
        public async Task GetProfile_WithAuth_ShouldReturnUserProfile()
        {
            // Arrange
            var (token, userId) = await RegisterAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/users/profile");
            var result = await response.Content.ReadFromJsonAsync<UserDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);

            // No other changes are needed as the FluentAssertions library provides the 'Should()' extension method.
        }

        [Fact]
        public async Task GetProfile_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/users/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateProfile_WithValidData_ShouldReturnUpdatedUser()
        {
            // Arrange
            var (token, userId) = await RegisterAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateDto = new UpdateUserDto
            {
                UserName = $"updateduser_{Guid.NewGuid()}",
                Email = $"updated_{Guid.NewGuid()}@example.com"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/users/profile", updateDto);
            var result = await response.Content.ReadFromJsonAsync<UserDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.UserName.Should().Be(updateDto.UserName);
            result.Email.Should().Be(updateDto.Email);
        }

        [Fact]
        public async Task ChangePassword_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var password = "Test123!@#";
            var registerDto = new RegisterDto
            {
                UserName = $"pwduser_{Guid.NewGuid()}",
                Email = $"pwd_{Guid.NewGuid()}@example.com",
                Password = password,
                ConfirmPassword = password
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = password,
                NewPassword = "NewPassword123!@#",
                ConfirmNewPassword = "NewPassword123!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/change-password", changePasswordDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ChangePassword_WithInvalidCurrentPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var (token, userId) = await RegisterAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "WrongPassword123",
                NewPassword = "NewPassword123!@#",
                ConfirmNewPassword = "NewPassword123!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/change-password", changePasswordDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUsers_WithAuth_ShouldReturnUsersList()
        {
            // Arrange
            var (token, userId) = await RegisterAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/users?page=1&pageSize=10");
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            users.Should().NotBeNull();
            users.Should().NotBeEmpty();
        }
    }
}
