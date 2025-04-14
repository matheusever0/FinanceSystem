using Microsoft.AspNetCore.Http;

namespace FinanceSystem.Web.Extensions
{
    public static class HttpContextUserExtensions
    {
        /// <summary>
        /// Gets the user's name safely from HttpContext, returning a default value if not available.
        /// </summary>
        /// <param name="context">The HttpContext instance.</param>
        /// <param name="defaultValue">Optional default value to return if username is not available.</param>
        /// <returns>The username or default value if not found.</returns>
        public static string GetUserName(this HttpContext context, string defaultValue = "guest")
        {
            return context?.User?.Identity?.Name ?? defaultValue;
        }

        /// <summary>
        /// Checks if the user from HttpContext is authenticated.
        /// </summary>
        /// <param name="context">The HttpContext instance.</param>
        /// <returns>True if the user is authenticated, false otherwise.</returns>
        public static bool IsUserAuthenticated(this HttpContext context)
        {
            return context?.User?.Identity?.IsAuthenticated ?? false;
        }

        /// <summary>
        /// Checks if the user from HttpContext is in a specific role.
        /// </summary>
        /// <param name="context">The HttpContext instance.</param>
        /// <param name="role">The role to check.</param>
        /// <returns>True if the user is in the role, false otherwise.</returns>
        public static bool UserIsInRole(this HttpContext context, string role)
        {
            return context?.User?.IsInRole(role) ?? false;
        }
    }
}