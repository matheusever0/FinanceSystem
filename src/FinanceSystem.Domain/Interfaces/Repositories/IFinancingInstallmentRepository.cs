using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IFinancingInstallmentRepository : IRepositoryBase<FinancingInstallment>
    {
        Task<IEnumerable<FinancingInstallment>> GetInstallmentsByFinancingIdAsync(Guid financingId);
        Task<IEnumerable<FinancingInstallment>> GetInstallmentsByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<FinancingInstallment>> GetPendingInstallmentsByUserIdAsync(Guid userId);
        Task<IEnumerable<FinancingInstallment>> GetOverdueInstallmentsByUserIdAsync(Guid userId);
        Task<IEnumerable<FinancingInstallment>> GetInstallmentsByStatusAsync(Guid userId, FinancingInstallmentStatus status);
    }
}