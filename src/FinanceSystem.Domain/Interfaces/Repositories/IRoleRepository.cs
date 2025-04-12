using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IRoleRepository : IRepositoryBase<Role>
    {
        Task<Role?> GetByNameAsync(string name);
        Task<IEnumerable<Role?>> GetAllWithUsersAsync();
    }
}