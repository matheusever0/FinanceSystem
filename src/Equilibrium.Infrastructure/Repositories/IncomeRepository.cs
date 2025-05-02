using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class IncomeRepository : RepositoryBase<Income>, IIncomeRepository
    {
        public IncomeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Income?> GetIncomeWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(i => i.IncomeType)
                .Include(i => i.Installments)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Income>> GetIncomesByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(i => i.UserId == userId)
                .Include(i => i.IncomeType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Income>> GetIncomesByUserIdAndMonthAsync(Guid userId, int month, int year)
        {
            return await _dbSet
                .Where(i => i.UserId == userId &&
                       i.DueDate.Month == month &&
                       i.DueDate.Year == year)
                .Include(i => i.IncomeType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Income>> GetPendingIncomesByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(i => i.UserId == userId && i.Status == Domain.Enums.IncomeStatus.Pending)
                .Include(i => i.IncomeType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Income>> GetOverdueIncomesByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && p.DueDate < DateTime.Now && !p.ReceivedDate.HasValue)
                .Include(p => p.IncomeType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Income>> GetReceivedIncomesByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(i => i.UserId == userId && i.Status == Domain.Enums.IncomeStatus.Received)
                .Include(i => i.IncomeType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Income>> GetIncomesByTypeAsync(Guid userId, Guid incomeTypeId)
        {
            return await _dbSet
                .Where(i => i.UserId == userId && i.IncomeTypeId == incomeTypeId)
                .Include(i => i.IncomeType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Income>> GetRecurringIncomesWithDetailsAsync()
        {
            return await _dbSet
                .Where(i => i.IsRecurring)
                .Include(i => i.IncomeType)
                .Include(i => i.User)
                .Where(e => e.DueDate.Month == DateTime.Now.Month)
                .ToListAsync();
        }

        public async Task<IEnumerable<Income>> GetIncomesByPeriodAndDetailsAsync(Guid userId, Guid incomeTypeId, string description, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(i =>
                    i.UserId == userId &&
                    i.IncomeTypeId == incomeTypeId &&
                    i.Description == description &&
                    i.DueDate >= startDate &&
                    i.DueDate <= endDate)
                .ToListAsync();
        }
    }
}