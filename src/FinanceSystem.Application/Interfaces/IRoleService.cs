using FinanceSystem.Application.DTOs.Role;

namespace FinanceSystem.Application.Interfaces
{
    public interface IRoleService
    {
        Task<RoleDto> GetByIdAsync(Guid id);
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto> CreateAsync(CreateRoleDto createRoleDto);
        Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto updateRoleDto);
        Task DeleteAsync(Guid id);
        Task<bool> HasPermissionAsync(Guid roleId, string permissionSystemName);
        Task<RoleDto> UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds);
    }
}