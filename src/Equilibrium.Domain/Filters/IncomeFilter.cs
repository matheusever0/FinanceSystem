using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.DTOs.Filters
{
    public class IncomeFilter : BaseFilter
    {
        public string Description { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IncomeStatus? Status { get; set; }
        public Guid? IncomeTypeId { get; set; }
        public bool? IsRecurring { get; set; }
    }
}
