using Equilibrium.Web.Models.CreditCard;

namespace Equilibrium.Web.Interfaces
{
    public interface ICreditCardInvoiceService
    {
        Task<CreditCardInvoiceModel> GetCurrentInvoiceAsync(string creditCardId, string token);
        Task<IEnumerable<CreditCardInvoiceModel>> GetInvoiceHistoryAsync(string creditCardId, int months, string token);
        Task<CreditCardInvoiceDetailModel> GetInvoiceDetailsAsync(string creditCardId, DateTime? referenceDate, string token);
        Task<bool> PayInvoiceAsync(string creditCardId, PayInvoiceModel paymentData, string token);
    }
}
