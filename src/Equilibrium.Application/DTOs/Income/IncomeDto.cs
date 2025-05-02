using Equilibrium.Application.DTOs.IncomeInstallment;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Income
{
    public class IncomeDto
    {
        public Guid Id { get; set; }
        public required string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public IncomeStatus Status { get; set; }
        public string StatusDescription => Status.ToString();
        public bool IsRecurring { get; set; }
        public required string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid UserId { get; set; }
        public Guid IncomeTypeId { get; set; }
        public required string IncomeTypeName { get; set; }

        public List<IncomeInstallmentDto> Installments { get; set; } = [];
    }
}