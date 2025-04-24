namespace FinanceSystem.Web.Models.Investment
{
    public class InvestmentModel
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal CurrentTotal { get; set; }
        public decimal GainLossPercentage { get; set; }
        public decimal GainLossValue { get; set; }
        public DateTime LastUpdate { get; set; }
        public string UserId { get; set; }
        public List<InvestmentTransactionModel> Transactions { get; set; } = [];
    }
}
