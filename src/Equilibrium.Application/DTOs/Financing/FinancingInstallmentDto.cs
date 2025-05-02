using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Financing
{
    public class FinancingInstallmentDto
    {
        public Guid Id { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalCorrection { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal AmortizationAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public FinancingInstallmentStatus Status { get; set; }
        public string StatusDescription => Status.ToString();
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid FinancingId { get; set; }
    }
}