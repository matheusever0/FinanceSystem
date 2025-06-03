using Equilibrium.Application.DTOs.PaymentInstallment;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public required string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusDescription => Status.ToString();
        public bool IsRecurring { get; set; }
        public required string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        public Guid PaymentTypeId { get; set; }
        public required string PaymentTypeName { get; set; }
        public Guid PaymentMethodId { get; set; }
        public required string PaymentMethodName { get; set; }
        public Guid? CreditCardId { get; set; }
        public string? CreditCardName { get; set; }
        public Guid? FinancingId { get; set; }
        public string? FinancingDescription { get; set; }
        public Guid? FinancingInstallmentId { get; set; }

        public List<PaymentInstallmentDto> Installments { get; set; } = [];
    }
}