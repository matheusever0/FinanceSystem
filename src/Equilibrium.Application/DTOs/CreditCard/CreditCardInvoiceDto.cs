namespace Equilibrium.Application.DTOs.CreditCard
{
    public class CreditCardInvoiceDto
    {
        public Guid CreditCardId { get; set; }
        public string CreditCardName { get; set; } = string.Empty;
        public DateTime ReferenceDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsOverdue { get; set; }
        public int TransactionCount { get; set; }
        public decimal AvailableLimit { get; set; }
        public decimal UsedLimit { get; set; }
        public decimal TotalLimit { get; set; }
    }
}