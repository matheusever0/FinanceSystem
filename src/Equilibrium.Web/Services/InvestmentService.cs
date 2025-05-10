using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Investment;

namespace Equilibrium.Web.Services
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
                return await _apiService.GetAsync<IEnumerable<InvestmentModel>>("/api/Investments", token);
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
                return await _apiService.GetAsync<InvestmentModel>($"/api/Investments/{id}", token);
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
                return await _apiService.GetAsync<IEnumerable<InvestmentModel>>($"/api/Investments/type/{type}", token);
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
                _logger.LogInformation("Criando novo investimento: {Symbol}", model.Symbol);
                return await _apiService.PostAsync<InvestmentModel>("/api/Investments", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar investimento: {Symbol}", model.Symbol);
                throw;
            }
        }

        public async Task DeleteInvestmentAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Excluindo investimento com ID: {InvestmentId}", id);
                await _apiService.DeleteAsync($"/api/Investments/{id}", token);
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
                return await _apiService.PostAsync<InvestmentModel>($"/api/Investments/{id}/refresh", null, token);
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
                return await _apiService.PostAsync<IEnumerable<InvestmentModel>>("/api/Investments/refresh-all", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar preços de todos os investimentos");
                throw;
            }
        }
        public async Task<PagedResult<Models.Investment.InvestmentModel>> GetFilteredAsync(InvestmentFilter filter, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo registros filtrados");
                return await _apiService.GetFilteredAsync<PagedResult<Models.Investment.InvestmentModel>>("/api/Investments/filter", filter, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter registros filtrados");
                throw;
            }
        }
    }
}
