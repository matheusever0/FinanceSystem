using FinanceSystem.Resources;
using System.Security.Claims;

namespace FinanceSystem.API.Extensions
{
    public static class HttpContextExtensions
    {
        public static Guid GetCurrentUserId(this HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException(ResourceFinanceApi.Auth_UserNotAuthenticated);
            }
            return Guid.Parse(userIdClaim);
        }
    }
}
