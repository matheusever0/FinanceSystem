namespace FinanceSystem.Web.Models.Financing
{
    public class FinancingInstallmentSimulationModel
    {
        public int Number { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal AmortizationAmount { get; set; }
        public decimal RemainingDebt { get; set; }
        public DateTime DueDate { get; set; }

        public string GetFormattedTotalAmount()
        {
            return string.Format("{0:C2}", TotalAmount);
        }

        public string GetFormattedInterestAmount()
        {
            return string.Format("{0:C2}", InterestAmount);
        }

        public string GetFormattedAmortizationAmount()
        {
            return string.Format("{0:C2}", AmortizationAmount);
        }

        public string GetFormattedRemainingDebt()
        {
            return string.Format("{0:C2}", RemainingDebt);
        }

        public string GetFormattedDueDate()
        {
            return DueDate.ToString("dd/MM/yyyy");
        }
    }
}
