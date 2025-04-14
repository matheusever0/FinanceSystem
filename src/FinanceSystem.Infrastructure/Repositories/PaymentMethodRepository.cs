using FinanceSystem.Domain.Enums;
using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Infrastructure.Repositories
{
    public class PaymentMethodRepository : RepositoryBase<PaymentMethod>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PaymentMethod>> GetAllSystemMethodsAsync()
        {
            return await _dbSet
                .Where(pm => pm.IsSystem)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentMethod>> GetUserMethodsAsync(Guid userId)
        {
            return await _dbSet
                .Where(pm => !pm.IsSystem && pm.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentMethod>> GetAllAvailableForUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(pm => pm.IsSystem || pm.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentMethod>> GetByTypeAsync(PaymentMethodType type)
        {
            return await _dbSet
                .Where(pm => pm.Type == type)
                .ToListAsync();
        }

        public async Task<PaymentMethod> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(pm => pm.Name.ToLower() == name.ToLower());
        }
    }
}
