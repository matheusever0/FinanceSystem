using FinanceSystem.Application.DTOs;

namespace FinanceSystem.Application.Services.Interfaces
{
    public interface IRoleService
    {
        Task<RoleDto> GetByIdAsync(Guid id);
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto> CreateAsync(CreateRoleDto createRoleDto);
        Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto updateRoleDto);
        Task DeleteAsync(Guid id);
    }
}
