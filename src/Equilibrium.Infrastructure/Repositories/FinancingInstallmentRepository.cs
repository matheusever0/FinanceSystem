using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class FinancingInstallmentRepository : RepositoryBase<FinancingInstallment>, IFinancingInstallmentRepository
    {
        public FinancingInstallmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FinancingInstallment>> GetInstallmentsByFinancingIdAsync(Guid financingId)
        {
            return await _dbSet
                .Where(i => i.FinancingId == financingId)
                .OrderBy(i => i.InstallmentNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancingInstallment>> GetInstallmentsByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(i => i.Financing)
                .Where(i => i.Financing.UserId == userId &&
                            i.DueDate >= startDate &&
                            i.DueDate <= endDate)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancingInstallment>> GetPendingInstallmentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(i => i.Financing)
                .Where(i => i.Financing.UserId == userId &&
                            (i.Status == FinancingInstallmentStatus.Pending ||
                             i.Status == FinancingInstallmentStatus.PartiallyPaid))
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancingInstallment>> GetOverdueInstallmentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(i => i.Financing)
                .Where(i => i.Financing.UserId == userId &&
                            i.Status == FinancingInstallmentStatus.Overdue)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancingInstallment>> GetInstallmentsByStatusAsync(Guid userId, FinancingInstallmentStatus status)
        {
            return await _dbSet
                .Include(i => i.Financing)
                .Where(i => i.Financing.UserId == userId && i.Status == status)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancingInstallment>> GetRecentlyPaidInstallmentsAsync(Guid userId, DateTime startDate)
        {
            return await _dbSet
                .Include(i => i.Financing)
                .Where(i => i.Financing.UserId == userId &&
                            i.Status == FinancingInstallmentStatus.Paid &&
                            i.PaymentDate >= startDate)
                .OrderByDescending(i => i.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancingInstallment>> GetUpcomingInstallmentsAsync(Guid userId, int numberOfMonths = 3)
        {
            var today = DateTime.Today;
            var endDate = today.AddMonths(numberOfMonths);

            return await _dbSet
                .Include(i => i.Financing)
                .Where(i => i.Financing.UserId == userId &&
                            i.Status == FinancingInstallmentStatus.Pending &&
                            i.DueDate >= today &&
                            i.DueDate <= endDate)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPendingAmountByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(i => i.Financing)
                .Where(i => i.Financing.UserId == userId &&
                            (i.Status == FinancingInstallmentStatus.Pending ||
                             i.Status == FinancingInstallmentStatus.PartiallyPaid))
                .SumAsync(i => i.RemainingAmount);
        }
    }
}