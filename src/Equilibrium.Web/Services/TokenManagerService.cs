using Equilibrium.Web.Interfaces;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Equilibrium.Web.Services
{
    public class TokenManagerService : ITokenManagerService
    {
        private readonly ILogger<TokenManagerService> _logger;
        private readonly IUserService _userService;
        private static readonly ConcurrentDictionary<string, TokenInfo> _tokenCache = new();
        private readonly Timer _cleanupTimer;

        private class TokenInfo
        {
            public DateTime ExpiresAt { get; set; }
            public bool IsValid { get; set; }
            public DateTime LastValidated { get; set; }
        }

        public TokenManagerService(ILogger<TokenManagerService> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;

            _cleanupTimer = new Timer(async _ => await CleanupExpiredTokensAsync(),
                null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
        }

        public Task<bool> IsTokenValidAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return Task.FromResult(false);

            try
            {
                if (_tokenCache.TryGetValue(token, out var cachedInfo) && cachedInfo.LastValidated > DateTime.UtcNow.AddMinutes(-5))
                {
                    return Task.FromResult(cachedInfo.IsValid && cachedInfo.ExpiresAt > DateTime.UtcNow);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                {
                    CacheTokenInfo(token, DateTime.UtcNow, false);
                    return Task.FromResult(false);
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);
                var isValid = jwtToken.ValidTo > DateTime.UtcNow;

                CacheTokenInfo(token, jwtToken.ValidTo, isValid);

                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar token JWT");
                CacheTokenInfo(token, DateTime.UtcNow, false);
                return Task.FromResult(false);
            }
        }

        public Task<bool> IsTokenNearExpirationAsync(string token, TimeSpan threshold)
        {
            var expiration = GetTokenExpiration(token);
            if (expiration == null)
                return Task.FromResult(false);

            var timeUntilExpiration = expiration.Value - DateTime.UtcNow;
            return Task.FromResult(timeUntilExpiration <= threshold);
        }

        public async Task<string?> RefreshTokenAsync(string currentToken)
        {
            try
            {
                var claims = GetTokenClaims(currentToken);
                if (claims == null)
                    return null;

                var username = claims.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(username))
                    return null;

                _logger.LogWarning("Renovação de token não implementada para usuário: {Username}", username);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar renovar token");
                return null;
            }
        }

        public Task InvalidateTokenAsync(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _tokenCache.TryRemove(token, out _);
            }
            return Task.CompletedTask;
        }

        public Task CleanupExpiredTokensAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredTokens = _tokenCache
                    .Where(kvp => kvp.Value.ExpiresAt <= now ||
                                 kvp.Value.LastValidated <= now.AddHours(-1))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var expiredToken in expiredTokens)
                {
                    _tokenCache.TryRemove(expiredToken, out _);
                }

                if (expiredTokens.Count > 0)
                {
                    _logger.LogInformation("Removidos {Count} tokens expirados do cache", expiredTokens.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar cache de tokens");
            }

            return Task.CompletedTask;
        }

        public DateTime? GetTokenExpiration(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                    return null;

                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch
            {
                return null;
            }
        }

        public ClaimsPrincipal? GetTokenClaims(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                    return null;

                var jwtToken = tokenHandler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return null;
            }
        }

        private void CacheTokenInfo(string token, DateTime expiresAt, bool isValid)
        {
            var tokenInfo = new TokenInfo
            {
                ExpiresAt = expiresAt,
                IsValid = isValid,
                LastValidated = DateTime.UtcNow
            };

            _tokenCache.AddOrUpdate(token, tokenInfo, (key, oldValue) => tokenInfo);
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }
}