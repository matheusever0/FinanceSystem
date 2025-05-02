using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Repositories
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
