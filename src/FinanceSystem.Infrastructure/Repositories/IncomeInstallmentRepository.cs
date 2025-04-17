using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Infrastructure.Repositories
{
    public class IncomeInstallmentRepository : RepositoryBase<IncomeInstallment>, IIncomeInstallmentRepository
    {
        public IncomeInstallmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<IncomeInstallment>> GetInstallmentsByIncomeIdAsync(Guid incomeId)
        {
            return await _dbSet
                .Where(ii => ii.IncomeId == incomeId)
                .OrderBy(ii => ii.InstallmentNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<IncomeInstallment>> GetInstallmentsByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(ii => ii.Income)
                .Where(ii => ii.Income.UserId == userId &&
                       ii.DueDate >= startDate &&
                       ii.DueDate <= endDate)
                .OrderBy(ii => ii.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<IncomeInstallment>> GetPendingInstallmentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(ii => ii.Income)
                .Where(ii => ii.Income.UserId == userId &&
                       ii.Status == Domain.Enums.IncomeStatus.Pending)
                .OrderBy(ii => ii.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<IncomeInstallment>> GetReceivedInstallmentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(ii => ii.Income)
                .Where(ii => ii.Income.UserId == userId &&
                       ii.Status == Domain.Enums.IncomeStatus.Received)
                .OrderBy(ii => ii.DueDate)
                .ToListAsync();
        }
    }
}