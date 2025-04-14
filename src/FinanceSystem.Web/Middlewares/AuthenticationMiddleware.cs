using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;

namespace FinanceSystem.Web.Middlewares
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

        private async Task HandleUnauthorizedResponse(HttpContext context, Stream originalBodyStream)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Session.Clear();

            context.Response.Clear();
            context.Response.StatusCode = 302; context.Response.Headers["Location"] = "/Account/Login?expired=true";

            byte[] responseBytes = Encoding.UTF8.GetBytes("Redirecting to login page...");
            await originalBodyStream.WriteAsync(responseBytes);
        }

        private async Task CopyResponseToOriginalStream(MemoryStream responseBody, Stream originalBodyStream)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}