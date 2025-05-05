using Equilibrium.Web.Models.Enums;
using Equilibrium.Web.Models.Enums.Extensions;
using Equilibrium.Web.Models.Investment;

namespace Equilibrium.Web.Models.ViewModels
{
    public class InvestmentViewModel
    {
        public InvestmentModel Investment { get; set; }

        public string TypeDescription => ((InvestmentType)Investment.Type).GetInvestmentTypeDescription();

        public bool HasGains => Investment.GainLossPercentage > 0;
        public bool HasLosses => Investment.GainLossPercentage < 0;

        public string GainLossDisplayClass => HasGains ? "text-success" : (HasLosses ? "text-danger" : "text-muted");
        public string GainLossIcon => HasGains ? "fa-arrow-up" : (HasLosses ? "fa-arrow-down" : "");

        public bool HasTransactions => Investment.Transactions?.Any() == true;

        public string TransactionTypeDescription(int type)
        {
            return ((TransactionType)type).GetTransactionTypeDescription();
        }
    }
}