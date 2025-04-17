using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs
{
    public class IncomeInstallmentDto
    {
        public Guid Id { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public IncomeStatus Status { get; set; }
        public string StatusDescription => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid IncomeId { get; set; }
    }
}