﻿namespace FinanceSystem.API.Configuration
{
    public static class LogMessageConstants
    {
        public const string AuthLogin = "Auth.LoginAttempt";
        public const string AuthLoginSuccess = "Auth.LoginSuccess";
        public const string AuthLoginFailed = "Auth.LoginFailed";
        public const string AuthTokenValid = "Auth.TokenValid";
        public const string AuthTokenInvalid = "Auth.TokenInvalid";
        public const string AuthTokenVerify = "Auth.TokenVerify";
        public const string AuthTokenVerifyError = "Auth.TokenVerifyError";
        public const string AuthGetPermissions = "Auth.GetPermissions";

        public const string PaymentGet = "Payment.Get";
        public const string PaymentGetAll = "Payment.GetAll";
        public const string PaymentCreate = "Payment.Create";
        public const string PaymentUpdate = "Payment.Update";
        public const string PaymentDelete = "Payment.Delete";
        public const string PaymentMarkAsPaid = "Payment.MarkAsPaid";

        public const string IncomeGet = "Income.Get";
        public const string IncomeGetAll = "Income.GetAll";
        public const string IncomeCreate = "Income.Create";
        public const string IncomeUpdate = "Income.Update";
        public const string IncomeDelete = "Income.Delete";
        public const string IncomeMarkAsReceived = "Income.MarkAsReceived";

        public const string ErrorNotFound = "Error.NotFound";
        public const string ErrorUnauthorized = "Error.Unauthorized";
        public const string ErrorInvalidOperation = "Error.InvalidOperation";
        public const string ErrorGeneric = "Error.Generic";
    }
}
