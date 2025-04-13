using FinanceSystem.Web.Models;

namespace FinanceSystem.Web.Services
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionModel>> GetAllPermissionsAsync(string token);
        Task<PermissionModel> GetPermissionByIdAsync(string id, string token);
        Task<PermissionModel> CreatePermissionAsync(CreatePermissionModel model, string token);
        Task<PermissionModel> UpdatePermissionAsync(string id, UpdatePermissionModel model, string token);
        Task DeletePermissionAsync(string id, string token);
        Task<IEnumerable<PermissionModel>> GetPermissionsByRoleIdAsync(string roleId, string token);
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string token);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string token);
        Task<IEnumerable<PermissionModel>> GetPermissionsByUserIdAsync(string userId, string token);
    }
}