using Equilibrium.Web.Models.CreditCard;
using Equilibrium.Web.Models.Payment;

namespace Equilibrium.Web.Models.ViewModels
{
    public class CreditCardViewModel
    {
        public CreditCardModel CreditCard { get; set; }
        public List<PaymentModel> RecentPayments { get; set; } = new List<PaymentModel>();

        public decimal UsedPercentage => CreditCard.Limit > 0
            ? (CreditCard.Limit - CreditCard.AvailableLimit) / CreditCard.Limit * 100
            : 0;

        public string ColorClass
        {
            get
            {
                if (UsedPercentage > 75) return "danger";
                if (UsedPercentage > 50) return "warning";
                return "success";
            }
        }

        public DateTime NextClosingDate
        {
            get
            {
                var today = DateTime.Today;
                if (today.Day <= CreditCard.ClosingDay)
                {
                    return new DateTime(today.Year, today.Month, CreditCard.ClosingDay);
                }
                else
                {
                    var nextMonth = today.AddMonths(1);
                    return new DateTime(nextMonth.Year, nextMonth.Month, CreditCard.ClosingDay);
                }
            }
        }

        public DateTime NextDueDate
        {
            get
            {
                var today = DateTime.Today;
                if (today.Day <= CreditCard.DueDay)
                {
                    return new DateTime(today.Year, today.Month, CreditCard.DueDay);
                }
                else
                {
                    var nextMonth = today.AddMonths(1);
                    return new DateTime(nextMonth.Year, nextMonth.Month, CreditCard.DueDay);
                }
            }
        }

        public int DaysToClosing => (NextClosingDate - DateTime.Today).Days;
        public int DaysToDue => (NextDueDate - DateTime.Today).Days;

        public bool HasPayments => RecentPayments != null && RecentPayments.Count > 0;
    }
}