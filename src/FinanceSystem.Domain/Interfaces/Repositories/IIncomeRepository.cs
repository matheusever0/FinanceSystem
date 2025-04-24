using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IIncomeRepository : IRepositoryBase<Income>
    {
        Task<Income?> GetIncomeWithDetailsAsync(Guid id);
        Task<IEnumerable<Income>> GetIncomesByUserIdAsync(Guid userId);
        Task<IEnumerable<Income>> GetIncomesByUserIdAndMonthAsync(Guid userId, int month, int year);
        Task<IEnumerable<Income>> GetPendingIncomesByUserIdAsync(Guid userId);
        Task<IEnumerable<Income>> GetReceivedIncomesByUserIdAsync(Guid userId);
        Task<IEnumerable<Income>> GetIncomesByTypeAsync(Guid userId, Guid incomeTypeId);
        Task<IEnumerable<Income>> GetOverdueIncomesByUserIdAsync(Guid userId);
        Task<IEnumerable<Income>> GetRecurringIncomesWithDetailsAsync();
        Task<IEnumerable<Income>> GetIncomesByPeriodAndDetailsAsync(Guid userId, Guid incomeTypeId, string description, DateTime startDate, DateTime endDate);
    }
}