namespace Equilibrium.Web.Models.Investment
{
    public class CreateInvestmentTransactionModel
    {
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Taxes { get; set; }
        public string Broker { get; set; }
        public string Notes { get; set; }
    }
}
