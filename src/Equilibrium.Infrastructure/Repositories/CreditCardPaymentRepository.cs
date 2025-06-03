using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
{
    public class CreditCardPaymentRepository : RepositoryBase<CreditCardPayment>, ICreditCardPaymentRepository
    {
        public CreditCardPaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CreditCardPayment>> GetPaymentsByCreditCardIdAsync(Guid creditCardId)
        {
            return await _dbSet
                .Where(p => p.CreditCardId == creditCardId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditCardPayment>> GetPaymentsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(p => p.CreditCard)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CreditCardPayment>> GetPaymentsByPeriodAsync(Guid creditCardId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CreditCardId == creditCardId &&
                           p.PaymentDate >= startDate &&
                           p.PaymentDate < endDate)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaidInPeriodAsync(Guid creditCardId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CreditCardId == creditCardId &&
                           p.PaymentDate >= startDate &&
                           p.PaymentDate < endDate)
                .SumAsync(p => p.Amount);
        }
    }
}
