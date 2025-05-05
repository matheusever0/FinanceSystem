using Equilibrium.Web.Models.CreditCard;
using System.Collections.Generic;
using System.Linq;

namespace Equilibrium.Web.Models.ViewModels
{
    public class CreditCardsListViewModel
    {
        public List<CreditCardModel> CreditCards { get; set; } = new List<CreditCardModel>();

        public decimal TotalLimit => CreditCards.Sum(c => c.Limit);
        public decimal TotalAvailableLimit => CreditCards.Sum(c => c.AvailableLimit);
        public decimal TotalUsedLimit => CreditCards.Sum(c => c.Limit - c.AvailableLimit);

        public decimal OverallUsedPercentage => TotalLimit > 0 ? TotalUsedLimit / TotalLimit * 100 : 0;

        public bool HasCreditCards => CreditCards.Any();

        public string GetUsageColorClass(decimal usedPercentage)
        {
            if (usedPercentage > 75) return "danger";
            if (usedPercentage > 50) return "warning";
            return "success";
        }
    }
}