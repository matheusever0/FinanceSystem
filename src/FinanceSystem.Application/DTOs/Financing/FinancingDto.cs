using FinanceSystem.Application.DTOs.Payment;
using FinanceSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Financing
{
    public class FinancingDto
    {
        public Guid Id { get; set; }
        public required string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public FinancingType Type { get; set; }
        public string TypeDescription => Type.ToString();
        public FinancingStatus Status { get; set; }
        public string StatusDescription => Status.ToString();
        public CorrectionIndexType CorrectionIndex { get; set; }
        public string CorrectionIndexDescription => CorrectionIndex.ToString();
        public required string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        public decimal RemainingDebt { get; set; }
        public DateTime LastCorrectionDate { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int InstallmentsPaid { get; set; }
        public int InstallmentsRemaining { get; set; }
    }
}