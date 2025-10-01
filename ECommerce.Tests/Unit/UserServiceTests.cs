using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.DTOs.User;
using ECommerce.BLL.Interfaces;
using ECommerce.BLL.Services;
using ECommerce.DAL.Interfaces;
using ECommerce.DAL.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Tests.Unit
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _userService = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                UserName = "testuser",
                Email = "test@example.com"
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.UserName.Should().Be("testuser");
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidData_ShouldReturnUpdatedUser()
        {
            // Arrange
            var existingUser = new User
            {
                UserId = 1,
                UserName = "oldusername",
                Email = "old@example.com"
            };

            var updateDto = new UpdateUserDto
            {
                UserName = "newusername",
                Email = "new@example.com"
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(existingUser);
            _mockUserRepository.Setup(x => x.UsernameExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var result = await _userService.UpdateUserAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("newusername");
            result.Email.Should().Be("new@example.com");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidCurrentPassword_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Password = "hashedOldPassword"
            };

            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword("OldPassword123", "hashedOldPassword"))
                .Returns(true);
            _mockPasswordHasher.Setup(x => x.HashPassword("NewPassword123"))
                .Returns("hashedNewPassword");

            // Act
            var result = await _userService.ChangePasswordAsync(1, changePasswordDto);

            // Assert
            result.Should().BeTrue();
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ShouldThrowException()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Password = "hashedOldPassword"
            };

            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _userService.ChangePasswordAsync(1, changePasswordDto)
            );
        }
    }
}
