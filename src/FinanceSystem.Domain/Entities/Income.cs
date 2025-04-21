using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Entities
{
    public class Income
    {
        public Guid Id { get; protected set; }
        public string Description { get; protected set; }
        public decimal Amount { get; protected set; }
        public DateTime DueDate { get; protected set; }
        public DateTime? ReceivedDate { get; protected set; }
        public IncomeStatus Status { get; protected set; }
        public bool IsRecurring { get; protected set; }
        public string Notes { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        public Guid UserId { get; protected set; }
        public User User { get; protected set; }

        public Guid IncomeTypeId { get; protected set; }
        public IncomeType IncomeType { get; protected set; }

        public ICollection<IncomeInstallment> Installments { get; protected set; }

        protected Income()
        {
            Installments = new List<IncomeInstallment>();
        }

        public Income(
            string description,
            decimal amount,
            DateTime dueDate,
            IncomeType incomeType,
            User user,
            bool isRecurring = false,
            string notes = "")
        {
            Id = Guid.NewGuid();
            Description = description;
            Amount = amount;
            DueDate = dueDate;
            Status = IncomeStatus.Pending;
            IsRecurring = isRecurring;
            Notes = notes;
            CreatedAt = DateTime.Now;

            IncomeTypeId = incomeType.Id;
            IncomeType = incomeType;

            UserId = user.Id;
            User = user;

            Installments = new List<IncomeInstallment>();
        }

        public void MarkAsReceived(DateTime receivedDate)
        {
            ReceivedDate = receivedDate;
            Status = IncomeStatus.Received;
            UpdateUpdatedAt();
        }

        public void Cancel()
        {
            Status = IncomeStatus.Cancelled;
            UpdateUpdatedAt();
        }

        public void Pending()
        {
            ReceivedDate = null;
            Status = IncomeStatus.Pending;
            UpdateUpdatedAt();
        }

        public void AddInstallments(int numberOfInstallments)
        {
            if (numberOfInstallments <= 1)
                return;

            decimal installmentAmount = Math.Round(Amount / numberOfInstallments, 2);
            decimal remainingAmount = Amount - (installmentAmount * (numberOfInstallments - 1));

            for (int i = 0; i < numberOfInstallments; i++)
            {
                var amount = i == numberOfInstallments - 1 ? remainingAmount : installmentAmount;
                var dueDate = DueDate.AddMonths(i);

                var installment = new IncomeInstallment(this, i + 1, amount, dueDate);
                Installments.Add(installment);
            }

            UpdateUpdatedAt();
        }

        public void UpdateDescription(string description)
        {
            Description = description;
            UpdateUpdatedAt();
        }

        public void UpdateAmount(decimal amount)
        {
            Amount = amount;
            UpdateUpdatedAt();
        }

        public void UpdateDueDate(DateTime dueDate)
        {
            DueDate = dueDate;
            UpdateUpdatedAt();
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
            UpdateUpdatedAt();
        }

        public void UpdateType(IncomeType incomeType)
        {
            IncomeType = incomeType;
            UpdateUpdatedAt();
        }
        public void UpdateRecurring(bool recurring)
        {
            IsRecurring = recurring;
            UpdateUpdatedAt();
        }

        private void UpdateUpdatedAt()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}