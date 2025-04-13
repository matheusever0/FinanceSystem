using System.Security.Claims;

namespace FinanceSystem.Domain.Interfaces.Services
{
    public interface IPermissionAuthorizationService
    {
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionSystemName);
    }
}