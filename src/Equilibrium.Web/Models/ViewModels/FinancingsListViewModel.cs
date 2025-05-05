using Equilibrium.Web.Models.Financing;

namespace Equilibrium.Web.Models.ViewModels
{
    public class FinancingsListViewModel
    {
        public List<FinancingModel> Financings { get; set; } = new List<FinancingModel>();
        public bool ShowOnlyActive { get; set; }

        public decimal TotalAmount => Financings.Sum(f => f.TotalAmount);
        public decimal TotalPaid => Financings.Sum(f => f.TotalPaid);
        public decimal TotalRemaining => Financings.Sum(f => f.TotalRemaining);

        public int ActiveCount => Financings.Count(f => f.Status == 1);
        public int CompletedCount => Financings.Count(f => f.Status == 2);
        public int CanceledCount => Financings.Count(f => f.Status == 3);

        public bool HasFinancings => Financings.Any();
    }
}