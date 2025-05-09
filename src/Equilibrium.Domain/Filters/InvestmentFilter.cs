using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.DTOs.Filters
{
    public class InvestmentFilter : BaseFilter
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public InvestmentType? Type { get; set; }
        public decimal? MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinGainLossPercentage { get; set; }
        public decimal? MaxGainLossPercentage { get; set; }
        public string Currency { get; set; }
    }
}
