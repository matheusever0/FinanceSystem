using System.Security.Claims;

namespace Equilibrium.Domain.Interfaces.Services
{
    public interface IPermissionAuthorizationService
    {
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionSystemName);
    }
}