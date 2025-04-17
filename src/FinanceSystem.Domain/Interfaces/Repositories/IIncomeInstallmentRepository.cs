using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IIncomeInstallmentRepository : IRepositoryBase<IncomeInstallment>
    {
        Task<IEnumerable<IncomeInstallment>> GetInstallmentsByIncomeIdAsync(Guid incomeId);
        Task<IEnumerable<IncomeInstallment>> GetInstallmentsByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<IncomeInstallment>> GetPendingInstallmentsByUserIdAsync(Guid userId);
        Task<IEnumerable<IncomeInstallment>> GetReceivedInstallmentsByUserIdAsync(Guid userId);
    }
}