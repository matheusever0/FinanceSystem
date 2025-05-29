using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Income
{
    public class IncomeFilter
    {
        public string? Description { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public IncomeStatus? Status { get; set; }
        public Guid? IncomeTypeId { get; set; }
        public bool? IsRecurring { get; set; }
        public bool? HasInstallments { get; set; }
        public DateTime? ReceivedStartDate { get; set; }
        public DateTime? ReceivedEndDate { get; set; }
        public string OrderBy { get; set; } = "DueDate";
        public bool Ascending { get; set; } = true;
        public bool IsValidMonth()
        {
            return !Month.HasValue || (Month >= 1 && Month <= 12);
        }
    }
}