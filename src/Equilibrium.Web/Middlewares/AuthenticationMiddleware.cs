using Equilibrium.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Equilibrium.Web.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await CheckAndRefreshAuthentication(context);

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                if (context.Response.StatusCode == 401)
                {
                    _logger.LogWarning("Resposta 401 detectada. Redirecionando para login.");

                    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                        context.Request.Headers["Accept"].ToString().Contains("application/json"))
                    {
                        await CopyResponseToOriginalStream(responseBody, originalBodyStream);
                    }
                    else
                    {
                        await HandleUnauthorizedResponse(context, originalBodyStream);
                    }
                    return;
                }

                await CopyResponseToOriginalStream(responseBody, originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task CheckAndRefreshAuthentication(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Account/Login") ||
                context.Request.Path.StartsWithSegments("/Account/AccessDenied") ||
                context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/images"))
            {
                return;
            }

            if (!context.IsUserAuthenticated())
                return;

            var token = context.GetJwtToken();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token JWT não encontrado para usuário autenticado");
                await HandleExpiredAuthentication(context);
                return;
            }

            if (!IsJwtTokenValid(token))
            {
                _logger.LogWarning("Token JWT expirado para usuário: {User}", context.GetUserName());
                await HandleExpiredAuthentication(context);
                return;
            }

            var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authResult.Succeeded || authResult.Properties?.ExpiresUtc == null ||
                authResult.Properties.ExpiresUtc < DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("Cookie de autenticação expirado para usuário: {User}", context.GetUserName());
                await HandleExpiredAuthentication(context);
                return;
            }

            var hoursUntilExpiry = (authResult.Properties.ExpiresUtc.Value - DateTimeOffset.UtcNow).TotalHours;
            if (hoursUntilExpiry <= 2)
            {
                _logger.LogInformation("Renovando autenticação para usuário: {User}", context.GetUserName());
                await RefreshAuthentication(context, authResult);
            }
        }

        private static bool IsJwtTokenValid(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                    return false;

                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private async Task RefreshAuthentication(HttpContext context, AuthenticateResult authResult)
        {
            try
            {
                var newProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                };

                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal,
                    newProperties);

                var token = context.GetJwtToken();
                if (!string.IsNullOrEmpty(token))
                {
                    context.SetJwtTokenCookie(token);
                }

                _logger.LogInformation("Autenticação renovada com sucesso para usuário: {User}", context.GetUserName());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renovar autenticação para usuário: {User}", context.GetUserName());
                await HandleExpiredAuthentication(context);
            }
        }

        private static async Task HandleExpiredAuthentication(HttpContext context)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Session.Clear();
            context.Response.Cookies.Delete("JWToken");

            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                context.Response.StatusCode = 401;
                return;
            }

            context.Response.Redirect("/Account/Login?expired=true");
        }

        private static async Task HandleUnauthorizedResponse(HttpContext context, Stream originalBodyStream)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Session.Clear();
            context.Response.Cookies.Delete("JWToken");

            context.Response.Clear();
            context.Response.StatusCode = 302;
            context.Response.Headers["Location"] = "/Account/Login?expired=true";

            byte[] responseBytes = Encoding.UTF8.GetBytes("Redirecting to login page...");
            await originalBodyStream.WriteAsync(responseBytes);
        }

        private static async Task CopyResponseToOriginalStream(MemoryStream responseBody, Stream originalBodyStream)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}