using FinanceSystem.Application.DTOs;

namespace FinanceSystem.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<PermissionDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PermissionDto>> GetAllAsync();
        Task<PermissionDto> CreateAsync(CreatePermissionDto createPermissionDto);
        Task<PermissionDto> UpdateAsync(Guid id, UpdatePermissionDto updatePermissionDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId);
        Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId);
        Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
        Task<IEnumerable<PermissionDto>> GetPermissionsByUserIdAsync(Guid userId);
    }
}