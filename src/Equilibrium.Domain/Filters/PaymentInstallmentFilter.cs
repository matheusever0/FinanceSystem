using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.DTOs.Filters
{
    public class PaymentInstallmentFilter : BaseFilter
    {
        public int? InstallmentNumber { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
        public PaymentStatus? Status { get; set; }
        public Guid? PaymentId { get; set; }
    }
}
