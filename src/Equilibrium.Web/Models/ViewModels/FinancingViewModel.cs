using Equilibrium.Web.Models.Enums;
using Equilibrium.Web.Models.Enums.Extensions;
using Equilibrium.Web.Models.Financing;
using System;

namespace Equilibrium.Web.Models.ViewModels
{
    public class FinancingViewModel
    {
        public FinancingDetailModel Financing { get; set; }

        public string StatusBadgeClass => ((StatusType)Financing.Status).GetStatusBadgeClass();
        public string StatusDescription => ((StatusType)Financing.Status).GetStatusDescription();
        public string TypeDescription => ((FinancingType)Financing.Type).GetFinancingTypeDescription();
        public string CorrectionIndexDescription => ((CorrectionIndexType)Financing.CorrectionIndex).GetCorrectionIndexDescription();

        public string ProgressBarClass
        {
            get
            {
                var percentage = Financing.ProgressPercentage;
                if (percentage >= 90) return "bg-success";
                if (percentage >= 60) return "bg-info";
                if (percentage >= 30) return "bg-warning";
                return "bg-danger";
            }
        }

        public bool IsActive => Financing.Status == (int)StatusType.Pending;
        public bool CanBeCanceled => Financing.Status == (int)StatusType.Pending;
        public bool CanBeCompleted => Financing.Status == (int)StatusType.Pending;

        public decimal TotalPaidPercentage => Financing.TotalAmount > 0 ? Financing.TotalPaid / Financing.TotalAmount * 100 : 0;

        public string InstallmentStatusBadgeClass(int status)
        {
            return ((StatusType)status).GetStatusBadgeClass();
        }

        public string PaymentStatusBadgeClass(int status)
        {
            return ((StatusType)status).GetStatusBadgeClass();
        }
    }
}