using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class IncomeTypeRepository : RepositoryBase<IncomeType>, IIncomeTypeRepository
    {
        public IncomeTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IncomeType?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(it => it.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<IncomeType>> GetAllSystemTypesAsync()
        {
            return await _dbSet.Where(it => it.IsSystem).ToListAsync();
        }

        public async Task<IEnumerable<IncomeType>> GetUserTypesAsync(Guid userId)
        {
            return await _dbSet
                .Where(it => !it.IsSystem && it.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<IncomeType>> GetAllAvailableForUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(it => it.IsSystem || it.UserId == userId)
                .ToListAsync();
        }
    }
}