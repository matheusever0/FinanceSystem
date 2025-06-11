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
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.AddDays(1),
                Path = "/",
                IsEssential = true
            };

            httpContext.Response.Cookies.Append(JwtTokenKey, token, cookieOptions);
            httpContext.Session.SetString(JwtTokenKey, token);
        }

        public static string GetJwtToken(this HttpContext httpContext)
        {
            var sessionToken = httpContext.Session.GetString(JwtTokenKey);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                return sessionToken;
            }

            if (httpContext.Request.Cookies.TryGetValue(JwtTokenKey, out var cookieToken) &&
                !string.IsNullOrEmpty(cookieToken))
            {
                return cookieToken;
            }

            return string.Empty;
        }

        public static void ClearJwtToken(this HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete(JwtTokenKey, new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            httpContext.Session.Remove(JwtTokenKey);
        }
    }
}