namespace FinanceSystem.Application.DTOs.Investment
{
    public class StockQuoteDto
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal PreviousClose { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
