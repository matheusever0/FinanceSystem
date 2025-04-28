using FinanceSystem.Application.DTOs.Financing;

namespace FinanceSystem.Application.Interfaces
{
    public interface IFinancingCorrectionService
    {
        Task<FinancingCorrectionDto> GetByIdAsync(Guid id);
        Task<IEnumerable<FinancingCorrectionDto>> GetByFinancingIdAsync(Guid financingId);
        Task<IEnumerable<FinancingCorrectionDto>> GetByDateRangeAsync(Guid financingId, DateTime startDate, DateTime endDate);
        Task<FinancingCorrectionDto> ApplyCorrectionAsync(ApplyCorrectionDto correctionDto);
        Task<decimal> GetTotalCorrectionImpactAsync(Guid financingId);
    }

}
