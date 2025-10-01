using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.DTOs.Auth;
using ECommerce.BLL.DTOs.User;
using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Interfaces;
using ECommerce.DAL.Models;

namespace ECommerce.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(registerDto.UserName))
            {
                return new AuthResponseDto { Success = false, Message = "Username already exists" };
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                return new AuthResponseDto { Success = false, Message = "Email already exists" };
            }

            // Hash the password
            var hashedPassword = _passwordHasher.HashPassword(registerDto.Password);

            // Create new user
            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Password = hashedPassword,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(createdUser);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiryTime = _jwtService.GetRefreshTokenExpiryTime();

            // Update user with refresh token
            await _userRepository.UpdateRefreshTokenAsync(createdUser.UserId, refreshToken, refreshTokenExpiryTime);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiry = refreshTokenExpiryTime,
                User = MapToUserDto(createdUser)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.UserName);

            if (user == null || !_passwordHasher.VerifyPassword(loginDto.Password, user.Password))
            {
                return new AuthResponseDto { Success = false, Message = "Invalid username or password" };
            }

            // Update last login time
            await _userRepository.UpdateLastLoginAsync(user.UserId);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiryTime = _jwtService.GetRefreshTokenExpiryTime();

            // Update user with refresh token
            await _userRepository.UpdateRefreshTokenAsync(user.UserId, refreshToken, refreshTokenExpiryTime);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiry = refreshTokenExpiryTime,
                User = MapToUserDto(user)
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshTokenDto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return new AuthResponseDto { Success = false, Message = "Invalid or expired refresh token" };
            }

            // Generate new tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiryTime = _jwtService.GetRefreshTokenExpiryTime();

            // Update user with new refresh token
            await _userRepository.UpdateRefreshTokenAsync(user.UserId, refreshToken, refreshTokenExpiryTime);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiry = refreshTokenExpiryTime,
                User = MapToUserDto(user)
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if (user != null)
            {
                await _userRepository.RevokeRefreshTokenAsync(user.UserId);
                return true;
            }
            return false;
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
