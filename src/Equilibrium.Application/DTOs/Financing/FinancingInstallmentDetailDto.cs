using Equilibrium.Application.DTOs.Payment;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Financing
{
    public class FinancingInstallmentDetailDto : FinancingInstallmentDto
    {
        public string FinancingDescription { get; set; }
        public List<PaymentDto> Payments { get; set; } = [];
        public decimal PercentagePaid => TotalAmount > 0 ? (PaidAmount / TotalAmount) * 100 : 0;
        public int DaysOverdue => Status == FinancingInstallmentStatus.Overdue
            ? (int)(DateTime.Now - DueDate).TotalDays
            : 0;
    }
}
