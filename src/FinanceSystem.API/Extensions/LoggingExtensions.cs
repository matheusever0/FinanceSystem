namespace FinanceSystem.API.Extensions
{
    public static class LoggingExtensions
    {
        public static void LogPaymentOperation(this ILogger logger, string operation, Guid paymentId)
        {
            logger.LogInformation("Payment operation: {Operation}, ID: {PaymentId}", operation, paymentId);
        }

        public static void LogIncomeOperation(this ILogger logger, string operation, Guid incomeId)
        {
            logger.LogInformation("Income operation: {Operation}, ID: {IncomeId}", operation, incomeId);
        }

        public static void LogUserOperation(this ILogger logger, string operation, Guid userId)
        {
            logger.LogInformation("User operation: {Operation}, ID: {UserId}", operation, userId);
        }

        public static void LogAuthOperation(this ILogger logger, string operation, string username)
        {
            logger.LogInformation("Auth operation: {Operation}, Username: {Username}", operation, username);
        }
    }
}
