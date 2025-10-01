using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.DTOs.User;
using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Interfaces;
using ECommerce.DAL.Models;

namespace ECommerce.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10)
        {
            var users = await _userRepository.GetAllAsync(page, pageSize);
            return users.Select(MapToUserDto);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            // Check if new username already exists (excluding current user)
            if (!string.IsNullOrEmpty(updateUserDto.UserName) &&
                updateUserDto.UserName != user.UserName &&
                await _userRepository.UsernameExistsAsync(updateUserDto.UserName, id))
            {
                throw new InvalidOperationException("Username already exists");
            }

            // Check if new email already exists (excluding current user)
            if (!string.IsNullOrEmpty(updateUserDto.Email) &&
                updateUserDto.Email != user.Email &&
                await _userRepository.EmailExistsAsync(updateUserDto.Email, id))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Update user properties
            if (!string.IsNullOrEmpty(updateUserDto.UserName))
                user.UserName = updateUserDto.UserName;

            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.Email = updateUserDto.Email;

            user.ModifiedDate = DateTime.Now;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToUserDto(updatedUser);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // Verify current password
            if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.Password))
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            // Hash new password
            user.Password = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
            user.ModifiedDate = DateTime.Now;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                LastLoginTime = user.LastLoginTime,
                CreatedAt = user.CreatedDate,
                UpdatedAt = user.ModifiedDate
            };
        }
    }
}