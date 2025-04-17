using FinanceSystem.Web.Models.Role;

namespace FinanceSystem.Web.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleModel>> GetAllRolesAsync(string token);
        Task<RoleModel> GetRoleByIdAsync(string id, string token);
        Task<RoleModel> CreateRoleAsync(CreateRoleModel model, string token);
        Task<RoleModel> UpdateRoleAsync(string id, UpdateRoleModel model, string token);
        Task DeleteRoleAsync(string id, string token);
        Task<bool> HasPermissionAsync(string roleId, string permissionSystemName, string token);
        Task<RoleModel> UpdateRolePermissionsAsync(string roleId, List<string> permissionIds, string token);
    }
}