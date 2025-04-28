using FinanceSystem.Domain.Enums;
using System;

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

        public void MarkAsPaid(DateTime paymentDate)
        {
            PaymentDate = paymentDate;
            Status = FinancingInstallmentStatus.Paid;
            PaidAmount = TotalAmount;
            RemainingAmount = 0;

            UpdatedAt = DateTime.Now;
        }

        public void MarkAsPartiallyPaid(decimal amount, DateTime paymentDate)
        {
            if (amount <= 0 || amount >= RemainingAmount)
                throw new InvalidOperationException("Invalid amount for partial payment");

            PaidAmount += amount;
            RemainingAmount -= amount;

            Status = FinancingInstallmentStatus.PartiallyPaid;

            if (PaymentDate == null || paymentDate > PaymentDate)
                PaymentDate = paymentDate;

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

        public void AddPayment(Payment payment)
        {
            Payments.Add(payment);

            if (payment.Amount >= RemainingAmount)
            {
                MarkAsPaid(payment.PaymentDate ?? DateTime.Now);
            }
            else
            {
                MarkAsPartiallyPaid(payment.Amount, payment.PaymentDate ?? DateTime.Now);
            }
        }
    }
}