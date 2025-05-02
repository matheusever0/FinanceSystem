namespace Equilibrium.Application.DTOs.Financing
{
    public class FinancingInstallmentPaymentDto
    {
        public Guid InstallmentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public Guid PaymentMethodId { get; set; }
        public string? Notes { get; set; }
        public bool IsAmortization { get; set; } = false;
        public bool RecalculateInstallments { get; set; } = true;
    }
}
