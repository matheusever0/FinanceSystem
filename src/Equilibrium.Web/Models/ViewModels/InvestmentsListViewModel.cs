using Equilibrium.Web.Models.Investment;

namespace Equilibrium.Web.Models.ViewModels
{
    public class InvestmentsListViewModel
    {
        public List<InvestmentModel> Investments { get; set; } = new List<InvestmentModel>();
        public int? FilterType { get; set; }

        public decimal TotalInvested => Investments.Sum(i => i.TotalInvested);
        public decimal TotalCurrentValue => Investments.Sum(i => i.CurrentTotal);
        public decimal TotalGainLoss => Investments.Sum(i => i.GainLossValue);

        public decimal TotalGainLossPercentage => TotalInvested > 0 ? TotalGainLoss / TotalInvested * 100 : 0;

        public bool HasInvestments => Investments.Any();

        public string GetFilterName()
        {
            if (!FilterType.HasValue)
                return "Todos";

            return FilterType.Value switch
            {
                1 => "Ações",
                2 => "Fundos Imobiliários",
                3 => "ETFs",
                4 => "Ações Estrangeiras",
                5 => "Renda Fixa",
                _ => "Investimentos"
            };
        }
    }
}