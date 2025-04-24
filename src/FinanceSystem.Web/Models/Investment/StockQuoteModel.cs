namespace FinanceSystem.Web.Models.Investment
{
    public class StockQuoteModel
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal PreviousClose { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
