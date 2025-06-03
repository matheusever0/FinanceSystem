using Equilibrium.Application.DTOs.Payment;

namespace Equilibrium.Application.DTOs.CreditCard
{
    public class CreditCardInvoiceDetailDto : CreditCardInvoiceDto
    {
        public List<PaymentDto> Transactions { get; set; } = [];
        public List<InvoicePaymentDto> Payments { get; set; } = [];

        public decimal InterestCharges { get; set; }
        public decimal Fees { get; set; }
        public decimal Credits { get; set; }

        public DateTime? PreviousBalance { get; set; }
        public decimal PreviousBalanceAmount { get; set; }
    }
}