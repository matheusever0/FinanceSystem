using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Payment;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Helpers;
using Equilibrium.Web.Models.Generics;

namespace Equilibrium.Web.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IApiService _apiService;

        public PaymentService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IEnumerable<PaymentModel>> GetFilteredPaymentsAsync(PaymentFilter filter, string token)
        {
            var queryParams = FilterHelper.BuildPaymentQueryString(filter);
            var endpoint = string.IsNullOrEmpty(queryParams)
                ? "/api/payments"
                : $"/api/payments?{queryParams}";

            return await _apiService.GetAsync<IEnumerable<PaymentModel>>(endpoint, token);
        }

        public async Task<PagedResult<PaymentModel>> GetPagedPaymentsAsync(PaymentFilter filter, string token)
        {
            var queryParams = FilterHelper.BuildPaymentQueryString(filter);
            var endpoint = string.IsNullOrEmpty(queryParams)
                ? "/api/payments"
                : $"/api/payments?{queryParams}";

            return await _apiService.GetAsync<PagedResult<PaymentModel>>(endpoint, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetAllPaymentsAsync(string token)
        {
            return await _apiService.GetAsync<IEnumerable<PaymentModel>>("/api/payments", token);
        }

        public async Task<PaymentModel> GetPaymentByIdAsync(string id, string token)
        {
            return await _apiService.GetAsync<PaymentModel>($"/api/payments/{id}", token);
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByMonthAsync(int month, int year, string token)
        {
            var filter = new PaymentFilter
            {
                Month = month,
                Year = year,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetPendingPaymentsAsync(string token)
        {
            var filter = FilterHelper.QuickFilters.PendingPayments();
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetOverduePaymentsAsync(string token)
        {
            var filter = FilterHelper.QuickFilters.OverduePayments();
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByTypeAsync(string typeId, string token)
        {
            var filter = new PaymentFilter
            {
                PaymentTypeId = typeId,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByMethodAsync(string methodId, string token)
        {
            var filter = new PaymentFilter
            {
                PaymentMethodId = methodId,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate, string token)
        {
            var filter = new PaymentFilter
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByAmountRangeAsync(decimal minAmount, decimal maxAmount, string token)
        {
            var filter = new PaymentFilter
            {
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                OrderBy = "amount",
                Ascending = false
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetRecurringPaymentsAsync(string token)
        {
            var filter = new PaymentFilter
            {
                IsRecurring = true,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByCreditCardAsync(string creditCardId, string token)
        {
            var filter = new PaymentFilter
            {
                CreditCardId = creditCardId,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> GetFinancingPaymentsAsync(string financingId, string token)
        {
            var filter = new PaymentFilter
            {
                FinancingId = financingId,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        public async Task<IEnumerable<PaymentModel>> SearchPaymentsAsync(string searchTerm, string token)
        {
            var filter = new PaymentFilter
            {
                Description = searchTerm,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredPaymentsAsync(filter, token);
        }

        // Métodos de ação (CRUD)
        public async Task<PaymentModel> CreatePaymentAsync(CreatePaymentModel model, string token)
        {
            return await _apiService.PostAsync<PaymentModel>("/api/payments", model, token);
        }

        public async Task<PaymentModel> UpdatePaymentAsync(string id, UpdatePaymentModel model, string token)
        {
            return await _apiService.PutAsync<PaymentModel>($"/api/payments/{id}", model, token);
        }

        public async Task DeletePaymentAsync(string id, string token)
        {
            await _apiService.DeleteAsync($"/api/payments/{id}", token);
        }

        public async Task<PaymentModel> MarkAsPaidAsync(string id, DateTime? paymentDate, string token)
        {
            return await _apiService.PostAsync<PaymentModel>($"/api/payments/{id}/paid", paymentDate, token);
        }

        public async Task<PaymentModel> MarkAsOverdueAsync(string id, string token)
        {
            return await _apiService.PostAsync<PaymentModel>($"/api/payments/{id}/overdue", null, token);
        }

        public async Task<PaymentModel> CancelPaymentAsync(string id, string token)
        {
            return await _apiService.PostAsync<PaymentModel>($"/api/payments/{id}/cancel", null, token);
        }

        // Métodos para parcelas
        public async Task<string> GetInstallmentParentPaymentAsync(string installmentId, string token)
        {
            // Este método pode precisar ser implementado na API ou através de uma consulta específica
            return await _apiService.GetAsync<string>($"/api/paymentinstallments/{installmentId}/parent", token);
        }

        public async Task<bool> MarkInstallmentAsPaidAsync(string installmentId, DateTime paymentDate, string token)
        {
            var data = new { paymentDate };
            await _apiService.PostAsync<object>($"/api/paymentinstallments/{installmentId}/paid", data, token);
            return true;
        }

        public async Task<bool> MarkInstallmentAsOverdueAsync(string installmentId, string token)
        {
            await _apiService.PostAsync<object>($"/api/paymentinstallments/{installmentId}/overdue", null, token);
            return true;
        }

        public async Task<bool> CancelInstallmentAsync(string installmentId, string token)
        {
            await _apiService.PostAsync<object>($"/api/paymentinstallments/{installmentId}/cancel", null, token);
            return true;
        }

        // Métodos estatísticos
        public async Task<decimal> GetTotalPaymentsByPeriodAsync(int month, int year, string token)
        {
            var filter = new PaymentFilter
            {
                Month = month,
                Year = year,
                Status = "Paid"
            };

            var payments = await GetFilteredPaymentsAsync(filter, token);
            return payments.Sum(p => p.Amount);
        }

        public async Task<decimal> GetPendingPaymentsTotalAsync(string token)
        {
            var filter = FilterHelper.QuickFilters.PendingPayments();
            var payments = await GetFilteredPaymentsAsync(filter, token);
            return payments.Sum(p => p.Amount);
        }

        public async Task<decimal> GetOverduePaymentsTotalAsync(string token)
        {
            var filter = FilterHelper.QuickFilters.OverduePayments();
            var payments = await GetFilteredPaymentsAsync(filter, token);
            return payments.Sum(p => p.Amount);
        }

        // Métodos para relatórios
        public async Task<Dictionary<string, decimal>> GetPaymentsByTypeAsync(int month, int year, string token)
        {
            var filter = new PaymentFilter
            {
                Month = month,
                Year = year,
                Status = "Paid"
            };

            var payments = await GetFilteredPaymentsAsync(filter, token);
            return payments
                .GroupBy(p => p.PaymentTypeName)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
        }

        public async Task<Dictionary<string, decimal>> GetPaymentsByMethodAsync(int month, int year, string token)
        {
            var filter = new PaymentFilter
            {
                Month = month,
                Year = year,
                Status = "Paid"
            };

            var payments = await GetFilteredPaymentsAsync(filter, token);
            return payments
                .GroupBy(p => p.PaymentMethodName)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
        }
    }
}