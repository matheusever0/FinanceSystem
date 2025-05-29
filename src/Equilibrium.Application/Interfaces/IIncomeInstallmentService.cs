using Equilibrium.Application.DTOs.IncomeInstallment;

namespace Equilibrium.Application.Interfaces
{
    public interface IIncomeInstallmentService
    {
        Task<IncomeInstallmentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<IncomeInstallmentDto>> GetByIncomeIdAsync(Guid incomeId);
        Task<IEnumerable<IncomeInstallmentDto>> GetByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<IncomeInstallmentDto>> GetPendingAsync(Guid userId);
        Task<IEnumerable<IncomeInstallmentDto>> GetReceivedAsync(Guid userId);
        Task<IncomeInstallmentDto> MarkAsReceivedAsync(Guid id, DateTime? receivedDate = null);
        Task<IncomeInstallmentDto> CancelAsync(Guid id);
    }
}
