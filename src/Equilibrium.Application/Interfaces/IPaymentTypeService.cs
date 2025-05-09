using Equilibrium.Application.DTOs.Common;
using Equilibrium.Application.DTOs.PaymentType;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Interfaces
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
        Task<PagedResult<PaymentTypeDto>> GetFilteredAsync(PaymentTypeFilter filter, Guid userId);
    }
}

