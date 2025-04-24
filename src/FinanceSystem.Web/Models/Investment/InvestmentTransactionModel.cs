namespace FinanceSystem.Web.Models.Investment
{
    public class InvestmentTransactionModel
    {
        public required string Id { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Taxes { get; set; }
        public required string Broker { get; set; }
        public required string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
