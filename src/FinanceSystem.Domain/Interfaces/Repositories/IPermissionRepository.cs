using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IPermissionRepository : IRepositoryBase<Permission>
    {
        Task<Permission> GetBySystemNameAsync(string systemName);
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
    }
}