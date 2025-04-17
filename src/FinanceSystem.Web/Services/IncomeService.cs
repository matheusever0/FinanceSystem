using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models.Income;

namespace FinanceSystem.Web.Services
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
                _logger.LogInformation("Obtendo todas as receitas");
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>("/api/incomes", token);
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
                _logger.LogInformation("Obtendo receita com ID: {IncomeId}", id);
                return await _apiService.GetAsync<IncomeModel>($"/api/incomes/{id}", token);
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
                _logger.LogInformation("Obtendo receitas do mês {Month}/{Year}", month, year);
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>($"/api/incomes/month/{year}/{month}", token);
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
                _logger.LogInformation("Obtendo receitas pendentes");
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>("/api/incomes/pending", token);
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
                _logger.LogInformation("Obtendo receitas recebidas");
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>("/api/incomes/received", token);
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
                _logger.LogInformation("Obtendo receitas por tipo: {TypeId}", typeId);
                return await _apiService.GetAsync<IEnumerable<IncomeModel>>($"/api/incomes/type/{typeId}", token);
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
                _logger.LogInformation("Criando nova receita: {Description}", model.Description);
                return await _apiService.PostAsync<IncomeModel>("/api/incomes", model, token);
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
                _logger.LogInformation("Atualizando receita com ID: {IncomeId}", id);
                return await _apiService.PutAsync<IncomeModel>($"/api/incomes/{id}", model, token);
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
                _logger.LogInformation("Excluindo receita com ID: {IncomeId}", id);
                await _apiService.DeleteAsync($"/api/incomes/{id}", token);
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
                _logger.LogInformation("Marcando receita como recebida: {IncomeId}", id);
                return await _apiService.PostAsync<IncomeModel>($"/api/incomes/{id}/received", receivedDate, token);
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
                _logger.LogInformation("Cancelando receita: {IncomeId}", id);
                return await _apiService.PostAsync<IncomeModel>($"/api/incomes/{id}/cancel", null, token);
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
                _logger.LogInformation("Obtendo receita pai da parcela com ID: {InstallmentId}", installmentId);

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
                _logger.LogInformation("Marcando parcela como recebida: {InstallmentId}", installmentId);
                return await _apiService.PostAsync<bool>($"/api/income-installments/{installmentId}/received", receivedDate, token);
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
                _logger.LogInformation("Cancelando parcela: {InstallmentId}", installmentId);
                return await _apiService.PostAsync<bool>($"/api/income-installments/{installmentId}/cancel", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar parcela: {InstallmentId}", installmentId);
                throw;
            }
        }
    }
}