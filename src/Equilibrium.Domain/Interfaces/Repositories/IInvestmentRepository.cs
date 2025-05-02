using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IInvestmentRepository : IRepositoryBase<Investment>
    {
        Task<IEnumerable<Investment?>> GetInvestmentsByUserIdAsync(Guid userId);
        Task<IEnumerable<Investment?>> GetInvestmentsByTypeAsync(Guid userId, InvestmentType type);
        Task<Investment?> GetInvestmentWithTransactionsAsync(Guid id);
        Task<Investment?> GetInvestmentBySymbolAsync(Guid userId, string symbol);
    }
}
