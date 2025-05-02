namespace Equilibrium.Web.Models.Financing
{
    public class FinancingSimulationModel
    {
        public decimal TotalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public int Type { get; set; }
        public string TypeDescription { get; set; }
        public decimal FirstInstallmentAmount { get; set; }
        public decimal LastInstallmentAmount { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalCost { get; set; }
        public decimal MonthlyDecreaseAmount { get; set; }
        public List<FinancingInstallmentSimulationModel> Installments { get; set; } = new List<FinancingInstallmentSimulationModel>();

        public string GetFormattedTotalAmount()
        {
            return string.Format("{0:C2}", TotalAmount);
        }

        public string GetFormattedInterestRate()
        {
            return string.Format("{0:F2}%", InterestRate);
        }

        public string GetFormattedTotalInterest()
        {
            return string.Format("{0:C2}", TotalInterest);
        }

        public string GetFormattedTotalCost()
        {
            return string.Format("{0:C2}", TotalCost);
        }
    }
}
