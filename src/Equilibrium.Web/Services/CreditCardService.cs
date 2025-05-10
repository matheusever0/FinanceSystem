using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.CreditCard;

namespace Equilibrium.Web.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<CreditCardService> _logger;

        public CreditCardService(IApiService apiService, ILogger<CreditCardService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<CreditCardModel>> GetAllCreditCardsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo todos os cartões de crédito");
                return await _apiService.GetAsync<IEnumerable<CreditCardModel>>("/api/CreditCards", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter cartões de crédito");
                throw;
            }
        }

        public async Task<CreditCardModel> GetCreditCardByIdAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo cartão de crédito com ID: {CardId}", id);
                return await _apiService.GetAsync<CreditCardModel>($"/api/CreditCards/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter cartão de crédito com ID: {CardId}", id);
                throw;
            }
        }

        public async Task<CreditCardModel> CreateCreditCardAsync(CreateCreditCardModel model, string token)
        {
            try
            {
                _logger.LogInformation("Criando novo cartão de crédito: {Name}", model.Name);
                return await _apiService.PostAsync<CreditCardModel>("/api/CreditCards", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cartão de crédito: {Name}", model.Name);
                throw;
            }
        }

        public async Task<CreditCardModel> UpdateCreditCardAsync(string id, UpdateCreditCardModel model, string token)
        {
            try
            {
                _logger.LogInformation("Atualizando cartão de crédito com ID: {CardId}", id);
                return await _apiService.PutAsync<CreditCardModel>($"/api/CreditCards/{id}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar cartão de crédito com ID: {CardId}", id);
                throw;
            }
        }

        public async Task DeleteCreditCardAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Excluindo cartão de crédito com ID: {CardId}", id);
                await _apiService.DeleteAsync($"/api/CreditCards/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir cartão de crédito com ID: {CardId}", id);
                throw;
            }
        }
        public async Task<PagedResult<CreditCardModel>> GetFilteredAsync(CreditCardFilter filter, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo registros filtrados");
                return await _apiService.GetFilteredAsync<PagedResult<CreditCardModel>>("/api/CreditCards/filter", filter, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter registros filtrados");
                throw;
            }
        }
    }
}
