using FinanceSystem.API.Configuration;
using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Common;
using FinanceSystem.Resources;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.Json;

namespace FinanceSystem.API.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IStringLocalizer<ResourceFinanceApi> _localizer;

        public ErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlerMiddleware> logger,
            IStringLocalizer<ResourceFinanceApi> localizer)
        {
            _next = next;
            _logger = logger;
            _localizer = localizer;
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
                string messageKey = LogMessageConstants.ErrorGeneric;
                int statusCode = (int)HttpStatusCode.InternalServerError;

                switch (error)
                {
                    case KeyNotFoundException ex:
                        statusCode = (int)HttpStatusCode.NotFound;
                        messageKey = LogMessageConstants.ErrorNotFound;
                        _logger.LogWarningOperation(messageKey, _localizer, ex.Message);
                        break;

                    case InvalidOperationException ex:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        messageKey = LogMessageConstants.ErrorInvalidOperation;
                        _logger.LogWarningOperation(messageKey, _localizer, ex.Message);
                        break;

                    case UnauthorizedAccessException ex:
                        statusCode = (int)HttpStatusCode.Unauthorized;
                        messageKey = LogMessageConstants.ErrorUnauthorized;
                        _logger.LogWarningOperation(messageKey, _localizer, ex.Message);
                        break;

                    default:
                        statusCode = (int)HttpStatusCode.InternalServerError;
                        _logger.LogErrorOperation(error, messageKey, _localizer);
                        break;
                }

                var apiResponse = ApiResponse<object>.ErrorResult(messageKey, statusCode);

                apiResponse.Message = GetLocalizedErrorMessage(error);

                var result = JsonSerializer.Serialize(apiResponse);
                response.StatusCode = statusCode;
                await response.WriteAsync(result);
            }
        }

        /// <summary>
        /// Obtém a mensagem de erro localizada com base na exceção
        /// </summary>
        private string GetLocalizedErrorMessage(Exception ex)
        {
            if (ex is KeyNotFoundException)
            {
                var (entity, id) = TryExtractEntityInfo(ex.Message);
                if (!string.IsNullOrEmpty(entity) && !string.IsNullOrEmpty(id))
                {
                    string specificKey = $"{char.ToUpper(entity[0])}{entity.Substring(1)}.NotFound";

                    var localizedValue = _localizer[specificKey, id].Value;
                    if (localizedValue != specificKey)
                    {
                        return localizedValue;
                    }

                    return _localizer[LogMessageConstants.ErrorNotFound, entity, id];
                }

                return ex.Message;
            }
            else if (ex is InvalidOperationException)
            {
                return _localizer[LogMessageConstants.ErrorInvalidOperation, ex.Message];
            }
            else if (ex is UnauthorizedAccessException)
            {
                return _localizer[LogMessageConstants.ErrorUnauthorized, ex.Message];
            }

            return _localizer[LogMessageConstants.ErrorGeneric];
        }

        /// <summary>
        /// Tenta extrair informações de entidade da mensagem de erro
        /// </summary>
        private static (string entity, string id) TryExtractEntityInfo(string message)
        {
            if (message.Contains(" with ID ") && message.EndsWith(" not found"))
            {
                string[] parts = message.Split(new[] { " with ID " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string entity = parts[0].ToLower();
                    string id = parts[1].Replace(" not found", "").Trim();
                    return (entity, id);
                }
            }

            return (string.Empty, string.Empty);
        }
    }
}