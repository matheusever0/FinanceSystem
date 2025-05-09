using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.DTOs.Filters
{
    public class IncomeInstallmentFilter : BaseFilter
    {
        public int? InstallmentNumber { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public DateTime? ReceivedDateFrom { get; set; }
        public DateTime? ReceivedDateTo { get; set; }
        public IncomeStatus? Status { get; set; }
        public Guid? IncomeId { get; set; }
    }
}
