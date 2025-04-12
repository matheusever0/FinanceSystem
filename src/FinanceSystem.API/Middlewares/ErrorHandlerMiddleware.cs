using System.Net;
using System.Text.Json;

namespace FinanceSystem.API.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                switch (error)
                {
                    case KeyNotFoundException:
                        // Item não encontrado
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case InvalidOperationException:
                        // Regra de negócio violada
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case UnauthorizedAccessException:
                        // Acesso não autorizado
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    default:
                        // Erro não tratado
                        _logger.LogError(error, error.Message);
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonSerializer.Serialize(new { message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}