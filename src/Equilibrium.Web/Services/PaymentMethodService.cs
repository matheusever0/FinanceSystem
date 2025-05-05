using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.PaymentMethod;

namespace Equilibrium.Web.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PaymentMethodService> _logger;

        public PaymentMethodService(IApiService apiService, ILogger<PaymentMethodService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentMethodModel>> GetAllPaymentMethodsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo todos os métodos de pagamento");
                return await _apiService.GetAsync<IEnumerable<PaymentMethodModel>>("/api/PaymentMethods", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métodos de pagamento");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentMethodModel>> GetSystemPaymentMethodsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo métodos de pagamento do sistema");
                return await _apiService.GetAsync<IEnumerable<PaymentMethodModel>>("/api/PaymentMethods/system", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métodos de pagamento do sistema");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentMethodModel>> GetUserPaymentMethodsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo métodos de pagamento do usuário");
                return await _apiService.GetAsync<IEnumerable<PaymentMethodModel>>("/api/PaymentMethods/user", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métodos de pagamento do usuário");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentMethodModel>> GetByTypeAsync(int type, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo métodos de pagamento por tipo: {Type}", type);
                return await _apiService.GetAsync<IEnumerable<PaymentMethodModel>>($"/api/PaymentMethods/type/{type}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métodos de pagamento por tipo: {Type}", type);
                throw;
            }
        }

        public async Task<PaymentMethodModel> GetPaymentMethodByIdAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo método de pagamento com ID: {MethodId}", id);
                return await _apiService.GetAsync<PaymentMethodModel>($"/api/PaymentMethods/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter método de pagamento com ID: {MethodId}", id);
                throw;
            }
        }

        public async Task<PaymentMethodModel> CreatePaymentMethodAsync(CreatePaymentMethodModel model, string token)
        {
            try
            {
                _logger.LogInformation("Criando novo método de pagamento: {Name}", model.Name);
                return await _apiService.PostAsync<PaymentMethodModel>("/api/PaymentMethods", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar método de pagamento: {Name}", model.Name);
                throw;
            }
        }

        public async Task<PaymentMethodModel> UpdatePaymentMethodAsync(string id, UpdatePaymentMethodModel model, string token)
        {
            try
            {
                _logger.LogInformation("Atualizando método de pagamento com ID: {MethodId}", id);
                return await _apiService.PutAsync<PaymentMethodModel>($"/api/PaymentMethods/{id}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar método de pagamento com ID: {MethodId}", id);
                throw;
            }
        }

        public async Task DeletePaymentMethodAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Excluindo método de pagamento com ID: {MethodId}", id);
                await _apiService.DeleteAsync($"/api/PaymentMethods/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir método de pagamento com ID: {MethodId}", id);
                throw;
            }
        }
    }
}