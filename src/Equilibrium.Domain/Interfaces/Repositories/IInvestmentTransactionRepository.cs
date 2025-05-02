using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IInvestmentTransactionRepository : IRepositoryBase<InvestmentTransaction>
    {
        Task<IEnumerable<InvestmentTransaction>> GetTransactionsByInvestmentIdAsync(Guid investmentId);
        Task<IEnumerable<InvestmentTransaction>> GetTransactionsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
    }
}
