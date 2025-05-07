namespace Equilibrium.Web.Models.Investment
{
    public class CreateInvestmentModel
    {
        public string Symbol { get; set; }
        public string Currency { get; set; }
        public int Type { get; set; }
        public decimal InitialQuantity { get; set; }
        public decimal InitialPrice { get; set; }
        public string Broker { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
