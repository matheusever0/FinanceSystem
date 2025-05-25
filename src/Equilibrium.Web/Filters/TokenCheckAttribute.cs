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

            // Pular verificação para endpoints que não precisam de autenticação
            if (ShouldSkipTokenCheck(httpContext))
            {
                base.OnActionExecuting(context);
                return;
            }

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
                if (!await IsAuthenticationCookieValidAsync(httpContext))
                {
                    await SignOutAndRedirect(context);
                    return;
                }

                // Verificar cache (com validação mais rigorosa)
                if (_validatedTokens.TryGetValue(token, out var cachedExpiryTime))
                {
                    if (cachedExpiryTime > DateTime.UtcNow)
                    {
                        base.OnActionExecuting(context);
                        return;
                    }
                    // Token expirado no cache, remover
                    _validatedTokens.TryRemove(token, out _);
                }

                // Validar e cachear se passou em todas as verificações
                try
                {
                    var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    _validatedTokens.TryAdd(token, jwtToken.ValidTo);
                }
                catch
                {
                    await SignOutAndRedirect(context);
                    return;
                }

                base.OnActionExecuting(context);
                return;
            }

            base.OnActionExecuting(context);
        }

        private static bool ShouldSkipTokenCheck(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value?.ToLower() ?? "";

            // Endpoints que não precisam de verificação de token
            var skipPaths = new[]
            {
                "/account/login",
                "/account/logout",
                "/account/accessdenied",
                "/account/verifytoken",
                "/css/",
                "/js/",
                "/images/",
                "/lib/",
                "/favicon.ico"
            };

            return skipPaths.Any(skipPath => path.Contains(skipPath));
        }

        private async Task SignOutAndRedirect(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var logger = httpContext.RequestServices.GetService<ILogger<TokenCheckAttribute>>();

            try
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                httpContext.Session.Clear();
                httpContext.ClearJwtToken();

                logger?.LogInformation("Usuário deslogado devido a token/cookie inválido");

                // Limpar token do cache se existir
                var token = httpContext.GetJwtToken();
                if (!string.IsNullOrEmpty(token))
                {
                    _validatedTokens.TryRemove(token, out _);
                }

                context.Result = new RedirectToActionResult("Login", "Account", new { expired = true });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Erro ao fazer logout automático");
                context.Result = new RedirectToActionResult("Login", "Account", new { expired = true });
            }
        }

        private static bool IsTokenValid(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return false;

                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                    return false;

                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Verificar se o token ainda não expirou
                return jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> IsAuthenticationCookieValidAsync(HttpContext httpContext)
        {
            try
            {
                var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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
            try
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

                if (expiredTokens.Count > 0)
                {
                    Console.WriteLine($"Limpeza de cache: {expiredTokens.Count} tokens expirados removidos");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na limpeza de cache de tokens: {ex.Message}");
            }
        }
    }
}