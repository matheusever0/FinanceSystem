namespace Equilibrium.Web.Extensions
{
    public static class HttpContextExtensions
    {
        private const string JwtTokenKey = "JWToken";

        public static void SetJwtTokenCookie(this HttpContext httpContext, string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Importante: httpOnly para segurança
                Secure = true, // HTTPS obrigatório em produção
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.AddDays(1), // 24 horas - igual ao token JWT
                Path = "/",
                IsEssential = true
            };

            // Definir cookie httpOnly
            httpContext.Response.Cookies.Append(JwtTokenKey, token, cookieOptions);

            // IMPORTANTE: NÃO armazenar em session pois é menos seguro
            // O token será recuperado via cookie httpOnly
        }

        public static string GetJwtToken(this HttpContext httpContext)
        {
            // Tentar buscar o token do cookie httpOnly primeiro
            if (httpContext.Request.Cookies.TryGetValue(JwtTokenKey, out var cookieToken) &&
                !string.IsNullOrEmpty(cookieToken))
            {
                return cookieToken;
            }

            // Fallback: tentar buscar da session (para compatibilidade)
            var sessionToken = httpContext.Session.GetString(JwtTokenKey);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                return sessionToken;
            }

            return string.Empty;
        }

        public static void SetJwtToken(this HttpContext httpContext, string token)
        {
            // Método mantido para compatibilidade, mas prefira usar SetJwtTokenCookie
            httpContext.Session.SetString(JwtTokenKey, token);
        }

        public static void ClearJwtToken(this HttpContext httpContext)
        {
            // Limpar cookie
            httpContext.Response.Cookies.Delete(JwtTokenKey, new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            // Limpar session
            httpContext.Session.Remove(JwtTokenKey);
        }
    }
}