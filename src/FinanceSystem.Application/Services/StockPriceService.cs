using FinanceSystem.Application.DTOs.Investment;
using FinanceSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace FinanceSystem.Application.Services
{
    public class StockPriceService : IStockPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public StockPriceService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["BrapiApi:ApiKey"]!;
        }

        public async Task<decimal> GetCurrentPriceAsync(string symbol)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://brapi.dev/api/quote/{symbol}?token={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BrapiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Results == null || result.Results.Count == 0
                    ? throw new Exception($"Nenhum resultado encontrado para o símbolo {symbol}")
                    : result.Results[0].RegularMarketPrice;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<StockQuoteDto>> GetBatchQuotesAsync(List<string> symbols)
        {
            try
            {
                var symbolsString = string.Join(",", symbols);
                var response = await _httpClient.GetAsync($"https://brapi.dev/api/quote/{symbolsString}?token={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BrapiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Results == null || result.Results.Count == 0
                    ? new List<StockQuoteDto>()
                    : result.Results.Select(r => new StockQuoteDto
                {
                    Symbol = r.Symbol,
                    Price = r.RegularMarketPrice,
                    Change = r.RegularMarketChange,
                    ChangePercent = r.RegularMarketChangePercent,
                    PreviousClose = r.RegularMarketPreviousClose,
                    UpdatedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private class BrapiResponse
        {
            public required List<BrapiResult> Results { get; set; }
        }

        private class BrapiResult
        {
            public required string Symbol { get; set; }
            public decimal RegularMarketPrice { get; set; }
            public decimal RegularMarketChange { get; set; }
            public decimal RegularMarketChangePercent { get; set; }
            public decimal RegularMarketPreviousClose { get; set; }
        }
    }
}
