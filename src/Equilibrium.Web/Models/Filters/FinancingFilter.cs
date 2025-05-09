using System;

namespace Equilibrium.Web.Models.Filters
{
    public class FinancingFilter : BaseFilter
    {
        public string Description { get; set; }
        public decimal? MinTotalAmount { get; set; }
        public decimal? MaxTotalAmount { get; set; }
        public decimal? MinInterestRate { get; set; }
        public decimal? MaxInterestRate { get; set; }
        public int? MinTermMonths { get; set; }
        public int? MaxTermMonths { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public int? CorrectionIndex { get; set; }
        public decimal? MinRemainingDebt { get; set; }
        public decimal? MaxRemainingDebt { get; set; }
    }
}
