using FinanceSystem.Application.DTOs.Investment;

namespace FinanceSystem.Application.Interfaces
{
    public interface IStockPriceService
    {
        Task<decimal> GetCurrentPriceAsync(string symbol);
        Task<IEnumerable<StockQuoteDto>> GetBatchQuotesAsync(List<string> symbols);
    }
}
