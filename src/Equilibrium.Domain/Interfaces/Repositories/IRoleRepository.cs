using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IRoleRepository : IRepositoryBase<Role>
    {
        Task<Role?> GetByNameAsync(string name);
        Task<IEnumerable<Role?>> GetAllWithUsersAsync();
        Task<Role?> GetRoleWithPermissionsAsync(Guid id);
    }
}