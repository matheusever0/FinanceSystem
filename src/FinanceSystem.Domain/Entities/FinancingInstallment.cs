using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Entities
{
    public class FinancingInstallment
    {
        public Guid Id { get; protected set; }
        public int InstallmentNumber { get; protected set; }
        public decimal TotalAmount { get; protected set; }
        public decimal? TotalCorrection { get; protected set; } 
        public decimal InterestAmount { get; protected set; }
        public decimal AmortizationAmount { get; protected set; } 
        public DateTime DueDate { get; protected set; }  
        public DateTime? PaymentDate { get; protected set; }  
        public FinancingInstallmentStatus Status { get; protected set; }
        public decimal PaidAmount { get; protected set; } 
        public decimal RemainingAmount { get; protected set; }
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

        public void UpdateValues(decimal totalAmount)
        {
            TotalAmount = totalAmount;
            RemainingAmount = totalAmount - PaidAmount;
            TotalCorrection = 0;
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
                decimal principalPortion = (amountToApply / TotalAmount) * AmortizationAmount;
                PaidAmount += amountToApply;

                Financing.UpdateRemainingDebt(principalPortion);

                RemainingAmount = TotalAmount - PaidAmount;
            }
            else
            {
                PaidAmount += amountToApply;
                RemainingAmount -= amountToApply;
            }

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

            TotalCorrection = PaidAmount - (InterestAmount + AmortizationAmount);
        }

        public void RevertPayment(decimal newPaidAmount, decimal newRemainingAmount, FinancingInstallmentStatus newStatus)
        {
            PaidAmount = newPaidAmount;
            RemainingAmount = newRemainingAmount;
            Status = newStatus;
            TotalCorrection = 0 - (InterestAmount + AmortizationAmount);

            if (newPaidAmount <= 0)
            {
                PaymentDate = null;
            }

            UpdatedAt = DateTime.Now;
        }
    }
}