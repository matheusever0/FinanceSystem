using FinanceSystem.Application.DTOs.PaymentInstallmentDto;

namespace FinanceSystem.Application.Interfaces
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
    }
}
