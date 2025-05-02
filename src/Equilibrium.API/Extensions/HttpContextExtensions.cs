using Equilibrium.Resources;
using System.Security.Claims;

namespace Equilibrium.API.Extensions
{
    public static class HttpContextExtensions
    {
        public static Guid GetCurrentUserId(this HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim == null
                ? throw new UnauthorizedAccessException(ResourceFinanceApi.Auth_UserNotAuthenticated)
                : Guid.Parse(userIdClaim);
        }
    }
}
