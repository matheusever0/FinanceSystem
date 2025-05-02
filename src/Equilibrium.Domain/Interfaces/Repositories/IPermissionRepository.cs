using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IPermissionRepository : IRepositoryBase<Permission>
    {
        Task<Permission?> GetBySystemNameAsync(string systemName);
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
    }
}