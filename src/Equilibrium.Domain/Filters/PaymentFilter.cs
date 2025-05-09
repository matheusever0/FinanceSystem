using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.DTOs.Filters
{
    public class PaymentFilter : BaseFilter
    {
        public string Description { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PaymentStatus? Status { get; set; }
        public Guid? PaymentTypeId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public bool? IsRecurring { get; set; }
    }
}
