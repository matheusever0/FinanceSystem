using FinanceSystem.Application.DTOs.PaymentType;

namespace FinanceSystem.Application.Interfaces
{
    public interface IPaymentTypeService
    {
        Task<PaymentTypeDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentTypeDto>> GetAllSystemTypesAsync();
        Task<IEnumerable<PaymentTypeDto>> GetUserTypesAsync(Guid userId);
        Task<IEnumerable<PaymentTypeDto>> GetAllAvailableForUserAsync(Guid userId);
        Task<PaymentTypeDto> CreateAsync(CreatePaymentTypeDto createPaymentTypeDto, Guid userId);
        Task<PaymentTypeDto> UpdateAsync(Guid id, UpdatePaymentTypeDto updatePaymentTypeDto);
        Task DeleteAsync(Guid id);
    }
}
