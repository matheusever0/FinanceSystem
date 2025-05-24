using Equilibrium.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace Equilibrium.Web.Filters
{
    public class TokenCheckAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, DateTime> _validatedTokens = new();

        private static readonly Timer _cacheCleanupTimer = new Timer(CleanupExpiredTokens, null,
            TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            if (httpContext.IsUserAuthenticated())
            {
                var token = httpContext.GetJwtToken();

                if (string.IsNullOrEmpty(token))
                {
                    await SignOutAndRedirect(context);
                    return;
                }

                // Verificar se o token está válido
                if (!IsTokenValid(token))
                {
                    await SignOutAndRedirect(context);
                    return;
                }

                // Verificar se o cookie de autenticação ainda é válido
                if (!IsAuthenticationCookieValid(httpContext))
                {
                    await SignOutAndRedirect(context);
                    return;
                }

                // Check cache first - mas com validação mais rigorosa
                if (_validatedTokens.TryGetValue(token, out var cachedExpiryTime))
                {
                    // Verificar se o token ainda é válido no cache E se não expirou
                    if (cachedExpiryTime > DateTime.UtcNow)
                    {
                        base.OnActionExecuting(context);
                        return;
                    }
                    // Token expirado no cache, remover
                    _validatedTokens.TryRemove(token, out _);
                }

                // Validar e cachear apenas se passou em todas as verificações
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                _validatedTokens.TryAdd(token, jwtToken.ValidTo);

                base.OnActionExecuting(context);
                return;
            }

            base.OnActionExecuting(context);
        }

        private async Task SignOutAndRedirect(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            httpContext.Session.Clear();
            httpContext.Response.Cookies.Delete("JWToken");

            // Limpar token do cache se existir
            var token = httpContext.GetJwtToken();
            if (!string.IsNullOrEmpty(token))
            {
                _validatedTokens.TryRemove(token, out _);
            }

            context.Result = new RedirectToActionResult("Login", "Account", new { expired = true });
        }

        private static bool IsTokenValid(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                    return false;

                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Verificação mais rigorosa - sem margem de erro
                return jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsAuthenticationCookieValid(HttpContext httpContext)
        {
            try
            {
                var authResult = httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;

                if (!authResult.Succeeded || authResult.Properties?.ExpiresUtc == null)
                    return false;

                // Verificar se o cookie de autenticação não expirou
                return authResult.Properties.ExpiresUtc > DateTimeOffset.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private static void CleanupExpiredTokens(object state)
        {
            var now = DateTime.UtcNow;
            var expiredTokens = _validatedTokens
                .Where(kvp => kvp.Value <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var expiredToken in expiredTokens)
            {
                _validatedTokens.TryRemove(expiredToken, out _);
            }
        }
    }
}