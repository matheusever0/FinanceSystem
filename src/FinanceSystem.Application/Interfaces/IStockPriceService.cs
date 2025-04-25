using FinanceSystem.Application.DTOs.Investment;

namespace FinanceSystem.Application.Interfaces
{
    public interface IStockPriceService
    {
        Task<List<StockQuoteDto>> GetBatchQuotesAsync(List<string> symbols);
        Task<StockQuoteDto?> GetBatchQuoteAsync(string symbols);
    }
}
