using Equilibrium.Application.DTOs.PaymentMethod;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<PaymentMethodDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentMethodDto>> GetAllSystemMethodsAsync();
        Task<IEnumerable<PaymentMethodDto>> GetUserMethodsAsync(Guid userId);
        Task<IEnumerable<PaymentMethodDto>> GetAllAvailableForUserAsync(Guid userId);
        Task<IEnumerable<PaymentMethodDto>> GetByTypeAsync(PaymentMethodType type);
        Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto createPaymentMethodDto, Guid userId);
        Task<PaymentMethodDto> UpdateAsync(Guid id, UpdatePaymentMethodDto updatePaymentMethodDto);
        Task DeleteAsync(Guid id);
    }
}
