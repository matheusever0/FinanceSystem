using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.Interfaces
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
