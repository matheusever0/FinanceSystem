using Equilibrium.Web.Models.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web;

namespace Equilibrium.Web.Helpers
{
    public static class FilterHelper
    {
        /// <summary>
        /// Constrói uma query string a partir de um filtro de pagamentos
        /// </summary>
        public static string BuildPaymentQueryString(PaymentFilter filter)
        {
            if (filter == null) return string.Empty;

            var parameters = new List<string>();

            // Filtros de texto
            AddParameter(parameters, "description", filter.Description);
            AddParameter(parameters, "notes", filter.Notes);

            // Filtros de valor
            AddParameter(parameters, "minAmount", filter.MinAmount);
            AddParameter(parameters, "maxAmount", filter.MaxAmount);

            // Filtros de data de vencimento
            AddParameter(parameters, "startDate", filter.StartDate?.ToString("yyyy-MM-dd"));
            AddParameter(parameters, "endDate", filter.EndDate?.ToString("yyyy-MM-dd"));

            // Filtros de mês/ano
            AddParameter(parameters, "month", filter.Month);
            AddParameter(parameters, "year", filter.Year);

            // Filtros de data de pagamento
            AddParameter(parameters, "paymentStartDate", filter.PaymentStartDate?.ToString("yyyy-MM-dd"));
            AddParameter(parameters, "paymentEndDate", filter.PaymentEndDate?.ToString("yyyy-MM-dd"));

            // Filtros de seleção
            AddParameter(parameters, "status", filter.Status);
            AddParameter(parameters, "paymentTypeId", filter.PaymentTypeId);
            AddParameter(parameters, "paymentMethodId", filter.PaymentMethodId);
            AddParameter(parameters, "financingId", filter.FinancingId);
            AddParameter(parameters, "financingInstallmentId", filter.FinancingInstallmentId);
            AddParameter(parameters, "creditCardId", filter.CreditCardId);

            // Filtros booleanos
            AddParameter(parameters, "isRecurring", filter.IsRecurring);
            AddParameter(parameters, "hasInstallments", filter.HasInstallments);
            AddParameter(parameters, "isFinancingPayment", filter.IsFinancingPayment);

            // Filtros de ordenação
            AddParameter(parameters, "orderBy", filter.OrderBy);
            AddParameter(parameters, "ascending", filter.Ascending);

            return string.Join("&", parameters);
        }

        /// <summary>
        /// Constrói uma query string a partir de um filtro de receitas
        /// </summary>
        public static string BuildIncomeQueryString(IncomeFilter filter)
        {
            if (filter == null) return string.Empty;

            var parameters = new List<string>();

            // Filtros de texto
            AddParameter(parameters, "description", filter.Description);

            // Filtros de valor
            AddParameter(parameters, "minAmount", filter.MinAmount);
            AddParameter(parameters, "maxAmount", filter.MaxAmount);

            // Filtros de data de vencimento
            AddParameter(parameters, "startDate", filter.StartDate?.ToString("yyyy-MM-dd"));
            AddParameter(parameters, "endDate", filter.EndDate?.ToString("yyyy-MM-dd"));

            // Filtros de mês/ano
            AddParameter(parameters, "month", filter.Month);
            AddParameter(parameters, "year", filter.Year);

            // Filtros de data de recebimento
            AddParameter(parameters, "receivedStartDate", filter.ReceivedStartDate?.ToString("yyyy-MM-dd"));
            AddParameter(parameters, "receivedEndDate", filter.ReceivedEndDate?.ToString("yyyy-MM-dd"));

            // Filtros de seleção
            AddParameter(parameters, "status", filter.Status);
            AddParameter(parameters, "incomeTypeId", filter.IncomeTypeId);

            // Filtros booleanos
            AddParameter(parameters, "isRecurring", filter.IsRecurring);
            AddParameter(parameters, "hasInstallments", filter.HasInstallments);

            // Filtros de ordenação
            AddParameter(parameters, "orderBy", filter.OrderBy);
            AddParameter(parameters, "ascending", filter.Ascending);

            return string.Join("&", parameters);
        }

        /// <summary>
        /// Converte filtros em Dictionary para facilitar uso com HttpClient
        /// </summary>
        public static Dictionary<string, string> PaymentFilterToDictionary(PaymentFilter filter)
        {
            var dict = new Dictionary<string, string>();

            if (filter == null) return dict;

            AddToDictionary(dict, "description", filter.Description);
            AddToDictionary(dict, "notes", filter.Notes);
            AddToDictionary(dict, "minAmount", filter.MinAmount);
            AddToDictionary(dict, "maxAmount", filter.MaxAmount);
            AddToDictionary(dict, "startDate", filter.StartDate?.ToString("yyyy-MM-dd"));
            AddToDictionary(dict, "endDate", filter.EndDate?.ToString("yyyy-MM-dd"));
            AddToDictionary(dict, "month", filter.Month);
            AddToDictionary(dict, "year", filter.Year);
            AddToDictionary(dict, "paymentStartDate", filter.PaymentStartDate?.ToString("yyyy-MM-dd"));
            AddToDictionary(dict, "paymentEndDate", filter.PaymentEndDate?.ToString("yyyy-MM-dd"));
            AddToDictionary(dict, "status", filter.Status);
            AddToDictionary(dict, "paymentTypeId", filter.PaymentTypeId);
            AddToDictionary(dict, "paymentMethodId", filter.PaymentMethodId);
            AddToDictionary(dict, "financingId", filter.FinancingId);
            AddToDictionary(dict, "financingInstallmentId", filter.FinancingInstallmentId);
            AddToDictionary(dict, "creditCardId", filter.CreditCardId);
            AddToDictionary(dict, "isRecurring", filter.IsRecurring);
            AddToDictionary(dict, "hasInstallments", filter.HasInstallments);
            AddToDictionary(dict, "isFinancingPayment", filter.IsFinancingPayment);
            AddToDictionary(dict, "orderBy", filter.OrderBy);
            AddToDictionary(dict, "ascending", filter.Ascending);

            return dict;
        }

        /// <summary>
        /// Converte filtros de receita em Dictionary
        /// </summary>
        public static Dictionary<string, string> IncomeFilterToDictionary(IncomeFilter filter)
        {
            var dict = new Dictionary<string, string>();

            if (filter == null) return dict;

            AddToDictionary(dict, "description", filter.Description);
            AddToDictionary(dict, "minAmount", filter.MinAmount);
            AddToDictionary(dict, "maxAmount", filter.MaxAmount);
            AddToDictionary(dict, "startDate", filter.StartDate?.ToString("yyyy-MM-dd"));
            AddToDictionary(dict, "endDate", filter.EndDate?.ToString("yyyy-MM-dd"));
            AddToDictionary(dict, "month", filter.Month);
            AddToDictionary(dict, "year", filter.Year);
            AddToDictionary(dict, "receivedStartDate", filter.ReceivedStartDate?.ToString("yyyy-MM-dd"));
            AddToDictionary(dict, "receivedEndDate", filter.ReceivedEndDate?.ToString("yyyy-MM-dd"));
            AddToDictionary(dict, "status", filter.Status);
            AddToDictionary(dict, "incomeTypeId", filter.IncomeTypeId);
            AddToDictionary(dict, "isRecurring", filter.IsRecurring);
            AddToDictionary(dict, "hasInstallments", filter.HasInstallments);
            AddToDictionary(dict, "orderBy", filter.OrderBy);
            AddToDictionary(dict, "ascending", filter.Ascending);

            return dict;
        }

        /// <summary>
        /// Cria filtros pré-definidos comuns para pagamentos
        /// </summary>
        public static class QuickFilters
        {
            public static PaymentFilter ThisMonth()
            {
                var now = DateTime.Now;
                return new PaymentFilter
                {
                    Month = now.Month,
                    Year = now.Year,
                    OrderBy = "dueDate",
                    Ascending = true
                };
            }

            public static PaymentFilter PendingPayments()
            {
                return new PaymentFilter
                {
                    Status = "Pending",
                    OrderBy = "dueDate",
                    Ascending = true
                };
            }

            public static PaymentFilter OverduePayments()
            {
                return new PaymentFilter
                {
                    Status = "Overdue",
                    OrderBy = "dueDate",
                    Ascending = true
                };
            }

            public static PaymentFilter ThisWeek()
            {
                var now = DateTime.Now;
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);

                return new PaymentFilter
                {
                    StartDate = startOfWeek.Date,
                    EndDate = endOfWeek.Date,
                    OrderBy = "dueDate",
                    Ascending = true
                };
            }

            public static PaymentFilter PaidThisMonth()
            {
                var now = DateTime.Now;
                return new PaymentFilter
                {
                    Status = "Paid",
                    Month = now.Month,
                    Year = now.Year,
                    OrderBy = "paymentDate",
                    Ascending = false
                };
            }

            public static IncomeFilter PendingIncomes()
            {
                return new IncomeFilter
                {
                    Status = "Pending",
                    OrderBy = "dueDate",
                    Ascending = true
                };
            }

            public static IncomeFilter ReceivedThisMonth()
            {
                var now = DateTime.Now;
                return new IncomeFilter
                {
                    Status = "Received",
                    Month = now.Month,
                    Year = now.Year,
                    OrderBy = "receivedDate",
                    Ascending = false
                };
            }

            public static IncomeFilter ThisMonthIncomes()
            {
                var now = DateTime.Now;
                return new IncomeFilter
                {
                    Month = now.Month,
                    Year = now.Year,
                    OrderBy = "dueDate",
                    Ascending = true
                };
            }

            public static IncomeFilter ThisWeekIncomes()
            {
                var now = DateTime.Now;
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);

                return new IncomeFilter
                {
                    StartDate = startOfWeek.Date,
                    EndDate = endOfWeek.Date,
                    OrderBy = "dueDate",
                    Ascending = true
                };
            }
        }

        /// <summary>
        /// Opções padrão para dropdowns
        /// </summary>
        public static class DropdownOptions
        {
            public static List<SelectListItem> GetPaymentOrderByOptions()
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "dueDate", Text = "Data de Vencimento", Selected = true },
                    new SelectListItem { Value = "description", Text = "Descrição" },
                    new SelectListItem { Value = "amount", Text = "Valor" },
                    new SelectListItem { Value = "paymentDate", Text = "Data de Pagamento" },
                    new SelectListItem { Value = "status", Text = "Status" },
                    new SelectListItem { Value = "createdAt", Text = "Data de Criação" }
                };
            }

            public static List<SelectListItem> GetIncomeOrderByOptions()
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "dueDate", Text = "Data de Vencimento", Selected = true },
                    new SelectListItem { Value = "description", Text = "Descrição" },
                    new SelectListItem { Value = "amount", Text = "Valor" },
                    new SelectListItem { Value = "receivedDate", Text = "Data de Recebimento" },
                    new SelectListItem { Value = "status", Text = "Status" },
                    new SelectListItem { Value = "createdAt", Text = "Data de Criação" }
                };
            }

            public static List<SelectListItem> GetPaymentStatusOptions()
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pending", Text = "Pendente" },
                    new SelectListItem { Value = "Paid", Text = "Pago" },
                    new SelectListItem { Value = "Overdue", Text = "Vencido" },
                    new SelectListItem { Value = "Cancelled", Text = "Cancelado" }
                };
            }

            public static List<SelectListItem> GetIncomeStatusOptions()
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pending", Text = "Pendente" },
                    new SelectListItem { Value = "Received", Text = "Recebido" },
                    new SelectListItem { Value = "Cancelled", Text = "Cancelado" }
                };
            }
        }

        // Métodos auxiliares privados
        private static void AddParameter(List<string> parameters, string name, object? value)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                var encodedValue = HttpUtility.UrlEncode(value.ToString());
                parameters.Add($"{name}={encodedValue}");
            }
        }

        private static void AddToDictionary(Dictionary<string, string> dict, string key, object? value)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                dict[key] = value.ToString()!;
            }
        }

        /// <summary>
        /// Popula filtro a partir de parâmetros da query string
        /// </summary>
        public static PaymentFilter PopulatePaymentFilterFromQuery(IQueryCollection query)
        {
            var filter = new PaymentFilter();

            filter.Description = query["description"].FirstOrDefault();
            filter.Notes = query["notes"].FirstOrDefault();

            if (decimal.TryParse(query["minAmount"], out var minAmount))
                filter.MinAmount = minAmount;

            if (decimal.TryParse(query["maxAmount"], out var maxAmount))
                filter.MaxAmount = maxAmount;

            if (DateTime.TryParse(query["startDate"], out var startDate))
                filter.StartDate = startDate;

            if (DateTime.TryParse(query["endDate"], out var endDate))
                filter.EndDate = endDate;

            if (int.TryParse(query["month"], out var month))
                filter.Month = month;

            if (int.TryParse(query["year"], out var year))
                filter.Year = year;

            if (DateTime.TryParse(query["paymentStartDate"], out var paymentStartDate))
                filter.PaymentStartDate = paymentStartDate;

            if (DateTime.TryParse(query["paymentEndDate"], out var paymentEndDate))
                filter.PaymentEndDate = paymentEndDate;

            filter.Status = query["status"].FirstOrDefault();
            filter.PaymentTypeId = query["paymentTypeId"].FirstOrDefault();
            filter.PaymentMethodId = query["paymentMethodId"].FirstOrDefault();
            filter.FinancingId = query["financingId"].FirstOrDefault();
            filter.CreditCardId = query["creditCardId"].FirstOrDefault();

            if (bool.TryParse(query["isRecurring"], out var isRecurring))
                filter.IsRecurring = isRecurring;

            if (bool.TryParse(query["hasInstallments"], out var hasInstallments))
                filter.HasInstallments = hasInstallments;

            filter.OrderBy = query["orderBy"].FirstOrDefault() ?? "dueDate";

            if (bool.TryParse(query["ascending"], out var ascending))
                filter.Ascending = ascending;

            return filter;
        }

        /// <summary>
        /// Limpa filtros mantendo apenas ordenação
        /// </summary>
        public static T ClearFiltersKeepOrder<T>(T filter) where T : BaseFilter, new()
        {
            var orderBy = filter.OrderBy;
            var ascending = filter.Ascending;

            if (filter is PaymentFilter paymentFilter)
            {
                paymentFilter.Clear();
                paymentFilter.OrderBy = orderBy;
                paymentFilter.Ascending = ascending;
            }
            else if (filter is IncomeFilter incomeFilter)
            {
                incomeFilter.Clear();
                incomeFilter.OrderBy = orderBy;
                incomeFilter.Ascending = ascending;
            }

            return filter;
        }

        /// <summary>
        /// Verifica se dois filtros são equivalentes
        /// </summary>
        public static bool AreFiltersEqual(PaymentFilter filter1, PaymentFilter filter2)
        {
            if (filter1 == null && filter2 == null) return true;
            if (filter1 == null || filter2 == null) return false;

            return filter1.Description == filter2.Description &&
                   filter1.Notes == filter2.Notes &&
                   filter1.MinAmount == filter2.MinAmount &&
                   filter1.MaxAmount == filter2.MaxAmount &&
                   filter1.StartDate == filter2.StartDate &&
                   filter1.EndDate == filter2.EndDate &&
                   filter1.Month == filter2.Month &&
                   filter1.Year == filter2.Year &&
                   filter1.Status == filter2.Status &&
                   filter1.PaymentTypeId == filter2.PaymentTypeId &&
                   filter1.PaymentMethodId == filter2.PaymentMethodId &&
                   filter1.IsRecurring == filter2.IsRecurring;
        }
    }
}