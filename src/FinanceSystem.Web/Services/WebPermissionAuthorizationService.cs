using FinanceSystem.Web.Interfaces;
using System.Security.Claims;

namespace FinanceSystem.Web.Services
{
    public class WebPermissionAuthorizationService : IWebPermissionAuthorizationService
    {
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebPermissionAuthorizationService(
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionSystemName)
        {
            if (!user.Identity.IsAuthenticated)
                return false;

            if (user.IsInRole("Admin"))
                return true;

            var token = _httpContextAccessor.HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return false;

                var permissions = await _permissionService.GetPermissionsByUserIdAsync(userIdClaim.Value, token);
                return permissions.Any(p => p.SystemName == permissionSystemName);
            }
            catch
            {
                return false;
            }
        }
    }
}