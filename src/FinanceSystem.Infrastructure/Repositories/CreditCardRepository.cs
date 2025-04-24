using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Infrastructure.Repositories
{
    public class CreditCardRepository : RepositoryBase<CreditCard>, ICreditCardRepository
    {
        public CreditCardRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CreditCard?>> GetCreditCardsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(cc => cc.UserId == userId)
                .Include(cc => cc.PaymentMethod)
                .ToListAsync();
        }

        public async Task<CreditCard?> GetCreditCardWithDetailsAsync(Guid creditCardId)
        {
            return await _dbSet
                .Include(cc => cc.PaymentMethod)
                .FirstOrDefaultAsync(cc => cc.Id == creditCardId);
        }
    }
}
