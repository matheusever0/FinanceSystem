namespace Equilibrium.Web.Extensions
{
    public static class HttpContextExtensions
    {
        private const string JwtTokenKey = "JWToken";

        public static void SetJwtTokenCookie(this HttpContext httpContext, string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // For HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.AddDays(1)
            };

            httpContext.Response.Cookies.Append("JWToken", token, cookieOptions);
            // Keep session for quick access
            httpContext.Session.SetString("JWToken", token);
        }

        public static string GetJwtToken(this HttpContext httpContext)
        {
            // Try session first (faster)
            var sessionToken = httpContext.Session.GetString(JwtTokenKey);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                return sessionToken;
            }

            // Fall back to cookie
            if (httpContext.Request.Cookies.TryGetValue("JWToken", out var cookieToken))
            {
                // Restore session for future requests
                httpContext.Session.SetString(JwtTokenKey, cookieToken);
                return cookieToken;
            }

            return string.Empty;
        }

        public static void SetJwtToken(this HttpContext httpContext, string token)
        {
            httpContext.Session.SetString(JwtTokenKey, token);
        }
    }
}