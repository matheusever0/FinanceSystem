using FinanceSystem.Domain.Entities;
using FinanceSystem.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace FinanceSystem.Infrastructure.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockJwtSection;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockJwtSection = new Mock<IConfigurationSection>();

            _mockConfiguration.Setup(c => c["JwtSettings:Secret"]).Returns("YourSuperSecretKeyWithAtLeast32Characters!");
            _mockConfiguration.Setup(c => c["JwtSettings:Issuer"]).Returns("FinanceSystem");
            _mockConfiguration.Setup(c => c["JwtSettings:Audience"]).Returns("FinanceSystemClient");
            _mockConfiguration.Setup(c => c["JwtSettings:ExpiryHours"]).Returns("1");

            _authService = new AuthService(_mockConfiguration.Object);
        }

        [Fact]
        public void HashPassword_ShouldReturnDifferentHashForSamePassword()
        {
            // Arrange
            var password = "password123";

            // Act
            var hash1 = _authService.HashPassword(password);
            var hash2 = _authService.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2); // BCrypt adds a random salt to each hash
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
        {
            // Arrange
            var password = "password123";
            var hash = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(password, hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
        {
            // Arrange
            var password = "password123";
            var hash = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword("wrongpassword", hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GenerateJwtToken_ShouldReturnValidToken()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "hashedpassword");
            var role = new Role("Admin", "Administrator role");
            user.AddRole(role);

            // Act
            var token = await _authService.GenerateJwtToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            // Verify token can be parsed
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check token claims
            Assert.Contains(jwtToken.Claims, c => c.Type == "unique_name" && c.Value == user.Username);
            Assert.Contains(jwtToken.Claims, c => c.Type == "email" && c.Value == user.Email);
            Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == "Admin");
        }
    }
}