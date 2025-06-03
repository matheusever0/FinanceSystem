namespace Equilibrium.Application.DTOs.CreditCard
{
    public class InvoicePaymentDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}