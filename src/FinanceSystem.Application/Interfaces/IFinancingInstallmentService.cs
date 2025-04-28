using FinanceSystem.Application.DTOs.Financing;

namespace FinanceSystem.Application.Interfaces
{
    public interface IFinancingInstallmentService
    {
        Task<FinancingInstallmentDto> GetByIdAsync(Guid id);
        Task<FinancingInstallmentDetailDto> GetDetailsByIdAsync(Guid id);
        Task<IEnumerable<FinancingInstallmentDto>> GetByFinancingIdAsync(Guid financingId);
        Task<IEnumerable<FinancingInstallmentDto>> GetByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<FinancingInstallmentDto>> GetPendingAsync(Guid userId);
        Task<IEnumerable<FinancingInstallmentDto>> GetOverdueAsync(Guid userId);
        Task<FinancingInstallmentDto> ProcessPaymentAsync(FinancingInstallmentPaymentDto paymentDto);
        Task<FinancingInstallmentDto> MarkAsOverdueAsync(Guid id);
    }
}
