using FinanceSystem.Application.DTOs.Common;
using FinanceSystem.Resources;
using System.Net;
using System.Text.Json;

namespace FinanceSystem.API.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlerMiddleware> logger)
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
                string message = error.Message; 
                int statusCode;

                switch (error)
                {
                    case KeyNotFoundException:
                        statusCode = (int)HttpStatusCode.NotFound;
                        _logger.LogWarning("Entidade não encontrada: {Message}", error.Message);
                        break;

                    case InvalidOperationException:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        _logger.LogWarning("Operação inválida: {Message}", error.Message);
                        break;

                    case UnauthorizedAccessException:
                        statusCode = (int)HttpStatusCode.Unauthorized;
                        _logger.LogWarning("Acesso não autorizado: {Message}", error.Message);
                        break;

                    default:
                        statusCode = (int)HttpStatusCode.InternalServerError;
                        message = ResourceFinanceApi.Error_Generic; 
                        _logger.LogError(error, "Erro inesperado: {Message}", error.Message);
                        break;
                }

                var apiResponse = ApiResponse<object>.ErrorResult(message, statusCode);
                apiResponse.Message = message;

                var result = JsonSerializer.Serialize(apiResponse);
                response.StatusCode = statusCode;
                await response.WriteAsync(result);
            }
        }
    }
}