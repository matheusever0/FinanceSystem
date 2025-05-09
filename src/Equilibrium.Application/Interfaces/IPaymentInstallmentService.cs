using Equilibrium.Application.DTOs.Common;
using Equilibrium.Application.DTOs.PaymentInstallment;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Interfaces
{
    public interface IPaymentInstallmentService
    {
        Task<PaymentInstallmentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentInstallmentDto>> GetByPaymentIdAsync(Guid paymentId);
        Task<IEnumerable<PaymentInstallmentDto>> GetByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentInstallmentDto>> GetPendingAsync(Guid userId);
        Task<IEnumerable<PaymentInstallmentDto>> GetOverdueAsync(Guid userId);
        Task<PaymentInstallmentDto> MarkAsPaidAsync(Guid id, DateTime paymentDate);
        Task<PaymentInstallmentDto> MarkAsOverdueAsync(Guid id);
        Task<PaymentInstallmentDto> CancelAsync(Guid id);
        Task<PagedResult<PaymentInstallmentDto>> GetFilteredAsync(PaymentInstallmentFilter filter, Guid userId);
    }
}

