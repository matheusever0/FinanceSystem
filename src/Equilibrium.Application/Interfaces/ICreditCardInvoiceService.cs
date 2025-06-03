using Equilibrium.Application.DTOs.CreditCard;

namespace Equilibrium.Application.Interfaces
{
    public interface ICreditCardInvoiceService
    {
        Task<CreditCardInvoiceDto> GetCurrentInvoiceAsync(Guid creditCardId);
        Task<CreditCardInvoiceDto> GetInvoiceByPeriodAsync(Guid creditCardId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<CreditCardInvoiceDto>> GetInvoiceHistoryAsync(Guid creditCardId, int months = 12);
        Task<CreditCardDto> PayInvoiceAsync(Guid creditCardId, PayInvoiceDto payInvoiceDto);
        Task<CreditCardInvoiceDetailDto> GetInvoiceDetailsAsync(Guid creditCardId, DateTime referenceDate);
    }
}