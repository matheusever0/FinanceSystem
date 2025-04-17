using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs.PaymentInstallmentDto
{
    public class UpdatePaymentInstallmentDto
    {
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus? Status { get; set; }
    }
}
