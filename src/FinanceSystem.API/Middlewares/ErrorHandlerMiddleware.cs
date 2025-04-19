// API/Middlewares/ErrorHandlerMiddleware.cs
using System.Net;
using System.Text.Json;
using FinanceSystem.API.Configuration;
using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Common;
using Microsoft.Extensions.Localization;

namespace FinanceSystem.API.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlerMiddleware> logger,
            IStringLocalizer<SharedResource> localizer)
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

                // Determinar o status code e a chave da mensagem com base no tipo de exceção
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

                // Criar resposta de API padronizada
                var apiResponse = ApiResponse<object>.ErrorResult(messageKey, statusCode);

                // Obter a mensagem localizada
                apiResponse.Message = GetLocalizedErrorMessage(error);

                // Serializar e retornar a resposta
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
            // Verificar o tipo de exceção e tentar obter uma mensagem mais específica
            if (ex is KeyNotFoundException)
            {
                // Tentar extrair a entidade e o ID
                var (entity, id) = TryExtractEntityInfo(ex.Message);
                if (!string.IsNullOrEmpty(entity) && !string.IsNullOrEmpty(id))
                {
                    // Tentar obter uma mensagem específica para a entidade
                    string specificKey = $"{char.ToUpper(entity[0])}{entity.Substring(1)}.NotFound";

                    // Se o valor localizado não for igual à chave, então a localização foi bem-sucedida
                    var localizedValue = _localizer[specificKey, id].Value;
                    if (localizedValue != specificKey)
                    {
                        return localizedValue;
                    }

                    // Caso contrário, usamos a mensagem genérica
                    return _localizer[LogMessageConstants.ErrorNotFound, entity, id];
                }

                // Se não conseguimos extrair a entidade e o ID, devolvemos a mensagem original
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

            // Para outras exceções, retornamos uma mensagem genérica
            return _localizer[LogMessageConstants.ErrorGeneric];
        }

        /// <summary>
        /// Tenta extrair informações de entidade da mensagem de erro
        /// </summary>
        private static (string entity, string id) TryExtractEntityInfo(string message)
        {
            // Formato esperado: "{Entity} with ID {id} not found"
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