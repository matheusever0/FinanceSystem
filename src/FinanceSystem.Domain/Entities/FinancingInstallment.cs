using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Entities
{
    public class FinancingInstallment
    {
        public Guid Id { get; protected set; }
        public int InstallmentNumber { get; protected set; }
        public decimal TotalAmount { get; protected set; }      // Valor total da parcela
        public decimal InterestAmount { get; protected set; }   // Valor de juros
        public decimal AmortizationAmount { get; protected set; } // Valor de amortização
        public DateTime DueDate { get; protected set; }         // Data de vencimento
        public DateTime? PaymentDate { get; protected set; }    // Data de pagamento
        public FinancingInstallmentStatus Status { get; protected set; }
        public decimal PaidAmount { get; protected set; }       // Valor pago
        public decimal RemainingAmount { get; protected set; }  // Valor restante
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        public Guid FinancingId { get; protected set; }
        public Financing Financing { get; protected set; }

        public ICollection<Payment> Payments { get; protected set; }

        protected FinancingInstallment()
        {
            Payments = new List<Payment>();
        }

        public FinancingInstallment(
            Financing financing,
            int installmentNumber,
            decimal totalAmount,
            decimal interestAmount,
            decimal amortizationAmount,
            DateTime dueDate)
        {
            Id = Guid.NewGuid();
            InstallmentNumber = installmentNumber;
            TotalAmount = totalAmount;
            InterestAmount = interestAmount;
            AmortizationAmount = amortizationAmount;
            DueDate = dueDate;
            Status = FinancingInstallmentStatus.Pending;
            PaidAmount = 0;
            RemainingAmount = totalAmount;
            CreatedAt = DateTime.Now;

            FinancingId = financing.Id;
            Financing = financing;

            Payments = new List<Payment>();
        }

        public void UpdateValues(decimal totalAmount, decimal interestAmount, decimal amortizationAmount)
        {
            TotalAmount = totalAmount;
            InterestAmount = interestAmount;
            AmortizationAmount = amortizationAmount;
            RemainingAmount = totalAmount - PaidAmount;

            UpdatedAt = DateTime.Now;
        }

        public void MarkAsPaid(DateTime paymentDate, decimal? amount)
        {
            PaymentDate = paymentDate;
            Status = FinancingInstallmentStatus.Paid;
            PaidAmount = amount ?? TotalAmount;
            RemainingAmount = 0;

            UpdatedAt = DateTime.Now;
        }

        public void MarkAsOverdue()
        {
            Status = FinancingInstallmentStatus.Overdue;
            UpdatedAt = DateTime.Now;
        }

        public void MarkAsAdjusted()
        {
            Status = FinancingInstallmentStatus.Adjusted;
            UpdatedAt = DateTime.Now;
        }

        public void AddPayment(Payment payment, bool isAmortization = false)
        {
            Payments.Add(payment);

            decimal amountToApply = payment.Amount;

            if (isAmortization)
            {
                // For amortization, we reduce only the principal amount
                // Calculate what portion of this payment goes to principal vs interest
                decimal principalPortion = (amountToApply / TotalAmount) * AmortizationAmount;
                PaidAmount += amountToApply;

                // Signal to the financing that this much principal was paid
                Financing.UpdateRemainingDebt(principalPortion);

                // Reduce remaining amount
                RemainingAmount = TotalAmount - PaidAmount;
            }
            else
            {
                // Regular payment
                PaidAmount += amountToApply;
                RemainingAmount -= amountToApply;
            }

            // Update status based on remaining amount
            if (RemainingAmount <= 0)
            {
                MarkAsPaid(payment.PaymentDate ?? DateTime.Now, amountToApply);
            }
            else
            {
                Status = FinancingInstallmentStatus.PartiallyPaid;
                if (PaymentDate == null || payment.PaymentDate > PaymentDate)
                    PaymentDate = payment.PaymentDate;
                UpdatedAt = DateTime.Now;
            }
        }

        public void RevertPayment(decimal newPaidAmount, decimal newRemainingAmount, FinancingInstallmentStatus newStatus)
        {
            PaidAmount = newPaidAmount;
            RemainingAmount = newRemainingAmount;
            Status = newStatus;

            if (newPaidAmount <= 0)
            {
                PaymentDate = null;
            }

            UpdatedAt = DateTime.Now;
        }
    }
}