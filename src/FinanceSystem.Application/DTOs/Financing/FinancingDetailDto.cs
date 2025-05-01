using FinanceSystem.Application.DTOs.Payment;

namespace FinanceSystem.Application.DTOs.Financing
{
    public class FinancingDetailDto : FinancingDto
    {
        public List<FinancingInstallmentDto> Installments { get; set; } = [];
        public List<PaymentDto> Payments { get; set; } = [];

        public decimal TotalInterestPaid { get; set; }
        public decimal TotalInterestRemaining { get; set; }
        public decimal TotalAmortizationPaid { get; set; }
        public decimal AverageInstallmentAmount { get; set; }
        public decimal MonthlyAveragePayment { get; set; }
        public decimal EstimatedTotalCost { get; set; }
    }
}
