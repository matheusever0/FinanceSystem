using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IFinancingCorrectionRepository : IRepositoryBase<FinancingCorrection>
    {
        Task<IEnumerable<FinancingCorrection>> GetCorrectionsByFinancingIdAsync(Guid financingId);
        Task<IEnumerable<FinancingCorrection>> GetCorrectionsByDateRangeAsync(Guid financingId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalCorrectionImpactAsync(Guid financingId);
    }
}