using Equilibrium.Web.Models.Enums;
using Equilibrium.Web.Models.Enums.Extensions;
using Equilibrium.Web.Models.Payment;
using Equilibrium.Web.Models.PaymentInstallment;

namespace Equilibrium.Web.Models.ViewModels
{
    public class PaymentViewModel
    {
        public PaymentModel Payment { get; set; }
        public List<PaymentInstallmentModel> Installments { get; set; } = new List<PaymentInstallmentModel>();

        public string StatusBadgeClass => ((StatusType)Payment.Status).GetStatusBadgeClass();
        public string StatusDescription => ((StatusType)Payment.Status).GetStatusDescription();

        public bool HasInstallments => Installments.Any();
        public bool CanBePaid => Payment.Status == (int)StatusType.Pending || Payment.Status == (int)StatusType.Overdue;
        public bool CanBeCanceled => Payment.Status != (int)StatusType.Canceled;

        public DateTime TodayDate => DateTime.Today;

        public string InstallmentStatusBadgeClass(int status)
        {
            return ((StatusType)status).GetStatusBadgeClass();
        }
    }
}