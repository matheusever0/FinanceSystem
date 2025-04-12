using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceSystem.Infrastructure.Repositories
{
    public class RoleRepository : RepositoryBase<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Role> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Role>> GetAllWithUsersAsync()
        {
            return await _dbSet
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .ToListAsync();
        }
    }
}