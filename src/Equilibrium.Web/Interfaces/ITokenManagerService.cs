using System.Security.Claims;

namespace Equilibrium.Web.Interfaces
{
    public interface ITokenManagerService
    {
        Task<bool> IsTokenValidAsync(string token);
        Task<bool> IsTokenNearExpirationAsync(string token, TimeSpan threshold);
        Task<string?> RefreshTokenAsync(string currentToken);
        Task InvalidateTokenAsync(string token);
        Task CleanupExpiredTokensAsync();
        DateTime? GetTokenExpiration(string token);
        ClaimsPrincipal? GetTokenClaims(string token);
    }
}