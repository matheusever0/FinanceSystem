using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs
{
    public class UpdatePaymentInstallmentDto
    {
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus? Status { get; set; }
    }
}
