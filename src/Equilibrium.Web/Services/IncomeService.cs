using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Income;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Helpers;
using Equilibrium.Web.Models.Generics;

namespace Equilibrium.Web.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly IApiService _apiService;

        public IncomeService(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Método principal com filtros avançados
        public async Task<IEnumerable<IncomeModel>> GetFilteredIncomesAsync(IncomeFilter filter, string token)
        {
            var queryParams = FilterHelper.BuildIncomeQueryString(filter);
            var endpoint = string.IsNullOrEmpty(queryParams)
                ? "/api/incomes"
                : $"/api/incomes?{queryParams}";

            return await _apiService.GetAsync<IEnumerable<IncomeModel>>(endpoint, token);
        }

        // Método para paginação
        public async Task<PagedResult<IncomeModel>> GetPagedIncomesAsync(IncomeFilter filter, string token)
        {
            var queryParams = FilterHelper.BuildIncomeQueryString(filter);
            var endpoint = string.IsNullOrEmpty(queryParams)
                ? "/api/incomes"
                : $"/api/incomes?{queryParams}";

            return await _apiService.GetAsync<PagedResult<IncomeModel>>(endpoint, token);
        }

        // Métodos existentes mantidos para compatibilidade
        public async Task<IEnumerable<IncomeModel>> GetAllIncomesAsync(string token)
        {
            return await _apiService.GetAsync<IEnumerable<IncomeModel>>("/api/incomes", token);
        }

        public async Task<IncomeModel> GetIncomeByIdAsync(string id, string token)
        {
            return await _apiService.GetAsync<IncomeModel>($"/api/incomes/{id}", token);
        }

        public async Task<IEnumerable<IncomeModel>> GetIncomesByMonthAsync(int month, int year, string token)
        {
            var filter = new IncomeFilter
            {
                Month = month,
                Year = year,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredIncomesAsync(filter, token);
        }

        public async Task<IEnumerable<IncomeModel>> GetPendingIncomesAsync(string token)
        {
            var filter = FilterHelper.QuickFilters.PendingIncomes();
            return await GetFilteredIncomesAsync(filter, token);
        }

        public async Task<IEnumerable<IncomeModel>> GetReceivedIncomesAsync(string token)
        {
            var filter = FilterHelper.QuickFilters.ReceivedThisMonth();
            return await GetFilteredIncomesAsync(filter, token);
        }

        public async Task<IEnumerable<IncomeModel>> GetOverdueIncomesAsync(string token)
        {
            var filter = new IncomeFilter
            {
                Status = "Overdue",
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredIncomesAsync(filter, token);
        }

        public async Task<IEnumerable<IncomeModel>> GetIncomesByTypeAsync(string typeId, string token)
        {
            var filter = new IncomeFilter
            {
                IncomeTypeId = typeId,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredIncomesAsync(filter, token);
        }

        // Novos métodos usando filtros avançados
        public async Task<IEnumerable<IncomeModel>> GetIncomesByDateRangeAsync(DateTime startDate, DateTime endDate, string token)
        {
            var filter = new IncomeFilter
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredIncomesAsync(filter, token);
        }

        public async Task<IEnumerable<IncomeModel>> GetIncomesByAmountRangeAsync(decimal minAmount, decimal maxAmount, string token)
        {
            var filter = new IncomeFilter
            {
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                OrderBy = "amount",
                Ascending = false
            };
            return await GetFilteredIncomesAsync(filter, token);
        }

        public async Task<IEnumerable<IncomeModel>> GetRecurringIncomesAsync(string token)
        {
            var filter = new IncomeFilter
            {
                IsRecurring = true,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredIncomesAsync(filter, token);
        }

        public async Task<IEnumerable<IncomeModel>> SearchIncomesAsync(string searchTerm, string token)
        {
            var filter = new IncomeFilter
            {
                Description = searchTerm,
                OrderBy = "dueDate",
                Ascending = true
            };
            return await GetFilteredIncomesAsync(filter, token);
        }

        public async Task<IEnumerable<IncomeModel>> GetIncomesByReceivedDateRangeAsync(DateTime startDate, DateTime endDate, string token)
        {
            var filter = new IncomeFilter
            {
                ReceivedStartDate = startDate,
                ReceivedEndDate = endDate,
                Status = "Received",
                OrderBy = "receivedDate",
                Ascending = false
            };
            return await GetFilteredIncomesAsync(filter, token);
        }

        // Métodos de ação (CRUD)
        public async Task<IncomeModel> CreateIncomeAsync(CreateIncomeModel model, string token)
        {
            return await _apiService.PostAsync<IncomeModel>("/api/incomes", model, token);
        }

        public async Task<IncomeModel> UpdateIncomeAsync(string id, UpdateIncomeModel model, string token)
        {
            return await _apiService.PutAsync<IncomeModel>($"/api/incomes/{id}", model, token);
        }

        public async Task DeleteIncomeAsync(string id, string token)
        {
            await _apiService.DeleteAsync($"/api/incomes/{id}", token);
        }

        public async Task<IncomeModel> MarkAsReceivedAsync(string id, DateTime? receivedDate, string token)
        {
            var data = receivedDate.HasValue ? new { receivedDate = receivedDate.Value } : null;
            return await _apiService.PostAsync<IncomeModel>($"/api/incomes/{id}/received", data, token);
        }

        public async Task<IncomeModel> CancelIncomeAsync(string id, string token)
        {
            return await _apiService.PostAsync<IncomeModel>($"/api/incomes/{id}/cancel", null, token);
        }

        // Métodos para parcelas
        public async Task<string> GetInstallmentParentIncomeAsync(string installmentId, string token)
        {
            return await _apiService.GetAsync<string>($"/api/incomeinstallments/{installmentId}/parent", token);
        }

        public async Task<bool> MarkInstallmentAsReceivedAsync(string installmentId, DateTime receivedDate, string token)
        {
            var data = new { receivedDate };
            await _apiService.PostAsync<object>($"/api/incomeinstallments/{installmentId}/received", data, token);
            return true;
        }

        public async Task<bool> CancelInstallmentAsync(string installmentId, string token)
        {
            await _apiService.PostAsync<object>($"/api/incomeinstallments/{installmentId}/cancel", null, token);
            return true;
        }

        // Métodos estatísticos
        public async Task<decimal> GetTotalIncomesByPeriodAsync(int month, int year, string token)
        {
            var filter = new IncomeFilter
            {
                Month = month,
                Year = year,
                Status = "Received"
            };

            var incomes = await GetFilteredIncomesAsync(filter, token);
            return incomes.Sum(i => i.Amount);
        }

        public async Task<decimal> GetPendingIncomesTotalAsync(string token)
        {
            var filter = FilterHelper.QuickFilters.PendingIncomes();
            var incomes = await GetFilteredIncomesAsync(filter, token);
            return incomes.Sum(i => i.Amount);
        }

        public async Task<decimal> GetReceivedIncomesTotalAsync(string token)
        {
            var filter = new IncomeFilter
            {
                Status = "Received"
            };
            var incomes = await GetFilteredIncomesAsync(filter, token);
            return incomes.Sum(i => i.Amount);
        }

        // Métodos para relatórios
        public async Task<Dictionary<string, decimal>> GetIncomesByTypeAsync(int month, int year, string token)
        {
            var filter = new IncomeFilter
            {
                Month = month,
                Year = year,
                Status = "Received"
            };

            var incomes = await GetFilteredIncomesAsync(filter, token);
            return incomes
                .GroupBy(i => i.IncomeTypeName)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Amount));
        }

        public async Task<List<IncomeModel>> GetRecentIncomesAsync(int count, string token)
        {
            var filter = new IncomeFilter
            {
                OrderBy = "createdAt",
                Ascending = false
            };

            var incomes = await GetFilteredIncomesAsync(filter, token);
            return incomes.Take(count).ToList();
        }

        public async Task<List<IncomeModel>> GetUpcomingIncomesAsync(int days, string token)
        {
            var endDate = DateTime.Today.AddDays(days);
            var filter = new IncomeFilter
            {
                StartDate = DateTime.Today,
                EndDate = endDate,
                Status = "Pending",
                OrderBy = "dueDate",
                Ascending = true
            };

            return (await GetFilteredIncomesAsync(filter, token)).ToList();
        }

        // Métodos para análise de tendências
        public async Task<Dictionary<string, decimal>> GetMonthlyIncomeAnalysisAsync(int year, string token)
        {
            var monthlyData = new Dictionary<string, decimal>();

            for (int month = 1; month <= 12; month++)
            {
                var monthName = new DateTime(year, month, 1).ToString("MMM");
                try
                {
                    var total = await GetTotalIncomesByPeriodAsync(month, year, token);
                    monthlyData[monthName] = total;
                }
                catch
                {
                    monthlyData[monthName] = 0;
                }
            }

            return monthlyData;
        }

        public async Task<decimal> GetAverageMonthlyIncomeAsync(int year, string token)
        {
            var monthlyData = await GetMonthlyIncomeAnalysisAsync(year, token);
            var activeMonths = monthlyData.Values.Count(v => v > 0);

            return activeMonths > 0 ? monthlyData.Values.Sum() / activeMonths : 0;
        }

        // Método para comparação de períodos
        public async Task<(decimal currentPeriod, decimal previousPeriod, decimal percentageChange)>
            ComparePeriodsAsync(DateTime currentStart, DateTime currentEnd, string token)
        {
            var periodDays = (currentEnd - currentStart).Days;
            var previousStart = currentStart.AddDays(-periodDays);
            var previousEnd = currentStart.AddDays(-1);

            var currentFilter = new IncomeFilter
            {
                StartDate = currentStart,
                EndDate = currentEnd,
                Status = "Received"
            };

            var previousFilter = new IncomeFilter
            {
                StartDate = previousStart,
                EndDate = previousEnd,
                Status = "Received"
            };

            var currentIncomes = await GetFilteredIncomesAsync(currentFilter, token);
            var previousIncomes = await GetFilteredIncomesAsync(previousFilter, token);

            var currentTotal = currentIncomes.Sum(i => i.Amount);
            var previousTotal = previousIncomes.Sum(i => i.Amount);

            var percentageChange = previousTotal > 0
                ? ((currentTotal - previousTotal) / previousTotal) * 100
                : 0;

            return (currentTotal, previousTotal, percentageChange);
        }
    }
}