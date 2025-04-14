using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models;

namespace FinanceSystem.Web.Services
{
    public class PaymentTypeService : IPaymentTypeService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PaymentTypeService> _logger;

        public PaymentTypeService(IApiService apiService, ILogger<PaymentTypeService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentTypeModel>> GetAllPaymentTypesAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo todos os tipos de pagamento");
                return await _apiService.GetAsync<IEnumerable<PaymentTypeModel>>("/api/payment-types", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipos de pagamento");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTypeModel>> GetSystemPaymentTypesAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo tipos de pagamento do sistema");
                return await _apiService.GetAsync<IEnumerable<PaymentTypeModel>>("/api/payment-types/system", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipos de pagamento do sistema");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTypeModel>> GetUserPaymentTypesAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo tipos de pagamento do usuário");
                return await _apiService.GetAsync<IEnumerable<PaymentTypeModel>>("/api/payment-types/user", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipos de pagamento do usuário");
                throw;
            }
        }

        public async Task<PaymentTypeModel> GetPaymentTypeByIdAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo tipo de pagamento com ID: {TypeId}", id);
                return await _apiService.GetAsync<PaymentTypeModel>($"/api/payment-types/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipo de pagamento com ID: {TypeId}", id);
                throw;
            }
        }

        public async Task<PaymentTypeModel> CreatePaymentTypeAsync(CreatePaymentTypeModel model, string token)
        {
            try
            {
                _logger.LogInformation("Criando novo tipo de pagamento: {Name}", model.Name);
                return await _apiService.PostAsync<PaymentTypeModel>("/api/payment-types", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tipo de pagamento: {Name}", model.Name);
                throw;
            }
        }

        public async Task<PaymentTypeModel> UpdatePaymentTypeAsync(string id, UpdatePaymentTypeModel model, string token)
        {
            try
            {
                _logger.LogInformation("Atualizando tipo de pagamento com ID: {TypeId}", id);
                return await _apiService.PutAsync<PaymentTypeModel>($"/api/payment-types/{id}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tipo de pagamento com ID: {TypeId}", id);
                throw;
            }
        }

        public async Task DeletePaymentTypeAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Excluindo tipo de pagamento com ID: {TypeId}", id);
                await _apiService.DeleteAsync($"/api/payment-types/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tipo de pagamento com ID: {TypeId}", id);
                throw;
            }
        }
    }
}