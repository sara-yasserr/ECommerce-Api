using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.DTOs.Auth;
using ECommerce.BLL.Interfaces;
using ECommerce.BLL.Services;
using ECommerce.DAL.Interfaces;
using ECommerce.DAL.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Tests.Unit
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _authService = new AuthService(_mockUserRepository.Object, _mockJwtService.Object, _mockPasswordHasher.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!"
            };

            _mockUserRepository.Setup(x => x.UsernameExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
            _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashedPassword");
            _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(new User { UserId = 1, UserName = "testuser", Email = "test@example.com" });
            _mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("accessToken");
            _mockJwtService.Setup(x => x.GenerateRefreshToken())
                .Returns("refreshToken");
            _mockJwtService.Setup(x => x.GetRefreshTokenExpiryTime())
                .Returns(DateTime.UtcNow.AddDays(7));

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ShouldReturnFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "existinguser",
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!"
            };

            _mockUserRepository.Setup(x => x.UsernameExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Username already exists");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                UserName = "testuser",
                Password = "Test123!"
            };

            var user = new User
            {
                UserId = 1,
                UserName = "testuser",
                Email = "test@example.com",
                Password = "hashedPassword"
            };

            _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            _mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("accessToken");
            _mockJwtService.Setup(x => x.GenerateRefreshToken())
                .Returns("refreshToken");
            _mockJwtService.Setup(x => x.GetRefreshTokenExpiryTime())
                .Returns(DateTime.UtcNow.AddDays(7));

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AccessToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailure()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                UserName = "testuser",
                Password = "WrongPassword"
            };

            var user = new User
            {
                UserId = 1,
                UserName = "testuser",
                Password = "hashedPassword"
            };

            _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid username or password");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto { RefreshToken = "validRefreshToken" };
            var user = new User
            {
                UserId = 1,
                UserName = "testuser",
                RefreshToken = "validRefreshToken",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };

            _mockUserRepository.Setup(x => x.GetByRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("newAccessToken");
            _mockJwtService.Setup(x => x.GenerateRefreshToken())
                .Returns("newRefreshToken");

            // Act
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AccessToken.Should().Be("newAccessToken");
        }
    }
}