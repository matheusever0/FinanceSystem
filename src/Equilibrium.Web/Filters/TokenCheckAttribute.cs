using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Equilibrium.Web.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Concurrent;

namespace Equilibrium.Web.Filters
{
    public class TokenCheckAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, DateTime> _validatedTokens = new();
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

                // Check cache first
                if (_validatedTokens.TryGetValue(token, out var expiryTime))
                {
                    if (expiryTime > DateTime.UtcNow)
                    {
                        // Token still valid according to cache
                        base.OnActionExecuting(context);
                        return;
                    }
                    // Expired in cache, remove it
                    _validatedTokens.TryRemove(token, out _);
                }

                // Validate and cache
                if (IsTokenValid(token))
                {
                    var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    _validatedTokens.TryAdd(token, jwtToken.ValidTo);
                    base.OnActionExecuting(context);
                    return;
                }

                await SignOutAndRedirect(context);
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

                return jwtToken.ValidTo > DateTime.UtcNow.AddMinutes(-5);
            }
            catch
            {
                return false;
            }
        }
    }
}
