using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models;

namespace FinanceSystem.Web.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IApiService apiService, ILogger<PaymentService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentModel>> GetAllPaymentsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo todos os pagamentos");
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>("/api/payments", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pagamentos");
                throw;
            }
        }

        public async Task<PaymentModel> GetPaymentByIdAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo pagamento com ID: {PaymentId}", id);
                return await _apiService.GetAsync<PaymentModel>($"/api/payments/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pagamento com ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByMonthAsync(int month, int year, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo pagamentos do mês {Month}/{Year}", month, year);
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>($"/api/payments/month/{year}/{month}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pagamentos do mês {Month}/{Year}", month, year);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetPendingPaymentsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo pagamentos pendentes");
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>("/api/payments/pending", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pagamentos pendentes");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetOverduePaymentsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo pagamentos vencidos");
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>("/api/payments/overdue", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pagamentos vencidos");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByTypeAsync(string typeId, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo pagamentos por tipo: {TypeId}", typeId);
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>($"/api/payments/type/{typeId}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pagamentos por tipo: {TypeId}", typeId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentsByMethodAsync(string methodId, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo pagamentos por método: {MethodId}", methodId);
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>($"/api/payments/method/{methodId}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pagamentos por método: {MethodId}", methodId);
                throw;
            }
        }

        public async Task<PaymentModel> CreatePaymentAsync(CreatePaymentModel model, string token)
        {
            try
            {
                _logger.LogInformation("Criando novo pagamento: {Description}", model.Description);
                return await _apiService.PostAsync<PaymentModel>("/api/payments", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pagamento: {Description}", model.Description);
                throw;
            }
        }

        public async Task<PaymentModel> UpdatePaymentAsync(string id, UpdatePaymentModel model, string token)
        {
            try
            {
                _logger.LogInformation("Atualizando pagamento com ID: {PaymentId}", id);
                return await _apiService.PutAsync<PaymentModel>($"/api/payments/{id}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pagamento com ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task DeletePaymentAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Excluindo pagamento com ID: {PaymentId}", id);
                await _apiService.DeleteAsync($"/api/payments/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pagamento com ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task<PaymentModel> MarkAsPaidAsync(string id, DateTime? paymentDate, string token)
        {
            try
            {
                _logger.LogInformation("Marcando pagamento como pago: {PaymentId}", id);
                return await _apiService.PostAsync<PaymentModel>($"/api/payments/{id}/paid", paymentDate, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar pagamento como pago: {PaymentId}", id);
                throw;
            }
        }

        public async Task<PaymentModel> MarkAsOverdueAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Marcando pagamento como vencido: {PaymentId}", id);
                return await _apiService.PostAsync<PaymentModel>($"/api/payments/{id}/overdue", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar pagamento como vencido: {PaymentId}", id);
                throw;
            }
        }

        public async Task<PaymentModel> CancelPaymentAsync(string id, string token)
        {
            try
            {
                _logger.LogInformation("Cancelando pagamento: {PaymentId}", id);
                return await _apiService.PostAsync<PaymentModel>($"/api/payments/{id}/cancel", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pagamento: {PaymentId}", id);
                throw;
            }
        }
    }
}