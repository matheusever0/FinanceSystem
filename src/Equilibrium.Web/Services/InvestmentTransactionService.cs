using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Investment;

namespace Equilibrium.Web.Services
{
    public class InvestmentTransactionService : IInvestmentTransactionService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<InvestmentTransactionService> _logger;

        public InvestmentTransactionService(IApiService apiService, ILogger<InvestmentTransactionService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<InvestmentTransactionModel>> GetTransactionsByInvestmentIdAsync(string investmentId, string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<InvestmentTransactionModel>>($"/api/InvestmentTransactions/investment/{investmentId}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter transações do investimento com ID: {InvestmentId}", investmentId);
                throw;
            }
        }

        public async Task<InvestmentTransactionModel> GetTransactionByIdAsync(string id, string token)
        {
            try
            {
                return await _apiService.GetAsync<InvestmentTransactionModel>($"/api/InvestmentTransactions/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter transação com ID: {TransactionId}", id);
                throw;
            }
        }

        public async Task<InvestmentTransactionModel> CreateTransactionAsync(string investmentId, CreateInvestmentTransactionModel model, string token)
        {
            try
            {
                return await _apiService.PostAsync<InvestmentTransactionModel>($"/api/InvestmentTransactions/investment/{investmentId}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar transação para o investimento com ID: {InvestmentId}", investmentId);
                throw;
            }
        }

        public async Task DeleteTransactionAsync(string id, string token)
        {
            try
            {
                await _apiService.DeleteAsync($"/api/InvestmentTransactions/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir transação com ID: {TransactionId}", id);
                throw;
            }
        }
    }
}