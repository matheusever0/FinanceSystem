using FinanceSystem.Application.DTOs.Common;
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

        public async Task<StockQuoteDto?> GetBatchQuoteAsync(string symbols)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://brapi.dev/api/quote/{symbols}?token={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<BrapiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var quote = result?.Results?.FirstOrDefault();
                if (quote == null) return null;

                return new StockQuoteDto
                {
                    Symbol = quote.Symbol,
                    LongName = quote.LongName,
                    ShortName = quote.ShortName,
                    Currency = quote.Currency,
                    Price = quote.RegularMarketPrice,
                    Change = quote.RegularMarketChange,
                    ChangePercent = quote.RegularMarketChangePercent,
                    PreviousClose = quote.RegularMarketPreviousClose,
                    UpdatedAt = DateTime.Now
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<StockQuoteDto>> GetBatchQuotesAsync(List<string> symbols)
        {
            var tasks = symbols.Select(async symbol =>
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

                    var quote = result?.Results?.FirstOrDefault();
                    if (quote == null) return null;

                    return new StockQuoteDto
                    {
                        Symbol = quote.Symbol,
                        LongName = quote.LongName,
                        ShortName = quote.ShortName,
                        Currency = quote.Currency,
                        Price = quote.RegularMarketPrice,
                        Change = quote.RegularMarketChange,
                        ChangePercent = quote.RegularMarketChangePercent,
                        PreviousClose = quote.RegularMarketPreviousClose,
                        UpdatedAt = DateTime.Now
                    };
                }
                catch
                {
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);
            return [.. results.Where(r => r != null)];
        }

    }
}
