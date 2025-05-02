using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class PaymentInstallmentRepository : RepositoryBase<PaymentInstallment>, IPaymentInstallmentRepository
    {
        public PaymentInstallmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PaymentInstallment>> GetInstallmentsByPaymentIdAsync(Guid paymentId)
        {
            return await _dbSet
                .Where(pi => pi.PaymentId == paymentId)
                .OrderBy(pi => pi.InstallmentNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentInstallment>> GetInstallmentsByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(pi => pi.Payment)
                .Where(pi => pi.Payment.UserId == userId &&
                       pi.DueDate >= startDate &&
                       pi.DueDate <= endDate)
                .OrderBy(pi => pi.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentInstallment>> GetPendingInstallmentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(pi => pi.Payment)
                .Where(pi => pi.Payment.UserId == userId &&
                       pi.Status == PaymentStatus.Pending)
                .OrderBy(pi => pi.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentInstallment>> GetOverdueInstallmentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(pi => pi.Payment)
                .Where(pi => pi.Payment.UserId == userId &&
                       pi.Status == PaymentStatus.Overdue)
                .OrderBy(pi => pi.DueDate)
                .ToListAsync();
        }
    }
}
