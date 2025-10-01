using ECommerce.BLL.DTOs.Auth;
using ECommerce.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("User registration attempt for username: {Username}", registerDto.UserName);

                var result = await _authService.RegisterAsync(registerDto);

                if (result.Success)
                {
                    _logger.LogInformation("User registered successfully: {Username}", registerDto.UserName);
                    return Ok(result);
                }

                _logger.LogWarning("Registration failed for username: {Username}. Reason: {Message}",
                    registerDto.UserName, result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for username: {Username}", registerDto.UserName);
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for username: {Username}", loginDto.UserName);

                var result = await _authService.LoginAsync(loginDto);

                if (result.Success)
                {
                    _logger.LogInformation("User logged in successfully: {Username}", loginDto.UserName);
                    return Ok(result);
                }

                _logger.LogWarning("Login failed for username: {Username}", loginDto.UserName);
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", loginDto.UserName);
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                var result = await _authService.RefreshTokenAsync(refreshTokenDto);

                if (result.Success)
                {
                    _logger.LogInformation("Token refreshed successfully");
                    return Ok(result);
                }

                _logger.LogWarning("Token refresh failed");
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                _logger.LogInformation("Token revocation attempt");

                var result = await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken);

                if (result)
                {
                    _logger.LogInformation("Token revoked successfully");
                    return Ok(new { message = "Token revoked successfully" });
                }

                _logger.LogWarning("Token revocation failed");
                return BadRequest(new { message = "Invalid token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            return await RevokeToken(refreshTokenDto);
        }
    }
}
