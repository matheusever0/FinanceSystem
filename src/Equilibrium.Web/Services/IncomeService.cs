using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Income;

namespace Equilibrium.Web.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<IncomeService> _logger;

        public IncomeService(IApiService apiService, ILogger<IncomeService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<IncomeModel>> GetAllIncomesAsync(string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>("/api/Incomes", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receitas");
                throw;
            }
        }

        public async Task<IncomeModel> GetIncomeByIdAsync(string id, string token)
        {
            try
            {
                return await _apiService.GetAsync<IncomeModel>($"/api/Incomes/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receita com ID: {IncomeId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<IncomeModel>> GetIncomesByMonthAsync(int month, int year, string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>($"/api/Incomes/month/{year}/{month}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receitas do mês {Month}/{Year}", month, year);
                throw;
            }
        }

        public async Task<IEnumerable<IncomeModel>> GetPendingIncomesAsync(string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>("/api/Incomes/pending", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receitas pendentes");
                throw;
            }
        }

        public async Task<IEnumerable<IncomeModel>> GetReceivedIncomesAsync(string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>("/api/Incomes/received", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receitas recebidas");
                throw;
            }
        }

        public async Task<IEnumerable<IncomeModel>> GetIncomesByTypeAsync(string typeId, string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>($"/api/Incomes/type/{typeId}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receitas por tipo: {TypeId}", typeId);
                throw;
            }
        }

        public async Task<IncomeModel> CreateIncomeAsync(CreateIncomeModel model, string token)
        {
            try
            {
                return await _apiService.PostAsync<IncomeModel>("/api/Incomes", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar receita: {Description}", model.Description);
                throw;
            }
        }

        public async Task<IncomeModel> UpdateIncomeAsync(string id, UpdateIncomeModel model, string token)
        {
            try
            {
                return await _apiService.PutAsync<IncomeModel>($"/api/Incomes/{id}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar receita com ID: {IncomeId}", id);
                throw;
            }
        }

        public async Task DeleteIncomeAsync(string id, string token)
        {
            try
            {
                await _apiService.DeleteAsync($"/api/Incomes/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir receita com ID: {IncomeId}", id);
                throw;
            }
        }

        public async Task<IncomeModel> MarkAsReceivedAsync(string id, DateTime? receivedDate, string token)
        {
            try
            {
                return await _apiService.PostAsync<IncomeModel>($"/api/Incomes/{id}/received", receivedDate, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar receita como recebida: {IncomeId}", id);
                throw;
            }
        }

        public async Task<IncomeModel> CancelIncomeAsync(string id, string token)
        {
            try
            {
                return await _apiService.PostAsync<IncomeModel>($"/api/Incomes/{id}/cancel", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar receita: {IncomeId}", id);
                throw;
            }
        }

        public async Task<string> GetInstallmentParentIncomeAsync(string installmentId, string token)
        {
            try
            {
                var incomes = await GetAllIncomesAsync(token);

                foreach (var income in incomes)
                {
                    if (income.Installments != null && income.Installments.Any(i => i.Id == installmentId))
                    {
                        return income.Id;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receita pai da parcela: {InstallmentId}", installmentId);
                throw;
            }
        }

        public async Task<bool> MarkInstallmentAsReceivedAsync(string installmentId, DateTime receivedDate, string token)
        {
            try
            {
                return await _apiService.PostAsync<bool>($"/api/IncomeInstallments/{installmentId}/received", receivedDate, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar parcela como recebida: {InstallmentId}", installmentId);
                throw;
            }
        }

        public async Task<bool> CancelInstallmentAsync(string installmentId, string token)
        {
            try
            {
                return await _apiService.PostAsync<bool>($"/api/IncomeInstallments/{installmentId}/cancel", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar parcela: {InstallmentId}", installmentId);
                throw;
            }
        }

        public async Task<IEnumerable<IncomeModel>> GetOverdueIncomesAsync(string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>("/api/Incomes/overdue", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter receitas vencidas");
                throw;
            }
        }
        public async Task<PagedResult<IncomeModel>> GetFilteredAsync(IncomeFilter filter, string token)
        {
            try
            {
                return await _apiService.GetFilteredAsync<PagedResult<IncomeModel>>("/api/Incomes/filter", filter, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter registros filtrados");
                throw;
            }
        }
    }
}
