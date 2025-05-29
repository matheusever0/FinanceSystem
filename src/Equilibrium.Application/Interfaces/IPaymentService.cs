using Equilibrium.Application.DTOs.Payment;

namespace Equilibrium.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentDto>> GetFilteredAsync(Guid userId, PaymentFilter filter);
        Task<PaymentDto> CreateAsync(CreatePaymentDto createPaymentDto, Guid userId);
        Task<PaymentDto> UpdateAsync(Guid id, UpdatePaymentDto updatePaymentDto);
        Task DeleteAsync(Guid id);
        Task<PaymentDto> MarkAsPaidAsync(Guid id, DateTime paymentDate);
        Task<PaymentDto> MarkAsOverdueAsync(Guid id);
        Task<PaymentDto> CancelAsync(Guid id);
    }
}

