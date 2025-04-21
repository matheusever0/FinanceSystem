using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Entities
{
    public class IncomeInstallment
    {
        public Guid Id { get; protected set; }
        public int InstallmentNumber { get; protected set; }
        public decimal Amount { get; protected set; }
        public DateTime DueDate { get; protected set; }
        public DateTime? ReceivedDate { get; protected set; }
        public IncomeStatus Status { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        public Guid IncomeId { get; protected set; }
        public Income Income { get; protected set; }

        protected IncomeInstallment() { }

        public IncomeInstallment(
            Income income,
            int installmentNumber,
            decimal amount,
            DateTime dueDate)
        {
            Id = Guid.NewGuid();
            InstallmentNumber = installmentNumber;
            Amount = amount;
            DueDate = dueDate;
            Status = IncomeStatus.Pending;
            CreatedAt = DateTime.Now;

            IncomeId = income.Id;
            Income = income;
        }

        public void MarkAsReceived(DateTime receivedDate)
        {
            ReceivedDate = receivedDate;
            Status = IncomeStatus.Received;
            UpdatedAt = DateTime.Now;
        }

        public void Cancel()
        {
            Status = IncomeStatus.Cancelled;
            UpdatedAt = DateTime.Now;
        }
    }
}