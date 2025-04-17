using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs.PaymentInstallmentDto
{
    public class PaymentInstallmentDto
    {
        public Guid Id { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusDescription => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid PaymentId { get; set; }
    }
}
