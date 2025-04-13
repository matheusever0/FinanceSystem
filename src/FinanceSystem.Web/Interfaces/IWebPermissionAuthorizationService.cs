namespace FinanceSystem.Web.Interfaces
{
    public interface IWebPermissionAuthorizationService
    {
        Task<bool> HasPermissionAsync(System.Security.Claims.ClaimsPrincipal user, string permissionSystemName);
    }
}