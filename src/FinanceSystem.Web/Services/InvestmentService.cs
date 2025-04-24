using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models.Investment;

namespace FinanceSystem.Web.Services
{
    public class InvestmentService : IInvestmentService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<InvestmentService> _logger;

        public InvestmentService(IApiService apiService, ILogger<InvestmentService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<InvestmentModel>> GetAllInvestmentsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo todos os investimentos");
                return await _apiService.GetAsync<IEnumerable<InvestmentModel>>("/api/investments", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter investimentos");
                throw;
            }
        }

        public async Task<InvestmentModel> GetInvestmentByIdAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo investimento com ID: {InvestmentId}", id);
                return await _apiService.GetAsync<InvestmentModel>($"/api/investments/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter investimento com ID: {InvestmentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<InvestmentModel>> GetInvestmentsByTypeAsync(int type, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo investimentos por tipo: {Type}", type);
                return await _apiService.GetAsync<IEnumerable<InvestmentModel>>($"/api/investments/type/{type}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter investimentos por tipo: {Type}", type);
                throw;
            }
        }

        public async Task<InvestmentModel> CreateInvestmentAsync(CreateInvestmentModel model, string token)
        {
            try
            {
                _logger.LogInformation("Criando novo investimento: {Symbol} - {Name}", model.Symbol, model.Name);
                return await _apiService.PostAsync<InvestmentModel>("/api/investments", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar investimento: {Symbol} - {Name}", model.Symbol, model.Name);
                throw;
            }
        }

        public async Task<InvestmentModel> UpdateInvestmentAsync(string id, UpdateInvestmentModel model, string token)
        {
            try
            {
                _logger.LogInformation("Atualizando investimento com ID: {InvestmentId}", id);
                return await _apiService.PutAsync<InvestmentModel>($"/api/investments/{id}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar investimento com ID: {InvestmentId}", id);
                throw;
            }
        }

        public async Task DeleteInvestmentAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Excluindo investimento com ID: {InvestmentId}", id);
                await _apiService.DeleteAsync($"/api/investments/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir investimento com ID: {InvestmentId}", id);
                throw;
            }
        }

        public async Task<InvestmentModel> RefreshPriceAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Atualizando preço do investimento com ID: {InvestmentId}", id);
                return await _apiService.PostAsync<InvestmentModel>($"/api/investments/{id}/refresh", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar preço do investimento com ID: {InvestmentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<InvestmentModel>> RefreshAllPricesAsync(string token)
        {
            try
            {
                _logger.LogInformation("Atualizando preços de todos os investimentos");
                return await _apiService.PostAsync<IEnumerable<InvestmentModel>>("/api/investments/refresh-all", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar preços de todos os investimentos");
                throw;
            }
        }
    }
}