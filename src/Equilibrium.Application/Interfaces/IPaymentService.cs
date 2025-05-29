using Equilibrium.Application.DTOs.Payment;

namespace Equilibrium.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentDto>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<PaymentDto>> GetByMonthAsync(Guid userId, int month, int year);
        Task<IEnumerable<PaymentDto>> GetPendingAsync(Guid userId);
        Task<IEnumerable<PaymentDto>> GetOverdueAsync(Guid userId);
        Task<IEnumerable<PaymentDto>> GetByTypeAsync(Guid userId, Guid paymentTypeId);
        Task<IEnumerable<PaymentDto>> GetByMethodAsync(Guid userId, Guid paymentMethodId);
        Task<PaymentDto> CreateAsync(CreatePaymentDto createPaymentDto, Guid userId);
        Task<PaymentDto> UpdateAsync(Guid id, UpdatePaymentDto updatePaymentDto);
        Task DeleteAsync(Guid id);
        Task<PaymentDto> MarkAsPaidAsync(Guid id, DateTime paymentDate);
        Task<PaymentDto> MarkAsOverdueAsync(Guid id);
        Task<PaymentDto> CancelAsync(Guid id);
    }
}

