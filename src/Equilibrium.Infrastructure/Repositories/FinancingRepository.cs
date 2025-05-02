using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class FinancingRepository : RepositoryBase<Financing>, IFinancingRepository
    {
        public FinancingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Financing?> GetFinancingWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(f => f.Installments)
                .Include(f => f.Payments)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Financing?>> GetFinancingsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Financing?>> GetActiveFinancingsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && f.Status == FinancingStatus.Active)
                .Include(f => f.Installments.Where(i => i.Status == FinancingInstallmentStatus.Pending))
                .ToListAsync();
        }

        public async Task<IEnumerable<Financing?>> GetFinancingsByStatusAsync(Guid userId, FinancingStatus status)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && f.Status == status)
                .ToListAsync();
        }


        public async Task<decimal> GetTotalRemainingDebtByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && f.Status == FinancingStatus.Active)
                .SumAsync(f => f.RemainingDebt);
        }

        public async Task<IEnumerable<Financing>> GetFinancingsByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && f.Status == FinancingStatus.Active)
                .Include(f => f.Installments.Where(i => i.DueDate >= startDate && i.DueDate <= endDate))
                .Where(f => f.Installments.Any(i => i.DueDate >= startDate && i.DueDate <= endDate))
                .ToListAsync();
        }

        public async Task<IEnumerable<Financing>> GetRecentlyModifiedFinancingsAsync(Guid userId, DateTime startDate)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && (f.UpdatedAt >= startDate || f.CreatedAt >= startDate))
                .Include(f => f.Installments)
                .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt)
                .ToListAsync();
        }
    }
}