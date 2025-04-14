namespace FinanceSystem.Web.Extensions
{
    public static class HttpContextExtensions
    {
        private const string JwtTokenKey = "JWToken";

        /// <summary>
        /// Gets the JWT token from the session.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>The JWT token or an empty string if not found.</returns>
        public static string GetJwtToken(this HttpContext httpContext)
        {
            return httpContext.Session.GetString(JwtTokenKey) ?? string.Empty;
        }

        /// <summary>
        /// Sets the JWT token in the session.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="token">The JWT token to store.</param>
        public static void SetJwtToken(this HttpContext httpContext, string token)
        {
            httpContext.Session.SetString(JwtTokenKey, token);
        }

        /// <summary>
        /// Removes the JWT token from the session.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public static void RemoveJwtToken(this HttpContext httpContext)
        {
            httpContext.Session.Remove(JwtTokenKey);
        }
    }
}