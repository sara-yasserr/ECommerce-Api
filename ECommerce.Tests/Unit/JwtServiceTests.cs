using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.Services;
using ECommerce.DAL.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ECommerce.Tests.Unit
{
    public class JwtServiceTests
    {
        [Fact]
        public void GenerateAccessToken_ShouldReturnValidToken()
        {
            // Arrange
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Jwt:Key", "ThisIsAVerySecureSecretKeyWithAtLeast32Characters!"},
                    {"Jwt:Issuer", "TestIssuer"},
                    {"Jwt:Audience", "TestAudience"},
                    {"Jwt:AccessTokenExpirationMinutes", "30"}
                })
                .Build();

            var jwtService = new JwtService(configuration);
            var user = new User
            {
                UserId = 1,
                UserName = "testuser",
                Email = "test@example.com"
            };

            // Act
            var token = jwtService.GenerateAccessToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            token.Split('.').Should().HaveCount(3); // JWT has 3 parts
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnUniqueTokens()
        {
            // Arrange
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Jwt:Key", "ThisIsAVerySecureSecretKeyWithAtLeast32Characters!"}
                })
                .Build();

            var jwtService = new JwtService(configuration);

            // Act
            var token1 = jwtService.GenerateRefreshToken();
            var token2 = jwtService.GenerateRefreshToken();

            // Assert
            token1.Should().NotBeNullOrEmpty();
            token2.Should().NotBeNullOrEmpty();
            token1.Should().NotBe(token2);
        }
    }

}