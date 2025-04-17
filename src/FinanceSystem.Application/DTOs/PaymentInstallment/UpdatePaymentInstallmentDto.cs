using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs.PaymentInstallment
{
    public class UpdatePaymentInstallmentDto
    {
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus? Status { get; set; }
    }
}
