using Equilibrium.Web.Models.Filters;
using System.Text.Json;

namespace Equilibrium.Web.Helpers
{
    public static class FilterCacheHelper
    {
        private const string PAYMENT_FILTER_KEY = "PaymentFilter";
        private const string INCOME_FILTER_KEY = "IncomeFilter";

        public static void SavePaymentFilter(ISession session, PaymentFilter filter)
        {
            if (filter != null && filter.HasFilters())
            {
                var json = JsonSerializer.Serialize(filter);
                session.SetString(PAYMENT_FILTER_KEY, json);
            }
            else
            {
                session.Remove(PAYMENT_FILTER_KEY);
            }
        }

        public static PaymentFilter? GetPaymentFilter(ISession session)
        {
            var json = session.GetString(PAYMENT_FILTER_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    return JsonSerializer.Deserialize<PaymentFilter>(json) ?? new PaymentFilter();
                }
                catch
                {
                    session.Remove(PAYMENT_FILTER_KEY);
                }
            }
            return null;
        }

        public static void SaveIncomeFilter(ISession session, IncomeFilter filter)
        {
            if (filter != null && filter.HasFilters())
            {
                var json = JsonSerializer.Serialize(filter);
                session.SetString(INCOME_FILTER_KEY, json);
            }
            else
            {
                session.Remove(INCOME_FILTER_KEY);
            }
        }

        public static IncomeFilter? GetIncomeFilter(ISession session)
        {
            var json = session.GetString(INCOME_FILTER_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    return JsonSerializer.Deserialize<IncomeFilter>(json) ?? new IncomeFilter();
                }
                catch
                {
                    session.Remove(INCOME_FILTER_KEY);
                }
            }
            return null;
        }

        public static bool HasPaymentFilter(ISession session)
        {
            var filter = GetPaymentFilter(session);
            return filter.HasFilters();
        }

        public static bool HasIncomeFilter(ISession session)
        {
            var filter = GetIncomeFilter(session);
            return filter.HasFilters();
        }

        public static void ClearPaymentFilter(ISession session)
        {
            session.Remove(PAYMENT_FILTER_KEY);
        }

        public static void ClearIncomeFilter(ISession session)
        {
            session.Remove(INCOME_FILTER_KEY);
        }
    }
}