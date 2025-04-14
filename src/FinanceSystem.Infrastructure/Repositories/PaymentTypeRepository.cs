using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Infrastructure.Repositories
{
    public class PaymentTypeRepository : RepositoryBase<PaymentType>, IPaymentTypeRepository
    {
        public PaymentTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PaymentType>> GetAllSystemTypesAsync()
        {
            return await _dbSet
                .Where(pt => pt.IsSystem)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentType>> GetUserTypesAsync(Guid userId)
        {
            return await _dbSet
                .Where(pt => !pt.IsSystem && pt.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentType>> GetAllAvailableForUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(pt => pt.IsSystem || pt.UserId == userId)
                .ToListAsync();
        }

        public async Task<PaymentType> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(pt => pt.Name.ToLower() == name.ToLower());
        }
    }
}
