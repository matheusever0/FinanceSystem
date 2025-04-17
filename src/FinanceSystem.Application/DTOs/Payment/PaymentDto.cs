using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusDescription => Status.ToString();
        public bool IsRecurring { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid UserId { get; set; }
        public Guid PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; }
        public Guid PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }

        public List<PaymentInstallmentDto> Installments { get; set; } = new List<PaymentInstallmentDto>();
    }
}
