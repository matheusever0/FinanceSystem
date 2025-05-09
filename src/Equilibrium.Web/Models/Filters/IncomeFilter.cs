using System;

namespace Equilibrium.Web.Models.Filters
{
    public class IncomeFilter : BaseFilter
    {
        public string Description { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }
        public string IncomeTypeId { get; set; }
        public bool? IsRecurring { get; set; }
    }
}
