using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment?>> GetPaymentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .Include(p => p.Installments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment?>> GetPaymentsByUserIdAndMonthAsync(Guid userId, int month, int year)
        {
            var directPayments = await _dbSet
                .Where(p => p.UserId == userId &&
                       p.DueDate.Month == month &&
                       p.DueDate.Year == year)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .ToListAsync();

            var installmentPayments = await _dbSet
                .Where(p => p.UserId == userId)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .Include(p => p.Installments)
                .Where(p => p.Installments.Any(i => i.DueDate.Month == month && i.DueDate.Year == year))
                .ToListAsync();

            return directPayments.Union(installmentPayments);
        }

        public async Task<IEnumerable<Payment?>> GetPendingPaymentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && p.Status == PaymentStatus.Pending)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .Include(p => p.Installments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment?>> GetOverduePaymentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && p.Status == PaymentStatus.Overdue)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .Include(p => p.Installments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment?>> GetPaymentsByTypeAsync(Guid userId, Guid paymentTypeId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && p.PaymentTypeId == paymentTypeId)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .Include(p => p.Installments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment?>> GetPaymentsByMethodAsync(Guid userId, Guid paymentMethodId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && p.PaymentMethodId == paymentMethodId)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .Include(p => p.Installments)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentWithDetailsAsync(Guid paymentId)
        {
            return await _dbSet
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .Include(p => p.Installments)
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }
        public async Task<IEnumerable<Payment>> GetRecurringPaymentsWithDetailsAsync()
        {
            return await _dbSet
                .Where(p => p.IsRecurring)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentMethod)
                .Include(p => p.User)
                .Where(e => e.DueDate.Month == DateTime.Now.Month)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByPeriodAndDetailsAsync(Guid userId, Guid paymentTypeId, string description, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p =>
                    p.UserId == userId &&
                    p.PaymentTypeId == paymentTypeId &&
                    p.Description == description &&
                    p.DueDate >= startDate &&
                    p.DueDate <= endDate)
                .ToListAsync();
        }
    }
}
