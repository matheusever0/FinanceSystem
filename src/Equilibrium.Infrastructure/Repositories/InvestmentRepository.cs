using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class InvestmentRepository : RepositoryBase<Investment>, IInvestmentRepository
    {
        public InvestmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Investment?>> GetInvestmentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(i => i.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Investment?>> GetInvestmentsByTypeAsync(Guid userId, InvestmentType type)
        {
            return await _dbSet
                .Where(i => i.UserId == userId && i.Type == type)
                .ToListAsync();
        }

        public async Task<Investment?> GetInvestmentWithTransactionsAsync(Guid id)
        {
            return await _dbSet
                .Include(i => i.Transactions)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Investment?> GetInvestmentBySymbolAsync(Guid userId, string symbol)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.UserId == userId && i.Symbol.ToUpper() == symbol.ToUpper());
        }
    }
}
