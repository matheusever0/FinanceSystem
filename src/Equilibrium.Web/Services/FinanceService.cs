using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Financing;

namespace Equilibrium.Web.Services
{
    public class FinancingService : IFinancingService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<FinancingService> _logger;

        public FinancingService(IApiService apiService, ILogger<FinancingService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<FinancingModel>> GetAllFinancingsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo todos os financiamentos");
                return await _apiService.GetAsync<IEnumerable<FinancingModel>>("/api/Financings", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter financiamentos");
                throw;
            }
        }

        public async Task<IEnumerable<FinancingModel>> GetActiveFinancingsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo financiamentos ativos");
                return await _apiService.GetAsync<IEnumerable<FinancingModel>>("/api/Financings/active", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter financiamentos ativos");
                throw;
            }
        }

        public async Task<FinancingModel> GetFinancingByIdAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo financiamento com ID: {FinancingId}", id);
                return await _apiService.GetAsync<FinancingModel>($"/api/Financings/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter financiamento com ID: {FinancingId}", id);
                throw;
            }
        }

        public async Task<FinancingDetailModel> GetFinancingDetailsAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo detalhes do financiamento com ID: {FinancingId}", id);
                return await _apiService.GetAsync<FinancingDetailModel>($"/api/Financings/{id}/details", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes do financiamento com ID: {FinancingId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<FinancingInstallmentModel>> GetFinancingInstallmentsAsync(string financingId, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo parcelas do financiamento: {FinancingId}", financingId);
                return await _apiService.GetAsync<IEnumerable<FinancingInstallmentModel>>($"/api/FinancingInstallments/financing/{financingId}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter parcelas do financiamento: {FinancingId}", financingId);
                throw;
            }
        }

        public async Task<FinancingModel> CreateFinancingAsync(CreateFinancingModel model, string token)
        {
            try
            {
                _logger.LogInformation("Criando novo financiamento: {Description}", model.Description);
                return await _apiService.PostAsync<FinancingModel>("/api/Financings", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar financiamento: {Description}", model.Description);
                throw;
            }
        }

        public async Task<FinancingModel> UpdateFinancingAsync(string id, UpdateFinancingModel model, string token)
        {
            try
            {
                _logger.LogInformation("Atualizando financiamento com ID: {FinancingId}", id);
                return await _apiService.PutAsync<FinancingModel>($"/api/Financings/{id}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar financiamento com ID: {FinancingId}", id);
                throw;
            }
        }

        public async Task<bool> CancelFinancingAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Cancelando financiamento: {FinancingId}", id);
                return await _apiService.PostAsync<bool>($"/api/Financings/{id}/cancel", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar financiamento: {FinancingId}", id);
                throw;
            }
        }

        public async Task<bool> CompleteFinancingAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Marcando financiamento como concluído: {FinancingId}", id);
                return await _apiService.PostAsync<bool>($"/api/Financings/{id}/complete", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar financiamento como concluído: {FinancingId}", id);
                throw;
            }
        }

        public async Task<FinancingSimulationModel> SimulateFinancingAsync(FinancingSimulationRequestModel model, string token)
        {
            try
            {
                _logger.LogInformation("Simulando financiamento: {TotalAmount} em {TermMonths} meses",
                    model.TotalAmount, model.TermMonths);
                return await _apiService.PostAsync<FinancingSimulationModel>("/api/Financings/simulate", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao simular financiamento");
                throw;
            }
        }
        public async Task<PagedResult<FinancingModel>> GetFilteredAsync(FinancingFilter filter, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo registros filtrados");
                return await _apiService.GetFilteredAsync<PagedResult<FinancingModel>>("/api/Financings/filter", filter, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter registros filtrados");
                throw;
            }
        }
