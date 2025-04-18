using Microsoft.Extensions.Localization;

namespace FinanceSystem.API.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogOperation<T>(this ILogger<T> logger,
            string operationKey,
            IStringLocalizer localizer,
            params object[] args)
        {
            var message = localizer[operationKey];
            logger.LogInformation(message, args);
        }

        public static void LogWarningOperation<T>(this ILogger<T> logger,
            string operationKey,
            IStringLocalizer localizer,
            params object[] args)
        {
            var message = localizer[operationKey];
            logger.LogWarning(message, args);
        }

        public static void LogErrorOperation<T>(this ILogger<T> logger,
            Exception ex,
            string operationKey,
            IStringLocalizer localizer,
            params object[] args)
        {
            var message = localizer[operationKey];
            logger.LogError(ex, message, args);
        }
    }
}
