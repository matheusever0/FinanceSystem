using System.Security.Claims;

namespace Equilibrium.Web.Extensions
{
    public static class HttpContextUserExtensions
    {
        public static string GetUserName(this HttpContext context, string defaultValue = "guest")
        {
            return context?.User?.Identity?.Name ?? defaultValue;
        }

        public static bool IsUserAuthenticated(this HttpContext context)
        {
            return context?.User?.Identity?.IsAuthenticated ?? false;
        }

        public static bool UserIsInRole(this HttpContext context, string role)
        {
            return context?.User?.IsInRole(role) ?? false;
        }

        public static string? GetCurrentUserId(this HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userIdClaim;
        }
    }
}