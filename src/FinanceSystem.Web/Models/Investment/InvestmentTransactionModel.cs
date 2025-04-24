namespace FinanceSystem.Web.Models.Investment
{
    public class InvestmentTransactionModel
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string InvestmentType { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Taxes { get; set; }
        public string Broker { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
