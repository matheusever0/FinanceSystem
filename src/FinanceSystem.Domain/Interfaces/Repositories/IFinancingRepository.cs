using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IFinancingRepository : IRepositoryBase<Financing>
    {
        Task<Financing?> GetFinancingWithDetailsAsync(Guid id);
        Task<IEnumerable<Financing?>> GetFinancingsByUserIdAsync(Guid userId);
        Task<IEnumerable<Financing?>> GetActiveFinancingsByUserIdAsync(Guid userId);
        Task<IEnumerable<Financing?>> GetFinancingsByStatusAsync(Guid userId, FinancingStatus status);
    }
}