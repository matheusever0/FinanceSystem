using Equilibrium.Web.Models.CreditCard;
using Equilibrium.Web.Models.Income;
using Equilibrium.Web.Models.Investment;
using Equilibrium.Web.Models.Payment;

namespace Equilibrium.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<CreditCardModel> CreditCards { get; set; } = new List<CreditCardModel>();
        public List<IncomeModel> PendingIncomes { get; set; } = new List<IncomeModel>();
        public List<IncomeModel> OverdueIncomes { get; set; } = new List<IncomeModel>();
        public List<PaymentModel> PendingPayments { get; set; } = new List<PaymentModel>();
        public List<PaymentModel> OverduePayments { get; set; } = new List<PaymentModel>();
        public List<InvestmentModel> TopPerformingInvestments { get; set; } = new List<InvestmentModel>();

        public decimal TotalBalance { get; set; }
        public decimal IncomesMonth { get; set; }
        public decimal PaymentsMonth { get; set; }

        public decimal TotalInvested { get; set; }
        public decimal CurrentInvestmentsValue { get; set; }
        public decimal InvestmentsGainLoss => CurrentInvestmentsValue - TotalInvested;

        public decimal TotalPendingIncomes => PendingIncomes.Sum(i => i.Amount);
        public decimal TotalOverdueIncomes => OverdueIncomes.Sum(i => i.Amount);
        public decimal TotalPendingPayments => PendingPayments.Sum(p => p.Amount);
        public decimal TotalOverduePayments => OverduePayments.Sum(p => p.Amount);

        public List<MonthlyComparisonData> MonthlyComparisonData { get; set; } = new List<MonthlyComparisonData>();

        public bool HasCreditCards => CreditCards.Any();
        public bool HasInvestments => TopPerformingInvestments.Any();
        public bool HasPendingPayments => PendingPayments.Any();
        public bool HasOverduePayments => OverduePayments.Any();
        public bool HasPendingIncomes => PendingIncomes.Any();
        public bool HasOverdueIncomes => OverdueIncomes.Any();
    }
}