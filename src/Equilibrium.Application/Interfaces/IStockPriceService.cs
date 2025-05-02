using Equilibrium.Application.DTOs.Investment;

namespace Equilibrium.Application.Interfaces
{
    public interface IStockPriceService
    {
        Task<List<StockQuoteDto>> GetBatchQuotesAsync(List<string> symbols);
        Task<StockQuoteDto?> GetBatchQuoteAsync(string symbols);
    }
}
