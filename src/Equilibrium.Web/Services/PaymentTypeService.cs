using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models;

namespace Equilibrium.Web.Services
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
                return await _apiService.GetAsync<IEnumerable<PaymentTypeModel>>("/api/PaymentTypes", token);
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
                return await _apiService.GetAsync<IEnumerable<PaymentTypeModel>>("/api/PaymentTypes/system", token);
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
                return await _apiService.GetAsync<IEnumerable<PaymentTypeModel>>("/api/PaymentTypes/user", token);
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
                return await _apiService.GetAsync<PaymentTypeModel>($"/api/PaymentTypes/{id}", token);
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
                return await _apiService.PostAsync<PaymentTypeModel>("/api/PaymentTypes", model, token);
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
                return await _apiService.PutAsync<PaymentTypeModel>($"/api/PaymentTypes/{id}", model, token);
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
                await _apiService.DeleteAsync($"/api/PaymentTypes/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tipo de pagamento com ID: {TypeId}", id);
                throw;
            }
        }
    }
}
