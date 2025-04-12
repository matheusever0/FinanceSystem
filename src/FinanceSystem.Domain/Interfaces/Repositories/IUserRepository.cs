using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserWithRolesAsync(Guid id);
        Task<IEnumerable<User?>> GetUsersWithRolesAsync();
    }
}