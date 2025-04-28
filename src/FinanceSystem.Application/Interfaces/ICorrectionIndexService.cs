using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.Interfaces
{
    public interface ICorrectionIndexService
    {
        Task<decimal> GetCurrentIndexValueAsync(CorrectionIndexType indexType);
        Task<decimal> GetHistoricalIndexValueAsync(CorrectionIndexType indexType, DateTime date);
        Task<IEnumerable<KeyValuePair<DateTime, decimal>>> GetHistoricalSeriesAsync(
            CorrectionIndexType indexType,
            DateTime startDate,
            DateTime endDate);
        Task<decimal> GetProjectedIndexValueAsync(CorrectionIndexType indexType, int months);
    }
}
