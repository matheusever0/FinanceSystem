using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IFinancingRepository : IRepositoryBase<Financing>
    {
        Task<Financing?> GetFinancingWithDetailsAsync(Guid id);
        Task<IEnumerable<Financing?>> GetFinancingsByUserIdAsync(Guid userId);
        Task<IEnumerable<Financing?>> GetActiveFinancingsByUserIdAsync(Guid userId);
        Task<IEnumerable<Financing?>> GetFinancingsByStatusAsync(Guid userId, FinancingStatus status);
        Task<decimal> GetTotalRemainingDebtByUserIdAsync(Guid userId);
        Task<IEnumerable<Financing>> GetRecentlyModifiedFinancingsAsync(Guid userId, DateTime startDate);
        Task<IEnumerable<Financing>> GetFinancingsByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate);
    }
}