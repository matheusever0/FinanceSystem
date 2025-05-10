using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Financing;
using Equilibrium.Web.Models.Payment;

namespace Equilibrium.Web.Services
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
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>("/api/Payments", token);
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
                return await _apiService.GetAsync<PaymentModel>($"/api/Payments/{id}", token);
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
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>($"/api/Payments/month/{year}/{month}", token);
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
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>("/api/Payments/pending", token);
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
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>("/api/Payments/overdue", token);
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
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>($"/api/Payments/type/{typeId}", token);
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
                return await _apiService.GetAsync<IEnumerable<PaymentModel>>($"/api/Payments/method/{methodId}", token);
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
                return await _apiService.PostAsync<PaymentModel>("/api/Payments", model, token);
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
                return await _apiService.PutAsync<PaymentModel>($"/api/Payments/{id}", model, token);
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
                await _apiService.DeleteAsync($"/api/Payments/{id}", token);
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
                return await _apiService.PostAsync<PaymentModel>($"/api/Payments/{id}/paid", paymentDate, token);
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
                return await _apiService.PostAsync<PaymentModel>($"/api/Payments/{id}/overdue", null, token);
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
                return await _apiService.PostAsync<PaymentModel>($"/api/Payments/{id}/cancel", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pagamento: {PaymentId}", id);
                throw;
            }
        }

        public async Task<string> GetInstallmentParentPaymentAsync(string installmentId, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo pagamento pai da parcela com ID: {InstallmentId}", installmentId);

                var payments = await GetAllPaymentsAsync(token);

                foreach (var payment in payments)
                {
                    if (payment.Installments != null && payment.Installments.Any(i => i.Id == installmentId))
                    {
                        return payment.Id;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pagamento pai da parcela: {InstallmentId}", installmentId);
                throw;
            }
        }

        public async Task<bool> MarkInstallmentAsPaidAsync(string installmentId, DateTime paymentDate, string token)
        {
            try
            {
                _logger.LogInformation("Marcando parcela como paga: {InstallmentId}", installmentId);
                return await _apiService.PostAsync<bool>($"/api/PaymentInstallments/{installmentId}/paid", paymentDate, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar parcela como paga: {InstallmentId}", installmentId);
                throw;
            }
        }

        public async Task<bool> MarkInstallmentAsOverdueAsync(string installmentId, string token)
        {
            try
            {
                _logger.LogInformation("Marcando parcela como vencida: {InstallmentId}", installmentId);
                return await _apiService.PostAsync<bool>($"/api/PaymentInstallments/{installmentId}/overdue", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar parcela como vencida: {InstallmentId}", installmentId);
                throw;
            }
        }

        public async Task<bool> CancelInstallmentAsync(string installmentId, string token)
        {
            try
            {
                _logger.LogInformation("Cancelando parcela: {InstallmentId}", installmentId);
                return await _apiService.PostAsync<bool>($"/api/PaymentInstallments/{installmentId}/cancel", null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar parcela: {InstallmentId}", installmentId);
                throw;
            }
        }

        // Adicionar ao PaymentService.cs
        public async Task<IEnumerable<FinancingModel>> GetAvailableFinancingsAsync(string token)
        {
            try
            {
                _logger.LogInformation("Obtendo financiamentos disponíveis para pagamento");
                return await _apiService.GetAsync<IEnumerable<FinancingModel>>("/api/Payments/financing-options", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter financiamentos disponíveis");
                throw;
            }
        }

        public async Task<IEnumerable<FinancingInstallmentModel>> GetFinancingInstallmentsAsync(string financingId, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo parcelas do financiamento {FinancingId}", financingId);
                return await _apiService.GetAsync<IEnumerable<FinancingInstallmentModel>>(
                    $"/api/Payments/financing/{financingId}/installments", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter parcelas do financiamento {FinancingId}", financingId);
                throw;
            }
        }
        public async Task<PagedResult<PaymentModel>> GetFilteredAsync(PaymentFilter filter, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo registros filtrados");
                return await _apiService.GetFilteredAsync<PagedResult<PaymentModel>>("/api/Payments/filter", filter, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter registros filtrados");
                throw;
            }
        }
    }
}
