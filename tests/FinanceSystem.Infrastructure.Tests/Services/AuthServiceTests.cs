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
            var password = "password123";

            var hash1 = _authService.HashPassword(password);
            var hash2 = _authService.HashPassword(password);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
        {
            var password = "password123";
            var hash = _authService.HashPassword(password);

            var result = _authService.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
        {
            var password = "password123";
            var hash = _authService.HashPassword(password);

            var result = _authService.VerifyPassword("wrongpassword", hash);

            Assert.False(result);
        }

        [Fact]
        public async Task GenerateJwtToken_ShouldReturnValidToken()
        {
            var user = new User("testuser", "test@example.com", "hashedpassword");
            var role = new Role("Admin", "Administrator role");
            user.AddRole(role);

            var token = await _authService.GenerateJwtToken(user);

            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.Contains(jwtToken.Claims, c => c.Type == "unique_name" && c.Value == user.Username);
            Assert.Contains(jwtToken.Claims, c => c.Type == "email" && c.Value == user.Email);
            Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == "Admin");
        }
    }
}