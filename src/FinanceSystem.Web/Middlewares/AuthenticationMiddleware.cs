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
            // Capturar a resposta para poder verificar o status code
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Verificar se a resposta é 401 Unauthorized
                if (context.Response.StatusCode == 401)
                {
                    _logger.LogWarning("Resposta 401 detectada. Redirecionando para login.");

                    // Se for AJAX, retorne 401 para o cliente lidar com isso
                    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                        context.Request.Headers["Accept"].ToString().Contains("application/json"))
                    {
                        // Manter o 401 para requisições AJAX
                        await CopyResponseToOriginalStream(responseBody, originalBodyStream);
                    }
                    else
                    {
                        // Para requisições normais, redirecionar para login
                        await HandleUnauthorizedResponse(context, originalBodyStream);
                    }
                    return;
                }

                // Resposta normal - copiar para o stream original
                await CopyResponseToOriginalStream(responseBody, originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task HandleUnauthorizedResponse(HttpContext context, Stream originalBodyStream)
        {
            // Limpar autenticação
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Session.Clear();

            // Substituir a resposta por um redirecionamento
            context.Response.Clear();
            context.Response.StatusCode = 302; // Found/Redirect
            context.Response.Headers["Location"] = "/Account/Login?expired=true";

            // Escrever o corpo da resposta minimal
            byte[] responseBytes = Encoding.UTF8.GetBytes("Redirecting to login page...");
            await originalBodyStream.WriteAsync(responseBytes);
        }

        private async Task CopyResponseToOriginalStream(MemoryStream responseBody, Stream originalBodyStream)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    // Extensão para adicionar o middleware na pipeline
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}