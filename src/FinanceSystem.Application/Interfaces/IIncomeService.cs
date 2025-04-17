using FinanceSystem.Application.DTOs;

namespace FinanceSystem.Application.Interfaces
{
    public interface IIncomeService
    {
        Task<IncomeDto> GetByIdAsync(Guid id);
        Task<IEnumerable<IncomeDto>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<IncomeDto>> GetByMonthAsync(Guid userId, int month, int year);
        Task<IEnumerable<IncomeDto>> GetPendingAsync(Guid userId);
        Task<IEnumerable<IncomeDto>> GetReceivedAsync(Guid userId);
        Task<IEnumerable<IncomeDto>> GetByTypeAsync(Guid userId, Guid incomeTypeId);
        Task<IncomeDto> CreateAsync(CreateIncomeDto createIncomeDto, Guid userId);
        Task<IncomeDto> UpdateAsync(Guid id, UpdateIncomeDto updateIncomeDto);
        Task DeleteAsync(Guid id);
        Task<IncomeDto> MarkAsReceivedAsync(Guid id, DateTime? receivedDate = null);
        Task<IncomeDto> CancelAsync(Guid id);
    }
}