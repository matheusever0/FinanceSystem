using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.PaymentInstallment
{
    public class UpdatePaymentInstallmentDto
    {
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus? Status { get; set; }
    }
}
