using Equilibrium.Web.Models.Enums;
using Equilibrium.Web.Models.Enums.Extensions;
using Equilibrium.Web.Models.Income;
using Equilibrium.Web.Models.IncomeInstallment;

namespace Equilibrium.Web.Models.ViewModels
{
    public class IncomeViewModel
    {
        public IncomeModel Income { get; set; }
        public List<IncomeInstallmentModel> Installments { get; set; } = new List<IncomeInstallmentModel>();

        public string StatusBadgeClass => ((StatusType)Income.Status).GetStatusBadgeClass();
        public string StatusDescription => ((StatusType)Income.Status).GetStatusDescription();

        public bool IsPending => Income.Status == (int)StatusType.Pending;
        public bool CanBeReceived => Income.Status == (int)StatusType.Pending;
        public bool CanBeCanceled => Income.Status != (int)StatusType.Canceled;

        public bool HasInstallments => Installments.Any();

        public int DaysUntilDue
        {
            get
            {
                return (int)Math.Ceiling((Income.DueDate - DateTime.Now).TotalDays);
            }
        }

        public bool IsDueToday => DaysUntilDue == 0;
        public bool IsOverdue => DaysUntilDue < 0;

        public string InstallmentStatusBadgeClass(int status)
        {
            return ((StatusType)status).GetStatusBadgeClass();
        }
    }
}