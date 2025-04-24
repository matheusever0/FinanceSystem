namespace FinanceSystem.Web.Models.Payment
{
    public class MonthlyComparisonData
    {
        public required string Month { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal IncomeAmount { get; set; }
    }
}
