using FinanceSystem.Domain.Interfaces.Services;
using System.Security.Claims;

namespace FinanceSystem.Infrastructure.Services
{
    public class PermissionAuthorizationService : IPermissionAuthorizationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionAuthorizationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionSystemName)
        {
            if (!user.Identity.IsAuthenticated)
                return false;

            if (user.IsInRole("Admin"))
                return true;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return false;

            var userRoles = user.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();
            if (userRoles == null || !userRoles.Any())
                return false;

            var roles = await _unitOfWork.Roles.FindAsync(r => userRoles.Contains(r.Name));
            if (roles == null || !roles.Any())
                return false;

            foreach (var role in roles)
            {
                var roleWithPermissions = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(role!.Id);
                if (roleWithPermissions != null && roleWithPermissions.HasPermission(permissionSystemName))
                    return true;
            }

            return false;
        }
    }
}