using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models.Financing;

namespace FinanceSystem.Web.Services
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
                return await _apiService.GetAsync<IEnumerable<FinancingModel>>("/api/financings", token);
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
                return await _apiService.GetAsync<IEnumerable<FinancingModel>>("/api/financings/active", token);
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
                return await _apiService.GetAsync<FinancingModel>($"/api/financings/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter financiamento com ID: {FinancingId}", id);
                throw;
            }
        }

        public async Task<FinancingDetailDto> GetFinancingDetailsAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo detalhes do financiamento com ID: {FinancingId}", id);
                return await _apiService.GetAsync<FinancingDetailDto>($"/api/financings/{id}/details", token);
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
                return await _apiService.GetAsync<IEnumerable<FinancingInstallmentModel>>($"/api/financing-installments/financing/{financingId}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter parcelas do financiamento: {FinancingId}", financingId);
                throw;
            }
        }

        public async Task<FinancingModel> CreateFinancingAsync(CreateFinancingDto model, string token)
        {
            try
            {
                _logger.LogInformation("Criando novo financiamento: {Description}", model.Description);
                return await _apiService.PostAsync<FinancingModel>("/api/financings", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar financiamento: {Description}", model.Description);
                throw;
            }
        }

        public async Task<FinancingModel> UpdateFinancingAsync(string id, UpdateFinancingDto model, string token)
        {
            try
            {
                _logger.LogInformation("Atualizando financiamento com ID: {FinancingId}", id);
                return await _apiService.PutAsync<FinancingModel>($"/api/financings/{id}", model, token);
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
                return await _apiService.PostAsync<bool>($"/api/financings/{id}/cancel", null, token);
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
                return await _apiService.PostAsync<bool>($"/api/financings/{id}/complete", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar financiamento como concluído: {FinancingId}", id);
                throw;
            }
        }

        public async Task<FinancingSimulationDto> SimulateFinancingAsync(FinancingSimulationRequestDto model, string token)
        {
            try
            {
                _logger.LogInformation("Simulando financiamento: {TotalAmount} em {TermMonths} meses",
                    model.TotalAmount, model.TermMonths);
                return await _apiService.PostAsync<FinancingSimulationDto>("/api/financings/simulate", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao simular financiamento");
                throw;
            }
        }

        public async Task<FinancingForecastDto> ForecastFinancingAsync(string financingId, int forecastMonths, string token)
        {
            try
            {
                _logger.LogInformation("Gerando previsão para financiamento: {FinancingId} para {Months} meses",
                    financingId, forecastMonths);

                var request = new
                {
                    FinancingId = financingId,
                    ForecastMonths = forecastMonths
                };

                return await _apiService.PostAsync<FinancingForecastDto>("/api/financings/forecast", request, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar previsão para financiamento: {FinancingId}", financingId);
                throw;
            }
        }

        public async Task<bool> ApplyMonetaryCorrectionAsync(ApplyCorrectionDto model, string token)
        {
            try
            {
                _logger.LogInformation("Aplicando correção monetária ao financiamento: {FinancingId}",
                    model.FinancingId);
                return await _apiService.PostAsync<bool>("/api/financing-corrections/apply", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar correção monetária ao financiamento: {FinancingId}",
                    model.FinancingId);
                throw;
            }
        }
    }
}