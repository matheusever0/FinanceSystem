using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var user = await _dbSet
                .Include(e => e.UserRoles)
                .ThenInclude(e => e.Role)
                .Where(e => e.Username == username && !e.IsDeleted)
                .FirstOrDefaultAsync();


            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        }

        public async Task<User?> GetUserWithRolesAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User?>> GetUsersWithRolesAsync()
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }
    }
}