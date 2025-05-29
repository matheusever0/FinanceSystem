using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Payment
{
    public class PaymentFilter
    {
        public string? Description { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime? PaymentStartDate { get; set; }
        public DateTime? PaymentEndDate { get; set; }
        public PaymentStatus? Status { get; set; }
        public Guid? PaymentTypeId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public Guid? FinancingId { get; set; }
        public Guid? FinancingInstallmentId { get; set; }
        public Guid? CreditCardId { get; set; }
        public bool? IsRecurring { get; set; }
        public bool? HasInstallments { get; set; }
        public bool? IsFinancingPayment { get; set; }
        public string? Notes { get; set; }
        public string OrderBy { get; set; } = "DueDate";
        public bool Ascending { get; set; } = true;
        public bool IsValidMonth()
        {
            return !Month.HasValue || (Month >= 1 && Month <= 12);
        }
    }
}