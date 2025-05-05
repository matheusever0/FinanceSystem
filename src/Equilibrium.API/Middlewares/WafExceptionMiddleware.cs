using System.Net;
using System.Text.Json;

namespace Equilibrium.API.Middlewares
{
    public class WafExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WafExceptionMiddleware> _logger;

        public WafExceptionMiddleware(
            RequestDelegate next,
            ILogger<WafExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (IsWafException(context, ex))
            {
                _logger.LogWarning("WAF bloqueou requisição: {Message}", ex.Message);
                await HandleWafExceptionAsync(context);
            }
        }

        private bool IsWafException(HttpContext context, Exception exception)
        {
            // Verifique se o status code é 403 ou se a exceção contém informações relacionadas ao WAF
            return context.Response.StatusCode == (int)HttpStatusCode.Forbidden ||
                   exception.Message.Contains("WAF") ||
                   exception.Message.Contains("Forbidden");
        }

        private async Task HandleWafExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            var response = new
            {
                success = false,
                message = "A requisição foi bloqueada pelo WAF. Por favor, tente novamente ou entre em contato com o administrador.",
                statusCode = context.Response.StatusCode
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}