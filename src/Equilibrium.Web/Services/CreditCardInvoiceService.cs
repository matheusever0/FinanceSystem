using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.CreditCard;

namespace Equilibrium.Web.Services
{
    public class CreditCardInvoiceService : ICreditCardInvoiceService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<CreditCardInvoiceService> _logger;

        public CreditCardInvoiceService(IApiService apiService, ILogger<CreditCardInvoiceService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<CreditCardInvoiceModel> GetCurrentInvoiceAsync(string creditCardId, string token)
        {
            try
            {
                var invoice = await _apiService.GetAsync<CreditCardInvoiceModel>(
                    $"creditcards/{creditCardId}/invoices/current", token);
                return invoice ?? new CreditCardInvoiceModel
                {
                    CreditCardId = creditCardId,
                    CreditCardName = "Cartão não encontrado"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar fatura atual do cartão {CreditCardId}", creditCardId);
                throw;
            }
        }

        public async Task<IEnumerable<CreditCardInvoiceModel>> GetInvoiceHistoryAsync(string creditCardId, int months, string token)
        {
            try
            {
                var invoices = await _apiService.GetAsync<IEnumerable<CreditCardInvoiceModel>>(
                    $"creditcards/{creditCardId}/invoices/history?months={months}", token);
                return invoices ?? new List<CreditCardInvoiceModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar histórico de faturas do cartão {CreditCardId}", creditCardId);
                throw;
            }
        }

        public async Task<CreditCardInvoiceDetailModel> GetInvoiceDetailsAsync(string creditCardId, DateTime? referenceDate, string token)
        {
            try
            {
                var dateParam = referenceDate?.ToString("yyyy-MM-dd") ?? "";
                var invoiceDetails = await _apiService.GetAsync<CreditCardInvoiceDetailModel>(
                    $"creditcards/{creditCardId}/invoices/details?referenceDate={dateParam}", token);
                return invoiceDetails ?? new CreditCardInvoiceDetailModel
                {
                    CreditCardId = creditCardId,
                    CreditCardName = "Cartão não encontrado"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes da fatura do cartão {CreditCardId}", creditCardId);
                throw;
            }
        }

        public async Task<bool> PayInvoiceAsync(string creditCardId, PayInvoiceModel paymentData, string token)
        {
            try
            {
                await _apiService.PostAsync<object>(
                    $"creditcards/{creditCardId}/invoices/pay", paymentData, token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pagar fatura do cartão {CreditCardId}", creditCardId);
                throw;
            }
        }
    }
}