namespace FinanceSystem.Web.Extensions
{
    public static class HttpContextExtensions
    {
        private const string JwtTokenKey = "JWToken";

                                                public static string GetJwtToken(this HttpContext httpContext)
        {
            return httpContext.Session.GetString(JwtTokenKey) ?? string.Empty;
        }

                                                public static void SetJwtToken(this HttpContext httpContext, string token)
        {
            httpContext.Session.SetString(JwtTokenKey, token);
        }

                                        public static void RemoveJwtToken(this HttpContext httpContext)
        {
            httpContext.Session.Remove(JwtTokenKey);
        }
    }
}