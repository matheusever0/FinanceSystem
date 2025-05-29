using Equilibrium.Application.DTOs.Income;

namespace Equilibrium.Application.Interfaces
{
    public interface IIncomeService
    {
        Task<IEnumerable<IncomeDto>> GetFilteredAsync(Guid userId, IncomeFilter filter);
        Task<IncomeDto> GetByIdAsync(Guid id);
        Task<IncomeDto> CreateAsync(CreateIncomeDto createIncomeDto, Guid userId);
        Task<IncomeDto> UpdateAsync(Guid id, UpdateIncomeDto updateIncomeDto);
        Task DeleteAsync(Guid id);
        Task<IncomeDto> MarkAsReceivedAsync(Guid id, DateTime? receivedDate = null);
        Task<IncomeDto> CancelAsync(Guid id);
    }
}
