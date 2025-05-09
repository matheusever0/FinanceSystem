using System;

namespace Equilibrium.Web.Models.Filters
{
    public class CreditCardFilter : BaseFilter
    {
        public string Name { get; set; }
        public string CardBrand { get; set; }
        public string LastFourDigits { get; set; }
        public int? MinClosingDay { get; set; }
        public int? MaxClosingDay { get; set; }
        public int? MinDueDay { get; set; }
        public int? MaxDueDay { get; set; }
        public decimal? MinLimit { get; set; }
        public decimal? MaxLimit { get; set; }
        public decimal? MinAvailableLimit { get; set; }
        public decimal? MaxAvailableLimit { get; set; }
        public string PaymentMethodId { get; set; }
    }
}
